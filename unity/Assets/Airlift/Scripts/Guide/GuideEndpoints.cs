namespace Airlift.Guide
{
    /// Where the app mints guide sessions. Dev: the Mac on the home LAN (cleartext, dev builds only).
    /// Production: the deployed proxy (https). Updated by editor script when the proxy is deployed.
    public static class GuideEndpoints
    {
        public const string MintUrl = "http://192.168.86.20:8787/session";
    }
}
