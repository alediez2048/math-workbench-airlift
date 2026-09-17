
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
    /// Orchestrates consent → voice welcome → lesson cards → the open lesson workbench, and feeds the guide
    /// app-authored context. Math and lesson state stay in the LessonStations (Cargo Crew: CargoStation wrapping
    /// OnboardingDirector/CargoLessonDirector); shared voice tools go to the open station through LessonToolRouter.
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
        /// Every lesson workbench in the scene (found at Start when not wired).
        public LessonStation[] stations;
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
            stations = Stations;
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
            // Paused means paused: a tool call still in flight from before Pause runs nothing and asks for no speech.
            if (Paused) { guide.SubmitToolResult(callId, GuideTools.PausedResult, GuidePolicy.SpeakAfterToolResult(Paused)); return; }
            var routing = LessonToolRouter.Route(name, Flow.ActiveLessonId, Stations);
            switch (routing.Route)
            {
                case ToolRoute.Welcome: OnWelcomeTool(name, callId, args); break;
                case ToolRoute.Shared: OnSharedTool(name, callId); break;
                case ToolRoute.Lesson:
                {
                    JObject parsed = null; try { parsed = JObject.Parse(string.IsNullOrEmpty(args) ? "{}" : args); } catch { }
                    if (ActiveStation.TryLessonTool(name, parsed, out var r)) SubmitLessonResult(callId, r.Ok, r.Reason);
                    else guide.SubmitToolResult(callId, "{\"error\":\"unknown tool\"}");
                    break;
                }
                case ToolRoute.NoLesson:
                case ToolRoute.WrongLesson: SubmitLessonResult(callId, false, routing.Reason); break;
                default: guide.SubmitToolResult(callId, "{\"error\":\"unknown tool\"}"); break;
            }
        }

        void OnWelcomeTool(string name, string callId, string args)
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
                case "open_lesson":
                {
                    // A spoken "open Cargo Crew" does exactly what pointing at the card does.
                    string cardId = GuideTools.CardId(args); var card = LessonCatalog.Find(cardId);
                    var decision = GuideTools.OpenLesson(Flow.Phase, cardId);
                    if (decision == OpenLessonDecision.Open && StationFor(cardId) == null) decision = OpenLessonDecision.ComingSoon;   // playable card, no workbench in this build
                    switch (decision)
                    {
                        case OpenLessonDecision.Open:
                            OpenCard(card, narrate: false);
                            guide.SubmitToolResult(callId, ResultNow(true, "", new JObject { ["lesson"] = card.Title,
                                ["say"] = "Welcome the learner to " + ActiveStation.StoryName + " in one sentence using only on_table_now, then ask: would you like to get started? Then stop." }));
                            break;
                        case OpenLessonDecision.ComingSoon:
                            Caption(card.Title + " is coming soon.");
                            guide.SubmitToolResult(callId, new JObject { ["ok"] = false, ["reason"] = "coming soon",
                                ["say"] = "Say in one sentence that it is coming soon and " + LessonCatalog.ReadyPhrase() + " now." }.ToString(Newtonsoft.Json.Formatting.None)); break;
                        case OpenLessonDecision.NotShowing:
                            guide.SubmitToolResult(callId, "{\"ok\":false,\"reason\":\"the lesson cards are not showing right now\"}"); break;
                        default:
                            guide.SubmitToolResult(callId, "{\"ok\":false,\"reason\":\"unknown lesson\"}"); break;
                    }
                    break;
                }
            }
        }

        // ---- shared voice actions: each calls the same station method the button calls and reports what happened ----
        /// The open lesson's workbench, or null in the cards view.
        LessonStation ActiveStation => Flow.Phase == WelcomePhase.Lesson ? StationFor(Flow.ActiveLessonId) : null;

        LessonStation StationFor(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return null;
            foreach (var s in Stations) if (s != null && s.cardId == cardId) return s;
            return null;
        }

        /// Wired stations plus every LessonStation in the scene (a bench built after WireLessonStations still opens),
        /// in catalog order. A scene without a CargoStation gets one on the onboarding workbench, so Cargo Crew always
        /// works. Resolved once per assignment of `stations`.
        LessonStation[] foundStations, foundFrom;
        LessonStation[] Stations
        {
            get
            {
                if (foundStations != null && ReferenceEquals(foundFrom, stations)) return foundStations;
                var found = new System.Collections.Generic.List<LessonStation>();
                if (stations != null) foreach (var s in stations) if (s != null && !found.Contains(s)) found.Add(s);
                foreach (var s in FindObjectsByType<LessonStation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (s != null && !found.Contains(s) && s.gameObject.scene == gameObject.scene) found.Add(s);
                if (onboarding != null && !found.Exists(s => s != null && s.cardId == CargoStation.CardId))
                {
                    var cargo = onboarding.GetComponent<CargoStation>();
                    if (cargo == null) cargo = onboarding.gameObject.AddComponent<CargoStation>();
                    cargo.cardId = CargoStation.CardId; cargo.onboarding = onboarding; cargo.lesson = cargoLesson;
                    cargo.heading = onboarding.heading; cargo.body = onboarding.body;
                    if (cargo.visualRoots == null || cargo.visualRoots.Length == 0)
                        cargo.visualRoots = System.Array.FindAll(stationVisuals ?? new GameObject[0], g => g != null && !StationVisibility.IsShared(g.name));
                    if (!found.Contains(cargo)) found.Add(cargo);
                }
                found.Sort((a, b) => LessonCatalog.IndexOf(a.cardId).CompareTo(LessonCatalog.IndexOf(b.cardId)));
                foundStations = found.ToArray();
                if ((stations == null || stations.Length == 0) && foundStations.Length > 0) stations = foundStations;
                foundFrom = stations;
                return foundStations;
            }
        }

        void OnSharedTool(string name, string callId)
        {
            var station = ActiveStation;
            switch (name)
            {
                case "request_help":
                    guide.SubmitToolResult(callId, ResultNow(true, "", new JObject { ["hint"] = CurrentInstruction() })); break;
                case "advance_step":
                    if (station == null) SubmitLessonResult(callId, false, GuideTools.AdvanceRefusal(Flow.Phase, OnboardingStage.Catalog, false));
                    else { var r = station.Advance(); SubmitLessonResult(callId, r.Ok, r.Reason); }
                    break;
                case "next_chapter":
                case "restart_chapter":
                    if (station == null) SubmitLessonResult(callId, false, GuideTools.NoLesson);
                    else { var r = name == "next_chapter" ? station.NextChapter() : station.RestartChapter(); SubmitLessonResult(callId, r.Ok, r.Reason); }
                    break;
                case "back_to_lessons": VoiceBackToLessons(callId, station); break;
            }
        }

        /// Same as the Back button (the station's Close, then OnLessonBack), refused while anything is held.
        void VoiceBackToLessons(string callId, LessonStation station)
        {
            var gate = GuideTools.BackToLessons(Flow.Phase, station != null && station.AnyHeld);
            if (!gate.Ok) { SubmitLessonResult(callId, false, gate.Reason); return; }
            station?.Close();
            if (Flow.Phase == WelcomePhase.Lesson) ShowCatalog(narrate: false); // the tool result narrates
            SubmitLessonResult(callId, true, "", new JObject { ["say"] = "Say in one sentence that the lesson cards are showing and " + LessonCatalog.ReadyPhrase() + ". Then stop." });
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
            var station = ActiveStation;
            var step = CurrentStep();
            // Stations that list tools_now (café, garden) add lesson + tools_now; Cargo (null) keeps its accepted JSON.
            if (station != null)
                return GuideTools.ToolResult(ok, reason, step, CurrentInstruction(), station.ChapterActive ? station.Chapter : default, extra, station.Title, station.ToolsNow);
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
                if (narrate) Say("Say one short sentence: the lessons are in front of them and " + LessonCatalog.ReadyPhrase() + ". Then stop.");
            }
        }

        // ---- catalog ----
        public void SelectCard(string cardId)
        {
            var card = LessonCatalog.Find(cardId); if (card == null) return;
            if (!card.Playable || StationFor(card.Id) == null)
            {
                Caption(card.Title + " is coming soon.");
                Say("Say one short sentence: " + card.Title + " is coming soon, " + LessonCatalog.ReadyPhrase() + " now. Then stop.");
                return;
            }
            OpenCard(card, narrate: true);
        }

        /// Pointing at the card and saying "open Cargo Crew" both land here.
        void OpenCard(LessonCard card, bool narrate)
        {
            if (StationFor(card.Id) == null || !Flow.OpenLesson(card.Id)) return;
            if (narrate) guide?.Hush(); // a voice open keeps its own response; the tool result narrates
            ShowPhase();
            var station = ActiveStation;
            station.Open();
            if (guide != null)
            {
                guide.PushContext(GuideContextBuilder.Entered(card.Title, card.Facts));
                PushLessonState(); // before any speech, so the welcome describes this step and nothing later
                ApplyMicPolicy(); // questions are welcome inside the lesson
                if (narrate) Say("Welcome the learner to the " + station.Title + " workbench in one sentence using only on_table_now from the latest lesson_state (nothing can be grabbed yet), then ask: would you like to get started? Then stop and wait.");
            }
        }

        /// Wired to the workbench's Back button after OnboardingDirector.Back.
        public void OnLessonBack() { if (Flow.Phase == WelcomePhase.Lesson) GoToCatalog(); }

        // ---- workbench guide: instruction text is the app-authored event source ----
        void Update()
        {
            if (Flow.Phase != WelcomePhase.Lesson) { observedChapter = 0; observedComplete = false; return; }
            if (ActiveStation == null) return;
            if (StateKey() != lastInstruction) PushLessonState(); // context only; the guide speaks when asked
            NarrateChapterChange();
        }

        /// A chapter started or a check was accepted through a button: say one story line. Voice tools mark the
        /// new state as observed first, so their changes are narrated by the tool result instead.
        void NarrateChapterChange()
        {
            var station = ActiveStation;
            var chapter = station != null && station.ChapterActive ? station.Chapter : default;
            if (chapter.Number <= 0) { observedChapter = 0; observedComplete = false; return; }
            string line = GuideTools.StoryLine(observedChapter, observedComplete, chapter);
            observedChapter = chapter.Number; observedComplete = chapter.Complete;
            if (line != null) Say("Say this " + station.StoryName + " line warmly in your own words, in at most two short sentences, then stop: " + line);
        }

        void MarkChapterObserved()
        {
            var station = ActiveStation;
            var chapter = station != null && station.ChapterActive ? station.Chapter : default;
            observedChapter = chapter.Number > 0 ? chapter.Number : 0;
            observedComplete = chapter.Number > 0 && chapter.Complete;
        }

        /// The instruction the learner reads on the open station's card; in the cards view, the onboarding card.
        string CurrentInstruction()
        {
            var station = ActiveStation;
            if (station != null) return station.Instruction;
            return onboarding != null && onboarding.body != null ? GuideSteps.StripDiagnostics(onboarding.body.text) : "";
        }

        GuideStep CurrentStep()
        {
            var station = ActiveStation;
            return station != null ? station.CurrentStep() : GuideSteps.ForOnboarding(onboarding != null ? onboarding.Stage : OnboardingStage.Catalog);
        }

        string StateKey() => CurrentStep().Id + "|" + CurrentInstruction();
        void PushLessonState()
        {
            lastInstruction = StateKey();
            var station = ActiveStation; if (station == null) return;
            var step = CurrentStep();
            guide?.PushContext(GuideContextBuilder.LessonStep(station.Title, step.Id, step.OnTableNow, step.CanGrabNow, CurrentInstruction(), station.ToolsNow));
        }
        void AskHelp() { if (CanSay) { guide.Hush(); Say("Explain this instruction in at most two friendly sentences, then stop: " + CurrentInstruction()); } else Caption(CurrentInstruction()); }

        bool CanSay => guide != null && GuidePolicy.CanPrompt(Paused, guide.Mode == GuideMode.Live);
        /// Every spoken request goes through here: nothing is prompted while paused or offline.
        void Say(string instructions) { if (CanSay) guide.Prompt(instructions); }
        void ApplyMicPolicy() { guide?.SetMicEnabled(GuidePolicy.MicOn(Flow.Phase, Muted, Paused)); ApplyFallbackButtons(); }

        /// Buttons show whenever voice cannot hear the learner: offline, no mic permission, muted, paused, mic-off phase.
        void ApplyFallbackButtons()
        {
            var groups = FallbackGroups();
            if (groups.Count == 0) return;
            bool live = guide != null && guide.Mode == GuideMode.Live && Permission.HasUserAuthorizedPermission(Permission.Microphone);
            bool show = GuidePolicy.ShowFallbackButtons(live, GuidePolicy.MicOn(Flow.Phase, Muted, Paused));
            foreach (var group in groups)
            {
                if (group == null) continue;
                group.alpha = show ? 1f : 0f; group.interactable = show; group.blocksRaycasts = show;
            }
        }

        /// The director's own groups plus every station's button rows.
        System.Collections.Generic.List<CanvasGroup> FallbackGroups()
        {
            var groups = new System.Collections.Generic.List<CanvasGroup>();
            if (fallbackButtons != null) foreach (var g in fallbackButtons) if (g != null && !groups.Contains(g)) groups.Add(g);
            foreach (var s in Stations)
                if (s != null && s.fallbackGroups != null) foreach (var g in s.fallbackGroups) if (g != null && !groups.Contains(g)) groups.Add(g);
            return groups;
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

        bool autoPaused;
        /// Taking the headset off pauses the app: pause the guide so it does not talk to an empty room, and stay
        /// paused until the learner presses Play. A pause the learner chose is never undone here.
        public void HandleAppPause(bool pausing)
        {
            if (pausing)
            {
                if (Paused || Flow.Phase == WelcomePhase.Consent) return;
                TogglePause(); autoPaused = true;
                Caption("Paused while the headset was off. Press Play to continue.");
            }
            else if (autoPaused) { autoPaused = false; ApplyMicPolicy(); }
        }
        void OnApplicationPause(bool pausing) => HandleAppPause(pausing);

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
            // Each station's own roots show only while it is the open lesson; the rest of stationVisuals (the table
            // handle) shows in every lesson.
            var owned = new System.Collections.Generic.HashSet<GameObject>();
            foreach (var s in Stations)
            {
                if (s == null) continue;
                if (s.visualRoots != null) foreach (var root in s.visualRoots) if (root != null && !StationVisibility.IsShared(root.name)) owned.Add(root);
            }
            if (stationVisuals != null) foreach (var go in stationVisuals) if (go && !owned.Contains(go)) go.SetActive(StationVisibility.SharedShows(Flow.Phase));
            foreach (var s in Stations) if (s != null) s.SetVisualsActive(StationVisibility.StationShows(Flow.Phase, Flow.ActiveLessonId, s.cardId));
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
