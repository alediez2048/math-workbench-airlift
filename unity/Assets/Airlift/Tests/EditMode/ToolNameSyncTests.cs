using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Airlift.Lessons;
using Airlift.Welcome;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    /// CC-PL-04: the Unity app and the proxy's server-authored session agree on every tool name. A name the guide can
    /// call that the app does not route (or the reverse) fails here instead of on the headset.
    public class ToolNameSyncTests
    {
        static string ConfigPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../../services/guide-proxy/lib/sessionConfig.js"));

        /// Tool names declared in the proxy's TOOLS array, read as text.
        static string[] ProxyToolNames()
        {
            Assert.That(File.Exists(ConfigPath), Is.True, ConfigPath);
            string js = File.ReadAllText(ConfigPath);
            int start = js.IndexOf("export const TOOLS", StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), "TOOLS array in sessionConfig.js");
            int end = js.IndexOf("export function", start, StringComparison.Ordinal);
            string tools = end > start ? js.Substring(start, end - start) : js.Substring(start);
            return Regex.Matches(tools, @"type:\s*'function',\s*name:\s*'([a-z_]+)'").Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
        }

        /// Station tool names from the lesson station types that exist in this build, read from a throwaway instance.
        static string[] StationToolNames(string typeName)
        {
            var type = Type.GetType(typeName + ", Airlift.Runtime");
            if (type == null) return null;
            var go = new GameObject("tool names (test)");
            try { return ((LessonStation)go.AddComponent(type)).ToolNames; }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        static string[] AppToolNames() => LessonToolRouter.WelcomeTools.Concat(LessonToolRouter.SharedTools)
            .Concat(LessonToolRouter.CargoTools).Concat(LessonToolRouter.CafeTools).Concat(LessonToolRouter.GardenTools).ToArray();

        [Test] public void EveryAppToolIsInTheProxyAndTheReverse()
        {
            var proxy = ProxyToolNames();
            Assert.That(proxy.Length, Is.GreaterThan(10), "parsed the proxy tools");
            Assert.That(proxy.Distinct().Count(), Is.EqualTo(proxy.Length), "proxy tool names are unique");
            var app = AppToolNames();
            Assert.That(app.Except(proxy).ToArray(), Is.Empty, "app tools missing from sessionConfig.js");
            Assert.That(proxy.Except(app).ToArray(), Is.Empty, "proxy tools the app does not route");
        }

        [Test] public void StationsOwnExactlyTheirContractTools()
        {
            Assert.That(StationToolNames("Airlift.Lessons.CargoStation"), Is.EquivalentTo(LessonToolRouter.CargoTools));
            Assert.That(GuideTools.LessonToolNames.Except(LessonToolRouter.SharedTools).Except(LessonToolRouter.CargoTools).ToArray(), Is.Empty,
                "the Dock 7 tool list is shared plus Cargo tools");
            var cafe = StationToolNames("Airlift.Lessons.Cafe.CafeStation");
            if (cafe != null) Assert.That(cafe, Is.EquivalentTo(LessonToolRouter.CafeTools), "CafeStation.ToolNames");
            var garden = StationToolNames("Airlift.Lessons.Garden.GardenStation");
            if (garden != null) Assert.That(garden, Is.EquivalentTo(LessonToolRouter.GardenTools), "GardenStation.ToolNames");
        }
    }
}
