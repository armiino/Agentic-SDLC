using System.Text;

namespace AgenticSdlc.Host.Steward;

/// <summary>
/// Politur 1b (18.08.2026, Autor-Auftrag „der Chat gehört wieder mir"): Steward-gestartete Läufe laufen
/// IN-PROCESS (K13) und schrieben ihre ~566 Console-Zeilen mitten ins `du>`-Prompt (Kampagnen-Kernärger,
/// UX-TODO „Konsolen-Trennung"). Diese Weiche routet stdout PER ASYNC-FLOW: nur was im
/// <see cref="Redirect"/>-Scope eines Lauf-Tasks geschrieben wird, geht in die Lauf-Protokolldatei
/// (`logs/console.log` — Beleg-Gewinn); alles andere (Chat, ⚿-Fragen, UI-Server) fließt unverändert durch.
/// `steward --debug` reicht die Lauf-Zeilen ZUSÄTZLICH durch (Präfix „·"), damit der heutige Roh-Blick
/// als bewusster Modus erhalten bleibt (Autor-Wunsch). stderr bleibt IMMER ungefiltert — Fehler sind laut.
/// Kein Workaround: EINE Naht (Install im Steward-Prozess), AsyncLocal = das offizielle .NET-Mittel für
/// fluss-lokalen Zustand; die Lauf-Runner selbst bleiben unberührt.
/// </summary>
public static class StewardRunConsole
{
    private static readonly AsyncLocal<TextWriter?> Scope = new();
    private static bool _installed;

    /// <summary>Einmalig im Steward-Prozess: Console.Out durch die Weiche ersetzen.</summary>
    public static void Install(bool debugEcho)
    {
        if (_installed) return;
        Console.SetOut(new Mux(Console.Out, debugEcho));
        _installed = true;
    }

    // Kosmetik-Feil (Abnahme 4.0): async Lebenszyklus-Zeilen (⏸/✔/✖ vom Task-Ende) landen optisch HINTER
    // dem wartenden `du>`-Prompt und sehen aus wie Autor-Eingabe. Der Chat-Loop markiert die Prompt-Wartung;
    // Lebenszyklus-Zeilen setzen sie sichtbar ab und echoen den Prompt neu (reine Darstellung).
    private static volatile bool _promptWaiting;
    public static void MarkPromptWaiting(bool waiting) => _promptWaiting = waiting;

    public static void WriteLifecycle(string line) => Console.Write(RenderLifecycle(line, _promptWaiting));

    internal static string RenderLifecycle(string line, bool promptWaiting)
        => promptWaiting
            ? Environment.NewLine + line + Environment.NewLine + "du> "
            : line + Environment.NewLine;

    /// <summary>Scope um einen Lauf-Task: alles Console-stdout dieses async-Flusses geht an <paramref name="target"/>.</summary>
    public static IDisposable Redirect(TextWriter target)
    {
        var prev = Scope.Value;
        Scope.Value = target;
        return new PopScope(prev);
    }

    private sealed class PopScope(TextWriter? prev) : IDisposable
    {
        public void Dispose() => Scope.Value = prev;
    }

    /// <summary>Die Weiche selbst — internal + Original injizierbar, damit der Wächter-Test sie OHNE globales Console-SetOut prüfen kann.</summary>
    internal sealed class Mux(TextWriter original, bool debugEcho) : TextWriter
    {
        public override Encoding Encoding => original.Encoding;

        public override void Write(char value)
        {
            var t = Scope.Value;
            if (t is null) { original.Write(value); return; }
            t.Write(value);
            if (debugEcho) original.Write(value);
        }

        public override void Write(string? value)
        {
            var t = Scope.Value;
            if (t is null) { original.Write(value); return; }
            t.Write(value);
            if (debugEcho) original.Write(value);
        }

        public override void WriteLine(string? value)
        {
            var t = Scope.Value;
            if (t is null) { original.WriteLine(value); return; }
            t.WriteLine(value);
            t.Flush();
            // UI-Weg-Stille (18.08.): der deklarierte Nutzer-Kanal des Review-Servers (URL, Browser-Warnung)
            // MUSS den Autor auch im Umleitungs-Scope erreichen — Datei behaelt die Zeile als Beleg (oben).
            if (value is not null && value.StartsWith(AgenticSdlc.HumanReview.LocalReviewServerHost.UserChannelPrefix, StringComparison.Ordinal))
            { original.WriteLine(value); return; }
            if (debugEcho) original.WriteLine("· " + value);   // Präfix: erkennbar Maschine, nicht Steward
        }

        public override void Flush() { Scope.Value?.Flush(); original.Flush(); }
    }

    /// <summary>
    /// Protokoll-Schreiber eines Laufs: puffert, bis die runId bekannt ist (start-async liefert sie per
    /// Callback), und schreibt ab <see cref="SetTarget"/> in `runs/fullworkflow/&lt;id&gt;/logs/console.log`.
    /// </summary>
    internal sealed class RunLogWriter : TextWriter
    {
        private readonly StringBuilder _buffer = new();
        private readonly object _lock = new();
        private StreamWriter? _file;
        public string? TargetPath { get; private set; }

        public override Encoding Encoding => Encoding.UTF8;

        public void SetTarget(string path)
        {
            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                _file = new StreamWriter(path, append: true) { AutoFlush = true };
                TargetPath = path;
                if (_buffer.Length > 0) { _file.Write(_buffer.ToString()); _buffer.Clear(); }
            }
        }

        /// <summary>Fehlstart ohne runId: gepufferten stdout-Vorlauf zurückgeben (nichts still verschlucken).</summary>
        internal string DrainBuffered() { lock (_lock) { var s = _buffer.ToString(); _buffer.Clear(); return s; } }

        public override void Write(char value) { lock (_lock) { if (_file is null) _buffer.Append(value); else _file.Write(value); } }
        public override void Write(string? value) { lock (_lock) { if (_file is null) _buffer.Append(value); else _file.Write(value); } }
        public override void WriteLine(string? value) { Write(value + Environment.NewLine); }

        protected override void Dispose(bool disposing)
        {
            if (disposing) lock (_lock) { _file?.Dispose(); _file = null; }
            base.Dispose(disposing);
        }
    }
}
