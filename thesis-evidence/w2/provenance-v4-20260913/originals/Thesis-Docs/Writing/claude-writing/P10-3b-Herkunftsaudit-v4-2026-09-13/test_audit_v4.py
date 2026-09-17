import json, tempfile, unittest
from pathlib import Path
from audit_v4 import Audit, quote_result

A='20200101_000000_aaaaaa'; B='20200101_000000_bbbbbb'
TEXT='Die Übersicht soll die angekündigten Besuche der nächsten vierzehn Tage anzeigen.'

class AuditControls(unittest.TestCase):
    def setUp(self):
        self.tmp=tempfile.TemporaryDirectory();self.root=Path(self.tmp.name)
        self.put(f'runs/example/{A}/config.json',{'runId':A})
        self.put(f'runs/example/{B}/config.json',{'runId':B})
    def tearDown(self):self.tmp.cleanup()
    def put(self,path,doc):
        p=self.root/path;p.parent.mkdir(parents=True,exist_ok=True)
        p.write_text(doc if isinstance(doc,str) else json.dumps(doc));return p
    def claim(self,rid=A,ident='claim-x',text=TEXT,file='consumable.json'):
        self.put(f'runs/example/{rid}/{file}',{'claims':[{'id':ident,'proposition':text}]})
    def item(self,**kw):
        return {'itemId':'REQ-1','itemType':'requirement','text':TEXT,'version':1,'sourceRunId':A,'sourceClaimIds':['claim-x'],**kw}
    def evaluate(self,it,more=None,rels=None):
        return Audit(self.root).all({'items':[it]+(more or []),'relations':rels or []},{})['items'][0]
    def ingest(self,apply=True,input_id='AF-1',target='REQ-1',statement=TEXT):
        delta={'items':[{'itemId':'AF-1','itemType':'requirement','text':TEXT,'sourceArtifactType':'author-statement','origin':'AuthorFront'}]}
        self.put('input/delta.json',delta)
        self.put(f'runs/example/{A}/config.json',{'runId':A,'fromDelta':'input/delta.json'})
        self.put(f'runs/example/{A}/07-ingest/plan.json',{'sourceMeetingDeltaPath':'input/delta.json','operations':[
            {'incomingItemId':input_id,'kind':'REFINE','targetEntityId':target,'statement':statement}]})
        if apply:self.put(f'runs/example/{A}/07-ingest/applied/delta.json',{'applied':[{'incomingItemId':input_id,'entityId':target,'kind':'REFINE','outcome':'refined'}]})
    def test_01_unconnected_equal_id_rejected(self):
        self.claim(B);self.assertEqual(self.evaluate(self.item())['references'][0]['status'],'unresolved')
    def test_02_explicit_source_file_resolves(self):
        self.claim(B);self.put(f'runs/example/{A}/config.json',{'consumable':f'runs/example/{B}/consumable.json'})
        self.assertEqual(self.evaluate(self.item())['references'][0]['status'],'resolved')
    def test_03_different_same_level_definitions_are_ambiguous(self):
        self.claim(file='a/consumable.json');self.claim(text='Ein anderer Inhalt.',file='b/consumable.json')
        self.assertEqual(self.evaluate(self.item())['references'][0]['status'],'ambiguous')
    def test_04_missing_additional_reference_stays_visible(self):
        self.claim();r=self.evaluate(self.item(sourceClaimIds=['claim-x','missing']))
        self.assertTrue(r['directDefinition']);self.assertEqual(r['referenceCounts'],{'resolved':1,'unresolved':1})
    def test_05_wrong_speaker_is_not_full_match(self):
        self.assertEqual(quote_result('Erfunden: '+TEXT,'Pflege: '+TEXT,'t.txt')['status'],'body_only_prefix_differs')
    def test_06_changed_quote_end_fails(self):
        self.assertEqual(quote_result(TEXT+' Verändertes Ende.',TEXT,'t.txt')['status'],'no_match')
    def test_07_short_quote_and_no_bound_transcript(self):
        self.assertEqual(quote_result('Ja.',TEXT,'t.txt')['status'],'short')
        self.assertEqual(quote_result(TEXT,None,None)['status'],'no_bound_transcript')
    def test_08_full_quote_matches(self):
        self.assertEqual(quote_result('Pflege: '+TEXT,'Pflege: '+TEXT,'t.txt')['status'],'full_match')
    def test_09_wrong_relation_direction_does_not_resolve(self):
        self.claim();pbi=self.item(itemId='PBI-1',itemType='pbi',sourceClaimIds=[])
        r=self.evaluate(pbi,[self.item()],[{'fromId':'REQ-1','toId':'PBI-1','relationType':'covers'}])
        self.assertFalse(r['anyDefinitionOrContribution'])
    def test_10_relation_fallback_does_not_hide_missing_direct_ref(self):
        self.claim();pbi=self.item(itemId='PBI-1',itemType='pbi',sourceClaimIds=['missing'])
        r=self.evaluate(pbi,[self.item()],[{'fromId':'PBI-1','toId':'REQ-1','relationType':'covers'}])
        self.assertTrue(r['anyDefinitionOrContribution']);self.assertEqual(r['references'][0]['status'],'unresolved')
    def test_11_author_apply_input_links_change(self):
        self.ingest();r=self.evaluate(self.item(sourceClaimIds=[],version=2))
        self.assertTrue(r['inputDefinition']);self.assertEqual(r['latestChange']['status'],'linked_change_matching_text')
        self.assertEqual(r['identifiedInputKinds'],['author'])
    def test_12_wrong_input_id_is_not_a_source(self):
        self.ingest(input_id='AF-99');r=self.evaluate(self.item(sourceClaimIds=[],version=2))
        self.assertFalse(r['inputDefinition']);self.assertEqual(r['latestChange']['status'],'no_linked_change')
    def test_13_missing_apply_is_not_proof_of_change(self):
        self.ingest(apply=False);r=self.evaluate(self.item(sourceClaimIds=[],version=2))
        self.assertFalse(r['inputDefinition']);self.assertEqual(r['latestChange']['status'],'no_linked_change')
    def test_14_old_origin_does_not_prove_new_content(self):
        self.claim();r=self.evaluate(self.item(version=2,text='Eine neue fachliche Anforderung.'))
        self.assertTrue(r['directDefinition']);self.assertEqual(r['latestChange']['status'],'no_linked_change')
    def test_15_history_note_can_link_change_run(self):
        self.ingest();it=self.item(sourceRunId=B,sourceClaimIds=[],version=2,history=[{'versionId':1,'note':f'REFINE (ingestion {A}, via AF-1)'}])
        self.assertEqual(self.evaluate(it)['latestChange']['status'],'linked_change_matching_text')
    def test_16_snapshot_is_not_definition(self):
        self.claim(file='core-before.json');self.assertEqual(self.evaluate(self.item())['references'][0]['status'],'unresolved')
    def test_17_unit_reference_missing_remains_missing(self):
        self.put(f'runs/example/{A}/consumable.json',{'claims':[{'id':'claim-x','proposition':TEXT,'sourceUnitIds':['AU-good','AU-missing']}]})
        self.put(f'runs/example/{A}/step-00-atomic-units/output.json',{'units':[{'id':'AU-good','text':TEXT}]})
        checks=self.evaluate(self.item())['references'][0]['claimChecks']
        self.assertEqual([x['status'] for x in checks['units']],['resolved','unresolved'])
        self.assertEqual(checks['quoteStatus'],'no_quote')
    def test_18_identical_copies_not_ambiguous(self):
        self.claim(file='a/consumable.json');self.claim(file='b/consumable.json')
        r=self.evaluate(self.item())['references'][0];self.assertEqual(r['status'],'resolved');self.assertEqual(len(r['definitions']),2)
    def test_19_two_run_directories_are_ambiguous(self):
        self.claim();self.put(f'runsArchive/example/{A}/config.json',{'runId':A})
        self.assertEqual(self.evaluate(self.item())['references'][0]['status'],'unresolved')
    def test_20_prepared_core_delta_requires_explicit_input_edge(self):
        self.put(f'runs/example/{A}/04-delta/project-state.json',{'items':[{'itemId':'claim-x','text':TEXT}]})
        self.assertEqual(self.evaluate(self.item())['references'][0]['status'],'unresolved')
    def test_21_change_with_other_statement_is_not_current_text_match(self):
        self.ingest(statement='Anderer Text.');r=self.evaluate(self.item(sourceClaimIds=[],version=2))
        self.assertEqual(r['latestChange']['status'],'linked_change_without_text_match')
    def test_22_declared_ledger_wins_over_working_candidate(self):
        self.claim();self.claim(text='Frühere Kandidatenfassung.',file='step-01/output.json')
        self.assertEqual(self.evaluate(self.item())['references'][0]['definition']['object']['proposition'],TEXT)
    def test_23_long_manifest_prose_is_not_a_filesystem_path(self):
        self.claim();self.put(f'runs/example/{A}/config.json',{'baseline':{'description':TEXT*10}})
        self.assertEqual(self.evaluate(self.item())['references'][0]['status'],'resolved')
    def test_24_candidate_id_in_decision_is_not_candidate_definition(self):
        self.put(f'runs/example/{A}/human-review-package.json',{'items':[{'candidateId':'C-1','text':TEXT,'rationale':'Begründung.'}]})
        self.put(f'runs/example/{A}/human-decisions.json',{'decisions':[{'candidateId':'C-1','decision':'accept'}]})
        r=self.evaluate(self.item(sourceClaimIds=[],sourceCandidateId='C-1'))
        self.assertEqual(r['references'][0]['status'],'resolved')
    def test_25_named_run_and_explicit_metadata_input(self):
        self.put('runs/project-state/meeting-mini/meeting-delta.json',{'items':[{'itemId':'IN-1','text':TEXT}]})
        r=self.evaluate(self.item(sourceRunId='meeting-mini',sourceClaimIds=[],metadata={'ingestedFrom':'IN-1'}))
        self.assertTrue(r['inputDefinition']);self.assertEqual(r['inputLinks'][0]['evidenceLevel'],'metadata_input_only')
    def test_26_core_decision_target_is_not_external_claim(self):
        self.claim();dec=self.item(itemId='DEC-1',itemType='decision')
        r=self.evaluate(self.item(sourceClaimIds=[],sourceDecisionId='DEC-1'),[dec])
        self.assertFalse(r['directDefinition']);self.assertTrue(r['anyDefinitionOrContribution']);self.assertEqual(r['references'][0]['status'],'resolved_core')
    def test_27_applied_backlog_is_exact_producer_role(self):
        self.put(f'runs/example/{A}/backlog/applied/product-backlog.json',{'items':[{'pbiId':'OLD-1','title':TEXT}]})
        self.put(f'runs/example/{A}/backlog/human-decisions.json',{'decisions':[{'pbiId':'OLD-1','rationale':'Freigegeben.'}]})
        r=self.evaluate(self.item(itemType='pbi',sourceClaimIds=[],metadata={'legacyPbiId':'OLD-1','sourceRunId':A}))
        self.assertTrue(r['producerDefinition'])
    def test_28_exact_input_resolves_ambiguity_in_broader_run(self):
        self.ingest()
        self.put('input/delta-other.json',{'items':[{'itemId':'AF-1','text':'Andere Fassung.'}]})
        self.put(f'runs/example/{A}/other.json',{'items':[{'itemId':'AF-1','text':'Andere Fassung.'}]})
        r=self.evaluate(self.item(sourceClaimIds=['AF-1'],version=2))
        self.assertEqual(r['references'][0]['status'],'resolved_input')
        self.assertEqual(r['references'][0]['scopeResolution'],'exact_plan_apply_input')
    def test_29_old_edge_context_does_not_replace_current_reference(self):
        self.claim(B,ident='old-claim')
        it=self.item(sourceClaimIds=['new-missing'],version=2)
        c={'items':[it],'relations':[{'fromId':'REQ-1','toId':'old-claim','relationType':'evidenced_by_ledger_claim'}],
           'provenance':[{'itemId':'REQ-1','links':[{'targetId':B,'targetType':'run','relation':'produced_by'}]}]}
        r=Audit(self.root).all(c,{})['items'][0]
        self.assertEqual(r['references'][0]['status'],'unresolved')
        self.assertEqual(r['ledgerEdgeChecks'][0]['reference']['status'],'resolved')
        self.assertFalse(r['anyDefinitionOrContribution'])
    def test_30_duplicate_input_id_in_one_file_is_ambiguous(self):
        self.ingest();self.put('input/delta.json',{'items':[{'itemId':'AF-1','text':TEXT},{'itemId':'AF-1','text':'Andere Fassung.'}]})
        r=self.evaluate(self.item(sourceClaimIds=[],version=2))
        self.assertFalse(r['inputDefinition'])
    def test_31_equal_claim_text_different_units_is_ambiguous(self):
        for suffix,uid in [('a','AU-1'),('b','AU-2')]:
            self.put(f'runs/example/{A}/{suffix}/consumable.json',{'claims':[{'id':'claim-x','proposition':TEXT,'sourceUnitIds':[uid]}]})
        self.assertEqual(self.evaluate(self.item())['references'][0]['status'],'ambiguous')
    def test_32_own_output_is_not_an_external_source_path(self):
        self.put(f'runs/example/{A}/baselines/requirements/artifact.json',{'items':[{'itemId':'REQ-1','text':TEXT}]})
        r=self.evaluate(self.item(sourceClaimIds=[],sourceArtifactType='requirements',sourceArtifactId='REQ'))
        self.assertTrue(r['producerDefinition']);self.assertFalse(r['referenceOrInputPath'])
    def test_33_supersede_apply_maps_new_id_while_plan_targets_old(self):
        self.ingest();base=f'runs/example/{A}/07-ingest/'
        self.put(base+'plan.json',{'sourceMeetingDeltaPath':'input/delta.json','operations':[{'incomingItemId':'AF-1','kind':'SUPERSEDE','targetEntityId':'REQ-old','statement':TEXT}]})
        self.put(base+'applied/delta.json',{'applied':[{'incomingItemId':'AF-1','kind':'SUPERSEDE','entityId':'REQ-1','outcome':'superseded'}]})
        self.assertTrue(self.evaluate(self.item(sourceClaimIds=[]))['inputDefinition'])
    def test_34_refine_target_must_match_apply_target(self):
        self.ingest(target='REQ-other');self.put(f'runs/example/{A}/07-ingest/applied/delta.json',{'applied':[{'incomingItemId':'AF-1','kind':'REFINE','entityId':'REQ-1','outcome':'refined'}]})
        self.assertFalse(self.evaluate(self.item(sourceClaimIds=[]))['inputDefinition'])

if __name__=='__main__':unittest.main(verbosity=2)
