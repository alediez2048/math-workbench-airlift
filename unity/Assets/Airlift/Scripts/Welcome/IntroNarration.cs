using Airlift.Lessons;
using Newtonsoft.Json.Linq;

namespace Airlift.Welcome
{
    /// When and how Dee says a concept-intro step. A step reached by a button is said once from NerdyDirector.Update;
    /// a step reached through a voice tool is marked observed and said from the tool result's say_exactly instead.
    public static class IntroNarration
    {
        /// Same wording as GuideIntro.PromptFor.
        public static string PromptFor(IntroStep step) => "Say these exact words word for word, then stop: \"" + step.Say + "\"";

        /// True when the card shows an intro step the guide has not said or been told about yet.
        public static bool ShouldNarrate(string observedId, IntroStep current) => current != null && current.Id != observedId;

        /// The tool result's extra fields plus say_exactly while an intro step shows; extra unchanged otherwise.
        /// The intro line travels once, in say_exactly; an identical reason is emptied so Dee never says it twice.
        public static string ReasonFor(string reason, IntroStep current) => current != null && reason == current.Say ? "" : reason;

        public static JObject WithSayExactly(JObject extra, IntroStep current)
        {
            if (current == null) return extra;
            var merged = extra != null ? (JObject)extra.DeepClone() : new JObject();
            merged["say_exactly"] = current.Say;
            return merged;
        }
    }
}
