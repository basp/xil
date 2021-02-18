namespace Xil
{
    using System;
    using System.Reflection;

    public abstract partial class Value
    {
        public class ClrMember : Value
        {
            public ClrMember(MemberInfo memberInfo)
            {
                this.MemberInfo = memberInfo;
            }

            public MemberInfo MemberInfo { get; }

            public override ValueKind Kind => ValueKind.ClrMember;

            public override IValue Clone() =>
                new Value.ClrMember(this.MemberInfo);

            public override string ToString() =>
                $"clr:{this.MemberInfo.DeclaringType}.{this.MemberInfo.Name}";
        }
    }
}