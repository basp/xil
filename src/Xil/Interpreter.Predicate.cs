namespace Xil
{
    public partial class Interpreter : IInterpreter
    {
        private void Null_()
        {
            new Validator("null")
                .OneParameter()
                .Validate(this.stack);

            var x = this.Pop<IValue>();
            this.Push(new Value.Bool(Value.IsNull(x)));
        }

        private void Ge_() => Comprel(">=");

        private void Gt_() => Comprel(">");

        private void Le_() => Comprel("<=");

        private void Lt_() => Comprel("<");

        private void Ne_() => Comprel("!=");
        
        private void Eq_() => Comprel("=");

        private void Equal_()
        {
            new Validator("equal")
                .TwoParameters()
                .Validate(this.stack);

            var y = this.Pop<IValue>();
            var x = this.Pop<IValue>();
            this.Push(new Value.Bool(x.Equals(y)));
        }

        private void IsInt_() => TypeCheck<Value.Int>("integer");

        private void IsFloat_() => TypeCheck<Value.Float>("float");

        private void IsBool_() => TypeCheck<Value.Bool>("bool");

        private void IsChar_() => TypeCheck<Value.Char>("char");

        private void IsList_() => TypeCheck<Value.List>("list");

        private void IsString_() => TypeCheck<Value.String>("stringg");
    }
}