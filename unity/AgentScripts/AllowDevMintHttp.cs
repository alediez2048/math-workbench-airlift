using UnityEditor;
// TEMPORARY (Phase 0 dev): the guide mints sessions from the Mac over LAN HTTP until the https
// proxy is deployed. Revert to NotAllowed in the same change that switches GuideEndpoints to https.
public static class AllowDevMintHttp
{
    public static string Run() { PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed; AssetDatabase.SaveAssets(); return "insecureHttpOption=AlwaysAllowed (dev only)"; }
}
