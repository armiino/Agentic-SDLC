#!/usr/bin/env python3
"""Recompute the four W2 principal runs from archived usage; standard library only.

Run from any directory: python3 thesis-evidence/w2/recompute-token-usage.py
The default writes JSON to stdout. --output explicitly writes a result file.
No model calls, new semantic judgments or modification of archived source files.
"""
import argparse
from collections import Counter, defaultdict
import hashlib
import json
from pathlib import Path


def recompute(repo):
    hashes = {}

    def read(relative, jsonl=False):
        raw = (repo / relative).read_bytes()
        hashes[relative.as_posix()] = hashlib.sha256(raw).hexdigest()
        return [json.loads(row) for row in raw.splitlines() if row.strip()] if jsonl else json.loads(raw)

    result = []
    for group, case, rid, previous in [
        ('ledger', 'stress', '20260905_164650_8189ea', [182184, 41133]),
        ('ledger', 'meeting', '20260905_164206_e30646', [26731, 13524]),
        ('arm-f', 'stress', '20260906_143052_6540e2', [105166, 19555]),
        ('arm-f', 'meeting', '20260906_141518_12d046', [23040, 5708]),
    ]:
        base = Path('runs') / group / rid
        metric_path = base / 'logs/otel-metrics.jsonl'
        trace_path = base / 'logs/otel-traces.jsonl'
        metrics = read(metric_path, True)
        traces = read(trace_path, True)
        streams = defaultdict(list)
        for line, item in enumerate(metrics, 1):
            if item['name'] == 'gen_ai.client.token.usage':
                streams[json.dumps(item['tags'], sort_keys=True)].append((line, item))
        if not streams:
            raise ValueError(f'No usage metrics: {rid}')
        totals = Counter()
        details = []
        for key, stream in sorted(streams.items()):
            for (_, a), (_, b) in zip(stream, stream[1:]):
                if b['value']['count'] < a['value']['count'] or b['value']['sum'] < a['value']['sum']:
                    raise ValueError(f'Counter reset/nonmonotonic stream: {rid} {key}')
            line, final = stream[-1]
            token_type = final['tags']['gen_ai.token.type']
            totals[token_type] += final['value']['sum']
            details.append({'tags': final['tags'], 'periodic_exports': len(stream),
                            'last_line': line, 'last_value': final['value']})
        # Keep only individual chat calls, excluding the enclosing orchestrate_tools total.
        calls = [(line, t) for line, t in enumerate(traces, 1)
                 if t.get('tags', {}).get('gen_ai.operation.name') == 'chat'
                 and 'gen_ai.usage.input_tokens' in t['tags']]
        if not calls or len(calls) != len({(t['traceId'], t['spanId']) for _, t in calls}):
            raise ValueError(f'Absent/duplicate chat spans: {rid}')
        span_totals = {kind: sum(int(t['tags'].get(f'gen_ai.usage.{kind}_tokens', 0))
                                for _, t in calls) for kind in ['input', 'output']}
        if dict(totals) != span_totals:
            raise ValueError(f'Metrics and individual chat spans disagree: {rid}')
        for kind in ['input', 'output']:
            count = sum(d['last_value']['count'] for d in details
                        if d['tags']['gen_ai.token.type'] == kind)
            if count != len(calls):
                raise ValueError(f'Metric call count mismatch: {rid} {kind}')
        cached = sum(int(t['tags'].get('gen_ai.usage.cache_read.input_tokens', 0)) for _, t in calls)
        if group == 'arm-f':
            submitted = read(base / 'metrics.json')
            if [submitted['inputTokens'], submitted['outputTokens']] != [totals['input'], totals['output']]:
                raise ValueError(f'F metrics.json differs from trace/metric totals: {rid}')
        result.append({'run_id': rid, 'configuration': group, 'case': case,
                       'input_tokens': totals['input'], 'output_tokens': totals['output'],
                       'cache_read_input_tokens_included_in_input': cached,
                       'chat_calls': len(calls), 'before_correction_input_output': previous,
                       'metric_path': metric_path.as_posix(), 'trace_path': trace_path.as_posix(),
                       'metric_streams': details,
                       'chat_spans': [{'line': line, 'trace_id': t['traceId'], 'span_id': t['spanId'],
                                      'usage': {k:v for k,v in t['tags'].items() if k.startswith('gen_ai.usage.')}}
                                     for line, t in calls]})
    ledger = next(r for r in result if r['configuration'] == 'ledger' and r['case'] == 'stress')
    agent = next(r for r in result if r['configuration'] == 'arm-f' and r['case'] == 'stress')
    old_path = Path('thesis-evidence/w2/eval/w2-goldv2-eval-ledger-8189ea.json')
    old = read(old_path)['kosten']
    return {
        'schema_version': 1,
        'correction_date': '2026-09-10',
        'reviewed_pdf_sha256': '60a4aefe492d9d5df865500672257f4da4f80fb2c9b6db9bdb0d07d32865a6e9',
        'scope': 'Archived usage of four principal runs. No new system run, semantic annotation, invoice reconciliation or proof of instrumentation completeness.',
        'aggregation': 'Last cumulative token-usage value per observed tag stream, checked against distinct chat spans. Do not sum periodic cumulative exports, cache subcounts or parent orchestration totals.',
        'runs': result,
        'ledger_stress_correction': {
            'historical_result_file': old_path.as_posix(),
            'historical_input_tokens': old['inputTokens'],
            'corrected_input_tokens': ledger['input_tokens'],
            'cached_input_subset': ledger['cache_read_input_tokens_included_in_input'],
            'old_equals_total_plus_cache_subset': old['inputTokens'] == ledger['input_tokens'] + ledger['cache_read_input_tokens_included_in_input'],
            'interpretation': 'Exact equality supports a cache double-count explanation; the historical aggregation step itself was not reconstructed.'},
        'stress_ratios': {
            'ledger_over_f_input': ledger['input_tokens'] / agent['input_tokens'],
            'f_over_ledger_input': agent['input_tokens'] / ledger['input_tokens'],
            'ledger_over_f_output': ledger['output_tokens'] / agent['output_tokens'],
            'f_over_ledger_output': agent['output_tokens'] / ledger['output_tokens']},
        'source_sha256': dict(sorted(hashes.items()))}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repo', type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    text = json.dumps(recompute(args.repo), ensure_ascii=False, indent=2) + '\n'
    if args.output:
        args.output.write_text(text)
    else:
        print(text, end='')


if __name__ == '__main__':
    main()
