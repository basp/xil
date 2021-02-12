namespace Xil
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract partial class Value
    {
        public class String : Value, IAggregate
        {
            public String(string value)
            {
                this.Value = value;
            }

            public override ValueKind Kind => ValueKind.String;

            public string Value { get; }

            public int Size => this.Value.Length;

            public IList<IValue> Elements =>
                this.Value
                    .Select(x => new Value.Char(x))
                    .Cast<IValue>()
                    .ToList();

            public override IValue Clone() => new String(this.Value);

            public override string ToString() => $"\"{this.Value}\"";

            public override int GetHashCode() =>
                HashCode.Combine(this.Kind, this.Value.GetHashCode());

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as Value.String;
                if (other == null)
                {
                    return false;
                }

                return this.Value == other.Value;
            }

            public IValue At(int i) => new Value.Char(this.Value[i]);

            public IValue Concat(IAggregate value) =>
                value switch
                {
                    Value.String y =>
                        new Value.String(string.Concat(this.Value, y.Value)),
                    _ => throw new NotSupportedException(),
                };

            public IValue Cons(IValue value) =>
                value switch
                {
                    IOrdinal y => new Value.String(((Value.Char)y.Chr()).Value + this.Value),
                    _ =>
                        throw new RuntimeException(
                            Validator.GetErrorMessage("cons", "ordinal")),
                };

            public IAggregate Drop(int n) =>
                new Value.String(this.Value.Substring(n));

            public IValue First() =>
                new Value.Char(this.Value[0]);

            public IAggregate Rest() =>
                new Value.String(this.Value.Substring(1));

            public IAggregate Take(int n) =>
                new Value.String(this.Value.Substring(0, n));

            public IValue Uncons(out IAggregate rest)
            {
                rest = this.Rest();
                return this.First();
            }
        }
    }
}