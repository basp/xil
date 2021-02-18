namespace Xil
{
    using System;

    public partial class Interpreter : IInterpreter
    {
        [Builtin(
            "clr_type",
            "S -> T",
            "T is the CLR type instance of string S.")]
        private void ClrType_()
        {
            new Validator("clr_type")
                .StringOnTop()
                .Validate(this.stack);

            var s = this.Pop<Value.String>();
            var t = Type.GetType(s.Value);
            if (t == null)
            {
                throw new RuntimeException(
                    $"could not find CLR type `{s.Value}`");
            }

            this.Push(new Value.ClrType(t));
        }

        [Builtin(
            "clr_method",
            "T S -> M",
            "M is the CLR method representing member S on type T.")]
        private void ClrMethod_()
        {
            new Validator("clr_method")
                .ClrTypeAsSecond()
                .StringOnTop()
                .Validate(this.stack);

            var s = this.Pop<Value.String>();
            var t = this.Pop<Value.ClrType>();
            var m = t.Type.GetMethod(s.Value);

            this.Push(new Value.ClrMethod(m));
        }

        [Builtin(
            "clr_reflect",
            ".. -> ..",
            "Display information about type T.")]
        private void ClrReflect_()
        {
            new Validator("clr_reflect")
                .ClrKindOnTop()
                .Validate(this.stack);

            var t = this.Pop();
            switch (t)
            {
                case Value.ClrType x:
                    break;
                case Value.ClrMember x:
                    break;
                case Value.ClrMethod x:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}