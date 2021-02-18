namespace Xil.Tests
{
    using Xunit.Abstractions;

    public abstract class InterpreterTests
    {
        private readonly TestOutputPrinter printer;

        protected readonly IInterpreter i;

        protected virtual ITime CreateTime() =>
            new SystemTime();

        protected virtual IRandom CreateRandom() =>
            new SystemRandom();

        protected InterpreterTests(ITestOutputHelper output)
        {
            this.printer = new TestOutputPrinter(output);
            this.i = Interpreter.Create(
                this.CreateTime(),
                this.CreateRandom(),
                this.printer);
        }
    }
}