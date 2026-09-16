using System;

namespace Airlift.Math
{
    /// <summary>Exact semantic quantity; default(FractionValue) is zero.</summary>
    public readonly struct FractionValue : IEquatable<FractionValue>, IComparable<FractionValue>
    {
        readonly int numerator;
        readonly int denominator;
        public int Numerator => numerator;
        public int Denominator => denominator == 0 ? 1 : denominator;

        public FractionValue(int numerator, int denominator)
        {
            if (denominator <= 0) throw new ArgumentOutOfRangeException(nameof(denominator));
            long a = System.Math.Abs((long)numerator), b = denominator;
            while (b != 0) { long remainder = a % b; a = b; b = remainder; }
            this.numerator = (int)(numerator / a);
            this.denominator = (int)(denominator / a);
        }

        public int CompareTo(FractionValue other) =>
            ((long)Numerator * other.Denominator).CompareTo((long)other.Numerator * Denominator);
        public bool Equals(FractionValue other) => CompareTo(other) == 0;
        public override bool Equals(object value) => value is FractionValue fraction && Equals(fraction);
        public override int GetHashCode() => unchecked(Numerator * 397 ^ Denominator);
        public override string ToString() => Numerator + "/" + Denominator;
    }
}
