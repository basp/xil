namespace Xil.Tests
{
    using System;
    using Xunit;

    public class InterpreterTests
    {
        private readonly IInterpreter i;

        public InterpreterTests()
        {
            Action<int, string> print = (i, s) => { };
            this.i = Interpreter.Create(print);
        }

        [Fact]
        public void IntIntAddition()
        {
            i.Exec(new Value.Int(2));
            i.Exec(new Value.Int(3));
            i.Exec(new Value.Symbol("+"));
            var result = i.Pop<Value.Int>();

            Assert.Equal(5, result.Value);
        }

        [Fact]
        public void IntFloatAddition()
        {
            i.Exec(new Value.Int(2));
            i.Exec(new Value.Float(3.12345));
            i.Exec(new Value.Symbol("+"));
            var result = i.Pop<Value.Float>();

            Assert.Equal(5.12345, result.Value);
        }

        [Fact]
        public void FloatIntAddition()
        {
            i.Exec(new Value.Float(3.12345));
            i.Exec(new Value.Int(2));
            i.Exec(new Value.Symbol("+"));
            var result = i.Pop<Value.Float>();

            Assert.Equal(5.12345, result.Value);
        }

        [Fact]
        public void FloatFloatAddition()
        {
            i.Exec(new Value.Float(1.25));
            i.Exec(new Value.Float(1.25));
            i.Exec(new Value.Symbol("+"));
            var result = i.Pop<Value.Float>();

            Assert.Equal(2.5, result.Value);
        }
    }
}