using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

internal interface IGithubIssueClient
{
    Task<GithubIssueWriteResult> CreateIssueAsync(
        string repository,
        string title,
        string body,
        IReadOnlyList<string> labels,
        CancellationToken ct);

    Task<GithubIssueWriteResult> UpdateIssueAsync(
        string repository,
        int issueNumber,
        string title,
        string body,
        IReadOnlyList<string> labels,
        CancellationToken ct);

    Task<GithubIssueWriteResult> ReopenIssueAsync(
        string repository,
        int issueNumber,
        CancellationToken ct);

    Task<GithubIssueWriteResult> CloseIssueAsync(
        string repository,
        int issueNumber,
        CancellationToken ct);
}

internal sealed class GithubRestIssueClient(HttpClient http, string token, string userAgent) : IGithubIssueClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<GithubIssueWriteResult> CreateIssueAsync(
        string repository,
        string title,
        string body,
        IReadOnlyList<string> labels,
        CancellationToken ct)
    {
        var payload = new GithubIssueCreateRequest(title, body, labels);
        var response = await SendAsync(HttpMethod.Post, $"/repos/{repository}/issues", payload, ct).ConfigureAwait(false);
        return ToResult(response);
    }

    public async Task<GithubIssueWriteResult> UpdateIssueAsync(
        string repository,
        int issueNumber,
        string title,
        string body,
        IReadOnlyList<string> labels,
        CancellationToken ct)
    {
        var payload = new GithubIssueUpdateRequest(title, body, labels, null);
        var response = await SendAsync(HttpMethod.Patch, $"/repos/{repository}/issues/{issueNumber}", payload, ct).ConfigureAwait(false);
        return ToResult(response);
    }

    public async Task<GithubIssueWriteResult> ReopenIssueAsync(
        string repository,
        int issueNumber,
        CancellationToken ct)
    {
        var payload = new GithubIssueStateRequest("open");
        var response = await SendAsync(HttpMethod.Patch, $"/repos/{repository}/issues/{issueNumber}", payload, ct).ConfigureAwait(false);
        return ToResult(response);
    }

    public async Task<GithubIssueWriteResult> CloseIssueAsync(
        string repository,
        int issueNumber,
        CancellationToken ct)
    {
        var payload = new GithubIssueStateRequest("closed");
        var response = await SendAsync(HttpMethod.Patch, $"/repos/{repository}/issues/{issueNumber}", payload, ct).ConfigureAwait(false);
        return ToResult(response);
    }

    // Tor 3 / T3.4: Kommentar an ein bestehendes Issue (Rev 2 — bevorzugt statt Body-Overwrite bei manuell
    // bearbeiteten Issues). Nicht am Interface, weil nur der gated Forward-Apply es nutzt.
    public async Task<GithubIssueWriteResult> CreateCommentAsync(
        string repository,
        int issueNumber,
        string body,
        CancellationToken ct)
    {
        var payload = new GithubIssueCommentRequest(body);
        var response = await SendAsync(HttpMethod.Post, $"/repos/{repository}/issues/{issueNumber}/comments", payload, ct).ConfigureAwait(false);
        // Antwort ist ein Comment-Objekt (html_url = Kommentar-Link); die Issue-Nummer kennen wir bereits.
        return new GithubIssueWriteResult(issueNumber, response.HtmlUrl, "", "");
    }

    private async Task<GithubIssueResponse> SendAsync<T>(HttpMethod method, string path, T payload, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, new Uri(new Uri("https://api.github.com"), path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.ParseAdd(userAgent);
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        request.Content = new StringContent(JsonSerializer.Serialize(payload, Json), Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var message = TryReadGithubError(body) ?? body;
            throw new HttpRequestException($"GitHub API {(int)response.StatusCode} {response.ReasonPhrase}: {message}");
        }

        return JsonSerializer.Deserialize<GithubIssueResponse>(body, Json)
               ?? throw new InvalidOperationException("GitHub response konnte nicht gelesen werden.");
    }

    private static GithubIssueWriteResult ToResult(GithubIssueResponse response)
        => new(response.Number, response.HtmlUrl, response.State ?? "", response.Title ?? "");

    private static string? TryReadGithubError(string body)
    {
        try
        {
            var error = JsonSerializer.Deserialize<GithubErrorResponse>(body, Json);
            return error?.Message;
        }
        catch
        {
            return null;
        }
    }

    private sealed record GithubIssueCreateRequest(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("body")] string Body,
        [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels);

    private sealed record GithubIssueUpdateRequest(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("body")] string Body,
        [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels,
        // R-22: state=null darf NICHT mitgesendet werden — GitHubs PATCH-Schema lehnt null ab (422 oneOf).
        [property: JsonPropertyName("state")][property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? State);

    private sealed record GithubIssueStateRequest(
        [property: JsonPropertyName("state")] string State);

    private sealed record GithubIssueCommentRequest(
        [property: JsonPropertyName("body")] string Body);

    private sealed record GithubIssueResponse(
        [property: JsonPropertyName("number")] int Number,
        [property: JsonPropertyName("html_url")] string? HtmlUrl,
        [property: JsonPropertyName("state")] string? State,
        [property: JsonPropertyName("title")] string? Title);

    private sealed record GithubErrorResponse(
        [property: JsonPropertyName("message")] string? Message);
}
