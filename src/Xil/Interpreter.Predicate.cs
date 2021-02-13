namespace Xil
{
    using System;

    public partial class Interpreter : IInterpreter
    {
        [Builtin(
            "null",
            "X -> B",
            "Tests for empty aggregate X or zero numeric.")]
        private void Null_()
        {
            new Validator("null")
                .OneParameter()
                .Validate(this.stack);

            var x = this.Pop<IValue>();
            this.Push(new Value.Bool(Value.IsNull(x)));
        }

        [Builtin(
            ">=",
            "X Y -> B",
            "Either both X and Y are numeric or both are strings or symbols.",
            "Tests wheter X is greater than or equal to Y.",
            "Also supports float.")]
        private void Ge_() => Comprel(">=");

        [Builtin(
            ">",
            "X Y -> B",
            "Either both X and Y are numeric or both are strings or symbols.",
            "Tests wheter X is greater than Y.",
            "Also supports float.")]
        private void Gt_() => Comprel(">");

        [Builtin(
            "<=",
            "X Y -> B",
            "Either both X and Y are numeric or both are strings or symbols.",
            "Tests wheter X is less than or equal to Y.",
            "Also supports float.")]
        private void Le_() => Comprel("<=");

        [Builtin(
            "<",
            "X Y -> B",
            "Either both X and Y are numeric or both are strings or symbols.",
            "Tests wheter X is less than Y.",
            "Also supports float.")]
        private void Lt_() => Comprel("<");

        [Builtin(
            "!=",
            "X Y -> B",
            "Either both X and Y are numeric or both are strings or symbols.",
            "Tests wheter X does not equal Y.",
            "Also supports float.")]
        private void Ne_() => Comprel("!=");
        
        [Builtin(
            "=",
            "X Y -> B",
            "Either both X and Y are numeric or both are strings or symbols.",
            "Tests wheter X equals Y.",
            "Also supports float.")]
        private void Eq_() => Comprel("=");

        [Builtin(
            "equal",
            "T U -> B",
            "Recursively tests whether trees T and U are identical.")]
        private void Equal_()
        {
            new Validator("equal")
                .TwoParameters()
                .Validate(this.stack);

            var y = this.Pop<IValue>();
            var x = this.Pop<IValue>();
            this.Push(new Value.Bool(x.Equals(y)));
        }

        [Builtin(
            "has",
            "A X -> B",
            "Tests whether aggregate A has X as a member.")]
        private void Has_() =>
            throw new NotImplementedException();

        [Builtin(
            "in",
            "X A -> B",
            "Tests whether X is a member of aggregate A.")]
        private void In_() =>
            throw new NotImplementedException();

        [Builtin(
            "integer",
            "X -> B",
            "Tests whether X is an integer.")]
        private void IsInt_() => TypeCheck<Value.Int>("integer");

        [Builtin(
            "char",
            "X -> B",
            "Tests whether X is a character.")]
        private void IsChar_() => TypeCheck<Value.Char>("char");

        [Builtin(
            "logical",
            "X -> B",
            "Tests wheter X is a logical.")]
        private void IsBool_() => TypeCheck<Value.Bool>("bool");

        [Builtin(
            "set",
            "X -> B",
            "Tests whether X is a set.")]
        private void IsSet_() => TypeCheck<Value.String>("set");

        [Builtin(
            "string",
            "X -> B",
            "Tests whether X is a string.")]
        private void IsString_() => TypeCheck<Value.String>("string");

        [Builtin(
            "list",
            "X -> B",
            "Tests whether X is a list.")]
        private void IsList_() => TypeCheck<Value.List>("list");

        [Builtin(
            "user",
            "X -> B",
            "Tests whether X is a user-defined symbol.")]
        private void IsUser_() =>
            throw new NotImplementedException();

        [Builtin(
            "float",
            "X -> B",
            "Tests whether X is a float.")]
        private void IsFloat_() => TypeCheck<Value.Float>("float");

        [Builtin(
            "file",
            "X -> B",
            "Tests whether X is a file.")]
        private void IsFile_() =>
            throw new NotImplementedException();
    }
}