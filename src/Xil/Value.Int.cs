namespace Xil
{
    using System;

    public abstract partial class Value
    {
        public class Int : Value, IFloatable, IOrdinal
        {
            public Int(int value)
            {
                this.Value = value;
            }

            public override ValueKind Kind => ValueKind.Int;

            public int Value { get; }

            public int OrdinalValue => (int)this.Value;

            public static Value.Int operator -(Value.Int x) =>
                new Value.Int(-x.Value);

            public static Value.Int operator +(Value.Int x, Value.Int y) =>
                new Value.Int(x.Value + y.Value);

            public static Value.Int operator -(Value.Int x, Value.Int y) =>
                new Value.Int(x.Value - y.Value);

            public static Value.Int operator *(Value.Int x, Value.Int y) =>
                new Value.Int(x.Value * y.Value);

            public static Value.Int operator /(Value.Int x, Value.Int y) =>
                new Value.Int(x.Value / y.Value);

            public static implicit operator Value.Float(Value.Int x) =>
                new Value.Float(x.Value);

            public Value.Float AsFloat() => this;

            public override IValue Clone() => new Value.Int(this.Value);

            public override string ToString() => this.Value.ToString();

            public override int GetHashCode() =>
                HashCode.Combine(this.Kind, this.Value.GetHashCode());

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as Value.Int;
                if (other == null)
                {
                    return false;
                }

                return this.Value == other.Value;
            }

            public override IValue And(IValue value) =>
                value switch
                {
                    Value.Int i => new Value.Int(this.Value & i.Value),
                    _ => base.And(value),
                };

            public override IValue Or(IValue value) =>
                value switch
                {
                    Value.Int i => new Value.Int(this.Value | i.Value),
                    _ => base.And(value),
                };

            public override IValue Xor(IValue value) =>
                value switch
                {
                    Value.Int i => new Value.Int(this.Value ^ i.Value),
                    _ => base.And(value),
                };

            public IValue Add(IValue value) =>
                value switch
                {
                    Value.Int y => this + y,
                    Value.Float y => this + y,
                    _ => throw new NotSupportedException(),
                };

            public IValue Divide(IValue value) =>
                value switch
                {
                    Value.Int y => this / y,
                    Value.Float y => this / y,
                    _ => throw new NotSupportedException(),
                };

            public IValue Mul(IValue value) =>
                value switch
                {
                    Value.Int y => this * y,
                    Value.Float y => this * y,
                    _ => throw new NotSupportedException(),
                };

            public IValue Sub(IValue value) =>
                value switch
                {
                    Value.Int y => this - y,
                    Value.Float y => this - y,
                    _ => throw new NotSupportedException(),
                };

            public IValue Rem(IValue y) =>
                new Value.Int(this.Value % ((Value.Int)y).Value);

            public IValue Ord() => new Value.Int(this.Value);

            public IValue Chr() => new Value.Char((char)this.Value);

            public IValue Pred() => new Value.Int(this.Value - 1);

            public IValue Succ() => new Value.Int(this.Value + 1);

            public IValue Acos() => this.AsFloat().Acos();

            public IValue Asin() => this.AsFloat().Asin();

            public IValue Atan() => this.AsFloat().Atan();

            public IValue Ceil() => this.AsFloat().Ceil();

            public IValue Cos() => this.AsFloat().Cos();

            public IValue Cosh() => this.AsFloat().Cosh();

            public IValue Exp() => this.AsFloat().Exp();

            public IValue Floor() => this.AsFloat().Floor();

            public IValue Log() => this.AsFloat().Log();

            public IValue Log10() => this.AsFloat().Log10();

            public IValue Sin() => this.AsFloat().Sin();

            public IValue Sinh() => this.AsFloat().Sinh();

            public IValue Sqrt() => this.AsFloat().Sqrt();

            public IValue Tan() => this.AsFloat().Tan();

            public IValue Tanh() => this.AsFloat().Tanh();

            public IValue Min(IValue value) =>
                value switch
                {
                    Value.Int y =>
                        new Value.Int(Math.Min(this.Value, y.Value)),
                    Value.Float y =>
                        new Value.Float(Math.Min(this.AsFloat().Value, y.Value)),
                    _ => throw new NotSupportedException(),
                };

            public IValue Max(IValue value) =>
                value switch
                {
                    Value.Int y =>
                        new Value.Int(Math.Max(this.Value, y.Value)),
                    Value.Float y =>
                        new Value.Float(Math.Max(this.AsFloat().Value, y.Value)),
                    _ => throw new NotSupportedException(),
                };

            public IValue Min(IOrdinal value) =>
                this.OrdinalValue < value.OrdinalValue
                    ? this.Clone()
                    : value.Clone();

            public IValue Max(IOrdinal value) =>
                this.OrdinalValue > value.OrdinalValue
                    ? this.Clone()
                    : value.Clone();

            public int CompareTo(IOrdinal value) =>
                this.OrdinalValue.CompareTo(value.OrdinalValue);

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