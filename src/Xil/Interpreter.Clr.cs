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
            if(t == null)
            {
                throw new RuntimeException(
                    $"could not find CLR type `{s.Value}`");   
            }

            this.Push(new Value.ClrType(t));
        }

        [Builtin(
            "clr_reflect",
            ".. -> ..",
            "Display information about type T.")]
        private void ClrReflect_()
        {
            new Validator("clr_reflect")
                .ClrTypeOnTop()
                .Validate(this.stack);

            var t = this.Pop<Value.ClrType>();
            var members = t.Type.GetMembers();
            foreach(var m in members)
            {
                Console.WriteLine(m);
            }
        }
    }
}