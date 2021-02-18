namespace Xil.Tests
{
    public partial class OperandTests
    {
        class TestRandom : IRandom
        {
            public const int NextValue = 456;

            public int Next() => NextValue;

            public void Seed(int value) { }
        }
    }
}