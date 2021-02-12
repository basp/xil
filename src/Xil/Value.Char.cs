namespace Xil
{
    public abstract partial class Value
    {
        public class Char : Value, IOrdinal
        {
            public Char(char value)
            {
                this.Value = value;
            }

            public override ValueKind Kind => ValueKind.Char;

            public char Value { get; }

            public int OrdinalValue => (int)this.Value;

            public IValue Chr() => new Value.Char(this.Value);

            public override IValue Clone() => new Char(this.Value);

            public override string ToString() => $"'{this.Value}";

            public override int GetHashCode() =>
                System.HashCode.Combine(this.Kind, this.Value.GetHashCode());

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as Value.Char;
                if (other == null)
                {
                    return false;
                }

                return this.Value == other.Value;
            }

            public IValue Max(IOrdinal value) =>
                this.OrdinalValue > value.OrdinalValue
                    ? this.Clone()
                    : value.Clone();

            public IValue Min(IOrdinal value) =>
                this.OrdinalValue < value.OrdinalValue
                    ? this.Clone()
                    : value.Clone();

            public IValue Ord() => new Value.Int((int)this.Value);

            public IValue Pred() => new Value.Char((char)(this.Value - 1));

            public IValue Succ() => new Value.Char((char)(this.Value + 1));

            public int CompareTo(IOrdinal value) =>
                this.OrdinalValue.CompareTo(value.OrdinalValue);
        }
    }
}