
using System.Collections;
using Airlift.Guide;
using Airlift.Lessons;
using Airlift.Onboarding;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace Airlift.Welcome
{
    /// Orchestrates consent → voice welcome → lesson cards → Cargo workbench, and feeds the guide
    /// app-authored context. Math and lesson state stay in OnboardingDirector/CargoLessonDirector.
    public sealed class NerdyDirector : MonoBehaviour
    {
        [Header("Wiring")]
        public GuideSession guide;
        public AudioPlayback playback;
        public OnboardingDirector onboarding;
        public Transform head;
        public GameObject consentRoot, welcomeRoot, catalogRoot, hudRoot;
        public GameObject[] stationVisuals;
        public Transform welcomeAnchor;          // world-locked root placed in front of the head once
        public Transform hudWelcomeCanvas, hudStationCanvas;   // the assistant card lives on the welcome panel, then above the workbench
        public Vector2 hudWelcomePosition = new Vector2(0, -262);
        public TMP_Text captionText, stateText, userText;
        public Image orb;
        public Button muteButton, helpButton, repeatButton, skipButton, pauseButton, musicButton;
        public TMP_Text muteLabel, pauseLabel, musicLabel;
        public AmbientMusic music;
        public CargoLessonDirector cargoLesson;   // found at Start when not wired
        /// Voice-first: onboarding row and chapter button row. Hidden while the live guide listens (GuidePolicy).
        public CanvasGroup[] fallbackButtons;
        [Header("Placement")]
        public float welcomeDistance = 1.15f, welcomeBelowEyes = 0.15f;
        public bool adultTesterOnly = true;

        public WelcomeFlow Flow { get; } = new WelcomeFlow();
        public bool Muted { get; private set; }
        /// Pause = Hush + mic off + no prompts until Play.
        public bool Paused { get; private set; }
        string lastInstruction = ""; string lastGuideLine = ""; bool placed;
        int observedChapter; bool observedComplete;          // last chapter state the guide knows about
        int cachedStepKey = -1; GuideStep cachedChapterStep;

        void Start()
        {
            if (cargoLesson == null) cargoLesson = FindAnyObjectByType<CargoLessonDirector>(FindObjectsInactive.Include);
            ShowPhase();
            StartCoroutine(PlaceWhenTracked());
            if (guide != null)
            {
                guide.adultTesterOnlySatisfied = adultTesterOnly;
                guide.ModeChanged += m => { SetState(m == GuideMode.Live ? "live guide" : "offline guide"); ApplyFallbackButtons(); };
                guide.StateChanged += s => { if (stateText != null) stateText.text = s; };
                guide.GuideTranscriptDelta += d => { lastGuideLine += d; if (captionText != null) captionText.text = lastGuideLine; };
                guide.GuideTranscriptDone += t => { lastGuideLine = ""; if (captionText != null) captionText.text = t; };
                guide.UserTranscriptDone += t => { if (userText != null) userText.text = t; };
                guide.ToolCall += OnToolCall;
                guide.Error += e => Debug.LogWarning("[Guide] " + e);
            }
            if (muteButton != null) muteButton.onClick.AddListener(ToggleMute);
            if (helpButton != null) helpButton.onClick.AddListener(AskHelp);
            if (repeatButton != null) repeatButton.onClick.AddListener(() => Say("Repeat your last line in one short sentence, then stop."));
            if (skipButton != null) skipButton.onClick.AddListener(SkipQuestions);
            if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
            if (musicButton != null) musicButton.onClick.AddListener(ToggleMusic);
            if (pauseLabel != null) pauseLabel.text = "Pause";
            if (musicLabel != null && music != null) musicLabel.text = music.Enabled ? "Music on" : "Music off";
            ApplyFallbackButtons();
        }

        IEnumerator PlaceWhenTracked()
        {
            while (head == null || !OnboardingPlacement.IsHeadTracked(head.position)) yield return null;
            var forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized; if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            if (welcomeAnchor != null) welcomeAnchor.SetPositionAndRotation(head.position + forward * welcomeDistance - Vector3.up * welcomeBelowEyes, Quaternion.LookRotation(forward));
            placed = true;
        }

        // ---- consent ----
        public void ConsentAllowVoice() { Flow.Consent(true); ShowPhase(); if (guide != null) { ApplyMicPolicy(); guide.Begin(); } StartCoroutine(GreetWhenLive()); }
        public void ConsentNoVoice() { Flow.Consent(false); ShowPhase(); SetState("offline guide"); Caption("Welcome to Nerdy. Tap the answers below, then choose a lesson."); }

        IEnumerator GreetWhenLive()
        {
            float until = Time.time + 45f;
            while (guide != null && guide.Mode != GuideMode.Live && Time.time < until && guide.State != "offline") yield return null;
            if (guide != null && guide.Mode == GuideMode.Live) Say("Say exactly one short friendly sentence: welcome the learner to Nerdy and ask them to tap the answers on the card. Then stop.");
            else Caption("The live guide is offline right now. Tap the answers below, then choose a lesson.");
        }

        // ---- welcome answers (chips or voice) ----
        /// Chips are the source of truth during onboarding; the guide only acknowledges briefly.
        public void AnswerChip(string spokenEquivalent, string toolArgumentsJson)
        {
            if (Flow.Phase != WelcomePhase.Welcome) return;
            Flow.RecordProfile(toolArgumentsJson); Flow.Profile.Save();
            Caption("Got it: " + spokenEquivalent);
            if (guide != null && guide.Mode == GuideMode.Live)
            {
                guide.Hush();
                guide.PushContext("APP CONTEXT {\"kind\":\"chip_answer\",\"answer\":" + Newtonsoft.Json.JsonConvert.ToString(spokenEquivalent) + "}");
                Say("Acknowledge the tapped answer in at most five words, then stop.");
            }
            var p = Flow.Profile;
            if (!string.IsNullOrEmpty(p.ageBand) && p.interests.Count > 0 && !string.IsNullOrEmpty(p.goal)) StartCoroutine(CatalogAfter(1.2f));
        }

        /// "spoken|json" packed for a single-string persistent listener.
        public void AnswerChipPacked(string packed)
        {
            int i = packed.IndexOf('|'); if (i < 0) return;
            AnswerChip(packed.Substring(0, i), packed.Substring(i + 1));
        }

        public void SkipQuestions()
        {
            if (Flow.Phase != WelcomePhase.Welcome) return;
            guide?.Hush();
            GoToCatalog();
        }

        void OnToolCall(string name, string callId, string args)
        {
            switch (name)
            {
                case "record_profile":
                    bool ok = Flow.RecordProfile(args); if (ok) Flow.Profile.Save();
                    guide.SubmitToolResult(callId, ok ? "{\"ok\":true}" : "{\"ok\":false,\"reason\":\"no valid tags\"}"); break;
                case "end_welcome":
                    guide.SubmitToolResult(callId, "{\"ok\":true,\"next\":\"lesson cards are now visible\"}"); StartCoroutine(CatalogAfter(1.5f)); break;
                case "describe_card":
                    string id = ""; try { id = (string)Newtonsoft.Json.Linq.JObject.Parse(args)["cardId"]; } catch { }
                    guide.SubmitToolResult(callId, LessonCatalog.DescribeJson(id)); break;
                case "request_help":
                    guide.SubmitToolResult(callId, ResultNow(true, "", new JObject { ["hint"] = CurrentInstruction() })); break;
                case "open_lesson":
                {
                    // A spoken "open Cargo Crew" does exactly what pointing at the card does.
                    string cardId = GuideTools.CardId(args); var card = LessonCatalog.Find(cardId);
                    switch (GuideTools.OpenLesson(Flow.Phase, cardId))
                    {
                        case OpenLessonDecision.Open:
                            OpenCard(card, narrate: false);
                            guide.SubmitToolResult(callId, ResultNow(true, "", new JObject { ["lesson"] = card.Title,
                                ["say"] = "Welcome the learner to Dock 7 in one sentence using only on_table_now, then ask: would you like to get started? Then stop." }));
                            break;
                        case OpenLessonDecision.ComingSoon:
                            Caption(card.Title + " is coming soon.");
                            guide.SubmitToolResult(callId, "{\"ok\":false,\"reason\":\"coming soon\",\"say\":\"Say in one sentence that it is coming soon and Cargo Crew is ready now.\"}"); break;
                        case OpenLessonDecision.NotShowing:
                            guide.SubmitToolResult(callId, "{\"ok\":false,\"reason\":\"the lesson cards are not showing right now\"}"); break;
                        default:
                            guide.SubmitToolResult(callId, "{\"ok\":false,\"reason\":\"unknown lesson\"}"); break;
                    }
                    break;
                }
                case "advance_step": VoiceAdvance(callId); break;
                case "replay_demo": VoiceReplayDemo(callId); break;
                case "split_cargo": VoiceChapterAction(callId, l => l.TrySplit()); break;
                case "check_load": VoiceChapterAction(callId, l => l.TryLoad()); break;
                case "reset_cargo": VoiceChapterAction(callId, l => l.TryReset()); break;
                case "next_chapter": VoiceChapterAction(callId, l => l.TryNextChapter()); break;
                case "restart_chapter": VoiceChapterAction(callId, l => l.TryRestartChapter()); break;
                case "back_to_lessons": VoiceBackToLessons(callId); break;
                default: guide.SubmitToolResult(callId, "{\"error\":\"unknown tool\"}"); break;
            }
        }

        // ---- voice actions: each calls the same method the button calls and reports what actually happened ----
        bool ChapterActive => cargoLesson != null && cargoLesson.IsActive;
        OnboardingStage Stage => onboarding != null ? onboarding.Stage : OnboardingStage.Catalog;
        bool PracticeHeld => onboarding != null && onboarding.IsHolding;
        bool AnythingHeld => PracticeHeld || (cargoLesson != null && cargoLesson.AnyPieceHeld);
        bool PrimaryAvailable => onboarding != null && onboarding.primary != null && onboarding.primary.gameObject.activeInHierarchy && onboarding.primary.interactable;

        /// "yes / next": the on-card primary button during onboarding, next_chapter inside a chapter.
        void VoiceAdvance(string callId)
        {
            switch (GuideTools.AdvanceStep(Flow.Phase, ChapterActive, PrimaryAvailable, PracticeHeld))
            {
                case AdvanceRoute.NextChapter: { var r = cargoLesson.TryNextChapter(); SubmitLessonResult(callId, r.Ok, r.Reason); break; }
                case AdvanceRoute.OnboardingStep: onboarding.Continue(); SubmitLessonResult(callId, true, ""); break;
                default: SubmitLessonResult(callId, false, GuideTools.AdvanceRefusal(Flow.Phase, Stage, PracticeHeld)); break;
            }
        }

        /// Same as the Help button during practice; ok only when the demo actually started.
        void VoiceReplayDemo(string callId)
        {
            var gate = GuideTools.ReplayDemo(Flow.Phase, ChapterActive, Stage, PracticeHeld);
            if (!gate.Ok || onboarding == null) { SubmitLessonResult(callId, false, gate.Ok ? GuideTools.NoLesson : gate.Reason); return; }
            onboarding.Help();
            bool started = onboarding.Stage == OnboardingStage.Demonstration;
            SubmitLessonResult(callId, started, started ? "" : "The demo could not start right now.");
        }

        void VoiceChapterAction(string callId, System.Func<CargoLessonDirector, LessonActionResult> action)
        {
            var gate = GuideTools.ChapterTool(Flow.Phase, ChapterActive, Stage);
            if (!gate.Ok) { SubmitLessonResult(callId, false, gate.Reason); return; }
            var r = action(cargoLesson);
            SubmitLessonResult(callId, r.Ok, r.Reason);
        }

        /// Same as the Back button (OnboardingDirector.Back, CargoLessonDirector.Exit, OnLessonBack), refused while held.
        void VoiceBackToLessons(string callId)
        {
            var gate = GuideTools.BackToLessons(Flow.Phase, AnythingHeld);
            if (!gate.Ok) { SubmitLessonResult(callId, false, gate.Reason); return; }
            onboarding?.Back();
            cargoLesson?.Exit();
            if (Flow.Phase == WelcomePhase.Lesson) ShowCatalog(narrate: false); // the tool result narrates
            SubmitLessonResult(callId, true, "", new JObject { ["say"] = "Say in one sentence that the lesson cards are showing and Cargo Crew is ready. Then stop." });
        }

        /// Fresh lesson_state first, then the tool output; the voice change is already narrated, so no story line.
        void SubmitLessonResult(string callId, bool ok, string reason, JObject extra = null)
        {
            MarkChapterObserved();
            if (Flow.Phase == WelcomePhase.Lesson) PushLessonState();
            guide.SubmitToolResult(callId, ResultNow(ok, reason, extra));
        }

        string ResultNow(bool ok, string reason, JObject extra)
        {
            var step = CurrentStep();
            if (ChapterActive && cargoLesson.Chapter != null)
                return GuideTools.ToolResult(ok, reason, step, CurrentInstruction(), cargoLesson.Chapter, cargoLesson.ChapterComplete, cargoLesson.ExpressionText, cargoLesson.Feedback, extra);
            return GuideTools.ToolResult(ok, reason, step, CurrentInstruction(), extra: extra);
        }

        IEnumerator CatalogAfter(float seconds) { yield return new WaitForSeconds(seconds); GoToCatalog(); }

        public void GoToCatalog() { ShowCatalog(narrate: true); }

        void ShowCatalog(bool narrate)
        {
            if (Flow.Phase == WelcomePhase.Welcome) Flow.EndWelcome();
            else if (Flow.Phase == WelcomePhase.Lesson) Flow.BackToCatalog();
            ShowPhase();
            if (guide != null)
            {
                ApplyMicPolicy();
                guide.PushContext(GuideContextBuilder.Catalog());
                if (narrate) Say("Say one short sentence: the lessons are in front of them and Cargo Crew is ready. Then stop.");
            }
        }

        // ---- catalog ----
        public void SelectCard(string cardId)
        {
            var card = LessonCatalog.Find(cardId); if (card == null) return;
            if (!card.Playable)
            {
                Caption(card.Title + " is coming soon.");
                Say("Say one short sentence: " + card.Title + " is coming soon, Cargo Crew is ready now. Then stop.");
                return;
            }
            OpenCard(card, narrate: true);
        }

        /// Pointing at the card and saying "open Cargo Crew" both land here.
        void OpenCard(LessonCard card, bool narrate)
        {
            if (!Flow.OpenLesson(card.Id)) return;
            if (narrate) guide?.Hush(); // a voice open keeps its own response; the tool result narrates
            ShowPhase();
            onboarding?.ChooseCargo();
            if (guide != null)
            {
                guide.PushContext(GuideContextBuilder.Entered(card.Title, card.Facts));
                PushLessonState(); // before any speech, so the welcome describes this step and nothing later
                ApplyMicPolicy(); // questions are welcome inside the lesson
                if (narrate) Say("Welcome the learner to the Cargo Crew workbench in one sentence using only on_table_now from the latest lesson_state (nothing can be grabbed yet), then ask: would you like to get started? Then stop and wait.");
            }
        }

        /// Wired to the workbench's Back button after OnboardingDirector.Back.
        public void OnLessonBack() { if (Flow.Phase == WelcomePhase.Lesson) GoToCatalog(); }

        // ---- workbench guide: instruction text is the app-authored event source ----
        void Update()
        {
            if (Flow.Phase != WelcomePhase.Lesson) { observedChapter = 0; observedComplete = false; return; }
            if (onboarding == null || onboarding.body == null) return;
            if (StateKey() != lastInstruction) PushLessonState(); // context only; the guide speaks when asked
            NarrateChapterChange();
        }

        /// A chapter started or a load was accepted through a button: say one Dock 7 line. Voice tools mark the
        /// new state as observed first, so their changes are narrated by the tool result instead.
        void NarrateChapterChange()
        {
            if (!ChapterActive || cargoLesson.Chapter == null) { observedChapter = 0; observedComplete = false; return; }
            var chapter = cargoLesson.Chapter; bool complete = cargoLesson.ChapterComplete;
            string line = GuideTools.StoryLine(observedChapter, observedComplete, chapter, complete);
            observedChapter = chapter.Number; observedComplete = complete;
            if (line != null) Say("Say this Dock 7 line warmly in your own words, in at most two short sentences, then stop: " + line);
        }

        void MarkChapterObserved()
        {
            bool active = ChapterActive && cargoLesson.Chapter != null;
            observedChapter = active ? cargoLesson.Chapter.Number : 0;
            observedComplete = active && cargoLesson.ChapterComplete;
        }

        /// The instruction the learner reads, without the temporary practice controller readout (which
        /// changed every frame and flooded the conversation with telemetry).
        string CurrentInstruction()
        {
            var shown = ChapterActive && cargoLesson.body != null ? cargoLesson.body : onboarding != null ? onboarding.body : null;
            return shown != null ? GuideSteps.StripDiagnostics(shown.text) : "";
        }

        GuideStep CurrentStep()
        {
            if (!ChapterActive || cargoLesson.Chapter == null) return GuideSteps.ForOnboarding(Stage);
            var chapter = cargoLesson.Chapter; bool complete = cargoLesson.ChapterComplete; bool canSplit = cargoLesson.CanSplit;
            int key = chapter.Number * 4 + (complete ? 2 : 0) + (canSplit ? 1 : 0);   // cached: Update asks every frame
            if (key != cachedStepKey) { cachedStepKey = key; cachedChapterStep = GuideSteps.ForChapter(chapter, complete, canSplit); }
            return cachedChapterStep;
        }

        string StateKey() => CurrentStep().Id + "|" + CurrentInstruction();
        void PushLessonState()
        {
            lastInstruction = StateKey();
            var step = CurrentStep();
            guide?.PushContext(GuideContextBuilder.LessonStep("Cargo Crew", step.Id, step.OnTableNow, step.CanGrabNow, CurrentInstruction()));
        }
        void AskHelp() { if (CanSay) { guide.Hush(); Say("Explain this instruction in at most two friendly sentences, then stop: " + CurrentInstruction()); } else Caption(CurrentInstruction()); }

        bool CanSay => guide != null && GuidePolicy.CanPrompt(Paused, guide.Mode == GuideMode.Live);
        /// Every spoken request goes through here: nothing is prompted while paused or offline.
        void Say(string instructions) { if (CanSay) guide.Prompt(instructions); }
        void ApplyMicPolicy() { guide?.SetMicEnabled(GuidePolicy.MicOn(Flow.Phase, Muted, Paused)); ApplyFallbackButtons(); }

        /// Buttons show whenever voice cannot hear the learner: offline, no mic permission, muted, paused, mic-off phase.
        void ApplyFallbackButtons()
        {
            if (fallbackButtons == null) return;
            bool live = guide != null && guide.Mode == GuideMode.Live && Permission.HasUserAuthorizedPermission(Permission.Microphone);
            bool show = GuidePolicy.ShowFallbackButtons(live, GuidePolicy.MicOn(Flow.Phase, Muted, Paused));
            foreach (var group in fallbackButtons)
            {
                if (group == null) continue;
                group.alpha = show ? 1f : 0f; group.interactable = show; group.blocksRaycasts = show;
            }
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            if (playback != null) { var src = playback.GetComponent<AudioSource>(); if (src != null) src.mute = Muted; }
            ApplyMicPolicy();
            if (Muted) guide?.Hush();
            if (muteLabel != null) muteLabel.text = Muted ? "Unmute" : "Mute";
        }

        /// Pause: hush the guide, close the mic and block prompts. Play: resume the phase's normal policy.
        public void TogglePause()
        {
            Paused = !Paused;
            if (Paused) guide?.Hush();
            ApplyMicPolicy();
            if (pauseLabel != null) pauseLabel.text = Paused ? "Play" : "Pause";
            if (Paused) Caption("Guide paused. Press Play to continue.");
            else if (Flow.Phase == WelcomePhase.Lesson) Say("Say one short sentence: you are back, and what the learner should do now: " + CurrentInstruction());
            else Caption("");
        }

        public void ToggleMusic()
        {
            if (music == null) return;
            music.SetEnabled(!music.Enabled);
            if (musicLabel != null) musicLabel.text = music.Enabled ? "Music on" : "Music off";
        }

        void ShowPhase()
        {
            bool lesson = Flow.Phase == WelcomePhase.Lesson;
            if (consentRoot) consentRoot.SetActive(Flow.Phase == WelcomePhase.Consent);
            if (welcomeRoot) welcomeRoot.SetActive(Flow.Phase == WelcomePhase.Welcome);
            if (catalogRoot) catalogRoot.SetActive(Flow.Phase == WelcomePhase.Catalog);
            if (hudRoot)
            {
                hudRoot.SetActive(Flow.Phase != WelcomePhase.Consent);
                var target = lesson ? hudStationCanvas : hudWelcomeCanvas;
                if (target != null && hudRoot.transform.parent != target)
                {
                    hudRoot.transform.SetParent(target, false);
                    var r = hudRoot.GetComponent<RectTransform>(); if (r != null) { r.anchoredPosition = lesson ? Vector2.zero : hudWelcomePosition; r.localRotation = Quaternion.identity; r.localScale = Vector3.one; }
                }
                if (lesson) { LogHudPlacement("at lesson entry"); if (Application.isPlaying) StartCoroutine(LogHudLater()); }
            }
            foreach (var go in stationVisuals) if (go) go.SetActive(lesson);
            if (onboarding != null && onboarding.catalog != null) onboarding.catalog.SetActive(false);
            // The welcome canvas stays in the world during the lesson; its invisible ray surface must not
            // steal presses from the workbench card when it ends up closer to the controller.
            if (hudWelcomeCanvas != null) { var ray = hudWelcomeCanvas.Find("ISDK_RayCanvasInteraction"); if (ray != null) ray.gameObject.SetActive(!lesson); }
            ApplyFallbackButtons();
        }

        void LateUpdate()
        {
            if (orb == null || playback == null) return;
            float pulse = playback.IsSpeaking ? 1f + 0.12f * Mathf.Sin(Time.time * 9f) : 1f;
            orb.transform.localScale = Vector3.one * pulse;
        }

        // Owner report 2026-09-16 17:42: the assistant bar was not seen above the workbench on the headset while
        // the editor drive shows it 0.27 m above eye level. Runtime evidence for the next report.
        IEnumerator LogHudLater() { yield return new WaitForSeconds(1.5f); LogHudPlacement("1.5 s later"); }
        void LogHudPlacement(string when)
        {
            if (hudRoot == null) return;
            var t = hudRoot.transform; string path = t.name; for (var p = t.parent; p != null; p = p.parent) path = p.name + "/" + path;
            var canvas = t.GetComponentInParent<Canvas>(true);
            string rel = head != null ? " headDist=" + (t.position - head.position).magnitude.ToString("F2") + " elevDeg=" + (Mathf.Asin(Mathf.Clamp((t.position - head.position).normalized.y, -1f, 1f)) * Mathf.Rad2Deg).ToString("F0") + " facing=" + Vector3.Dot(t.forward, (t.position - head.position).normalized).ToString("F2") : "";
            Debug.Log("[Nerdy] HUD " + when + ": " + path + " active=" + hudRoot.activeInHierarchy + " world=" + t.position.ToString("F2") + " lossyScale=" + t.lossyScale.ToString("F4")
                + " canvas=" + (canvas != null ? canvas.name + " enabled=" + canvas.enabled + " mode=" + canvas.renderMode : "none") + rel);
        }

        void SetState(string s) { if (stateText != null) stateText.text = s; }
        void Caption(string s) { if (captionText != null) captionText.text = s; }
    }
}
