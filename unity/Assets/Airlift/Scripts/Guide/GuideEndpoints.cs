namespace Airlift.Guide
{
    /// Where the app mints guide sessions. The deployed proxy owns the provider key; the app never holds it.
    /// Deployed 2026-09-17 to Vercel (project nerdy-guide-proxy); /session rewrites to the api/session function,
    /// so dev and production differ only by host. The Mac dev mint (http://192.168.86.20:8787/session) stays as
    /// the offline fallback while insecureHttpOption is still AlwaysAllowed; bake it with AgentScripts/SetMintUrl.cs.
    public static class GuideEndpoints
    {
        public const string MintUrl = "https://nerdy-guide-proxy.vercel.app/session";
        public const string DevMintUrl = "http://192.168.86.20:8787/session";
    }
}
