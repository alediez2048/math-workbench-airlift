namespace Airlift.Welcome
{
    /// Voice languages the learner can pick on the consent card (owner 2026-09-17). The session is minted in that language
    /// and the server keeps Dee in it for the whole session. Unknown codes mean English.
    public static class GuideLanguage
    {
        public const string Default = "en";
        public const string PrefsKey = "nerdy.language";
        public static readonly string[] Codes = { "en", "es" };

        public static string Normalize(string code) => System.Array.IndexOf(Codes, code) >= 0 ? code : Default;
        /// Owner 2026-09-28: "it needs to start in English always". The saved choice is ignored at launch; the pill
        /// still switches Dee within the session.
        public static string AtLaunch(string saved) => Default;

        /// Owner 2026-09-28: lesson and tour lines are authored in English and sent as "word for word"; in another
        /// language that verbatim instruction beat the session's language rule and Dee read the English out. Every
        /// prompt passes through here: in Spanish the verbatim request becomes a faithful-rendering request.
        public static string Localize(string prompt, string language)
        {
            if (string.IsNullOrEmpty(prompt) || Normalize(language) == Default) return prompt;
            string name = Label(language) == "Español" ? "Spanish" : Label(language);
            string p = prompt
                .Replace("Say these exact words word for word, then stop:", "Say the following in " + name + ", rendered faithfully, then stop:")
                .Replace("Say this word for word, then stop:", "Say the following in " + name + ", rendered faithfully, then stop:")
                .Replace("word for word", "in " + name + ", rendered faithfully");
            return p + " Speak only " + name + "; never read the English text aloud.";
        }

        public static string Label(string code) => Normalize(code) == "es" ? "Español" : "English";
    }
}
