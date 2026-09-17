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

        public static string Label(string code) => Normalize(code) == "es" ? "Español" : "English";
    }
}
