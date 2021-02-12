namespace Xil
{
    public partial class Interpreter : IInterpreter
    {
        [Builtin(
            "rand",
            "-> I",
            "I is a random integer.")]
        private void Rand_() =>
            this.Push(new Value.Float(Rng.NextDouble()));
    }
}