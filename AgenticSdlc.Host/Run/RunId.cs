namespace AgenticSdlc.Host.Run;

//RunId generieren 
public static class RunId
{
    public static string New()
    {
        var ts = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var rnd = Guid.NewGuid().ToString("N")[..6];
        return $"{ts}_{rnd}";
    }
}