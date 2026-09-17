using AgenticSdlc.Host.Configuration;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// W1a-Erweiterung (06.08.): der EINE Schalter observability.captureReasoning steuert jetzt auch den
// sichtbaren Denk-Faden der TOOL-Agenten (Prompt-Appendix an der Factory-Naht) — Off = 0 Extra-Tokens
// (Mess-Baseline M-1), sonst 1-2 Sätze Text vor jedem Tool-Aufruf (log-only, response-text.md).
public sealed class ReasoningCaptureTests
{
    [Fact]
    public void ToolAgent_Appendix_folgt_dem_Schalter()
    {
        Assert.Equal("", ReasoningSchema.ToolAgentPromptAppendix(ReasoningCapture.Off));
        Assert.Contains("VOR jedem Tool-Aufruf", ReasoningSchema.ToolAgentPromptAppendix(ReasoningCapture.Enforced));
        Assert.Contains("nur Protokoll", ReasoningSchema.ToolAgentPromptAppendix(ReasoningCapture.Optional));
        Assert.StartsWith("\n\n", ReasoningSchema.ToolAgentPromptAppendix(ReasoningCapture.Enforced));   // echter Umbruch, kein Literal
    }
}
