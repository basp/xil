namespace Xil
{
    using System;

    public abstract partial class Value
    {
        public class Float : Value, IFloatable
        {
            public Float(double value)
            {
                this.Value = value;
            }

            public override ValueKind Kind => ValueKind.Float;

            public double Value { get; }

            public override IValue Clone() => new Float(this.Value);

            public override string ToString() => this.Value.ToString();

            public override int GetHashCode() =>
                System.HashCode.Combine(this.Kind, this.Value.GetHashCode());

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as Value.Float;
                if (other == null)
                {
                    return false;
                }

                return this.Value == other.Value;
            }

            public IValue Add(IValue value) =>
                value switch
                {
                    Value.Int y => new Value.Float(this.Value - (long)y.Value),
                    Value.Float y => new Value.Float(this.Value - y.Value),
                    _ => throw new NotSupportedException(),
                };

            public IValue Divide(IValue value) =>
                value switch
                {
                    Value.Int y => new Value.Float(this.Value - (long)y.Value),
                    Value.Float y => new Value.Float(this.Value - y.Value),
                    _ => throw new NotSupportedException(),
                };

            public IValue Mul(IValue value) =>
                value switch
                {
                    Value.Int y => new Value.Float(this.Value - (long)y.Value),
                    Value.Float y => new Value.Float(this.Value - y.Value),
                    _ => throw new NotSupportedException(),
                };

            public IValue Sub(IValue value) =>
                value switch
                {
                    Value.Int y => new Value.Float(this.Value - (long)y.Value),
                    Value.Float y => new Value.Float(this.Value - y.Value),
                    _ => throw new NotSupportedException(),
                };

            public IValue Rem(IValue value) =>
                value switch
                {
                    Value.Int y => new Value.Float(this.Value % (long)y.Value),
                    Value.Float y => new Value.Float(this.Value % y.Value),
                    _ => throw new NotSupportedException(),
                };

            public IValue Acos() =>
                new Value.Float(Math.Acos(this.Value));

            public IValue Asin() =>
                new Value.Float(Math.Asin(this.Value));

            public IValue Atan() =>
                new Value.Float(Math.Atan(this.Value));

            public IValue Ceil() =>
                new Value.Float(Math.Ceiling(this.Value));

            public IValue Cos() =>
                new Value.Float(Math.Cos(this.Value));

            public IValue Cosh() =>
                new Value.Float(Math.Cosh(this.Value));

            public IValue Exp() =>
                new Value.Float(Math.Exp(this.Value));

            public IValue Floor() =>
                new Value.Float(Math.Floor(this.Value));

            public IValue Log() =>
                new Value.Float(Math.Log(this.Value));

            public IValue Log10() =>
                new Value.Float(Math.Log10(this.Value));

            public IValue Sin() =>
                new Value.Float(Math.Sin(this.Value));

            public IValue Sinh() =>
                new Value.Float(Math.Sinh(this.Value));

            public IValue Sqrt() =>
                new Value.Float(Math.Sqrt(this.Value));

            public IValue Tan() =>
                new Value.Float(Math.Tan(this.Value));

            public IValue Tanh() =>
                new Value.Float(Math.Tanh(this.Value));

            public Value.Float AsFloat() => new Value.Float(this.Value);

            public IValue Min(IValue value) =>
                value switch
                {
                    Value.Int y =>
                        new Value.Float(Math.Min(this.Value, (long)y.Value)),
                    Value.Float y =>
                        new Value.Float(Math.Min(this.Value, (long)y.Value)),
                    _ => throw new NotSupportedException(),
                };

            public IValue Max(IValue value) =>
                value switch
                {
                    Value.Int y =>
                        new Value.Float(Math.Max(this.Value, (long)y.Value)),
                    Value.Float y =>
                        new Value.Float(Math.Max(this.Value, y.Value)),
                    _ => throw new NotSupportedException(),
                };

            public int CompareTo(IValue value) =>
                value switch
                {
                    Value.Int y => this.Value.CompareTo(y.Value),
                    Value.Float y => this.Value.CompareTo(y.Value),
                    _ => throw new NotSupportedException(),
                };
        }
    }
}