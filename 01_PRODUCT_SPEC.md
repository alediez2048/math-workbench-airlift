# WORKBENCH — product spec

> **Legacy WebXR product spec.** Preserved as research history only. Its scope
> and lesson priorities are superseded by `UNITY_PLAN.md`.

Build-ready detail for the three jobs. Read 02_PEDAGOGY.md for why each rule exists.

---

## 0. Frame

The child is building a backyard lemonade stand on their own kitchen table, in passthrough.
The virtual build sits on the real surface. The child's real hands are visible. A parent
in the room can see the child's face and the child can see them.

Three jobs, in order, each gated on the one before. Each job is a real situation where
arithmetic does work that counting cannot.

Session shape: 10 minutes hard cap with a visible wind-down. Not a timer on the math, a
cap on the session. Short supervised sessions are what the child-VR feasibility literature
supports, and ClassVR's own guidance for young children is 10 to 15 minutes.

Target band: grades 3-5 content. Ages 10-11 on the headset (Meta's floor is 10). Ages 8-9
and anyone without hardware get the same engine flat in a browser.

---

## 1. Shared systems (build these first, on Day 1)

### 1.1 The unit object
A grabbable, snappable primitive. Plain geometry, one flat color, no texture, no
character, no face, no animation idle. Perceptual richness competes with structure for
the same attentional budget, and decorated manipulatives measurably worsened first-grade
performance. Color carries meaning only where it encodes place value, unit size, or
partition count. Nothing else is colored.

### 1.2 The symbol panel
A small flat surface, always in the same field of view as the child's hands, that mirrors
the physical action as a number or an equation, updating live as the child moves objects.
This is the single most-skipped feature in VR math games and it is the one that makes the
manipulation into mathematics. Manipulation without an explicit, continuous symbolic link
does not teach.

Rules: the panel is never the input. The child never types or picks from it. It only
reflects. It sits at a fixed offset from the build so it never requires a head turn.

### 1.3 The CRA fade
Every job has three levels and the child advances on mastery, not on choice.

- C: objects present, panel mirrors them.
- R: objects present but ghosted, a diagram (ten-frame, array grid, number line) is the
  primary representation, panel mirrors.
- A: objects gone, symbols only, the child works in the panel's terms and the world
  confirms.

Mastery is defined at the A level. Concrete-level mastery does not count as mastery. Build
a hard fade gate: the child cannot stay in C.

### 1.4 The world signal
Three states, communicated by the object and the scene, never by a word.
- Neutral: nothing said.
- Fits: the piece seats, the frame closes, a soft physical click.
- Does not fit: the piece visibly will not seat, a gap is shown, the surface goes a muted
  warm tone. Never the word "Incorrect." Never a red X. Never a buzzer.

### 1.5 Error escalation
1. First wrong attempt: the world shows the discrepancy. Nothing else.
2. Second: step back one CRA phase automatically for this item only.
3. Third: a faded worked example with the first step already completed, in the same
   representation the child is working in.
Never a hint chain that ends in the answer.

### 1.6 Adaptive difficulty
Hold success near 80-85%. Items are drawn from a pool per job, interleaved by type within
a session, and a missed type is re-presented later in a session rather than immediately
re-drilled.

### 1.7 Banned
No timer on any item. No leaderboard. No stars, coins, streaks, XP or badges. No "great
job," no praise of the child, no normative comparison. No background music during a task.
No ambient wildlife, particles, or decorative props in the working volume. No teleporting,
climbing, or any movement that carries no mathematical meaning.

### 1.8 The action log
Every physical action writes a structured event: timestamp, job, level, item, action type,
objects involved, resulting quantity, and whether the world accepted it. This log, not the
correct/incorrect flag, is what the tutor reasons over and what the dashboard displays.

---

## 2. JOB 1 — THE TILL

Real situation: you sold lemonade all afternoon. Now count what you have and pay $2.40
for the lumber.

Standards: 1.NBT.B.2.a, 1.NBT.B.2.b, 1.NBT.B.2.c, 2.NBT.A.1.a, 2.NBT.A.3, 2.MD.C.8.
Hinge: unitizing. Ten ones become one ten, ten tens become one hundred. This is the
conceptual pivot of the entire base-ten system, and it is a physical act, which is why it
belongs in a spatial medium.

### Mechanic
- A jar of penny tokens spills on the table. Plain discs, one color.
- A bundler sits beside it. Drop ten pennies in and they physically fuse into one dime
  token: a different shape and color, and it thereafter behaves as one object. Nine
  pennies in the bundler do nothing and sit there visibly incomplete.
- Ten dimes into the bundler fuse into one dollar.
- The register accepts a payment only in the correct total. It also refuses to accept more
  than nine of any unit. Handing it fourteen pennies gets them handed back with the
  bundler highlighted. That refusal is the lesson, delivered as a rule of the world.
- Expanded form appears on the panel as the child bundles: 2 dollars + 4 dimes + 0 pennies
  = 240 pennies = $2.40. Zero as a placeholder appears explicitly, because failure to see
  zero as a placeholder is a documented misconception.

### The power beat
Level 3 gives the child 240 loose pennies and a five-minute wall clock they will never
beat by counting. Bundling takes twenty seconds. The child discovers the shortcut is the
place-value system, not a trick.

### Levels
- C: pennies and dimes visible, bundler physical, panel shows expanded form.
- R: pennies ghosted, a ten-frame grid is the primary object, panel shows expanded form.
- A: no coins. The panel shows "3 tens and 12 ones = ?" and the register accepts the
  number. The world confirms by paying out.

### Error types to log
- Counted all instead of bundling
- Bundled only when prompted
- Attempted to pay with more than nine of a unit
- Read a numeral as concatenated digits (206 as "2 and 6")
- Treated zero as absent rather than as a placeholder

### Generative moment
"How did you know you had enough?" The correct-strategy answer names the bundle. The
count-all answer describes counting. Claude classifies which.

---

## 3. JOB 2 — THE COUNTERTOP

Real situation: the stand needs a countertop. Tiles come in packs. How many packs do you
buy, and how do you know without counting every tile?

Standards: 2.OA.C.4, 3.OA.A.1, 3.OA.A.3, 3.OA.B.5, 3.MD.C.5, 3.MD.C.6, 3.MD.C.7.a,
3.MD.C.7.c, 3.MD.C.7.d.
Hinge: multiplicative structure. Moving from repeated addition to a x b as a
two-dimensional object is what makes the distributive property, the area model, partial
products, and eventually algebra tractable.

### Mechanic
- A rectangular counter outline sits on the real table. The child places unit squares to
  tile it. Gaps and overlaps are refused; the tile will not seat.
- Count-all is allowed and is deliberately slow. The panel counts up: 1, 2, 3...
- Once one full row is placed, the panel starts showing the row count and the column count
  separately: "6 in a row, 7 rows." Then it writes 7 x 6 = 42.
- The commutativity beat: grab the whole tiled rectangle and rotate it 90 degrees. It is
  the same tiles. The panel writes 6 x 7 = 42 and shows both equations side by side.
- THE SPLIT, which is the best beat in the game: the store only sells tiles in packs of
  five per row. The child grabs the vertical seam after column five and drags it apart.
  The rectangle physically separates into a 7x5 slab and a 7x1 strip, with a visible gap
  between them. Two totals appear: 35 and 7. They add to 42. The panel writes
  7 x 6 = (7 x 5) + (7 x 1) = 35 + 7 = 42.
  That is 3.MD.C.7.c rendered as an object the child pulled apart with their hands, and it
  is the highest-value single standard in the K-5 set for a spatial medium.

### The power beat
Level 3 gives an 8 x 7 counter, a fact most children this age do not have memorized. The
child cannot count it in time and does not know it. They split it into 8x5 + 8x2, get 40
and 16, and add to 56. They have just invented partial products. The panel names what they
did.

### Non-negotiable
Multiplication is presented as array and area from the first frame, alongside and never
solely as repeated addition. Teaching it only as repeated addition plants "multiplication
makes bigger," which detonates two to three grades later on fractions and decimals.

### Levels
- C: physical tiles, panel writes the equation.
- R: tiles ghosted, a grid diagram is primary, the child drags the split line on the
  diagram.
- A: no tiles. "You need 8 rows of 7. You know 8 x 5. Finish it." The world builds the
  counter when the child gets it.

### Error types to log
- Counted all with no attempt at rows and columns
- Did not recognize commutativity after rotation
- Split into unequal groups
- Added the split parts incorrectly
- Treated the array as repeated addition only, never as rows-by-columns

### Generative moment
"Show me a different way to get the same number." Multiple entry points, multiple
solution strategies. This is NCTM practice 2 and Standard for Mathematical Practice 7.

---

## 4. JOB 3 — THE CUT

Real situation: the support post is too long. Measure it, mark it, cut it. Measure wrong
and you waste the board.

Standards: 2.MD.A.1, 2.MD.A.2, 2.MD.B.6, 3.MD.B.4, 3.NF.A.1, 3.NF.A.2.a, 3.NF.A.2.b,
3.NF.A.3.a, 3.NF.A.3.d.
Hinge: fraction as a magnitude on a number line. This is the single highest-leverage
concept in all of K-5. Elementary fraction knowledge predicts high-school mathematics
achievement after controlling for IQ, working memory, family income, and parental
education, in two large longitudinal samples.

### Mechanic
- A tape measure the child physically grabs and pulls out along the post. It is strictly
  one-dimensional, strictly straight, unlaid on a flat plane, with no arc, no curve, no
  decoration, and no depth on the magnitude axis. This is a hard constraint, not a style
  choice: the linear layout is the mechanism. Circular board games produced zero effect
  where linear ones produced very large effects.
- Small values are always to the left, large to the right, everywhere in the product.
- Level A, whole units: the tape shows whole inches. Mark and cut at 7.
- The inverse-relation beat: measure the same post first with a foot-stick, then with an
  inch-stick. One reads 2, the other reads 24. Smaller unit, larger count. The panel shows
  both. This is 2.MD.A.2 and it is the fraction prerequisite hiding in the measurement
  standards, delivered two years before fractions normally arrive.
- Level B, halves: the tape now shows a tick between each inch. The child partitions the
  interval from 0 to 1 into two equal parts and marks 2 1/2. The partition is physical:
  the child drags a divider and it snaps only to equal parts. Unequal partitions are
  refused, which is 3.NF.A.1's "equal parts" as a rule of the world.
- Level C, quarters and eighths: the whole-number-bias trap, run deliberately. The child
  is asked which is longer, a 1/4 piece or a 1/8 piece. Most children this age say 1/8
  because 8 is bigger. Both pieces are then laid on the tape from 0. The 1/8 is visibly
  shorter. The world does the correcting, silently.
- Equivalence: 2/4 and 1/2 land on the same point on the tape. The panel shows them
  stacked at the same position. That is 3.NF.A.3.a delivered as a coincidence of location,
  which is exactly what equivalence means.
- The cut: mark, cut, seat the post in the frame. If the mark was wrong the post is too
  short and there is a visible gap, or too long and it will not seat. The board is spent.
  Feedback through the world.

### The power beat
Level 3 asks for 2 3/4 feet when the tape is marked in eighths. The child has to see that
3/4 and 6/8 are the same point. Getting it right seats the post and the stand stands up.
Getting it wrong wastes a board, and there are only three boards.

### Never
No pizzas. No pies. No shaded circles. Area and part-whole models alone leave whole-number
bias intact and never establish fraction-as-magnitude, which is the entire point. The
number line is the central representational tool, as IES recommends from the early grades
onward.

### Levels
- C: physical tape, physical dividers, panel names the fraction.
- R: tape ghosted, a clean number line diagram is primary, the child places a point.
- A: no tape. "Which is closer to 1, 3/4 or 5/8?" The world confirms by cutting correctly.

### Error types to log
- Whole-number bias: chose the larger denominator as the larger fraction
- Partitioned into unequal parts
- Counted tick marks rather than intervals
- Started measuring from 1 rather than 0
- Did not recognize equivalence at the same point
- Ignored the unit when comparing across different rulers

### Generative moment
"Why is 1/8 smaller than 1/4 when 8 is bigger than 4?" This is the highest-value
articulation in the game and the most quotable line in the demo video if a child says it
well.

---

## 5. The AI tutor

### Behavior
- Push to talk on the grip button. Never listening otherwise. Say so on screen; a parent
  should be able to see that the microphone is not always on.
- Context passed to Claude: the current job, level, item, the last 10 action-log events,
  the child's error history in this session, and the misconception taxonomy.
- The tutor is a shop foreman, not a cheerleader. It never praises the child. It talks
  about the work. "That board's short by a quarter inch. Look where you put the mark."
- Short turns. Two sentences. Feedback complexity is inversely related to error correction.
- It never gives the answer. If the child asks directly, it asks one question back and
  offers to step back a phase.

### The generative moment
At the end of each job the tutor asks the child to explain how they did it. The child
speaks. Two things happen:
1. The explanation is itself the generative learning activity that immersive media
   requires. Without one, high immersion produces more presence and less learning than the
   same content on a flat screen.
2. Claude classifies the explanation, together with the action log, into a strategy and a
   misconception set, and writes it to the dashboard.

### The strategy taxonomy the classifier emits
Addition and counting: count-all, count-on-from-first, count-on-from-larger,
decomposition, retrieval.
Place value: no-bundling, prompted-bundling, spontaneous-bundling, concatenated-digit
reading, zero-as-absent.
Multiplication: count-all, skip-counting, rows-by-columns, decomposition into partial
products, retrieval.
Fractions: whole-number-bias, unequal-partition, tick-counting, interval-counting,
magnitude-on-line, equivalence-by-position.

This taxonomy is the product. "Got it right" is worth nothing to a tutor. "Used count-all
rather than count-on for 8+5, three times in a row" is a lesson plan.

---

## 6. The web companion

Same repo, no headset, judges can click it immediately.

- Live mirror of the current job while a session is running.
- The mastery view: skills as a small graph, each annotated with the strategy the child
  actually used, not a percentage.
- Session history with the 10-minute cap visible.
- "Ask Maya about this skill" and "Book a tutor on this skill" buttons. Non-functional is
  fine, they show where this plugs into Nerdy's existing surfaces.
- One sentence of framing on the page: every action in the headset is a diagnostic event
  for the Study Plan.

This page is the strategically serious half of the product and it is the hedge. If the
headset never ships, the pedagogy, the engine, and the mastery model all survive.
