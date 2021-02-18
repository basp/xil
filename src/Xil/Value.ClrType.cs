using System;

namespace Xil
{
    public abstract partial class Value
    {
        public class ClrType : Value
        {
            public ClrType(Type type)
            {
                this.Type = type;
            }

            public Type Type { get; }

            public override ValueKind Kind => ValueKind.ClrType;

            public override IValue Clone() => new ClrType(this.Type);

            public override string ToString() => $"clr:{this.Type}";
        }
    }
}