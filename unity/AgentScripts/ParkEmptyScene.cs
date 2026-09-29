using UnityEditor.SceneManagement;
// Opens an empty scene so a branch switch never meets a loaded scene ("changed on disk" dialog).
public static class ParkEmptyScene
{
    public static string Run() { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); return "parked on an empty scene"; }
}
