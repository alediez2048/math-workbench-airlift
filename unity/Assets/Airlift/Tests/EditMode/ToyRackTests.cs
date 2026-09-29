using Airlift.Lounge;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    /// Owner 2026-09-29: toys stay where they are let go. The rack moves to whichever windowless wall is beside the
    /// board at each launch, so a remembered place is measured from the rack, not the room, and a toy put back on its
    /// slot is simply "home" again.
    public class ToyRackTests
    {
        static (ToyRack rack, ToyRack.Slot slot, Transform toy) Build()
        {
            var rackGo = new GameObject("rack"); var rack = rackGo.AddComponent<ToyRack>();
            var home = new GameObject("home").transform; home.SetParent(rackGo.transform, false); home.localPosition = new Vector3(-0.92f, 1.7f, 0f);
            var toy = new GameObject("toy"); toy.transform.SetParent(rackGo.transform, false); toy.transform.position = home.position;
            var place = toy.AddComponent<ToyPlace>(); place.home = home; place.homeScale = 1f;
            var slot = new ToyRack.Slot { id = "cargo_crew_fractions#1", anchor = home, toy = toy, place = place };
            rack.slots = new[] { slot };
            return (rack, slot, toy.transform);
        }

        [Test] public void ARememberedPlaceFollowsTheRackToWhicheverWallItIsOn()
        {
            var (rack, _, toy) = Build();
            var lib = new LibraryState(); lib.RecordCompleted("cargo_crew_fractions", 1);
            lib.RecordToyPose("cargo_crew_fractions#1", new Vector3(0.5f, 0.2f, 0.3f), Quaternion.identity, 1f);
            rack.transform.SetPositionAndRotation(new Vector3(0f, 0f, 2.7f), Quaternion.Euler(0f, 180f, 0f));
            rack.Refresh(lib);
            var expected = rack.transform.TransformPoint(new Vector3(0.5f, 0.2f, 0.3f));
            Assert.That(Vector3.Distance(toy.position, expected), Is.LessThan(0.001f), "half a metre along the rack, wherever the rack stands; got " + toy.position);
            Object.DestroyImmediate(rack.gameObject);
        }
        [Test] public void LettingGoElsewhereRemembersThePlaceMeasuredFromTheRack()
        {
            var (rack, slot, toy) = Build(); rack.Wire();
            var lib = new LibraryState(); lib.RecordCompleted("cargo_crew_fractions", 1);
            rack.transform.SetPositionAndRotation(new Vector3(0f, 0f, 2.7f), Quaternion.Euler(0f, 180f, 0f));
            rack.Refresh(lib);
            toy.position = rack.transform.TransformPoint(new Vector3(0.5f, 0.2f, 0.3f));
            slot.place.Released(slot.place);
            var pose = lib.ToyPose("cargo_crew_fractions#1");
            Assert.That(pose, Is.Not.Null);
            Assert.That(Vector3.Distance(pose.Value.position, new Vector3(0.5f, 0.2f, 0.3f)), Is.LessThan(0.001f), "got " + pose.Value.position);
            Object.DestroyImmediate(rack.gameObject);
        }

        [Test] public void LettingGoOnItsSlotForgetsTheRememberedPlace()
        {
            var (rack, slot, toy) = Build(); rack.Wire();
            var lib = new LibraryState(); lib.RecordCompleted("cargo_crew_fractions", 1);
            lib.RecordToyPose("cargo_crew_fractions#1", new Vector3(0.5f, 0.2f, 0.3f), Quaternion.identity, 1f);
            rack.Refresh(lib);
            toy.position = slot.anchor.position + new Vector3(0.02f, 0f, 0f);
            slot.place.Released(slot.place);
            Assert.That(lib.ToyPose("cargo_crew_fractions#1"), Is.Null, "back on the rack: nothing to remember");
            Assert.That(Vector3.Distance(toy.position, slot.anchor.position), Is.LessThan(0.001f), "and it snaps exactly onto the slot");
            Object.DestroyImmediate(rack.gameObject);
        }
    }
}
