namespace Airlift.Welcome
{
    /// Owner-approved opening of the story (2026-09-17): the guide is Dee, the AI assistant of Nerdy AI+VR.
    public static class GuideIntro
    {
        public const string Spoken = "Welcome to Nerdy AI plus VR! My name is Dee, and I will be your AI assistant throughout your elementary math journey. Tap your answers on the card to get started.";

        public const string SpokenSpanish = "¡Bienvenido a Nerdy AI plus VR! Me llamo Dee y seré tu asistente de IA durante todo tu viaje por las matemáticas de primaria. Toca tus respuestas en la tarjeta para empezar.";

        public static string GreetingPrompt => PromptFor(GuideLanguage.Default);

        public static string SpokenFor(string language) => GuideLanguage.Normalize(language) == "es" ? SpokenSpanish : Spoken;

        public static string PromptFor(string language) => "Say these exact words word for word, then stop: \"" + SpokenFor(language) + "\"";

        /// Shown when the learner chose no voice: the caption carries the same introduction.
        public const string OfflineCaption = "Welcome to Nerdy AI+VR! My name is Dee, your AI assistant for your elementary math journey. Tap the answers below, then choose a lesson.";
    }
}
