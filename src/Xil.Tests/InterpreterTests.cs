namespace Xil.Tests
{
    using Xunit.Abstractions;

    public abstract class InterpreterTests
    {
        private readonly ITestOutputHelper output;

        protected readonly IInterpreter i;

        protected virtual ITime CreateTime() =>
            new SystemTime();

        protected virtual IRandom CreateRandom() =>
            new SystemRandom();

        protected InterpreterTests(ITestOutputHelper output)
        {
            this.output = output;
            this.i = Interpreter.Create(
                this.CreateTime(),
                this.CreateRandom(),
                this.Print);
        }

        protected void Print(int stackSize, string s) =>
            this.output.WriteLine($"[{stackSize}] > {s}");
    }
}