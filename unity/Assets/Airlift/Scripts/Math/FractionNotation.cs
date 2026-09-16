namespace Airlift.Math
{
    /// <summary>Authored notation, deliberately not reduced (six eighths stays 6/8).</summary>
    public readonly struct FractionNotation
    {
        public int Numerator { get; }
        public int Denominator { get; }
        public FractionValue Value => new FractionValue(Numerator, Denominator);

        public FractionNotation(int numerator, int denominator)
        {
            _ = new FractionValue(numerator, denominator);
            Numerator = numerator;
            Denominator = denominator;
        }

        public override string ToString() => Numerator + "/" + Denominator;
    }
}
