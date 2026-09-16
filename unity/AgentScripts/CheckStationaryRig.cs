using System;
using System.Linq;
using UnityEngine;
public static class CheckStationaryRig
{
    public static string Run()
    {
        var active=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None)
            .Where(b=>b.GetType().Name=="FirstPersonLocomotor" && b.enabled && b.gameObject.activeInHierarchy).ToArray();
        if(active.Length>0)throw new InvalidOperationException("Stationary MR contains active gravity locomotor: "+string.Join(",",active.Select(x=>x.name)));
        return "PASS: no active FirstPersonLocomotor in stationary scene.";
    }
}
