namespace Xil.Tests
{
    using System;
    using Xunit;
    using Xunit.Abstractions;

    public partial class OperandTests : InterpreterTests
    {
        public OperandTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void PushFalse()
        {
            i.Exec(new Value.Symbol("false"));
            var result = i.Pop<Value.Bool>();

            Assert.False(result.Value);
        }

        [Fact]
        public void PushTrue()
        {
            i.Exec(new Value.Symbol("true"));
            var result = i.Pop<Value.Bool>();

            Assert.True(result.Value);
        }

        [Fact]
        public void PushStack()
        {
            i.Exec(new Value.Int(0));
            i.Exec(new Value.Int(1));
            i.Exec(new Value.Int(2));
            i.Exec(new Value.Symbol("stack"));
            var result = i.Pop<Value.List>();

            Assert.Equal(3, result.Size);
            Assert.Equal(new Value.Int(2), result.Elements[0]);
            Assert.Equal(new Value.Int(1), result.Elements[1]);
            Assert.Equal(new Value.Int(0), result.Elements[2]);
        }

        [Fact]
        public void PushPi()
        {
            i.Exec(new Value.Symbol("PI"));
            var result = i.Pop<Value.Float>();

            Assert.Equal(Math.PI, result.Value);
        }

        [Fact]
        public void PushE()
        {
            i.Exec(new Value.Symbol("E"));
            var result = i.Pop<Value.Float>();

            Assert.Equal(Math.E, result.Value);
        }

        [Fact]
        public void PushTime()
        {
            i.Exec(new Value.Symbol("time"));
            var result = i.Pop<Value.Int>();

            Assert.Equal(TestTime.UnitTimeSeconds, result.Value);
        }

        [Fact]
        public void PushRand()
        {
            i.Exec(new Value.Symbol("rand"));
            var result = i.Pop<Value.Int>();

            Assert.Equal(TestRandom.NextValue, result.Value);
        }

        protected override ITime CreateTime() => new TestTime();

        protected override IRandom CreateRandom() => new TestRandom();
    }
}