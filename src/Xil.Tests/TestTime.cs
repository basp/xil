namespace Xil.Tests
{
    public partial class OperandTests
    {
        class TestTime : ITime
        {
            public const int UnitTimeSeconds = 123;

            public long GetUnixTimeSeconds() => UnitTimeSeconds;
        }
    }
}