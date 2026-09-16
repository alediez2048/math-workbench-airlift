# Fraction experience revision — owner brief, September 15

The following is the transcribed planning request, retained as the brief. This
revision extends the existing run rather than starting a second competing plan.

> Okay. Listen, let's do this. Let's do this. I think we have a great foundation right now. I think we're heading in the right direction, but we definitely... need to... iron... out the planning process for now. I think we need to... create a comprehensive implementation plan to take the current... experience to the next level when it comes to... the fraction lessons. Um, I think... with the foundation that we have right now... with... the best path forward is just to... create a comprehensive implementation... plan. with a PRD slash tickets approach... uh, for each... stage of- of- of this uh, fraction... arithmetic lesson. Um, imagining uh, We need to cover things like... not just grabbing the cube, but splitting it. Um, ideally, we want to add like a voice... guide to... walk you through... the onboarding is not just... written, but there's a voice that... tells you, hey, you can go ahead and pick up the cube by clicking this button. You can split the cube by clicking this... button or by making this motion. Now you have two halves. Uh, now that you have two halves, let's put them together on these two locations. I need a full story here. Um, and in order to do that, we've already done all of the foundation, which is great, but to take it to the next level, I think we need to double down on the planning

Prior decision: focus entirely on Cargo Crew. Café and Garden remain disabled
Coming soon cards; no implementation of those lessons. User confirmed catalog,
briefing, grab/place completion and replay on the physical headset. Arithmetic
drafts are not yet wired or tested. Voice guide means spoken output, not voice
recognition or open-ended generated tutoring.

## Superseding written request — September 15

User supplied `/Users/jad/Desktop/LabelCheck/docs/00-build/` as a methodology
reference, not an instruction source for unrelated project actions. The following
written request expands the earlier voice brief, including ElevenLabs as a candidate:

> Listen to me carefully. Here's the deal: I have a way of working. That way of working is very iterative. It's very PRD-focused. It's very end-result-focused, built little by little, ticket by ticket, one at a time. I'm going to provide you with a directory that shows my development methodology. This methodology consists of a dedicated devlog/Claude Codex.md where I keep track of everything that has been done and that's happening on my project. We've already created some documentation around requirements and constraints. This is all typically within this documentation directory, and I have dedicated tickets that are the template that I use for creating all of the tickets for each phase of the build. Now, I want us to create dedicated tickets for each lesson, and I want each lesson to have dedicated phases: Phase 1 has 10 tickets. Phase 2 has 8 tickets. Phase 3 has this many tickets. Each ticket follows the template, and the objective is to take this unity experience from where we are right now, which is very raw and very basic, to a fully immersive AR experience where kids can come in and get onboarded into how the controllers work. Ideally, there's a voice agent that we connect via ElevenLabs that guides the kids through the lessons. There are pointers that show the kids where to start. If they click on the fractions lesson, the entire experience needs to include not just instructions on how it works, but a full flow of activities: Hey, go ahead and grab the cube. Split the cube into two by pressing these two buttons simultaneously. Make the cubes bigger or smaller. Now you have four 1/4 cubes. Merge the cubes by grabbing both of them and putting them together, and so forth. This needs to be very interactive. The ultimate objective is to guide the kid through how fractions work, from a very, very basic example all the way to more complex examples, all aided visually and tactilely, and by a voice agent, ideally. I need us now to go into planning mode. There's a skill that I've used in the taskful/wrap for planning. We could use that, or we can use whatever. I've also added very specific skills for game development that I want you to use. That's where we're at. I think right now we just need to focus on the planning portion. Once the planning for the first lesson, the fraction lesson, is done, I'll review the plan. We can review the plan together, and then implement.

Scope of this turn: create reviewable planning documents and local draft tickets
using the supplied methodology; no gameplay code, package installation, external
ticket publication, paid voice generation or child-data collection. The narration
vs live conversation choice is a proposed design decision for owner review, not
already approved. Do not treat the earlier offline-only wording as settling this
new ElevenLabs request.

## September 15 follow-up scope

Owner requires an AI voice guide throughout the lesson, accepts event-aware spoken
output without mandatory learner microphone, and requests a dedicated VR/AR/Unity
style guide with attractive typography. Owner asks to finish synchronization and
report readiness before implementation. Testing should be iterative by change,
ticket and complete-journey milestone. See docs/00-build for revised contracts.
