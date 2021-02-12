namespace Xil
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public abstract partial class Value
    {
        public class List : Value, IAggregate
        {
            public List(params IValue[] elements)
            {
                this.Elements = new List<IValue>(elements);
            }

            public override ValueKind Kind => ValueKind.List;

            public IList<IValue> Elements { get; }

            public int Size => this.Elements.Count;

            public IValue Index(int i) => this.Elements.ElementAt(i);

            public override IValue Clone()
            {
                var xs = this.Elements
                    .Select(x => x.Clone())
                    .ToArray();

                return new List(xs);
            }

            public override int GetHashCode() =>
                HashCode.Combine(this.Kind, this.Elements.GetHashCode());

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as Value.List;
                if (other == null)
                {
                    return false;
                }

                if (this.Size != other.Size)
                {
                    return false;
                }

                for (var i = 0; i < this.Size; i++)
                {
                    if (!this.Elements[i].Equals(other.Elements[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            public IValue Concat(IAggregate value) =>
                value switch
                {
                    List y =>
                        new List(this.Elements.Concat(y.Elements).ToArray()),
                    _ => throw new NotSupportedException(),
                };

            public IValue Cons(IValue value) =>
                new List(new[] { value }.Concat(this.Elements).ToArray());

            public IAggregate Drop(int n) =>
                new List(this.Elements.Skip(n).ToArray());

            public IValue First() => this.Elements.First();

            public IAggregate Rest() =>
                new List(this.Elements.Skip(1).ToArray());

            public IAggregate Take(int n) =>
                new List(this.Elements.Take(n).ToArray());

            public override string ToString()
            {
                var xs = this.Elements.Select(x => x.ToString());
                return $"[{string.Join(" ", xs)}]";
            }

            public IValue Uncons(out IAggregate rest)
            {
                rest = this.Rest();
                return this.First();
            }

            public IEnumerator<IValue> GetEnumerator() =>
                this.Elements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                this.GetEnumerator();
        }
    }
}