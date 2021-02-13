using System;

namespace Xil
{
    public partial class Interpreter : IInterpreter
    {
        [Builtin(
            "stack",
            ".. X Y Z -> .. X Y Z [Z Y X ..]",
            "Pushes the stack as a list.")]
        private void Stack_()
        {
            var xs = this.stack.ToArray();
            this.Push(new Value.List(xs));
        }

        [Builtin(
            "time",
            "-> I",
            "Pushes the current time (in seconds since the Epoch).")]
        private void Time_() =>
            this.Push(new Value.Int((int)this.time.GetUnixTimeSeconds()));

        [Builtin(
            "rand",
            "-> I",
            "I is a random integer.")]
        private void Rand_() =>
            this.Push(new Value.Int(this.random.Next()));

        private void Error_()
        {
            new Validator("error")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var x = this.Pop<Value.String>();
            throw new RuntimeException(x.Value);
        }
    }
}