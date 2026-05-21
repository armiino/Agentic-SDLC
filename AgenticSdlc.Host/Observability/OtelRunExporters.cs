using System.Diagnostics;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AgenticSdlc.Host.Observability;

/// <summary>
/// TODO: JAEGER!!!!    spätestesn wenn mehrrere agents zsm arbeiten // docker etc..
/// Erstellt OpenTelemetry-Provider für einen einzelnen Run und exportiert deren Daten
/// in lokale JSONL-Dateien im jeweiligen Run-Ordner.
///
/// OpenTelemetry liefert dabei die standardisierte Trace- und Metric-Infrastruktur:
/// Spans, Trace-IDs, Parent/Child-Beziehungen, Tags, Events, HTTP-Instrumentierung
/// und GenAI-Instrumentierung.
///
/// Der Export in otel-traces.jsonl und otel-metrics.jsonl ist bewusst projektspezifisch.
/// Es handelt sich nicht um ein standardisiertes OTel-Dateiformat, sondern um ein
/// lokales Analyseartefakt für die Masterarbeit.
/// überlegung hier wäre evtl nach "Jäger" zu exportieren.. 
/// </summary>
public sealed class OtelRunExporters : IDisposable
{
    private readonly TracerProvider? _tracerProvider;
    private readonly MeterProvider? _meterProvider;

    /*
     * Die Provider müsssen bis zum Ende des Runs leben.
     * Beim Dispose flushen und schliessen sie die OTel-Pipeline.
     */
    private OtelRunExporters(TracerProvider? tracerProvider, MeterProvider? meterProvider)
    {
        _tracerProvider = tracerProvider;
        _meterProvider = meterProvider;
    }

    /*
     * Aktiviert OpenTelemetry nur, wenn ENABLE_OTEL=1 gesetzt wurde.
     * Dadurch bleibt OTel optional: Normale Runs funktionieren auch ohne Trace-Export.
     */
    public static OtelRunExporters? TryCreate(
        bool enabled,
        string sourceName,
        string tracesPath,
        string metricsPath,
        string? rawTracesPath = null)
    {
        if (!enabled)
            return null;

        /*
         * Die Exportdateien liegen im aktuellen Run-Ordner.
         * Dadurch bleiben Traces und Metrics direkt bei den andern Run-Artefakten.
         */
        Directory.CreateDirectory(Path.GetDirectoryName(tracesPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(metricsPath)!);
        if (!string.IsNullOrWhiteSpace(rawTracesPath))
            Directory.CreateDirectory(Path.GetDirectoryName(rawTracesPath)!);

        /*
         * Resource beschreibt die beobachtete Anwendung.
         * Diese Metadaten werden an alle exportierten Spans und Metrics gebunden.
         */
        var resource = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: sourceName,
                serviceVersion: typeof(OtelRunExporters).Assembly.GetName().Version?.ToString() ?? "unknown")
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("telemetry.export", "jsonl")
            });

        /*
         * Trace-Pipeline:
         * - registriert eigene Spans aus AgenticSdlc.Host,
         * - registriert Spans aus Microsoft.Extensions.AI und Microsoft.Extensions.Agents,
         * - erfasst HTTP-Aufrufe, z.B. zu Ollama,
         * - exportiert Batches in eine lokale JSONL-Datei.
         */
        var tracerBuilder = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resource)
            .AddSource(sourceName)
            .AddSource("Microsoft.Extensions.AI*")
            .AddSource("Microsoft.Extensions.Agents*")
            .AddHttpClientInstrumentation()
            /*
             * Batch-Export reduziert Schreibzugriffe im Vergleich zu SimpleExportProcessor.
             * Das ist für längere Runs stabiler und erzeugt weniger IO-Last.
             */
            .AddProcessor(new BatchActivityExportProcessor(new JsonlActivityExporter(tracesPath)));

        /*
         * Optionaler Raw-Export:
         * Wenn ENABLE_OTEL_RAW=1 gesetzt ist, wird ein zweiter Trace-Exporter registriert.
         * Dieser schreibt dieselben OTel-Spans ohne projektspezifische Tag-Whitelist in eine
         * eigene Datei. Dadurch bleibt der gefilterte Export lesbar, während die Raw-Datei
         * als technische OTel-Referenz für Debugging und Forschung dienen kann.
         */
        if (!string.IsNullOrWhiteSpace(rawTracesPath))
        {
            tracerBuilder.AddProcessor(new BatchActivityExportProcessor(new RawJsonlActivityExporter(rawTracesPath)));
        }

        var tracerProvider = tracerBuilder.Build();

        /*
         * Metric-Pipeline:
         * - erfasst eigene und library-seitige Metriken,
         * - erfasst HTTP-Metriken,
         * - exportiert periodisch in eine lokale JSONL-Datei.
         *
         * Für dieses Projekt sind insbesondere Token-Metriken und Laufzeit-/HTTP-Metriken
         * interessant. Fachliche Qualität von Artefakten kann hier nicht bewertet werdem
         */
        var meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resource)
            .AddMeter(sourceName)
            .AddMeter("Microsoft.Extensions.AI*")
            .AddMeter("Microsoft.Extensions.Agents*")
            .AddHttpClientInstrumentation()
            .AddReader(new PeriodicExportingMetricReader(
                new JsonlMetricExporter(metricsPath),
                exportIntervalMilliseconds: 5000))
            .Build();

        return new OtelRunExporters(tracerProvider, meterProvider);
    }

    /*
     * Dispose beendet die Provider kontrolliert.
     * Dadurch werden gepufferte Trace- und Metric-Daten vor Programmende exportiert.
     */
    public void Dispose()
    {
        _meterProvider?.Dispose();
        _tracerProvider?.Dispose();
    }

    /*
     * Raw Trace Exporter.
     *
     * Dieser Exporter ist bewusst weniger projektspezifisch als JsonlActivityExporter.
     * Er schreibt die von OpenTelemetry gelieferten Span-Daten möglichst vollständig
     * in otel-traces.raw.jsonl:
     * - alle Span-Tags,
     * - alle Event-Tags,
     * - Trace-/Span-Beziehungen,
     * - Links und Baggage.
     *
     * Wichtig: Raw bedeutet nicht "Gedanken des Modells". Auch hier werden nur technische
     * OTel-Daten exportiert, die durch Instrumentierung und eigene Spans entstanden sind.
     */
    private sealed class RawJsonlActivityExporter : BaseExporter<Activity>
    {
        private readonly string _path;
        private readonly object _lock = new();
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };

        public RawJsonlActivityExporter(string path) => _path = path;

        /*
         * Exportiert abgeschlossene Activities ohne fachliche Filterung.
         * Das ist näher an einem klassischen OTel-Debug-Export als der gefilterte
         * thesis-orientierte Export.
         */
        public override ExportResult Export(in Batch<Activity> batch)
        {
            lock (_lock)
            {
                using var fs = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var sw = new StreamWriter(fs);

                foreach (var a in batch)
                {
                    var endTimeUtc = a.StartTimeUtc + a.Duration;

                    var record = new
                    {
                        exportedAtUtc = DateTime.UtcNow,
                        traceId = a.TraceId.ToString(),
                        spanId = a.SpanId.ToString(),
                        parentSpanId = a.ParentSpanId.ToString(),
                        traceStateString = a.TraceStateString,
                        name = a.DisplayName,
                        operationName = a.OperationName,
                        kind = a.Kind.ToString(),
                        source = a.Source.Name,
                        sourceVersion = a.Source.Version,
                        startTimeUtc = a.StartTimeUtc,
                        endTimeUtc,
                        durationMs = a.Duration.TotalMilliseconds,
                        status = a.Status.ToString(),
                        statusDescription = a.StatusDescription,
                        tags = RawTagMap(a.TagObjects),
                        events = a.Events.Select(e => new
                        {
                            name = e.Name,
                            tsUtc = e.Timestamp.UtcDateTime,
                            tags = RawTagMap(e.Tags)
                        }).ToList(),
                        links = a.Links.Select(l => new
                        {
                            traceId = l.Context.TraceId.ToString(),
                            spanId = l.Context.SpanId.ToString(),
                            traceFlags = l.Context.TraceFlags.ToString(),
                            traceState = l.Context.TraceState,
                            tags = RawTagMap(l.Tags)
                        }).ToList(),
                        baggage = a.Baggage.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal)
                    };

                    sw.WriteLine(JsonSerializer.Serialize(record, _json));
                }
            }

            return ExportResult.Success;
        }

        /*
         * Wandelt OTel-Attribute ohne Whitelist in ein JSON-freundliches Dictionary.
         * Doppelte Keys werden stabil behandelt: Der letzte Wert gewinnt.
         */
        private static Dictionary<string, object?> RawTagMap(IEnumerable<KeyValuePair<string, object?>>? tags)
        {
            var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
            if (tags is null)
                return dict;

            foreach (var kv in tags)
                dict[kv.Key] = NormalizeOtelValue(kv.Value);
            return dict;
        }

        /*
         * Erhält einfache Werttypen als native JSON-Werte und wandelt komplexere Werte
         * kontrolliert um. Dadurch bleiben Raw-Logs auswertbarer als bei reinem ToString().
         */
        private static object? NormalizeOtelValue(object? value)
        {
            if (value is null)
                return null;

            if (value is string or bool or byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal)
                return value;

            if (value is DateTime dt)
                return dt.ToUniversalTime();

            if (value is DateTimeOffset dto)
                return dto.UtcDateTime;

            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                var values = new List<object?>();
                foreach (var item in enumerable)
                    values.Add(NormalizeOtelValue(item));
                return values;
            }

            return value.ToString();
        }
    }

    /*
     * Custom Trace Exporter.
     *
     * OTel stellt BaseExporter<Activity> bereit. Diese Implementierung entscheidet,
     * welche Span-Daten in welchem JSON-Format in otel-traces.jsonl geschrieben werden.
     */
    private sealed class JsonlActivityExporter : BaseExporter<Activity>
    {
        private readonly string _path;
        private readonly object _lock = new();
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };

        public JsonlActivityExporter(string path) => _path = path;

        /*
         * Export wird von der OTel-Pipeline mit einem Batch abgeschlossener Activities
         * aufgerufen. Jede Activity entspricht einem Span, z.B. Chat, HTTP oder Tool.
         */
        public override ExportResult Export(in Batch<Activity> batch)
        {
            /*
             * Mehrere Export-Aufrufe können parallel auftreten.
             * Der Lock verhindert vermischte Schreibzugriffe auf dieselbe JSONL-Datei.
             */
            lock (_lock)
            {
                using var fs = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var sw = new StreamWriter(fs);

                foreach (var a in batch)
                {
                    var endTimeUtc = a.StartTimeUtc + a.Duration;

                    /*
                     * Reduziert den OTel-Span auf ein lesbares Run-Artefakt.
                     * TraceId, SpanId und ParentSpanId bleiben erhalten, damit der Ablaufbaum
                     * später rekonstruiert werden kann.
                     */
                    var record = new
                    {
                        exportedAtUtc = DateTime.UtcNow,
                        traceId = a.TraceId.ToString(),
                        spanId = a.SpanId.ToString(),
                        parentSpanId = a.ParentSpanId.ToString(),
                        name = a.DisplayName,
                        kind = a.Kind.ToString(),
                        startTimeUtc = a.StartTimeUtc,
                        endTimeUtc,
                        durationMs = a.Duration.TotalMilliseconds,
                        status = a.Status.ToString(),
                        statusDescription = a.StatusDescription,
                        tags = SafeTagMap(FilterTags(a.TagObjects)),
                        events = a.Events?.Select(e => new
                        {
                            name = e.Name,
                            tsUtc = e.Timestamp.UtcDateTime,
                            tags = SafeTagMap(FilterTags(e.Tags))
                        }).ToList()
                    };

                    sw.WriteLine(JsonSerializer.Serialize(record, _json));
                }
            }

            return ExportResult.Success;
        }

        private static IEnumerable<KeyValuePair<string, object?>> FilterTags(IEnumerable<KeyValuePair<string, object?>>? tags)
        {
            if (tags is null) yield break;

            foreach (var kv in tags)
            {
                /*
                 * Whitelist:
                 * Nur fachlich oder technisch relevante Tags werden exportiert.
                 * Das verhindert, dass neü Libraries die JSONL-Dateien mit irrelevanten
                 * oder sehr grossen Attributen überfrachten.
                 */
                if (IsInterestingKey(kv.Key))
                    yield return kv;
            }
        }

        private static bool IsInterestingKey(string key)
        {
            /*
             * Relevante Tag-Gruppen:
             * - run/phase/agent/turn für die fachliche Einordnung,
             * - gen_ai/llm für Modell- und Chatdaten,
             * - tool/fs für Tool- und Dateisystemoperationen,
             * - http/server/url für Provider-Kommunikation,
             * - error/exception/otel.status für Fehleranalyse.
             */
            return key.StartsWith("run.", StringComparison.Ordinal) ||
                   key.StartsWith("phase", StringComparison.Ordinal) ||
                   key.StartsWith("agent.", StringComparison.Ordinal) ||
                   key.StartsWith("turn.", StringComparison.Ordinal) ||
                   key.StartsWith("gen_ai.", StringComparison.Ordinal) ||
                   key.StartsWith("llm.", StringComparison.Ordinal) ||
                   key.StartsWith("tool.", StringComparison.Ordinal) ||
                   key.StartsWith("fs.", StringComparison.Ordinal) ||
                   key.StartsWith("http.", StringComparison.Ordinal) ||
                   key.StartsWith("server.", StringComparison.Ordinal) ||
                   key.StartsWith("url.", StringComparison.Ordinal) ||
                   key.StartsWith("error.", StringComparison.Ordinal) ||
                   key.StartsWith("exception.", StringComparison.Ordinal) ||
                   key.Equals("otel.status_code", StringComparison.Ordinal) ||
                   key.Equals("otel.status_description", StringComparison.Ordinal);
        }

        private static Dictionary<string, string?> SafeTagMap(IEnumerable<KeyValuePair<string, object?>> tags)
        {
            var dict = new Dictionary<string, string?>(StringComparer.Ordinal);

            foreach (var kv in tags)
            {
                /*
                 * Falls ein Tag mehrfach vorkommt, gewinnt der letzte Wert.
                 * Das ist für den Export robuster als ein Fehler durch doppelte Keys.
                 */
                dict[kv.Key] = kv.Value?.ToString();
            }

            return dict;
        }
    }

    /*
     * Custom Metric Exporter.
     *
     * OTel stellt BaseExporter<Metric> bereit. Diese Implementierung schreibt ausgewählte
     * Metric-Punkte als einfache JSONL-Datensätze in otel-metrics.jsonl.
     */
    private sealed class JsonlMetricExporter : BaseExporter<Metric>
    {
        private readonly string _path;
        private readonly object _lock = new();
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };

        public JsonlMetricExporter(string path) => _path = path;

        /*
         * Exportiert alle Metric-Punkte aus dem übergebenen Batch.
         * Eine Metric kann mehrere Punkte enthalten, z.B. je Tag-Kombination.
         */
        public override ExportResult Export(in Batch<Metric> batch)
        {
            /*
             * Schreibt synchronisiert in die JSONL-Datei, damit parallele Exporte
             * keine ungültigen oder vermischten Zeilen erzeugen.
             */
            lock (_lock)
            {
                using var fs = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var sw = new StreamWriter(fs);

                foreach (var metric in batch)
                {
                    foreach (ref readonly var point in metric.GetMetricPoints())
                    {
                        /*
                         * Bewusst kompaktes Metric-Format:
                         * Name, Typ, Tags und ein vereinfachter Wert reichen für die lokale
                         * Run-Analyse. Vollständige OTel-Metric-Semantik wird hier nicht
                         * abgebildet.
                         */
                        var record = new
                        {
                            exportedAtUtc = DateTime.UtcNow,
                            name = metric.Name,
                            type = metric.MetricType.ToString(),
                            tags = TagsToStringDictionary(point.Tags),
                            value = TryReadValue(metric.MetricType, point)
                        };

                        sw.WriteLine(JsonSerializer.Serialize(record, _json));
                    }
                }
            }

            return ExportResult.Success;
        }

        /*
         * Liest je nach Metric-Typ den passenden Wert aus dem MetricPoint.
         * Histogramme werden auf Count und Summe reduziert.
         */
        private static object? TryReadValue(MetricType type, in MetricPoint p) =>
            type switch
            {
                MetricType.LongSum => p.GetSumLong(),
                MetricType.DoubleSum => p.GetSumDouble(),
                MetricType.LongGauge => p.GetGaugeLastValueLong(),
                MetricType.DoubleGauge => p.GetGaugeLastValueDouble(),
                MetricType.Histogram => new { count = p.GetHistogramCount(), sum = p.GetHistogramSum() },
                _ => null
            };

        /*
         * Wandelt Metric-Tags in ein einfach serialisierbares Dictionary um.
         */
        private static Dictionary<string, string?> TagsToStringDictionary(ReadOnlyTagCollection tags)
        {
            var dict = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var kv in tags)
                dict[kv.Key] = kv.Value?.ToString();
            return dict;
        }
    }
}
