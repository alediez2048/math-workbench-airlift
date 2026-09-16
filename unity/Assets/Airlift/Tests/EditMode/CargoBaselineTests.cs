using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;

namespace Airlift.Tests
{
    public class CargoBaselineTests
    {
        [Test] public void ExplicitSceneBuild()
        {
            // Reflection avoids adding a reference from tests to predefined Assembly-CSharp-Editor.
            var type = System.AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Airlift.Editor.CargoBuild")).FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null);
            var scenes = (string[])type.GetMethod("Scenes").Invoke(null, null);
            Assert.That(scenes, Is.EqualTo(new[] { "Assets/Airlift/Scenes/CargoCrew.unity" }));
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[0]), Is.Not.Null);
        }

        [TestCase("DeviceProof", "7cdba1acb1c3809a4cc83d83218ff35e0bb2c9947f4a6f363a7ba7f364cd39bd")]
        [TestCase("Onboarding", "a5137f2036f7b646a19a84c2c12db829123a85e24659ded7ce5ed2c89112c355")]
        public void ProofSceneUnchanged(string name, string expected)
        {
            using (var hash = SHA256.Create())
                Assert.That(System.BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(
                    "Assets/Airlift/Scenes/" + name + ".unity"))).Replace("-", "").ToLowerInvariant(), Is.EqualTo(expected));
        }
    }
}
