namespace Xil.Tests
{
    using Xunit;
    using Xunit.Abstractions;

    public class OperatorTests : InterpreterTests
    {
        public OperatorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void TestId()
        {
            i.Exec(new Value.Int(123));
            i.Exec(new Value.Symbol("id"));
            var stack = i.GetStack();

            Assert.Single(stack);
            Assert.Equal(new Value.Int(123), stack[0]);
        }

        [Fact]
        public void TestDup()
        {
            i.Exec(new Value.Int(123));
            i.Exec(new Value.Symbol("dup"));
            var stack = i.GetStack();

            Assert.Equal(2, stack.Length);
            Assert.Equal(new Value.Int(123), stack[0]);
            Assert.Equal(new Value.Int(123), stack[1]);
        }

        [Fact]
        public void TestSwap()
        {
            i.Exec(new Value.Int(123));
            i.Exec(new Value.String("foo"));
            i.Exec(new Value.Symbol("swap"));
            var stack = i.GetStack();

            Assert.Equal(2, stack.Length);
            Assert.Equal(new Value.Int(123), stack[0]);
            Assert.Equal(new Value.String("foo"), stack[1]);
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