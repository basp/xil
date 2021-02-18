namespace Xil.Tests
{
    using System;
    using Xunit.Abstractions;

    internal class TestOutputPrinter : IPrinter
    {
        private readonly ITestOutputHelper testOutputHelper;

        public TestOutputPrinter(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public void Error(Exception ex)
        {
            this.testOutputHelper.WriteLine(ex.ToString());
        }

        public void Info(string message)
        {
            this.testOutputHelper.WriteLine(message);
        }

        public void Print(int stackSize, IValue value)
        {
            this.testOutputHelper.WriteLine($"[{stackSize}] > {value}");
        }

        public void Print(int stackSize, string s)
        {
            this.testOutputHelper.WriteLine($"[{stackSize}] > {s}");
        }
    }
}