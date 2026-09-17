using Airlift.Guide;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class GuideMessagesTests
    {
        [Test] public void ParsesGaAudioTranscriptAndToolEvents()
        {
            Assert.That(GuideMessages.Parse("{\"type\":\"response.output_audio.delta\",\"delta\":\"AAAA\"}").Delta, Is.EqualTo("AAAA"));
            var t = GuideMessages.Parse("{\"type\":\"response.output_audio_transcript.done\",\"transcript\":\"Hello \\\"you\\\"\"}");
            Assert.That(t.Transcript, Is.EqualTo("Hello \"you\""));
            var u = GuideMessages.Parse("{\"type\":\"conversation.item.input_audio_transcription.completed\",\"transcript\":\"I want the nerdy lesson\"}");
            Assert.That(u.Transcript, Is.EqualTo("I want the nerdy lesson"));
            var tool = GuideMessages.Parse("{\"type\":\"response.function_call_arguments.done\",\"name\":\"record_profile\",\"call_id\":\"call_1\",\"arguments\":\"{\\\"ageBand\\\":\\\"adult\\\"}\"}");
            Assert.That(tool.ToolName, Is.EqualTo("record_profile")); Assert.That(tool.CallId, Is.EqualTo("call_1"));
            Assert.That((string)JObject.Parse(tool.ArgumentsJson)["ageBand"], Is.EqualTo("adult"));
            Assert.That(GuideMessages.Parse("{\"type\":\"response.done\",\"response\":{\"status\":\"completed\"}}").ResponseStatus, Is.EqualTo("completed"));
            Assert.That(GuideMessages.Parse("{\"type\":\"error\",\"error\":{\"message\":\"nope\"}}").ErrorMessage, Is.EqualTo("nope"));
            Assert.That(GuideMessages.Parse("not json").Type, Is.EqualTo("malformed"));
        }

        [Test] public void BuildsWireMessagesTheProxyExpects()
        {
            var o = JObject.Parse(GuideMessages.ToolOutput("call_9", "{\"ok\":true}"));
            Assert.That((string)o["type"], Is.EqualTo("conversation.item.create"));
            Assert.That((string)o["item"]["type"], Is.EqualTo("function_call_output"));
            Assert.That((string)o["item"]["call_id"], Is.EqualTo("call_9"));
            var ctx = JObject.Parse(GuideMessages.SystemContext("stage=halves verdict=accepted"));
            Assert.That((string)ctx["item"]["role"], Is.EqualTo("system"));
            Assert.That((string)ctx["item"]["content"][0]["text"], Does.Contain("verdict=accepted"));
            var r = JObject.Parse(GuideMessages.ResponseCreate("Greet briefly."));
            Assert.That((string)r["response"]["instructions"], Is.EqualTo("Greet briefly."));
            Assert.That(JObject.Parse(GuideMessages.ResponseCreate())["response"], Is.Null);
            var a = JObject.Parse(GuideMessages.AudioAppend(new byte[] { 1, 2, 3, 4 }, 4));
            Assert.That((string)a["type"], Is.EqualTo("input_audio_buffer.append")); Assert.That((string)a["audio"], Is.EqualTo("AQIDBA=="));
        }

        [Test] public void Pcm16RoundTripsWithinQuantization()
        {
            var samples = new[] { 0f, 0.5f, -0.5f, 0.999f, -1f };
            var bytes = GuideMessages.ToPcm16(samples, samples.Length);
            var back = new float[8]; int n = GuideMessages.FromPcm16(bytes, back, 0);
            Assert.That(n, Is.EqualTo(5));
            for (int i = 0; i < samples.Length; i++) Assert.That(back[i], Is.EqualTo(samples[i]).Within(1f / 16384f));
        }

        [Test] public void RoutingFallsBackWhenNotMintedNotOpenOrGateUnsatisfied()
        {
            Assert.That(GuideRouting.Decide(true, true, true, true), Is.EqualTo(GuideMode.Live));
            Assert.That(GuideRouting.Decide(false, true, true, true), Is.EqualTo(GuideMode.Live), "no mic still allows spoken output with chips");
            Assert.That(GuideRouting.Decide(true, false, false, true), Is.EqualTo(GuideMode.Offline));
            Assert.That(GuideRouting.Decide(true, true, false, true), Is.EqualTo(GuideMode.Offline));
            Assert.That(GuideRouting.Decide(true, true, true, false), Is.EqualTo(GuideMode.Offline), "adult-only gate");
            Assert.That(GuideRouting.ShouldReconnect(0, true), Is.True);
            Assert.That(GuideRouting.ShouldReconnect(1, true), Is.False);
            Assert.That(GuideRouting.ShouldReconnect(0, false), Is.False);
        }
    }
}
