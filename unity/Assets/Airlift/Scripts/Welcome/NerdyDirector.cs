
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
        [Tooltip("The Nerdy lounge: the home the learner arrives in, is onboarded in and browses in. CC-FD-01.")]
        public Airlift.Lounge.LoungeRoom lounge;
        [Tooltip("The logo arrival that plays before anything else. CC-FD-03.")]
        public Airlift.Lounge.LoungeArrival arrival;
        bool arriving;
        [Header("Front door (CC-FD-05..09)")]
        public Airlift.Lounge.LoungeRundown rundown;
        public Airlift.Presentation.Dashboard.DashboardWall wall;
        public Airlift.Lounge.SettingsPanel settingsPanel;
        public Airlift.Lounge.ControllerHelper helper;
        public Airlift.Lounge.BackButtonWatcher backButton;
        public Airlift.Lounge.LoungeSettings loungeSettings;
        [Tooltip("‹ and › in the board's bottom-right corner (BuildNavArrows). Dimmed, never hidden.")]
        public Button navBack, navNext;
        public Button exitLessonButton, micButton;
        public TMP_Text micLabel;
        public Button conversationButton;
        public TMP_Text conversationLabel;
        public TMP_Text conversationNotice, settingsConversationNotice;
        public Airlift.Lounge.SettingsState Settings { get; private set; } = new Airlift.Lounge.SettingsState();
        public Airlift.Lounge.LibraryState Library { get; private set; } = new Airlift.Lounge.LibraryState();
        public Transform welcomeAnchor;          // world-locked root placed in front of the head once
        public Transform hudWelcomeCanvas, hudStationCanvas;   // the assistant card lives on the welcome panel, then above the workbench
        public Vector2 hudWelcomePosition = new Vector2(0, -262);
        public TMP_Text captionText, stateText, userText;
        public Image orb;
        public Button muteButton, helpButton, repeatButton, skipButton, pauseButton, musicButton;
        public TMP_Text muteLabel, pauseLabel, musicLabel;
        public AmbientMusic music;
        [Header("Voice language (consent card)")]
        public Button[] languageButtons;          // English, Español: same order as GuideLanguage.Codes
        public Sprite languageChosen, languageIdle;
        public string Language { get; private set; } = GuideLanguage.Default;
        public CargoLessonDirector cargoLesson;   // found at Start when not wired
        /// Every lesson workbench in the scene (found at Start when not wired).
        public LessonStation[] stations;
        /// Voice-first: onboarding row and chapter button row. Hidden while the live guide listens (GuidePolicy).
        public CanvasGroup[] fallbackButtons;
        [Header("Placement")]
        public float welcomeDistance = 1.15f, welcomeBelowEyes = 0.15f;
        public bool adultTesterOnly = true;

        /// Owner 2026-09-18: onboarding (questions + rundown) is off for now; the welcome card leads to the wall.
        public const bool OnboardingEnabled = true;
        /// Owner 2026-09-18: "leave the lessons alone" — no helper hints inside a lesson.
        public const bool LessonHintsEnabled = false;
        public WelcomeFlow Flow { get; } = new WelcomeFlow { SkipOnboarding = !OnboardingEnabled };
        public bool Muted { get; private set; }
        /// Pause = Hush + mic off + no prompts until Play.
        public bool Paused { get; private set; }
        string lastInstruction = ""; string lastGuideLine = ""; bool placed;
        int observedChapter; bool observedComplete;          // last chapter state the guide knows about
        int cachedStepKey = -1; GuideStep cachedChapterStep;
        string observedIntro;                                // last concept-intro step id the guide knows about

        void Start()
        {
            if (cargoLesson == null) cargoLesson = FindAnyObjectByType<CargoLessonDirector>(FindObjectsInactive.Include);
            stations = Stations;
            LoadStores();
            Debug.Log("[Nerdy] onboarding " + (OnboardingEnabled ? "on" : "off") + " skip=" + Flow.SkipOnboarding + " rundownSeen=" + Flow.RundownSeen + " profileComplete=" + Flow.Profile.IsComplete + " tour=" + (rundown != null));
            string savedLanguage = Settings.Language;
            try { savedLanguage = PlayerPrefs.GetString(GuideLanguage.PrefsKey, Settings.Language); } catch (System.Exception) { }
            ChooseLanguage(savedLanguage);
            WireFrontDoor();
            ApplySettings("all");
            ShowPhase();
            StartCoroutine(PlaceWhenTracked());
            StartCoroutine(RunArrival());
            if (guide != null)
            {
                guide.adultTesterOnlySatisfied = adultTesterOnly;
                guide.ModeChanged += m => { Debug.Log("[Guide] mode " + m + " language " + guide.language); SetState(m == GuideMode.Live ? "live guide" : "offline guide"); ApplyFallbackButtons(); };
                guide.StateChanged += s => { Debug.Log("[Guide] state " + s + " micEnabled=" + guide.MicEnabled + " micGateOpen=" + guide.MicGateOpen); if (stateText != null) stateText.text = s; };
                guide.GuideTranscriptDelta += d => { lastGuideLine += d; if (captionText != null) captionText.text = lastGuideLine; };
                guide.GuideTranscriptDone += t => { lastGuideLine = ""; if (captionText != null) captionText.text = t; };
                guide.UserTranscriptDone += t => { if (userText != null) userText.text = t; };
                guide.ToolCall += OnToolCall;
                guide.Error += e => Debug.LogWarning("[Guide] " + e);
            }
            if (muteButton != null) muteButton.onClick.AddListener(ToggleMute);
            if (helpButton != null) helpButton.onClick.AddListener(AskHelp);
            if (repeatButton != null) repeatButton.onClick.AddListener(() => { if (rundown != null && rundown.Running) AskHelp(); else Say("Repeat your last line in one short sentence, then stop."); });
            if (skipButton != null) skipButton.onClick.AddListener(SkipQuestions);
            if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
            if (musicButton != null) musicButton.onClick.AddListener(ToggleMusic);
            if (conversationButton != null) conversationButton.onClick.AddListener(TogglePause);
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
        public void BeginWelcome()
        {
            if (Flow.Phase != WelcomePhase.Consent) return;
            guide?.Hush();
            RequestMicPermissionOnce();
            Flow.Begin(); ShowPhase(); ApplyMicPolicy();
            if (Flow.Phase == WelcomePhase.Welcome)
            {
                const string line = "Choose your answers on the card, or select Skip to lessons.";
                Caption(line); Say("Say this word for word, then stop: " + line);
            }
            AfterConsent();
        }

        /// Owner 2026-09-18: the Android microphone prompt comes at the first card's press, where the learner already
        /// says yes to voice, instead of surprising them mid-lesson. Capture still waits for the age gate and Play.
        void RequestMicPermissionOnce()
        {
            if (Application.isPlaying && !Permission.HasUserAuthorizedPermission(Permission.Microphone)) Permission.RequestUserPermission(Permission.Microphone);
        }

        /// The Age pill on the settings card (owner 2026-09-18): the same gate as the questions, answerable later.
        void OnAgeChosen(string band)
        {
            Flow.Profile.ageBand = band; SaveProfile();
            bool browsingOrLearning = Flow.Phase == WelcomePhase.Catalog || Flow.Phase == WelcomePhase.Lesson;
            if (browsingOrLearning && GuidePolicy.MicAutoOnAfterQuestions(band, Settings.VoiceGuide, Paused)) EnableMicrophone();
            else { ApplyMicPolicy(); RefreshNavArrows(); }
        }

        /// The questions are done and the learner is an adult: Dee listens from here on. Stop still cuts it.
        void EnableMicrophone()
        {
            Flow.SetVoiceConsent(true);
            if (Application.isPlaying && !Permission.HasUserAuthorizedPermission(Permission.Microphone)) Permission.RequestUserPermission(Permission.Microphone);
            ApplyMicPolicy();
            Caption("Microphone on: talk to Dee or use the buttons. Stop turns it off.");
        }

        public void ToggleMicrophone()
        {
            if (!CanListen || (Flow.Phase != WelcomePhase.Catalog && Flow.Phase != WelcomePhase.Lesson)) return;
            Flow.SetVoiceConsent(!Flow.VoiceConsented);
            ApplyMicPolicy();
            Caption(Flow.VoiceConsented ? "Microphone enabled. You can speak to Dee or use the buttons." : "Microphone off. Buttons still work.");
            rundown?.SetVoice(guide != null && guide.MicStreaming);
        }
        public void ConsentAllowVoice()
        {
            RequestMicPermissionOnce();
            Flow.Consent(true); ShowPhase();
            if (guide != null) { guide.language = Language; ApplyMicPolicy(); if (!greeted) guide.Begin(); }
            if (!greeted) StartCoroutine(GreetWhenLive());
            AfterConsent();
            rundown?.ReportConsent();
        }

        /// Straight to the wall when the questions are off (or the learner is returning).
        void AfterConsent()
        {
            if (Flow.Phase != WelcomePhase.Catalog) return;
            if (TourDue) ShowCatalog(narrate: true);
            else if (Flow.Returning) WelcomeBack(); else ShowCatalog(narrate: true);
        }

        /// A returning learner lands on the wall in two presses, with a short welcome back and Continue first.
        void WelcomeBack()
        {
            ShowWall();
            var cont = Library.Continue;
            string where = cont != null && LessonCatalog.Find(cont.Value.lessonId) != null ? " They were last in " + LessonCatalog.Find(cont.Value.lessonId).Title + "." : "";
            Say("Say in one short sentence: welcome back, the wall is in front of them and they can continue where they left off." + where + " Then stop.");
            Caption("Welcome back. Pick up where you left off.");
        }

        /// Owner 2026-09-17: "Dee should kickstart as soon as i arrive to the lounge." The session connects while
        /// the logo is still animating, so she can speak the moment the room is there. The mic stays off until the
        /// learner allows voice (GuidePolicy.MicOn keeps it off outside the catalog and a lesson), so she greets
        /// without ever listening.
        void ConnectGuideSilently()
        {
            if (greeted || guide == null) return;
            greeted = true;
            guide.language = Language;
            guide.SetMicEnabled(false);
            guide.Begin();
        }

        public void GreetOnArrival() { ConnectGuideSilently(); StartCoroutine(GreetWhenLive()); }
        bool greeted;

        /// The logo arrival, then the board. Always ends, even with no guide and no network.
        IEnumerator RunArrival()
        {
            if (arrival == null || lounge == null) { GreetOnArrival(); yield break; }
            arriving = true; ShowPhase();
            arrival.Finished += OnArrivalFinished;
            arrival.Begin(() => lounge.StartupProgress, () => lounge.Startup.ShouldShowBar);
            ConnectGuideSilently();
            lounge.Startup.Complete(Airlift.Lounge.StartupStep.Microphone);   // not asked for until the learner allows voice
            while (!placed) yield return null;
            lounge.Startup.Complete(Airlift.Lounge.StartupStep.Assets);
            while (guide != null && guide.Mode == GuideMode.Offline && arrival.isActiveAndEnabled) yield return null;
            lounge.Startup.Complete(Airlift.Lounge.StartupStep.Guide);
        }

        void OnArrivalFinished()
        {
            if (arrival != null) arrival.Finished -= OnArrivalFinished;
            arriving = false;
            ShowPhase();
            StartCoroutine(GreetWhenLive());
        }

        /// First run with the onboarding on: the host tour is Dee's greeting. Returning learners get the short one.
        bool TourDue => OnboardingEnabled && Flow.Phase == WelcomePhase.Catalog && rundown != null && !Flow.RundownSeen && !rundown.Running;

        IEnumerator StartTourWhenLive()
        {
            float until = Time.time + 8f;   // a short wait for her voice; the tour reads fine in captions if it never comes
            while (guide != null && guide.Mode != GuideMode.Live && Time.time < until && guide.State != "offline") yield return null;
            if (TourDue) StartTour(0);
        }

        /// Consent card: the voice language for the whole session (owner 2026-09-17). The server locks the minted session to it.
        public void ChooseLanguage(string code)
        {
            string previous = Language;
            Language = GuideLanguage.Normalize(code);
            try { PlayerPrefs.SetString(GuideLanguage.PrefsKey, Language); } catch (System.Exception) { }
            settingsPanel?.SetLanguage(Language);
            if (guide != null) guide.language = Language;
            // Owner 2026-09-18: Spanish chosen in settings, Dee kept speaking English. The session is locked in the
            // language it was minted in, so she is reconnected and picks up where she was (tour line, lesson step, greeting).
            bool connected = guide != null && (guide.Mode == GuideMode.Live || guide.State == "minting" || guide.State == "connecting");
            if (Application.isPlaying && GuidePolicy.ReconnectForLanguage(previous, Language, connected, Paused))
            {
                guide.End(); guide.language = Language; guide.Begin();
                int version = ++resumeVersion;
                if (Flow.Phase == WelcomePhase.Consent) StartCoroutine(GreetWhenLive());
                else StartCoroutine(ResumeWhenLive(version));
                Caption(Language == "es" ? "Dee ahora habla español." : "Dee now speaks English.");
            }
            if (languageButtons != null)
                for (int i = 0; i < languageButtons.Length && i < GuideLanguage.Codes.Length; i++)
                {
                    var image = languageButtons[i] != null ? languageButtons[i].GetComponent<Image>() : null;
                    if (image == null) continue;
                    bool chosen = GuideLanguage.Codes[i] == Language;
                    var sprite = chosen ? languageChosen : languageIdle;
                    if (sprite == null) continue;
                    image.sprite = sprite; image.type = Image.Type.Sliced;
                    image.color = chosen ? Color.white : new Color(1f, 1f, 1f, 0.1f);
                    float half = Mathf.Max(1f, Mathf.Min(image.rectTransform.rect.width, image.rectTransform.rect.height)) / 2f;
                    image.pixelsPerUnitMultiplier = sprite.border.x / half * (100f / sprite.pixelsPerUnit);   // capsule ends
                }
        }
        public void ConsentNoVoice() { Flow.Consent(false); ShowPhase(); SetState("offline guide"); if (rundown == null || !rundown.Running) Caption(GuideIntro.OfflineCaption); AfterConsent(); rundown?.ReportConsent(); }

        IEnumerator GreetWhenLive()
        {
            float until = Time.time + 45f;
            while (guide != null && guide.Mode != GuideMode.Live && Time.time < until && guide.State != "offline") yield return null;
            if (Flow.Phase != WelcomePhase.Consent) yield break; // a delayed connection must not replay the welcome over the tour
            if (guide != null && guide.Mode == GuideMode.Live) Say(GuideIntro.PromptFor(Language));
            else Caption(GuideIntro.OfflineCaption);
        }

        // ---- welcome answers (chips or voice) ----
        /// Chips are the source of truth during onboarding; the guide only acknowledges briefly.
        public void AnswerChip(string spokenEquivalent, string toolArgumentsJson)
        {
            if (Flow.Phase != WelcomePhase.Welcome) return;
            Flow.RecordProfile(toolArgumentsJson); SaveProfile();
            Caption("Got it: " + spokenEquivalent);
            if (guide != null && guide.Mode == GuideMode.Live)
            {
                guide.Hush();
                guide.PushContext("APP CONTEXT {\"kind\":\"chip_answer\",\"answer\":" + Newtonsoft.Json.JsonConvert.ToString(spokenEquivalent) + "}");
                Say("Acknowledge the tapped answer in at most five words, then stop.");
            }
            var p = Flow.Profile;
            if (p.IsComplete) { if (rundown != null && rundown.Running) RefreshNavArrows(); else StartCoroutine(CatalogAfter(1.2f)); }
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
            RemindAge();
        }

        /// Owner 2026-09-18: said out loud and in the caption, not only on the bar. No age answer means Dee cannot
        /// listen (adult testers only); the Age pill on the settings card fixes that in one press.
        void RemindAge()
        {
            if (GuidePolicy.MicNotice(Flow.Profile.ageBand, Settings.VoiceGuide, Paused) != GuidePolicy.AgeNotice) return;
            Caption(GuidePolicy.AgeNotice + ".");
            Say("Say this word for word, then stop: Set your age in settings to talk with me.");
        }

        void OnToolCall(string name, string callId, string args)
        {
            Debug.Log("[Guide] tool " + name + " phase=" + Flow.Phase);
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
            if (rundown != null && rundown.Running)
            {
                bool blocked = ((name == "open_lesson" || name == "dashboard_open_tile") && !rundown.Script.CanOpenLesson)
                    || (name == "dashboard_filter" && rundown.Script.Current.Gate != Airlift.Lounge.RundownGate.FilterPressed)
                    || (name == "open_settings" && rundown.Script.Current.Gate != Airlift.Lounge.RundownGate.SettingsOpened);
                if (blocked) { guide.SubmitToolResult(callId, new JObject { ["ok"] = false, ["reason"] = TourFirst }.ToString(Newtonsoft.Json.Formatting.None)); return; }
            }
            switch (name)
            {
                case "record_profile":
                    bool ok = Flow.RecordProfile(args); if (ok) SaveProfile();
                    guide.SubmitToolResult(callId, ok ? "{\"ok\":true}" : "{\"ok\":false,\"reason\":\"no valid tags\"}"); break;
                case "end_welcome":
                    guide.SubmitToolResult(callId, "{\"ok\":true,\"next\":\"lesson cards are now visible\"}"); StartCoroutine(CatalogAfter(1.5f)); break;
                case "describe_card":
                    string id = ""; try { id = (string)Newtonsoft.Json.Linq.JObject.Parse(args)["cardId"]; } catch { }
                    guide.SubmitToolResult(callId, LessonCatalog.DescribeJson(id)); break;
                case "dashboard_open_tile":
                {
                    var (lessonId, chapter) = GuideTools.TileArgs(args);
                    if (lessonId == "continue") { var parts = DashboardCatalog.ContinueTileId(Library).Split('#'); lessonId = parts[0]; chapter = int.Parse(parts[1]); }
                    var decision = GuideTools.OpenTile(Flow.Phase, lessonId, chapter);
                    if (decision == OpenLessonDecision.Open && StationFor(lessonId) == null) decision = OpenLessonDecision.ComingSoon;
                    switch (decision)
                    {
                        case OpenLessonDecision.Open:
                            if (!OpenTile(lessonId, chapter, narrate: false)) { guide.SubmitToolResult(callId, "{\"ok\":false,\"reason\":\"Lesson could not open.\"}"); break; }
                            guide.SubmitToolResult(callId, ResultNow(true, "", new JObject { ["lesson"] = ActiveStation.Title, ["chapter"] = chapter,
                                ["say"] = "Welcome the learner to " + ActiveStation.StoryName + " in one sentence using only on_table_now, then ask: would you like to get started? Then stop." }));
                            break;
                        case OpenLessonDecision.ComingSoon:
                            Caption(GuideTools.ComingSoonRefusal);
                            guide.SubmitToolResult(callId, new JObject { ["ok"] = false, ["reason"] = GuideTools.ComingSoonRefusal }.ToString(Newtonsoft.Json.Formatting.None)); break;
                        case OpenLessonDecision.NotShowing:
                            guide.SubmitToolResult(callId, new JObject { ["ok"] = false, ["reason"] = GuideTools.WallNotShowing }.ToString(Newtonsoft.Json.Formatting.None)); break;
                        default:
                            guide.SubmitToolResult(callId, "{\"ok\":false,\"reason\":\"unknown lesson or chapter\"}"); break;
                    }
                    break;
                }
                case "dashboard_filter":
                {
                    var filter = GuideTools.ParseFilter(args);
                    if (Flow.Phase != WelcomePhase.Catalog || wall == null) guide.SubmitToolResult(callId, new JObject { ["ok"] = false, ["reason"] = GuideTools.WallNotShowing }.ToString(Newtonsoft.Json.Formatting.None));
                    else if (filter == null) guide.SubmitToolResult(callId, "{\"ok\":false,\"reason\":\"The wall sorts by featured, newest or most viewed.\"}");
                    else { bool touring = rundown != null && rundown.Running; wall.SetFilter(filter.Value); guide.SubmitToolResult(callId, new JObject { ["ok"] = true, ["filter"] = filter.Value.ToString(), ["say"] = "Say in a few words that the wall now shows " + FilterName(filter.Value) + ". Then stop." }.ToString(Newtonsoft.Json.Formatting.None), !touring); }
                    break;
                }
                case "open_settings":
                    if (loungeSettings == null || Flow.Phase == WelcomePhase.Lesson) guide.SubmitToolResult(callId, new JObject { ["ok"] = false, ["reason"] = GuideTools.SettingsInLessonOnly }.ToString(Newtonsoft.Json.Formatting.None));
                    else { loungeSettings.Show(true); guide.SubmitToolResult(callId, "{\"ok\":true,\"say\":\"Say in a few words that settings are open. Then stop.\"}"); }
                    break;
                case "replay_rundown":
                    if (!Flow.CanReplayRundown) guide.SubmitToolResult(callId, new JObject { ["ok"] = false, ["reason"] = GuideTools.RundownOnlyFromTheWall }.ToString(Newtonsoft.Json.Formatting.None));
                    else { guide.SubmitToolResult(callId, "{\"ok\":true}", GuidePolicy.SpeakAfterToolResult(true)); ReplayRundown(); }
                    break;
                case "open_lesson":
                {
                    // A spoken "open Cargo Crew" does exactly what pointing at the card does.
                    string cardId = GuideTools.CardId(args); var card = LessonCatalog.Find(cardId);
                    var decision = GuideTools.OpenLesson(Flow.Phase, cardId);
                    if (decision == OpenLessonDecision.Open && StationFor(cardId) == null) decision = OpenLessonDecision.ComingSoon;   // playable card, no workbench in this build
                    switch (decision)
                    {
                        case OpenLessonDecision.Open:
                            if (!OpenCard(card, narrate: false)) { guide.SubmitToolResult(callId, "{\"ok\":false,\"reason\":\"Lesson could not open.\"}"); break; }
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
            string seen = observedIntro;
            var shown = MarkIntroObserved();
            // An intro step reached by this voice tool is said once, from say_exactly; a refusal during the intro keeps
            // its reason and does not repeat the step already said.
            var intro = IntroNarration.ShouldNarrate(seen, shown) ? shown : null;
            if (Flow.Phase == WelcomePhase.Lesson) PushLessonState();
            guide.SubmitToolResult(callId, ResultNow(ok, IntroNarration.ReasonFor(reason, intro), IntroNarration.WithSayExactly(extra, intro)));
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

        IEnumerator CatalogAfter(float seconds) { yield return new WaitForSeconds(seconds); if (Flow.Phase == WelcomePhase.Welcome) GoToCatalog(); }

        public void GoToCatalog() { ShowCatalog(narrate: true); }

        void ShowCatalog(bool narrate)
        {
            bool fromQuestions = Flow.Phase == WelcomePhase.Welcome;
            if (Flow.Phase == WelcomePhase.Welcome) Flow.EndWelcome();
            else if (Flow.Phase == WelcomePhase.Lesson) Flow.BackToCatalog();
            if (fromQuestions) { Library.RundownSeen = false; SaveLibrary(); }
            ShowWall();
            if (fromQuestions && GuidePolicy.MicAutoOnAfterQuestions(Flow.Profile.ageBand, Settings.VoiceGuide, Paused)) EnableMicrophone();
            if (TourDue) { StartTour(0); return; }
            if (guide != null && narrate && !(rundown != null && rundown.Running)) Say("Say one short sentence: the lessons are in front of them and " + LessonCatalog.ReadyPhrase() + ". Then stop.");
        }

        /// The wall, refreshed from the library, with Dee told what is on it.
        void ShowWall()
        {
            ShowPhase();
            if (wall != null) wall.Refresh(Library);
            rundown?.Repoint();
            // The catalog context is unchanged on purpose: the Cargo golden recording pins it byte for byte. What the
            // wall holds (tiles, filters, the six coming-soon worlds) is in the proxy's persona and tool descriptions.
            if (guide != null)
            {
                ApplyMicPolicy();
                guide.PushContext(GuideContextBuilder.Catalog());
            }
        }

        static string FilterName(DashboardFilter f) => f == DashboardFilter.MostViewed ? "most viewed" : f.ToString().ToLowerInvariant();

        // ---- the host tour (spec 2026-09-18-host-tour-design.md) ----
        void StartTour(int at)
        {
            if (rundown == null || Flow.Phase != WelcomePhase.Catalog) return;
            if (wall != null && Flow.Phase == WelcomePhase.Catalog) { wall.SetFilter(DashboardFilter.Featured); wall.Refresh(Library); }
            rundown.Begin(voice: guide != null && guide.MicStreaming && Settings.VoiceGuide, at);
            RefreshNavArrows();
        }

        void OnRundownStep(Airlift.Lounge.RundownStep step, string say)
        {
            guide?.SpeakScripted(say, CanSay);
            Caption(say);
            RefreshNavArrows();
        }

        void OnRundownEnded(bool skipped)
        {
            guide?.EndScriptedNarration();
            Debug.Log("[Nerdy] catalog tour " + (skipped ? "skipped" : "completed"));
            Flow.MarkRundownSeen(); Library.RundownSeen = true; SaveLibrary();
            if (skipped) { Caption("You can replay the tour from the gear any time."); Say("Say in a few words that they can replay the tour from the gear any time. Then stop."); }
            RemindAge();
            RefreshNavArrows();
        }

        /// The gear's "Replay the tour" and the replay_rundown tool: the wall stops, while browsing.
        public void ReplayRundown()
        {
            if (!Flow.CanReplayRundown || rundown == null) return;
            loungeSettings?.Close();
            StartTour(Airlift.Lounge.RundownScript.WallStart);
        }

        bool SettingsOpen => loungeSettings != null && loungeSettings.IsOpen;
        BackAction BackNow() { var s = ActiveStation; return GuideTools.BackButton(Flow.Phase, s != null && s.AnyHeld, rundown != null && rundown.Running, SettingsOpen); }
        NextAction NextNow()
        {
            var s = ActiveStation; var chapter = s != null && s.ChapterActive ? s.Chapter : default;
            return GuideTools.NextButton(Flow.Phase, SettingsOpen, rundown != null && rundown.Running, rundown != null && rundown.ContinueLit,
                chapter.Number > 0, chapter.Complete, chapter.IsLast, s != null && s.AnyHeld, Flow.Profile.IsComplete);
        }

        /// The ‹ arrow and the physical B button: toward the main screen, one step.
        public void PressBack()
        {
            if (Flow.Phase == WelcomePhase.Lesson && ActiveStation != null && ActiveStation.AnyHeld) { Caption(GuideTools.LetGo); return; }
            if (!SettingsOpen && Flow.Phase == WelcomePhase.Catalog && !(rundown != null && rundown.Running)) { wall?.PreviousPage(); return; }
            var station = ActiveStation;
            switch (BackNow())
            {
                case BackAction.CloseSettings: loungeSettings.Close(); break;
                case BackAction.SkipRundown: rundown.ReportBack(); break;
                case BackAction.BackToLessons: station?.Close(); OnLessonBack(); break;
                case BackAction.BackToWelcomeCard: guide?.Hush(); if (Flow.BackToConsent()) { ShowPhase(); ApplyMicPolicy(); } break;
            }
            RefreshNavArrows();
        }

        /// The › arrow: forward, never destructive.
        public void PressNext()
        {
            switch (NextNow())
            {
                case NextAction.SkipQuestions: SkipQuestions(); break;
                case NextAction.RundownContinue: rundown.PressContinue(); break;
                case NextAction.NextPage: wall?.NextPage(); break;
                case NextAction.NextChapter: { var r = ActiveStation.NextChapter(); if (!r.Ok) Caption(r.Reason); break; }
            }
            RefreshNavArrows();
        }

        /// Dimmed when there is nothing to do; the corner never changes shape.
        void RefreshNavArrows()
        {
            bool lesson = Flow.Phase == WelcomePhase.Lesson;
            if (navBack != null) navBack.gameObject.SetActive(false);
            if (navNext != null) navNext.gameObject.SetActive(false);
            if (exitLessonButton != null) exitLessonButton.gameObject.SetActive(lesson);
            bool touring = rundown != null && rundown.Running;
            Dim(navBack, !touring && (BackNow() != BackAction.Nothing || Flow.Phase == WelcomePhase.Catalog));
            Dim(navNext, NextNow() != NextAction.Nothing);
            if (micButton != null) micButton.interactable = CanListen && (Flow.Phase == WelcomePhase.Catalog || lesson);
            if (micLabel != null) micLabel.text = Flow.VoiceConsented ? "Mic on" : "Mic off";
            if (conversationLabel != null) conversationLabel.text = Paused ? "▶ Play" : "■ Stop";
            if (pauseLabel != null) pauseLabel.text = Paused ? "Play" : "Stop";
            string notice = Paused ? CanListen && !Muted && Settings.VoiceGuide ? "Play enables microphone" : "Microphone stays off"
                : GuidePolicy.MicNotice(Flow.Profile.ageBand, Settings.VoiceGuide, Paused);
            if (conversationNotice != null) conversationNotice.text = notice;
            if (settingsConversationNotice != null) settingsConversationNotice.text = Paused || !string.IsNullOrEmpty(notice) ? notice : "Conversation";
        }
        static void Dim(Button b, bool on)
        {
            if (b == null) return;
            var g = b.GetComponent<CanvasGroup>(); if (g == null) g = b.gameObject.AddComponent<CanvasGroup>();
            float alpha = on ? 1f : 0.35f;
            if (Mathf.Abs(g.alpha - alpha) > 0.01f) g.alpha = alpha;
            if (b.interactable != on) b.interactable = on;
        }

        // ---- the wall (CC-FD-07) ----
        /// A tile press (or dashboard_open_tile): chapter 0 = the hero card, as today; 1..5 straight into the chapter.
        public void OpenTile(string lessonId, int chapter) => OpenTile(lessonId, chapter, narrate: true);

        bool OpenTile(string lessonId, int chapter, bool narrate)
        {
            if (!CanEnterLesson()) return false;
            var card = LessonCatalog.Find(lessonId);
            if (card == null || !card.Playable || StationFor(lessonId) == null)
            {
                Caption(GuideTools.ComingSoonRefusal);
                Say("Say in one short sentence that that one is coming soon and " + LessonCatalog.ReadyPhrase() + " now. Then stop.");
                return false;
            }
            if (!Flow.OpenLesson(card.Id)) return false;
            Library.RecordOpened(card.Id, chapter); SaveLibrary();
            if (narrate) guide?.Hush();
            ShowPhase();
            var station = ActiveStation;
            station.Open(chapter);
            rundown?.ReportLessonOpened();
            if (guide != null)
            {
                guide.PushContext(GuideContextBuilder.Entered(card.Title, card.Facts));
                PushLessonState();
                ApplyMicPolicy();
                if (narrate) Say(chapter > 0
                    ? "Welcome the learner to the " + station.Title + " workbench in one sentence, tell them chapter " + chapter + " is on the table using only on_table_now from the latest lesson_state, then stop and wait."
                    : "Welcome the learner to the " + station.Title + " workbench in one sentence using only on_table_now from the latest lesson_state (nothing can be grabbed yet), then ask: would you like to get started? Then stop and wait.");
            }
            return true;
        }

        const string TourFirst = "Finish this tour step, or select Skip tour before opening a lesson.";
        bool CanEnterLesson()
        {
            if (SettingsOpen) return false;
            if (rundown == null || rundown.Script.CanOpenLesson) return true;
            Caption(TourFirst); return false;
        }

        // ---- stores and settings (CC-FD-06/09) ----
        void LoadStores()
        {
#if UNITY_EDITOR
            string fixture = UnityEditor.SessionState.GetString("Nerdy.PreviewFixture", "");
            if (!string.IsNullOrEmpty(fixture))
            {
                previewFixture = true;
                Settings = new Airlift.Lounge.SettingsState();
                Library = new Airlift.Lounge.LibraryState();
                if (fixture != "fresh") Flow.Profile.Merge("{\"ageBand\":\"adult\",\"interests\":[\"space\"],\"goal\":\"curious\"}");
                Flow.RundownSeen = Library.RundownSeen = fixture == "returning" || fixture == "skipped";
                return;
            }
#endif
            Settings = Airlift.Lounge.LoungeStoreFiles.LoadSettings();
            Library = Airlift.Lounge.LoungeStoreFiles.LoadLibrary();
            Flow.RundownSeen = Library.RundownSeen;
            try { Flow.Profile.Merge(LearnerProfile.Load().ToJson()); } catch (System.Exception) { }
            Airlift.Lounge.NerdyHaptics.Enabled = Settings.Haptics;
        }

        bool previewFixture;
        void SaveLibrary() { if (!previewFixture) Airlift.Lounge.LoungeStoreFiles.Save(Library); }
        void SaveProfile() { if (!previewFixture) Flow.Profile.Save(); }

        void WireFrontDoor()
        {
            if (settingsPanel != null)
            {
                settingsPanel.Bind(Settings, Library);
                settingsPanel.Changed += ApplySettings;
                settingsPanel.ReplayRundownRequested += ReplayRundown;
                settingsPanel.SetAge(Flow.Profile.ageBand);
                settingsPanel.AgeChosen += OnAgeChosen;
                settingsPanel.SavedDataCleared += () => { Flow.RundownSeen = false; if (Flow.Phase == WelcomePhase.Catalog && wall != null) wall.Refresh(Library); };
            }
            if (rundown != null) { rundown.StepShown += OnRundownStep; rundown.Ended += OnRundownEnded; }
            if (loungeSettings != null) { loungeSettings.Opened += ApplyWelcomeRaySurface; loungeSettings.Closed += ApplyWelcomeRaySurface; }
            if (wall != null) wall.TileOpened += (lesson, chapter) => OpenTile(lesson, chapter);
            if (backButton != null) backButton.Pressed += PressBack;
        }

        /// One place turns a setting into an effect; "all" at start.
        void ApplySettings(string key)
        {
            Settings = settingsPanel != null ? settingsPanel.State : Settings;
            if (lounge != null && lounge.Mode != Settings.Scenery && (key == "all" || key == "Reset")) lounge.Apply(Settings.Scenery);
            if (captionText != null) captionText.gameObject.SetActive(Settings.Captions);
            Airlift.Lounge.NerdyHaptics.Enabled = Settings.Haptics;
            if (playback != null) { var src = playback.GetComponent<AudioSource>(); if (src != null) src.volume = Settings.VoiceVolume; }
            if (music != null) music.volumeScale = Settings.MusicVolume / 0.4f;
            if (!Settings.VoiceGuide) guide?.Hush();
            if (!Settings.ButtonLabels || !LessonHintsEnabled) helper?.Hide();
            ApplyMicPolicy();
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
        bool OpenCard(LessonCard card, bool narrate)
        {
            if (!CanEnterLesson() || StationFor(card.Id) == null || !Flow.OpenLesson(card.Id)) return false;
            Library.RecordOpened(card.Id, 0); SaveLibrary();
            if (narrate) guide?.Hush(); // a voice open keeps its own response; the tool result narrates
            ShowPhase();
            var station = ActiveStation;
            station.Open();
            rundown?.ReportLessonOpened();
            if (guide != null)
            {
                guide.PushContext(GuideContextBuilder.Entered(card.Title, card.Facts));
                PushLessonState(); // before any speech, so the welcome describes this step and nothing later
                ApplyMicPolicy(); // questions are welcome inside the lesson
                if (narrate) Say("Welcome the learner to the " + station.Title + " workbench in one sentence using only on_table_now from the latest lesson_state (nothing can be grabbed yet), then ask: would you like to get started? Then stop and wait.");
            }
            return true;
        }

        /// Wired to the workbench's Back button after OnboardingDirector.Back.
        public void OnLessonBack() { if (Flow.Phase == WelcomePhase.Lesson) GoToCatalog(); }

        // ---- workbench guide: instruction text is the app-authored event source ----
        void Update()
        {
            if (Flow.Phase != WelcomePhase.Lesson) { observedChapter = 0; observedComplete = false; observedIntro = null; return; }
            if (ActiveStation == null) return;
            if (StateKey() != lastInstruction) PushLessonState(); // context only; the guide speaks when asked
            NarrateIntroChange();
            NarrateChapterChange();
        }

        /// A concept-intro step appeared through a button (Next, Start fractions): Dee says it word for word once.
        void NarrateIntroChange()
        {
            var station = ActiveStation; var intro = station != null ? station.CurrentIntro : null;
            bool narrate = IntroNarration.ShouldNarrate(observedIntro, intro);
            observedIntro = intro?.Id;
            if (narrate) Say(IntroNarration.PromptFor(intro));
        }

        IntroStep MarkIntroObserved()
        {
            var station = ActiveStation; var intro = station != null ? station.CurrentIntro : null;
            observedIntro = intro?.Id;
            return intro;
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
        bool lastCanGrab;
        void PushLessonState()
        {
            lastInstruction = StateKey();
            var station = ActiveStation; if (station == null) return;
            var step = CurrentStep();
            // CC-FD-05a: the helper comes back inside a lesson the first time a step lets the learner grab.
            if (LessonHintsEnabled && step.CanGrabNow && !lastCanGrab && Settings.ButtonLabels && helper != null) helper.Show(Airlift.Lounge.ControllerButton.Grip, "Grip", 6f);
            lastCanGrab = step.CanGrabNow;
            guide?.PushContext(GuideContextBuilder.LessonStep(station.Title, step.Id, step.OnTableNow, step.CanGrabNow, CurrentInstruction(), station.ToolsNow));
        }
        void AskHelp()
        {
            if (rundown != null && rundown.Running && guide != null)
            {
                var line = guide.NarrationLine;
                guide.SpeakScripted(line, CanSay); Caption(line); return;
            }
            if (CanSay) { guide.Hush(); Say("Explain this instruction in at most two friendly sentences, then stop: " + CurrentInstruction()); } else Caption(CurrentInstruction());
        }

        bool CanSay => guide != null && GuidePolicy.CanPrompt(Paused, guide.Mode == GuideMode.Live, Settings.VoiceGuide);
        bool CanListen => adultTesterOnly && Flow.Profile.ageBand == "adult" && (Flow.Phase == WelcomePhase.Catalog || Flow.Phase == WelcomePhase.Lesson);
        /// Every spoken request goes through here: nothing is prompted while paused or offline.
        void Say(string instructions) { if (CanSay) guide.Prompt(instructions); }
        void ApplyMicPolicy()
        {
            bool on = Flow.VoiceConsented && CanListen && GuidePolicy.MicOn(Flow.Phase, Muted, Paused, Settings.VoiceGuide);
            if (guide != null && guide.MicEnabled != on) Debug.Log("[Guide] mic " + (on ? "on" : "off") + " phase=" + Flow.Phase + " muted=" + Muted + " paused=" + Paused);
            guide?.SetMicEnabled(on); ApplyFallbackButtons();
        }

        /// Buttons show whenever voice cannot hear the learner: offline, no mic permission, muted, paused, mic-off phase.
        void ApplyFallbackButtons()
        {
            var groups = FallbackGroups();
            if (groups.Count == 0) return;
            bool live = guide != null && guide.Mode == GuideMode.Live && Permission.HasUserAuthorizedPermission(Permission.Microphone);
            bool show = true; // voice and manual actions remain available together; chapter buttons are lesson-owned
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
            int version = ++resumeVersion;
            if (Paused) guide?.End();
            else
            {
                bool listening = CanListen && !Muted && Settings.VoiceGuide;
                if (listening)
                {
                    Flow.SetVoiceConsent(true); // A separate notice beside Play discloses microphone activation.
                    if (Application.isPlaying && !Permission.HasUserAuthorizedPermission(Permission.Microphone)) Permission.RequestUserPermission(Permission.Microphone);
                }
                if (Application.isPlaying && Settings.VoiceGuide && guide != null) { guide.Begin(); StartCoroutine(ResumeWhenLive(version)); }
            }
            ApplyMicPolicy();
            if (Paused) Caption("Dee stopped. Not listening. Use Play to resume.\n" + (rundown != null && rundown.Running ? rundown.CurrentInstruction : "You can keep using the buttons."));
            else Caption(Settings.VoiceGuide ? "Resuming Dee. Buttons remain available." : "Voice guide is off in Settings. Buttons remain available.");
            RefreshNavArrows();
        }

        int resumeVersion;
        IEnumerator ResumeWhenLive(int version)
        {
            float until = Time.time + 30f;
            while (guide != null && guide.Mode != GuideMode.Live && guide.State != "offline" && Time.time < until && !Paused && version == resumeVersion) yield return null;
            if (guide == null || Paused || version != resumeVersion) yield break;
            ApplyMicPolicy();
            if (Flow.Phase == WelcomePhase.Lesson) PushLessonState();
            else guide.PushContext(GuideContextBuilder.Catalog());
            if (rundown != null && rundown.Running) { var line = rundown.CurrentInstruction; guide.SpeakScripted(line, CanSay); Caption(line); }
            else if (Flow.Phase == WelcomePhase.Lesson) { Say("Say one short sentence explaining the current instruction: " + CurrentInstruction()); Caption(CurrentInstruction()); }
            else Caption(guide.Mode != GuideMode.Live ? "Dee is unavailable. You can keep using the buttons." : guide.MicActive ? "Dee is listening. Use Stop to end the conversation." : "Dee can speak. Microphone is not active.");
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
            // The lounge is the home: it stays through consent, welcome and the board, and goes only for a lesson,
            // so the workbench sits in the learner's own space exactly as it does today.
            if (lounge != null)
            {
                lounge.Show(Airlift.Lounge.LoungeVisibility.ShouldShow(lesson, onboardingDone: Flow.Phase != WelcomePhase.Consent));
                lounge.ShowBoard(!lesson);
            }
            if (consentRoot) consentRoot.SetActive(Flow.Phase == WelcomePhase.Consent && !arriving);
            if (welcomeRoot) welcomeRoot.SetActive(Flow.Phase == WelcomePhase.Welcome);
            // The tour happens ON the wall, so the wall is up during the rundown phase too.
            if (catalogRoot) catalogRoot.SetActive(Flow.Phase == WelcomePhase.Catalog);
            if (Flow.Phase == WelcomePhase.Lesson) lastCanGrab = false;
            if (hudRoot)
            {
                hudRoot.SetActive(GuidePolicy.AssistantBarVisible(Flow.Phase, arriving));
                var target = lesson ? hudStationCanvas : hudWelcomeCanvas;
                if (target != null && hudRoot.transform.parent != target)
                {
                    hudRoot.transform.SetParent(target, false);
                    var r = hudRoot.GetComponent<RectTransform>(); if (r != null) { r.anchoredPosition = lesson ? Vector2.zero : hudWelcomePosition; r.localRotation = Quaternion.identity; r.localScale = Vector3.one; }
                }
                // Always drawn last on its canvas: no card may cover Mute, Pause, Help or the gear (owner 2026-09-18).
                hudRoot.transform.SetAsLastSibling();
                if (rundown != null && rundown.header != null && rundown.header.transform.parent == hudRoot.transform.parent) rundown.header.transform.SetAsLastSibling();
                if (rundown != null && rundown.pointerRoot != null && rundown.pointerRoot.parent == hudRoot.transform.parent) rundown.pointerRoot.SetAsLastSibling();   // ring over the gear, never a raycast target
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
            ApplyWelcomeRaySurface();
            ApplyFallbackButtons();
        }

        /// Off during a lesson, back while the settings card (which lives on that canvas) is open.
        void ApplyWelcomeRaySurface()
        {
            if (hudWelcomeCanvas == null) return;
            var ray = hudWelcomeCanvas.Find("ISDK_RayCanvasInteraction");
            if (ray != null) ray.gameObject.SetActive(GuidePolicy.WelcomeRaySurfaceOn(Flow.Phase, SettingsOpen));
        }

        void LateUpdate()
        {
            RefreshNavArrows();
            bool live = guide != null && guide.Mode == GuideMode.Live;
            bool speaking = live && !Muted && !Paused && playback != null && playback.IsSpeaking;
            bool listening = live && !Muted && !Paused && guide.MicStreaming;
            if (rundown != null) rundown.SetVoice(live && Flow.VoiceConsented && CanListen && guide.MicEnabled && Permission.HasUserAuthorizedPermission(Permission.Microphone) && !Muted && !Paused && Settings.VoiceGuide);
            if (stateText != null) stateText.text = Paused ? "Stopped" : Muted ? "Muted" : !live ? "Offline" : speaking ? "Speaking" : listening ? "Listening" : "Narration";
            if (orb == null) return;
            orb.color = !live || Muted || Paused ? new Color(0.3f, 0.37f, 0.48f, 0.5f) : speaking || listening ? new Color(0.2f, 0.8f, 1f) : new Color(0.25f, 0.55f, 0.9f, 0.7f);
            float pulse = speaking || listening ? 1f + 0.10f * Mathf.Sin(Time.time * (speaking ? 9f : 4f)) : 1f;
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
