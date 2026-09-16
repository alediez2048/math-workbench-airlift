# Fraction lesson requirements and trace

## September 16 immediate-demo scope

Owner deferred live AI/spoken guidance and associated audio controls. Existing F-*
requirements remain traceable future scope, not acceptance gates for the immediate
whole/halves demo. Physical grabbing, stable placement, exact fractions and readable
labels remain essential. See [handoff](HANDOFF-2026-09-16.md) for current failures.

Draft F-* namespace belongs to this revision. Existing root FR/NFR IDs are retained
history, not silently renumbered.

| ID | Acceptance requirement | Tickets | Named test / evidence |
|---|---|---|---|
| F-01 | Cargo only enabled; mission before action; return and replay | P1-02, P1-03, P1-10 | CatalogGateTests, GuideCueTests, ChapterOneAcceptance |
| F-02 | Reachable stationary setup; grip/ray tutorial; one-controller equivalent path | P1-02, P1-03, P2-03, P2-07 | ComfortPlacementTests, ChordInputTests, MergeTransactionTests; device both hands |
| F-03 | Core state-aware AI voice, matching captions/pointers, mute/replay/offline fallback; no core mic | P1-03, P1-09, P1-10, P2-08 | CoreAIGuideTests, GuideServiceContractTests, GuideCueTests, LiveGuideCoversImplementedStages; real adult live proof distinct from fallback |
| F-04 | Fixed whole, exact quantity, numeral labels, same endpoints | P1-04 | FractionInvariantTests, WholeReferenceTests |
| F-05 | Actual equal split to 2 halves; chord shortcut plus fallback | P1-05, P2-07 | SplitTransactionTests, ChordInputTests |
| F-06 | Recompose 2 halves in guided locations; expression equals whole | P1-06 | RecomposeWholeTests |
| F-07 | Four equal quarters; labels and count/size distinction | P2-01, P2-02, P1-08 | QuarterConservationTests, ThreeQuarterTaskTests, FractionMeaningTests |
| F-08 | Bring two pieces together to merge, released guard, reversible lineage | P2-03 | MergeTransactionTests |
| F-09 | Bigger/smaller through split/merge and synchronized zoom, no false labels | P2-07 | ZoomInvariantTests; owner design approval |
| F-10 | Generate equivalents and compare on equal wholes with notation/reason | P2-04, P2-05 | EquivalentConstructionTests, ComparisonReasonTests |
| F-11 | Independent attempt and fresh values; wrong valid work preserved | P1-07, P2-06 | IndependentHalfTests, TransferAttemptTests |
| F-12 | Visual/tactile cues; no answer leaks or mandatory sound/color | P1-08, P3-03 | FeedbackParityTests, AccessibleRecoveryTests |
| F-13 | Reset, replay, Back, tracking loss and double callbacks conserve state | P1-09, P3-03 | RecoverySnapshotTests, AccessibleRecoveryTests |
| F-14 | Real cargo purpose, transitions and honest ending | P2-08, P3-02 | ChapterFlowTests, DispatchSummaryTests |
| F-15 | Exact scene build, no embedded secrets, artifact lineage, physical QA | P1-01, P1-10, P3-05, P3-06 | CargoBaselineTests, ReleaseSafetyTests, ReleaseManifestTests |
| F-16 | 72 Hz target, CPU/GPU p95 each ≤13.9 ms, no progressive replay leak | P3-04 | PerformanceCaptureTests + actual Quest three-run report |
| F-17 | Local phased tickets, one active at a time, Why mirrored into devlog | all; P1-01 | owner checkpoint + documentation validation |
| F-18 | OpenAI candidate/access/privacy/output gate for core guide; optional spoken questions and child release separately cleared | P1-03 early gates; P4-01 documentary review; P4-02–04 conversation/child readiness | CoreAIGuideTests, VoiceEligibilityGateTests, VoiceSessionContractTests, LiveGuideBoundaryTests, LiveGuideFailureTests |
| F-19 | Adaptive routing enhancement, closed scaffold selection and no AI grading; core AI remains required | P3-01 | ScaffoldRouterTests, synthetic golden-set report |
| F-20 | Deliberate typography system; owner-selected font direction; readable fraction glyphs, captions and buttons on Quest | P1-02, P1-10, P3-03 | TypographyAssetTests, FractionGlyphCoverageTests, TextLayoutTests; owner visual/headset review |

| F-21 | Cargo terminal visible on entry; truck, containers, loading platform and destinations establish purpose; parcel progress reflects accepted jobs | P1-02, P1-03, P1-07, P1-10, P2-02, P2-06, P2-08, P3-02 | CargoTerminalLayoutTests, ParcelJobPresentationTests, DispatchSummaryTests; owner catalog-to-dispatch review |

P*-* cells abbreviate CC-P*-*. Every ticket includes full IDs. Automated test names
are proposed suites, not existing passing tests (except existing OnboardingFlowTests).

## Standards alignment — limited claims

Grade 3 fraction foundations and Grade 4 equivalence/comparison are the intended
alignment: [3.NF](https://www.thecorestandards.org/Math/Content/3/NF/),
[4.NF.A.1–2](https://www.thecorestandards.org/Math/Content/4/NF/).
This sequence covers powers-of-two examples only. It does not claim complete
grade coverage, validated efficacy, retention or mastery.
Use equal sharing and number lines as recommended by
[IES fraction guidance](https://ies.ed.gov/ncee/wwc/docs/practiceguide/fractions_pg_093010.pdf).
Multimodal access follows [CAST UDL](https://udlguidelines.cast.org/static/udlg2.2-text-a11y.pdf);
that is design guidance, not a certification of this headset experience.
