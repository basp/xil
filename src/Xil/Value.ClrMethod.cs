namespace Xil
{
    using System;
    using System.Reflection;

    public abstract partial class Value
    {
        public class ClrMethod : Value
        {
            public ClrMethod(MethodInfo methodInfo)
            {
                this.MethodInfo = methodInfo;
            }

            public MethodInfo MethodInfo { get; }

            public override ValueKind Kind => ValueKind.ClrMethod;

            public override IValue Clone() =>
                new Value.ClrMethod(this.MethodInfo);

            public override string ToString() =>
                $"clr:{this.MethodInfo.DeclaringType}.{this.MethodInfo.Name}";
        }
    }
}