using TMPro;
using UnityEngine;
namespace Airlift.Presentation
{
    public sealed class FractionNotationView : MonoBehaviour
    {
        public TMP_Text numerator;
        public TMP_Text denominator;
        public GameObject fractionBar;
        public void Show(int top, int bottom)
        {
            if(top<0 || bottom<=0)throw new System.ArgumentOutOfRangeException("Fraction specimen must be nonnegative with positive denominator.");
            numerator.text=top.ToString(); denominator.text=bottom.ToString();
            denominator.gameObject.SetActive(true);fractionBar.SetActive(true);
        }
        public void ShowWhole(int value)
        {
            if(value<0)throw new System.ArgumentOutOfRangeException(nameof(value));
            numerator.text=value.ToString();denominator.gameObject.SetActive(false);fractionBar.SetActive(false);
        }
    }
}
