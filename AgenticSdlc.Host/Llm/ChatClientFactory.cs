using AgenticSdlc.Host.Configuration;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace AgenticSdlc.Host.Llm;

/// <summary>
/// Erstellt den technischen Basis-Chat-Client für den konfigurierten LLM-Provider.
/// </summary>
/// <remarks>
/// Diese Factory kapselt die Provider-spezifische Initialisierung. Der restliche Host muss dadurch nicht wissen,
/// ob ein lokales Ollama-Modell oder ein OpenAI-kompatibler OpenRouter-Endpunkt verwendet wird.
/// (Ollama-Modell sind in diesem fall die lokal laufenden KI-Modelle wie bspw qwen2.5:14b
/// Die Factory baut aber "nur" den Basis-Client.
/// Tool Invocation, OpenTelemetry und weitere Middlewares werden weiterhin ausserhalb in der
/// Chat-Pipeline konfiguriert. Dadurch kann derselbe Basis-Client später auch für weitere Phasen oder Agenten wiederverwendet werden.
/// </remarks>
public static class ChatClientFactory
{
    /// <summary>
    /// Erstellt einen <see cref="IChatClient"/> anhand der aktuellen Host-Settings: (/AgenticSdlc.Host/Configuration/..)
    /// </summary>
    public static IChatClient Create(HostSettings settings)
    {
        return settings.LlmProvider switch
        {
            "ollama" => new OllamaApiClient(new Uri(settings.OllamaBaseUrl), settings.ModelId),
            "openrouter" => CreateOpenRouterChatClient(settings),
            _ => throw new InvalidOperationException(
                $"Unknown LLM_PROVIDER '{settings.LlmProvider}'. Use 'ollama' or 'openrouter'.")
        };
    }

    private static IChatClient CreateOpenRouterChatClient(HostSettings settings)
    {
        if (!settings.ModelId.Contains('/'))
        {
            throw new InvalidOperationException(
                $"OpenRouter model id '{settings.ModelId}' is invalid for this project. Use an OpenRouter model id like 'openai/gpt-4.1-mini'.");
        }

        if (string.IsNullOrWhiteSpace(settings.OpenRouterApiKey))
        {
            throw new InvalidOperationException(
                "OPENROUTER_API_KEY is required when LLM_PROVIDER=openrouter.");
        }

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.OpenRouterBaseUrl)
        };

        var credential = new ApiKeyCredential(settings.OpenRouterApiKey);
        var chatClient = new ChatClient(settings.ModelId, credential, options);

        return chatClient.AsIChatClient();
    }
}
