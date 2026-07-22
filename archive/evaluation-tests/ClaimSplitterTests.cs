using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Claim-Pilot Stufe 3: deterministische Parse-Logik des ClaimSplitters (atomare Claims parsen,
/// leere verwerfen, Code-Fence-robust, Bad-JSON → leer → Runner-Fallback auf die Unit selbst).
/// Der eigentliche Split ist ein LLM-Run.
/// </summary>
public sealed class ClaimSplitterTests
{
    [Fact]
    public void ParseClaims_Reads_And_Trims()
    {
        var claims = ClaimSplitter.ParseClaims("{\"claims\":[\"  Kunden sehen Bestellungen.  \",\"Kunden laden Rechnungen herunter.\"]}");
        Assert.Equal(2, claims.Count);
        Assert.Equal("Kunden sehen Bestellungen.", claims[0]);
    }

    [Fact]
    public void ParseClaims_Drops_Empty()
    {
        var claims = ClaimSplitter.ParseClaims("{\"claims\":[\"A\",\"  \",\"\"]}");
        Assert.Single(claims);
        Assert.Equal("A", claims[0]);
    }

    [Fact]
    public void ParseClaims_Bad_Json_Returns_Empty()
    {
        Assert.Empty(ClaimSplitter.ParseClaims("kein json"));
        Assert.Empty(ClaimSplitter.ParseClaims("{\"foo\":1}"));
    }

    [Fact]
    public void ParseClaims_Extracts_From_Code_Fence()
    {
        var claims = ClaimSplitter.ParseClaims("```json\n{\"claims\":[\"Login per E-Mail.\"]}\n```");
        Assert.Single(claims);
        Assert.Equal("Login per E-Mail.", claims[0]);
    }

    [Fact]
    public void ParseAtomicClaims_Reads_Normalized_Claim_And_Facets()
    {
        var text = """
            {
              "atomicClaims": [
                {
                  "artifactQuote": "Das Kundenportal-Frontend zeigt Bestellungen an.",
                  "normalizedClaim": "Kunden koennen Bestellungen im Kundenportal sehen.",
                  "facets": {
                    "component": "Kundenportal-Frontend",
                    "actor": "Kunden",
                    "action": "sehen",
                    "object": "Bestellungen",
                    "modality": null,
                    "status": null,
                    "scope": "Kundenportal"
                  },
                  "splitReason": "Objekt aus Aufzaehlung isoliert, Oberflaeche erhalten."
                }
              ]
            }
            """;

        var claims = ClaimSplitter.ParseAtomicClaims(text);

        Assert.Single(claims);
        Assert.Equal("Kunden koennen Bestellungen im Kundenportal sehen.", claims[0].NormalizedClaim);
        Assert.Equal("Kundenportal-Frontend", claims[0].Facets.Component);
        Assert.Equal("Bestellungen", claims[0].Facets.Object);
    }

    [Fact]
    public void ParseClaims_Uses_Normalized_Claim_For_New_Format()
    {
        var claims = ClaimSplitter.ParseClaims("""
            {
              "atomicClaims": [
                {
                  "artifactQuote": "Frontend zeigt Rechnungen an.",
                  "normalizedClaim": "Kunden koennen Rechnungen im Kundenportal abrufen.",
                  "facets": {
                    "component": "Frontend",
                    "actor": "Kunden",
                    "action": "abrufen",
                    "object": "Rechnungen",
                    "modality": null,
                    "status": null,
                    "scope": null
                  },
                  "splitReason": "Download/Anzeige normalisiert, UI-Facette erhalten."
                }
              ]
            }
            """);

        Assert.Single(claims);
        Assert.Equal("Kunden koennen Rechnungen im Kundenportal abrufen.", claims[0]);
    }
}
