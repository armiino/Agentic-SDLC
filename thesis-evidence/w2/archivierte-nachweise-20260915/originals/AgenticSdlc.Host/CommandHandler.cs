namespace AgenticSdlc.Host;

/// <summary>Signatur aller CLI-Kommandos (R1). Registrierung: *Commands.Register je Kettenglied.</summary>
public delegate Task<int> CommandHandler(string[] args, Configuration.HostSettings settings, string repoRoot);
