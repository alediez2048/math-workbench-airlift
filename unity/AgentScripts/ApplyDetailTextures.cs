using UnityEditor;
using UnityEngine;

// CC-HF-01 (optional). Subtle 512 px detail maps multiply over the existing flat colours; _BaseColor is untouched.
// Idempotent. Run: unity command run_script --file AgentScripts/ApplyDetailTextures.cs --entry ApplyDetailTextures.Run
public static class ApplyDetailTextures
{
    static readonly (string mat, string map, float tiling)[] Details =
    {
        ("Assets/Airlift/Materials/GardenSoil.mat", "soil", 2f), ("Assets/Airlift/Materials/GardenPeat.mat", "soil", 2f),
        ("Assets/Airlift/Materials/GardenWood.mat", "wood", 1f), ("Assets/Airlift/Materials/Lounge/LoungeWood.mat", "wood", 1f),
        ("Assets/Airlift/Materials/Lounge/LoungeRug.mat", "rug", 3f), ("Assets/Airlift/Materials/Lounge/LoungeRugInner.mat", "rug", 3f),
        ("Assets/Airlift/Materials/Lounge/LoungeCouch.mat", "fabric", 2f),
    };

    public static string Run()
    {
        int n = 0;
        foreach (var (matPath, map, tiling) in Details)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Airlift/Art/Generated/Detail/" + map + ".png");
            if (mat == null || tex == null) continue;
            mat.SetTexture("_BaseMap", tex); mat.SetTextureScale("_BaseMap", new Vector2(tiling, tiling));
            EditorUtility.SetDirty(mat); n++;
        }
        AssetDatabase.SaveAssets();
        return n + " of " + Details.Length + " materials carry a detail map";
    }
}
