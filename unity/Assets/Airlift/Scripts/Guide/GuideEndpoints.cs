namespace Airlift.Guide
{
    /// Where the app mints guide sessions. The deployed proxy owns the provider key; the app never holds it.
    /// The proxy is the Vercel project nerdy-guide-proxy; /session rewrites to the api/session function.
    /// A local dev mint (services/guide-proxy/dev-mint.mjs) is configured per machine through
    /// LocalGuideConfig, never here, and only editor and development builds can use it.
    public static class GuideEndpoints
    {
        public const string MintUrl = "https://nerdy-guide-proxy.vercel.app/session";
    }
}
