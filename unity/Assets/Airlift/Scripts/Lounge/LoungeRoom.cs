using System;
using System.Collections.Generic;
using UnityEngine;

namespace Airlift.Lounge
{
    /// Where the learner is when they are not in a lesson. CC-FD-01.
    public enum Scenery { YourRoom, NerdyLounge }

    /// What each scenery actually shows. The furniture and the glows belong to Nerdy either way; only the room
    /// shell is the difference, so AR is the same lounge with the walls taken away rather than a separate design.
    public readonly struct SceneryState
    {
        public readonly bool ShellVisible, FurnitureVisible, PassthroughOn, SkyVisible, FogOn;
        public SceneryState(bool shell, bool furniture, bool passthrough, bool sky, bool fog)
        { ShellVisible = shell; FurnitureVisible = furniture; PassthroughOn = passthrough; SkyVisible = sky; FogOn = fog; }
    }

    public static class LoungeScenery
    {
        /// Passthrough until the learner asks for the room: their own space is the safer place to start, and the
        /// lessons are built for it.
        public const Scenery Default = Scenery.YourRoom;
        /// Owner 2026-09-18: "lessons only show in AR". Whatever scenery the learner chose on the wall, a lesson runs
        /// over passthrough with no sky or fog; the choice returns with the wall.
        public static SceneryState Lesson => For(Scenery.YourRoom);

        public static SceneryState For(Scenery scenery) => scenery == Scenery.NerdyLounge
            ? new SceneryState(shell: true, furniture: true, passthrough: false, sky: true, fog: true)
            : new SceneryState(shell: false, furniture: true, passthrough: true, sky: false, fog: false);
    }

    /// The lounge is the home: arrival, onboarding and browsing all happen in it. It hides for exactly one
    /// reason, so nobody has to guess why the room vanished.
    public static class LoungeVisibility
    {
        public static bool ShouldShow(bool lessonRunning, bool onboardingDone) => !lessonRunning;
    }

    public enum StartupStep { Assets, Guide, Microphone }

    /// The arrival bar reports these three and nothing else. If they are already done there is no bar: a progress
    /// bar over finished work is a lie, however pretty.
    public sealed class StartupWork
    {
        static readonly StartupStep[] All = { StartupStep.Assets, StartupStep.Guide, StartupStep.Microphone };
        readonly HashSet<StartupStep> done = new HashSet<StartupStep>();

        public void Complete(StartupStep step) => done.Add(step);
        public bool IsDone(StartupStep step) => done.Contains(step);
        public float Progress => (float)done.Count / All.Length;
        public bool Ready => done.Count == All.Length;
        public bool ShouldShowBar => !Ready;
    }

    /// The lounge root in the scene: the shell, the furniture, the glows and Dee's seat. Built by
    /// AgentScripts/BuildLounge.cs; the director shows and hides it.
    public sealed class LoungeRoom : MonoBehaviour
    {
        [Tooltip("Walls, floor and ceiling — everything that would cover passthrough. Off in Your room.")]
        public GameObject shell;
        [Tooltip("Seating, table, the wall the board hangs on, the glows and Dee's seat. On in both sceneries.")]
        public GameObject furniture;
        [Tooltip("The passthrough layer the rig already uses; left alone when absent.")]
        public OVRPassthroughLayer passthrough;
        [Tooltip("The gradient sky the room's windows look out on. Never shown over passthrough.")]
        public Material sky;
        [Tooltip("The logo arrival that plays before the board appears.")]
        public LoungeArrival arrival;
        [Tooltip("The board the onboarding panel is mounted on: one surface, not a whiteboard plus a card. It rides with the panel, so the handle still moves both.")]
        public GameObject board;
        [Tooltip("Fog colour and density for the room; cleared in Your room so passthrough stays clean.")]
        public Color fogColour = new Color(0.126f, 0.137f, 0.267f);
        public float fogDensity = 0.055f;

        public Scenery Mode { get; private set; } = LoungeScenery.Default;
        public StartupWork Startup { get; } = new StartupWork();
        public float StartupProgress => Startup.Progress;
        public bool Ready => Startup.Ready;

        public event Action<Scenery> SceneryChanged;

        void Awake() => Apply(Mode);

        /// Wired to the two scenery pills, and to the selector beside the gear on the board.
        public void Apply(Scenery scenery)
        {
            Mode = scenery;
            ApplyState(LoungeScenery.For(scenery));
            SceneryChanged?.Invoke(scenery);
        }

        void ApplyState(SceneryState state)
        {
            if (shell != null) shell.SetActive(state.ShellVisible);
            if (furniture != null) furniture.SetActive(state.FurnitureVisible);
            if (passthrough != null) passthrough.enabled = state.PassthroughOn;

            // A skybox or fog over passthrough would paint out the learner's own room, so both follow the scenery.
            RenderSettings.skybox = state.SkyVisible ? sky : null;
            RenderSettings.fog = state.FogOn;
            if (state.FogOn)
            {
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = fogColour;
                RenderSettings.fogDensity = fogDensity;
            }
            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = state.SkyVisible ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                if (!state.SkyVisible) cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            }
        }

        /// UnityEvent-friendly overloads for the scenery pills in the scene.
        public void ChooseYourRoom() => Apply(Scenery.YourRoom);
        public void ChooseNerdyLounge() => Apply(Scenery.NerdyLounge);

        /// The board rides with the welcome panel, so it is hidden by phase rather than by scenery.
        public void ShowBoard(bool on) { if (board != null && board.activeSelf != on) board.SetActive(on); }

        /// The whole root, off only while a lesson runs. The passthrough layer, sky and fog live outside this root, so
        /// hiding it is not enough: the lesson state is applied explicitly and the chosen scenery comes back with Show(true).
        public void Show(bool on)
        {
            if (gameObject.activeSelf != on) gameObject.SetActive(on);
            if (on) Apply(Mode); else ApplyState(LoungeScenery.Lesson);
        }
    }
}
