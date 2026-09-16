# Reachable fraction-task fixtures

Proposed fixtures created/tested in CC-P1-04, extended by their activity tickets.
Every inventory has root R[0..8), halves H0/H1 of width4, quarters Q0..Q3 width2,
eighths E0..E7 width1. IDs are prefixed by WholeId. Children are stable and inactive
until split; active leaves/merged parents cover eight source cells exactly once.
Lane positions are separate from these immutable source intervals.

| Stage | Initial inventory / lane | Legal successful action trace |
|---|---|---|
| Whole | W1 root in supply, lane empty | place R, select 1, Submit |
| Halves | W1 root in split dock, released | Split R → H0,H1; labels 1/2 |
| Recompose | W1 H0,H1 supply | place H0 then H1, Submit → 1 |
| Independent half | W1 H0,H1 supply | place either half, select 1/2, Submit |
| Meaning | W1 H0 lane, H1 supply | select 1/2, Submit |
| Four quarters | W1 H0,H1 supply | dock/release/split H0 then H1 → Q0..Q3 |
| Three quarters | W1 Q0..Q3 supply | place Q0,Q1,Q2, choose 3/4, Submit |
| Join | W1 Q0..Q3 supply, seam marks on Q0,Q1 | dock Q0,Q1, release/Join → H0; Q2,Q3 remain |
| Equivalent half | WL and WR share ReferenceUnitId; H0,H1 active in each. WL H0 placed and H1 supply; WR both halves supply; roots/descendants inactive | right dock/release/split H0 into Q0,Q1, place both; choose 1/2 = 2/4 and same_endpoint; Submit |
| Equivalent 3/4 | Q0..Q3 active in WL and WR; WL Q0..Q2 placed, Q3 supply; all WR quarters supply. Roots/halves/eighths inactive | right dock/release/split Q0,Q1,Q2, place E0..E5; choose 3/4 = 6/8 and same_endpoint; Submit |
| Compare | WL quarters; WR eighths | place left Q0..Q2, right E0..E4; select > and farther_endpoint, Submit |
| Transfer equal | WL half lane; WR eighths supply | place four right eighths, select 1/2 = 4/8 and same_endpoint, Submit |
| Transfer compare | WL quarters; WR eighths | left Q0, right three eighths; choose < and farther_endpoint with right lane identified, Submit |

All unused active pieces remain in their own supply. Each dual-lane task has
two separate eight-cell inventories sharing ReferenceUnitId. Never silently
create pieces by copying a lane. The learner cannot transfer or merge between
inventory owners; both trays and their lane connection are visually clear.
Comparator evaluates quantity ratios to the shared unit, not whether source IDs match.

P1-04 validates fixture schemas, ownership and WholeStageIsSolvable only.
Each subsequent activity ticket implements and tests its own legal trace; future
descriptions do not require premature runtime behavior. P2-08 runs all implemented
traces in StageFixturesAreSolvable. At their introducing milestones also test:
EquivalentLanesConserveSources, CrossLaneTransferCannotDuplicate,
ReorderedQuartersKeepSourceIdentity, RemovedPieceReturnsToSameSupply,
NonSiblingQuartersRecomposeToHalf, DelayedSplitAfterMergeRejected,
CorrectValueWrongNotationRejected, ChangedArrangementInvalidatesSubmission,
HelpRecordedOnce and AllStageCueReferencesResolve.
These are new acceptance cases, not tests already implemented.

When a placed piece must split, remove it, dock and release first. This avoids
ambiguous split-in-lane rearrangement. Incompatible quarter connectors are a tool
limitation, not a claim that the two quarters fail to sum to a half.
