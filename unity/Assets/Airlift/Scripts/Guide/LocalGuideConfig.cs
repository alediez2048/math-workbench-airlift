using UnityEngine;

namespace Airlift.Guide
{
    /// Per-machine guide settings that never go in git. Create the folder
    /// Assets/Airlift/Resources/LocalGuideConfig/ (gitignored) and add:
    ///   client-key.txt    the GUIDE_CLIENT_KEY the proxy expects, sent as x-nerdy-key
    ///   dev-mint-url.txt  optional local dev mint, e.g. http://192.0.2.10:8787/session
    /// A build without client-key.txt runs the guide offline. The dev mint URL is read only in the
    /// editor and in development builds, so a release build always uses GuideEndpoints.MintUrl.
    public static class LocalGuideConfig
    {
        public const string ClientKeyHeader = "x-nerdy-key";

        public static string ClientKey => Read("LocalGuideConfig/client-key");

        public static string DevMintUrl
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return Read("LocalGuideConfig/dev-mint-url");
#else
                return "";
#endif
            }
        }

        static string Read(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            return asset == null ? "" : asset.text.Trim();
        }
    }
}
