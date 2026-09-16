using UnityEngine;

namespace Airlift.Lessons
{
    [CreateAssetMenu(menuName = "Airlift/Half Lesson Content")]
    public sealed class HalfLessonContent : ScriptableObject
    {
        [TextArea] public string briefing = "You are helping prepare an aircraft delivery. The cargo team needs straps cut to the right length. Start by measuring one whole strap.";
        [TextArea] public string grabPractice = "Bring either controller to the strap. Hold the grip button under your middle finger, lift the strap, then release it.";
        [TextArea] public string wholeTask = "This ruler shows one whole strap, from 0 to 1. Place the whole strap along it, then choose Submit.";
        [TextArea] public string partition = "The next cargo package needs half this length. Choose Split into two equal parts. Both halves together still make one whole.";
        [TextArea] public string halfTask = "Build one-half of the same whole on the ruler. You can move pieces and change your mind. Choose Submit when ready.";
        [TextArea] public string repair = "Compare the length you placed with the whole ruler. One-half is one of two equal parts. Adjust your strap and submit again.";
        [TextArea] public string completion = "Your strap reaches one-half of the whole length. Two equal halves would cover the whole ruler. This is the length the cargo team requested.";
        [Min(0.1f)] public float wholeLengthMeters = 0.4f;
        [Min(0.02f)] public float pieceWidthMeters = 0.065f;
        [Min(0.02f)] public float pieceHeightMeters = 0.04f;
    }
}
