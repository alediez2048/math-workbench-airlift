using System.Reflection;
using Airlift.Guide;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    public class TourSpeechRegressionTests
    {
        [Test] public void IsolatedRequestHasNoConversationHistoryOrTools()
        {
            var request = Newtonsoft.Json.Linq.JObject.Parse(GuideMessages.ScriptedResponse("Open Settings.", "tour-one"));
            Assert.That((string)request.SelectToken("response.conversation"), Is.EqualTo("none"));
            Assert.That(request.SelectToken("response.input").HasValues, Is.False);
            Assert.That(request.SelectToken("response.tools").HasValues, Is.False);
            Assert.That((string)request.SelectToken("response.metadata.narration"), Is.EqualTo("tour-one"));
        }

        static void ScriptResponse(GuideSession guide, string token, string id, string spoken, string status = "completed")
        {
            Receive(guide, "{\"type\":\"response.created\",\"response\":{\"id\":\"" + id + "\",\"metadata\":{\"narration\":\"" + token + "\"}}}");
            Receive(guide, "{\"type\":\"response.output_audio.delta\",\"response_id\":\"" + id + "\",\"delta\":\"AAA=\"}");
            Receive(guide, "{\"type\":\"response.output_audio_transcript.done\",\"response_id\":\"" + id + "\",\"transcript\":\"" + spoken + "\"}");
            Receive(guide, "{\"type\":\"response.done\",\"response\":{\"id\":\"" + id + "\",\"status\":\"" + status + "\"}}");
        }

        [TestCase("Press the green Done button.", "completed", false)]
        [TestCase("Click the gear.", "completed", true)]
        [TestCase("Click the gear.", "cancelled", false)]
        public void ActualSessionOnlyReleasesCurrentVerbatimCompletedNarration(string spoken, string status, bool expected)
        {
            var root = new GameObject("scripted speech regression");
            try
            {
                var guide = root.AddComponent<GuideSession>();
                string token = null, heard = null;
                guide.OutgoingTap = json => { var o = Newtonsoft.Json.Linq.JObject.Parse(json); if ((string)o["type"] == "response.create") token = (string)o.SelectToken("response.metadata.narration"); };
                guide.GuideTranscriptDone += text => heard = text;
                guide.SpeakScripted("Click the gear.");
                ScriptResponse(guide, token, "current", spoken, status);
                Assert.That(heard != null, Is.EqualTo(expected));
                Assert.That(guide.NarrationStatus, Is.EqualTo(expected ? "verified" : "rejected"));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test] public void LateScriptAndCompetingConversationCannotReplaceNewStepEvenAfterExit()
        {
            var root = new GameObject("stale tour response");
            try
            {
                var guide = root.AddComponent<GuideSession>(); string token = null, heard = null;
                guide.OutgoingTap = json => { var o = Newtonsoft.Json.Linq.JObject.Parse(json); if ((string)o["type"] == "response.create") token = (string)o.SelectToken("response.metadata.narration"); };
                guide.GuideTranscriptDone += text => heard = text;
                guide.SpeakScripted("Click the gear."); var old = token;
                guide.SpeakScripted("Press Done."); var current = token;
                ScriptResponse(guide, old, "old-script", "Click the gear."); Assert.That(heard, Is.Null);
                Receive(guide, "{\"type\":\"response.created\",\"response\":{\"id\":\"competing\"}}");
                Receive(guide, "{\"type\":\"response.output_audio_transcript.done\",\"response_id\":\"competing\",\"transcript\":\"Choose a lesson.\"}");
                Assert.That(heard, Is.Null);
                ScriptResponse(guide, current, "current-script", "Press Done."); Assert.That(heard, Is.EqualTo("Press Done."));
                guide.EndScriptedNarration(); heard = null;
                ScriptResponse(guide, old, "late-script", "Click the gear."); Assert.That(heard, Is.Null);
                Receive(guide, "{\"type\":\"response.created\",\"response\":{\"id\":\"normal\"}}");
                Receive(guide, "{\"type\":\"response.output_audio_transcript.done\",\"response_id\":\"normal\",\"transcript\":\"Normal lesson guidance.\"}");
                Assert.That(heard, Is.EqualTo("Normal lesson guidance."));
            }
            finally { Object.DestroyImmediate(root); }
        }

        static void Receive(GuideSession guide, string json) => typeof(GuideSession)
            .GetMethod("Handle", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(guide, new object[] { GuideMessages.Parse(json) });

        [Test] public void HushRejectsLateTranscriptFromCancelledResponse()
        {
            var root = new GameObject("speech regression");
            try
            {
                var guide = root.AddComponent<GuideSession>();
                string heard = null;
                guide.GuideTranscriptDone += text => heard = text;
                Receive(guide, "{\"type\":\"response.created\",\"response\":{\"id\":\"old\"}}");
                guide.Hush();
                Receive(guide, "{\"type\":\"response.output_audio_transcript.done\",\"response_id\":\"old\",\"transcript\":\"Press the green Done button.\"}");
                Assert.That(heard, Is.Null, "Cancelled speech must not replace the current tour instruction.");
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
