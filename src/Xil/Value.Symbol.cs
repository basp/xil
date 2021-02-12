namespace Xil
{
    using System;

    public abstract partial class Value
    {
        public class Symbol : Value
        {
            public Symbol(string value)
            {
                this.Value = value;
            }

            public string Value { get; }

            public override ValueKind Kind => ValueKind.Symbol;

            public override IValue Clone() => new Symbol(this.Value);

            public override string ToString() => this.Value;

            public override int GetHashCode() =>
                HashCode.Combine(this.Kind, this.Value.GetHashCode());

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as Value.Symbol;
                if (other == null)
                {
                    return false;
                }

                return this.Value == other.Value;
            }
        }
    }
}