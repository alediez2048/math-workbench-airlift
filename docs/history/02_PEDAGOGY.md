# WORKBENCH — pedagogical foundation

> **Legacy research draft.** Preserved for its source notes, but its absolute
> rules and product mappings are superseded by `UNITY_PLAN.md`.

The evidence base, the design rules that follow from it, and the anti-patterns. This file
is also the source material for section 3 of the written submission, so the citations are
kept in full.

---

## 1. The one-paragraph thesis

Number is spatial and quantity is physical. That is the only intellectually honest reason
to put a three-dimensional device in front of a child: not because immersion is engaging,
but because a headset lets a child grab, split, bundle and measure a quantity with their
hands in a way a mouse cannot. Everything else about immersion is a cost that has to be
repaid by instructional method. So the mechanic is manipulation of quantity, never
selection of an answer, and every design decision below exists to keep the medium from
eating the lesson.

---

## 2. The evidence, in six blocks

### 2.1 Concrete to representational to abstract, with a scheduled fade

CRA descends from Bruner's enactive, iconic and symbolic modes. The 2025 meta-analysis
(Ebner, MacDonald, Grekov and Aspiranti, Learning Disabilities Research and Practice; 30
single-case studies, 116 students, 169 effects, 47% in K-5) found large effects across
addition through fractions, with one significant moderator: non-integrated CRA, where the
phases are separated across sessions, outperformed compressing all three into one session.
Read it skeptically, the literature is narrow and two researchers authored 67% of the
studies.
https://journals.sagepub.com/doi/10.1177/09388982241292299

Fyfe, McNeil and Borjas (2015, Learning and Instruction) is the key child experiment.
Second and third graders on math equivalence, three conditions: concrete only, abstract
only, and concreteness fading. Fading beat both. Concrete only was the worst on transfer.
https://www.sciencedirect.com/science/article/abs/pii/S0959475214000942

Carbonneau, Marley and Selig (2013, Journal of Educational Psychology; 55 studies,
N = 7,237) found manipulatives beat symbols-only with moderate-to-large effects on
retention but only small effects on problem solving, transfer, and justification.
Manipulatives help you remember. They do not automatically buy transfer.
https://eric.ed.gov/?id=EJ1007941

IES/WWC 2021, "Assisting Students Struggling with Mathematics: Intervention in the
Elementary Grades," rates all six of its recommendations Strong Evidence, including using
a well-chosen set of concrete and semi-concrete representations, and using the number line
to facilitate learning of mathematical concepts and procedures.
https://ies.ed.gov/ncee/wwc/PracticeGuide/26

### 2.2 Perceptual richness is the enemy of structure

Kaminski, Sloutsky and Heckler (2008, Science 320:454-455): learners taught a mathematical
structure via generic symbols versus concrete instantiations. Concrete sometimes won on
initial acquisition, but generic crushed concrete on transfer, roughly 78% versus 54%
against a 38% chance baseline. All of the generic learners could align the structure
across domains; only a quarter of the concrete learners could. The mechanism: superficial
features and relational structure compete for attention.
https://www.science.org/doi/10.1126/science.1154659

Uttal's work is blunter still. Standard blocks versus versions with colors, swirls and
polka dots: first-graders using the attractive blocks performed worse and made more
calculation errors. They built towers instead of solving problems. And the dual
representation problem: a manipulative has to be simultaneously an object and a symbol,
and the more compelling it is as an object, the worse it works as a symbol.
https://www.erikson.edu/wp-content/uploads/2025/11/Uttal-Chapter-2016.10.02-to-teachers-jes.pdf
https://onlinelibrary.wiley.com/doi/10.1111/j.1750-8606.2009.00097.x
https://www.aft.org/ae/fall2017/willingham

The caution specific to this project: immersive environments are perceptual-richness
machines by construction. Every skybox, particle, ambient prop and animated mascot
competes for the same attentional budget structure-extraction needs. This is the single
largest structural risk in building a VR math game.

Moyer-Packenham and Westenskow's meta-analysis (66 studies) is the counterweight that
says the digital version is worth building: virtual manipulatives versus physical is
roughly a wash (g = 0.15), but virtual versus textbook is large (g = 0.75), and the
grade-band pattern matters enormously here. PreK to grade 4 shows 0.34 to 0.56, grades 5-6
drops to 0.11, grades 7-8 is essentially zero. The target band is the good band. The five
affordances they name are focused constraint, creative variation, simultaneous linked
representations, efficient precision, and motivation. The design value of digital is that
you can forbid mathematically illegal moves and link representations in real time. Wooden
blocks can do neither.
https://blogs.sd38.bc.ca/sd38mathandscience/wp-content/uploads/sites/14/2020/06/VMMetaAnalysisPaper.MoyerPackenhamWestenskow.pdf

### 2.3 Immersion is a cost, not a method

Makransky, Terkildsen and Mayer (2019, Learning and Instruction 60:225-236) is the result
every VR education pitch should have to answer. Same simulation, desktop versus head
mounted display. The headset produced significantly higher presence, significantly lower
learning, and higher cognitive load measured by EEG. The immersion bought engagement and
spent it on extraneous processing.
https://www.sciencedirect.com/science/article/abs/pii/S0959475217303274

The fix is not less immersion, it is adding a generative activity: summarizing, enacting,
self-explaining, or teaching. Parong and Mayer, and Makransky's follow-ups, find that
generative strategies are what make immersive VR learning work. A 2025 study also finds
that where you place the generative activity relative to the immersive experience changes
the result.
https://researchprofiles.ku.dk/en/publications/immersive-virtual-reality-increases-liking-but-not-learning-with-/
https://onlinelibrary.wiley.com/doi/10.1111/jcal.70045

Mayer's immersion principle states it plainly: immersive media do not necessarily improve
learning, but effective instructional methods within immersive environments do.
https://www.cambridge.org/core/books/abs/cambridge-handbook-of-multimedia-learning/immersion-principle-in-multimedia-learning/040AB7C84FA1D20D1B44567C8B62C9D7

CAMIL (Makransky and Petersen, 2021, Educational Psychology Review 33:937-958) is the
model to cite for structure. Immersion and control produce presence and agency, which feed
six factors: situational interest, intrinsic motivation, self-efficacy, embodiment,
cognitive load, and self-regulation. Two of the six are risks, not benefits: extraneous
load from a 360-degree visual field, and degraded self-regulation because the environment
is so engaging that goal focus slips.
https://link.springer.com/article/10.1007/s10648-020-09586-2

Signaling measurably helps: colour, spatial position and multimodal cueing improve learning
outcome and reduce cognitive load in immersive VR.
https://www.sciencedirect.com/science/article/pii/S0360131521000312

The novelty effect is real and it is measurable in exactly this product category. Math
Valley, seven room-scale VR math mini-games, 20 second-graders, 10 sessions over six
weeks: engagement with the learning content decreased as children became more focused on
exploring the virtual environment. Meta-analytically, VR interventions under two hours
show ES 0.72 and those over two hours show 0.47, which is the shape of a novelty effect.
Any claim from a single session is a claim about novelty.
https://link.springer.com/chapter/10.1007/978-3-032-02555-5_8
https://www.mdpi.com/2076-3417/13/1/593

### 2.4 Embodiment, and why the hands matter

Abrahamson's Mathematics Imagery Trainer for Proportion is the design template. The child
holds two hands at different heights, the system compares the ratio to a target, and the
screen turns green when the ratio is right. No mathematics is mentioned. The stated goal is
purely motor: keep it green. Students discovered the relationship through physical
exploration, and when a grid was overlaid as a deliberate breakdown of their existing
strategy, they spontaneously reformulated into proportional language. A controlled study
with 128 students found participants outperformed controls on conceptual items.
https://ccl.northwestern.edu/2014/AbrahamsonLindgren-embodiment-and-embodied-design-in-press_%202.pdf

Abrahamson's own limiting condition, from the 2020 Frontiers paper: actions must be
mathematically meaningful, and non-relevant movements show minimal learning benefit. That
sentence rules out most VR movement mechanics.
https://www.frontiersin.org/journals/education/articles/10.3389/feduc.2020.00147/full

Goldin-Meadow, Cook and Mitchell (2009, Psychological Science): 128 third and fourth
graders who solved zero pretest equivalence problems, instructed to produce particular
gestures while verbalizing the same strategy. Posttest performance rose with gesture
correctness, mediated by whether children spontaneously added the grouping strategy to
their own speech. The experimenter never mentioned grouping. The strategy existed only in
the gestures the children were told to make, and it migrated into their speech and their
reasoning.
https://cpb-us-w2.wpmucdn.com/voices.uchicago.edu/dist/c/1286/files/2018/09/Gesturing-gives-children-new-ideas-about-math-1a81v0s.pdf

Novack, Congdon, Hemani-Lopez and Goldin-Meadow (2014) compared concrete action on objects
against gesture, and gesture produced better generalization. This is the strongest single
argument for hand tracking over controllers: gesture generalizes better than instrumental
action, and a headset is the only medium that can both require a specific hand
configuration and read whether the child produced it.
https://journals.sagepub.com/doi/abs/10.1177/0956797613518351

Fischer et al. (2011, Psychonomic Bulletin and Review): 22 kindergarteners, a dance mat
where you step left for smaller and right for larger during magnitude comparison, versus a
tablet control with no spatial-numerical correspondence. Six sessions of 10-15 minutes over
three weeks. Dance mat beat tablet on number-line estimation (d = 0.68) and on counting
principles (d = 0.66), and the counting gains were mediated by improved mental-number-line
accuracy.
https://link.springer.com/article/10.3758/s13423-010-0031-3

Siegler and Ramani (2008, Developmental Science) is the cleanest template in the whole
literature. Preschoolers, a linear numerical board game versus a colour board game, four
15-minute sessions across two weeks. Number-line linearity, ordering accuracy and
magnitude comparison all improved with very large effects (d around 1.66 to 1.80), gains
endured at least nine weeks, and low-income children became statistically
indistinguishable from middle-income peers. The critical control: circular boards do not
work. Only the linear spatial layout produces the effect. The geometry is the mechanism,
not the decoration.
https://siegler.tc.columbia.edu/wp-content/uploads/2019/02/sieg-ram08.pdf

### 2.5 Which K-5 concepts are worth the effort

Ranked by leverage for a spatial medium.

1. Fraction as a magnitude on a number line. 3.NF.A.2, 3.NF.A.3.a, 4.NF.A.1. Siegler et
   al. (2012, Psychological Science 23:691-697) followed 3,677 UK children and 599 US
   children and found elementary fraction and whole-number division knowledge predicts
   high-school mathematics achievement after controlling for other math knowledge, verbal
   and nonverbal IQ, working memory, family income, parental education and demographics.
   Nothing else in K-5 has that. IES names the number line as the central representational
   tool for it, from the early grades onward.
   https://siegler.tc.columbia.edu/wp-content/uploads/2019/02/Siegler-etal-PsySci12.pdf
   https://ies.ed.gov/ncee/wwc/practiceguide/15

2. Unitizing. 1.NBT.B.2.a, 2.NBT.A.1.a. Ten ones become one ten, ten tens become one
   hundred. The conceptual pivot of base ten, and literally a physical act of composition
   that a virtual medium can enforce as a rule.

3. Multiplicative structure and the area model. 3.OA.A.1, 3.MD.C.7.a, 3.MD.C.7.c. The
   distributive property rendered as a rectangle decomposition is the highest-value single
   standard in K-5 for a spatial medium.

4. The relational equal sign. 1.OA.D.7. Only about 27% of students answer full relational
   items correctly, and second-grade equal-sign knowledge predicts fourth-grade algebra
   competence. Not in this build, but it is the obvious fourth job.
   https://pmc.ncbi.nlm.nih.gov/articles/PMC6421116/

5. Linear magnitude. 2.MD.B.6. Converts number from "how many" to "how far from zero."
6. Making ten. K.OA.A.3, K.OA.A.4, 1.OA.C.6.

Misconceptions to design against, each of which the game logs:
- Whole-number bias in fractions: 1/5 > 1/3 because 5 > 3. Ni and Zhou (2005).
- "Multiplication makes bigger." Created in grades 2-3 by teaching multiplication only as
  repeated addition, and it pays out in grades 5-7. Fischbein's primitive intuitive models.
  https://www.sciencedirect.com/science/article/abs/pii/0959475296000047
- Reading multidigit numerals as concatenated single digits, and zero as absent rather
  than as a placeholder.
- Smaller-from-larger subtraction.
- The operational rather than relational equal sign.

### 2.6 Feedback, difficulty and what to do when the child is wrong

Kluger and DeNisi (1996, Psychological Bulletin; 607 effect sizes) is the finding that
should govern every reward decision in this game. Average d = 0.41, but over 38% of
feedback interventions decreased performance, and the variance far exceeds sampling error.
Feedback Intervention Theory: effectiveness depends on where attention is directed, and it
decreases as attention moves up toward the self and away from the task. Normative
comparison, praise, and person-directed delivery all push attention toward the self.
Leaderboards, star ratings, streaks and "you're a math star" are exactly the cues
identified as performance-degrading.
https://mrbartonmaths.com/resourcesnew/8.%20Research/Marking%20and%20Feedback/The%20effects%20of%20feedback%20interventions.pdf

Shute (2008, Review of Educational Research / ETS RR-07-11): immediate feedback is better
for procedural and declarative retention, for tasks difficult relative to learner ability,
and for low achievers. Elaborated feedback beats simple verification, but overly complex
feedback backfires, and feedback complexity is inversely related to error-correction
ability. Do not interrupt an in-progress attempt. Do not pair grades with comments;
comments alone produce large gains and grades plus comments produce none. A K-5 arithmetic
population sits squarely in the immediate, elaborated, directive, task-focused cell.
https://myweb.fsu.edu/vshute/pdf/shute%202007_f.pdf

Sinha and Kapur (2021, Review of Educational Research; 53 studies, 166 comparisons) on
productive failure: problem-solving-first beats instruction-first for conceptual knowledge
overall (g = 0.36), but it gives essentially nothing for procedural knowledge (g = -0.03)
and the effect favours grades 6-10 and undergraduates. Younger students, grades 2-5,
benefited more from instruction first. So do not build a discovery-first K-5 game. Teach,
then explore.
https://journals.sagepub.com/doi/full/10.3102/00346543211019105

Barbieri et al. (2023, Educational Psychology Review; 43 articles, 181 effect sizes) on
worked examples: overall g = 0.48, correct examples beat incorrect or mixed ones, and
mandatory self-explanation prompts were a negative moderator, which is contrary to
conventional wisdom. Renkl and Atkinson on fading worked steps is the mechanism for
scaffolding escalation.
https://www.danamillercotto.com/uploads/4/7/7/2/47725475/barbieri_et_al__2023__we_meta-analysis.pdf

Rohrer, Dedrick, Hartwig and Cheung (2020, Journal of Educational Psychology 112:40-52) is
the strongest classroom RCT in this space: preregistered cluster RCT, 787 seventh graders,
54 classes, four months, delayed test one month later. Interleaved practice 61% versus
blocked 38%, d = 0.83, positive across all 15 teachers.
https://gwern.net/doc/psychology/spaced-repetition/2019-rohrer.pdf

Baker et al. on gaming the system: 10 to 40% of students game intelligent tutors, and the
causes are subject dislike, low self-drive and frustration, not speed-seeking. The
recommended countermeasures are affective: calibrate difficulty, shorten problems to
increase felt accomplishment, allow skipping while ensuring coverage, and give the child
choice over problem selection.
https://pact.cs.cmu.edu/koedinger/pubs/Baker,%20R.,%20Walonoski,%20J.A.,%20Heffernan,%20N.T.,%20Roll,%20I.%20Corbett,%20A.,%20Koedinger,%20K.R..pdf

On timing: IES 2021 rates regularly including timed activities as one way to build fluency
at Strong Evidence, and Boaler's Fluency Without Fear argues timed testing is unnecessary
and damaging, with math anxiety appearing in children as young as five. EdWeek's review
finds the evidence nuanced and the real failure mode is poorly implemented timing that
exacerbates disparities. The resolution this product adopts: no timer on any item, no
comparison to any other child, and progression measured against the child's own prior
performance. Fluency gets built by spacing and interleaving rather than by a clock.
https://www.youcubed.org/evidence/fluency-without-fear/
https://www.edweek.org/teaching-learning/what-is-math-fact-fluency-and-how-does-it-develop/2023/05

---

## 3. The design rules, as build constraints

Every one of these is enforced somewhere in 01_PRODUCT_SPEC.md.

1. Every concept runs an explicit C to R to A sequence with a scheduled advance, not an
   optional toggle. Mastery is defined at the abstract level only.
2. Strip every perceptual feature from a manipulative that does not carry mathematical
   structure. Colour only where it encodes place value, unit size, or partition count.
3. Every concrete action updates a visible symbolic representation in the same field of
   view, continuously. Understanding does not travel through the fingertips.
4. One canonical manipulative per concept for the life of the product.
5. The number line stays strictly one-dimensional and strictly linear. No arcs, no depth
   on the magnitude axis, no decoration.
6. Magnitude maps small-left to large-right everywhere.
7. Every required movement must be mathematically meaningful. No locomotion, no
   collectibles, no traversal.
8. Prefer hand gesture over controller-mediated action wherever both would work.
9. Add a generative activity to every lesson. The immersion will not teach on its own.
10. Apply the coherence principle ruthlessly. The working volume is nearly empty.
11. Ship a short pre-training tutorial that teaches controls and conventions before any
    mathematics appears. Budget the novelty there so it does not leak into the lesson.
12. Segment. Only the currently relevant objects exist and are interactable.
13. Feedback is immediate, task-focused, elaborated, and short.
14. Ban every self-directed cue. No leaderboards, no stars, no streaks, no praise.
15. Deliver "wrong" through the world. The piece does not seat, the tiles do not cover,
    the post leaves a gap. Never the word "Incorrect."
16. Escalate scaffolding on repeat error: world discrepancy, then step back one CRA phase,
    then a faded worked example with the first step done. Never a bottom-out hint.
17. Teach first, then explore. No discovery-first design for this age band.
18. Hold success near 80-85% and adapt continuously.
19. Give real choice and a legitimate skip.
20. Interleave item types within a session, space re-exposure across sessions.
21. Log error types, not just correctness, and route on misconception class.
22. Sessions are 10 minutes with a hard cap, seated, table-scale, supervised.

---

## 4. Anti-patterns, all of which are one decision away

- The decorated number line. A winding path, a rainbow arc, a 3D corridor. Circular boards
  produced zero effect where linear boards produced very large ones.
- Charming, textured, characterful manipulatives. Every unit of visual appeal subtracts a
  unit of attention from structure.
- Immersion as the pedagogy. Assuming the headset is the intervention.
- The rich explorable world. Ambient life, collectibles, side content. In XR there is no
  screen frame, so extraneous material is unbounded.
- Concrete forever. Building beautiful manipulatives and never fading to symbols, because
  the concrete phase is the fun part to build and demo.
- Manipulation without symbolic linkage.
- Extrinsic reward stacking.
- Binary "try again" feedback.
- Bottom-out hints.
- Movement for movement's sake.
- Discovery-first for young children.
- Multiplication as repeated addition only.
- Fractions as pizza only.
- Blocked practice by topic.
- Efficacy theatre: validating against a researcher-built, treatment-aligned test versus a
  business-as-usual control, reporting a percentage advantage with no sample size, no
  effect size and no limitations section. This is the shape of the best-known VR math
  efficacy claim on the market, and refusing to imitate it is a differentiator.
- Ignoring the physical room. No play-space buffer, no headset-fit protocol for small
  heads, no adult supervision plan. In the largest K-2 VR study, fit and collisions, not
  cybersickness, were the actual operational problems.

---

## 5. The measurement design to put in the submission

Do not claim learning gains from a five-day build. Present the study instead. This is the
paragraph that lands hardest, because Nerdy leadership was publicly criticized in 2026,
when Varsity Tutors for Schools closed, for shipping ahead of evidence.

- Design: randomized, two arms, four weeks.
- Participants: n = 120 children, ages 10-11, recruited from the consumer membership base,
  stratified by baseline numeracy.
- Arms: Workbench versus an active control, which is the equivalent content delivered
  through the existing free browser games. Not business-as-usual. The choice of control is
  what drives most ed-tech effect sizes, and an active control is the honest one.
- Dosage: four 15-minute sessions per week, matching the Siegler and Ramani template.
- Primary outcome: a validated early-numeracy instrument administered pre and post, not a
  researcher-built test aligned to the treatment.
- Secondary outcome: strategy-use coding from the action log against the developmental
  sequence, count-all to count-on-from-larger to decomposition to retrieval. This is the
  measure the product uniquely produces.
- Retention: a delayed post-test at four weeks after the intervention ends, because a
  single-session result is a novelty result.
- Falsification: if the treatment arm does not show a larger shift in strategy
  distribution than the active control, the embodiment argument is wrong and the headset
  is not earning its cost. Say that out loud.
