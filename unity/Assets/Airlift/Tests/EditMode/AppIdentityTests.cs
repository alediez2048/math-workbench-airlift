using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace Airlift.Tests
{
    public class AppIdentityTests
    {
        [Test] public void AppIsNamedNerdyWithItsOwnPackage()
        {
            Assert.That(PlayerSettings.productName, Is.EqualTo("Nerdy"));
            Assert.That(PlayerSettings.companyName, Is.EqualTo("Nerdy"));
            Assert.That(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android), Is.EqualTo("com.nerdy.vr"));
            Assert.That(PlayerSettings.bundleVersion, Is.EqualTo("0.2.0"));
        }

        [Test] public void LauncherIconsUseTheNerdyLogo()
        {
            var adaptive = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive);
            Assert.That(adaptive.Length, Is.GreaterThan(0));
            foreach (var icon in adaptive)
            {
                var layers = icon.GetTextures();
                Assert.That(layers.Length, Is.EqualTo(2), "adaptive icon has background and foreground");
                Assert.That(layers.All(t => t != null), Is.True, "every adaptive layer assigned");
                Assert.That(layers[1].name, Does.Contain("icon-adaptive-fg"));
            }
            Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Airlift/Branding/nerdy-logo-green.png"), Is.Not.Null);
        }
    }
}
