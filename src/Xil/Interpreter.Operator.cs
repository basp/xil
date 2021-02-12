namespace Xil
{
    using System;
    using System.Numerics;
    using M = System.Math;

    public partial class Interpreter : IInterpreter
    {
        [Builtin(
            "id",
            "->",
            "Identify function does nothing",
            "Any program of the form P id Q is equivalent to just P Q.")]
        private void Id_()
        {
            /* do nothing */
        }

        [Builtin(
            "dup",
            "X -> X X",
            "Pushes an extra copy of X onto the stack.")]
        private void Dup_()
        {
            new Validator("dup")
                .OneParameter()
                .Validate(this.stack);

            this.Push(this.Peek().Clone());
        }

        [Builtin(
            "swap",
            "X Y -> Y X",
            "Interchanges X and Y on top of the stack.")]
        private void Swap_()
        {
            new Validator("swap")
                .TwoParameters()
                .Validate(this.stack);

            var x = this.Pop();
            var y = this.Pop();
            this.Push(x);
            this.Push(y);
        }

        [Builtin(
            "rollup",
            "X Y Z -> Z X Y",
            "Moves X and Y up, moves Z down.")]
        private void Rollup_()
        {
            new Validator("rollup")
                .ThreeParameters()
                .Validate(this.stack);

            var z = this.Pop();
            var y = this.Pop();
            var x = this.Pop();
            this.Push(z);
            this.Push(x);
            this.Push(y);
        }

        [Builtin(
            "rolldown",
            "X Y Z -> Y Z X",
            "Moves Y and Z down, moves X up.")]
        private void Rolldown_()
        {
            new Validator("rolldown")
                .ThreeParameters()
                .Validate(this.stack);

            var z = this.Pop();
            var y = this.Pop();
            var x = this.Pop();
            this.Push(y);
            this.Push(z);
            this.Push(x);
        }

        [Builtin(
            "rotate",
            "X Y Z -> Z Y X",
            "Interchanges X and Z.")]
        private void Rotate_()
        {
            new Validator("rotate")
                .ThreeParameters()
                .Validate(this.stack);

            var z = this.Pop();
            var y = this.Pop();
            var x = this.Pop();
            this.Push(z);
            this.Push(y);
            this.Push(x);
        }

        [Builtin(
            "popd",
            "Y Z -> Z",
            "As if defined by `:popd [pop] dip;`.")]
        private void Popd_() =>
            this.Dipped("popd", v => v.TwoParameters(), this.Pop_);

        [Builtin(
            "dupd",
            "Y Z -> Y Y Z",
            "As if defined by `:dupd [dup] dip;`.")]
        private void Dupd_() =>
            this.Dipped("dupd", v => v.TwoParameters(), this.Dup_);

        [Builtin(
            "swapd",
            "X Y Z -> Y X Z",
            "As if defined by `:swapd [swap] dip;`.")]
        private void Swapd_() =>
            this.Dipped("swapd", v => v.ThreeParameters(), this.Swap_);

        [Builtin(
            "rollupd",
            "X Y Z W -> Z X Y W",
            "As if defined by `:rollupd [rollup] dip;`.")]
        private void Rollupd_() =>
            this.Dipped("rollupd", v => v.FourParameters(), this.Rollup_);

        [Builtin(
            "rolldownd",
            "X Y Z W -> Y Z X W",
            "As if defined by `:rolldownd [rolldown] dip;`.")]
        private void Rolldownd_() =>
            this.Dipped("rolldownd", v => v.FourParameters(), this.Rolldown_);

        [Builtin(
            "rotated",
            "X Y Z W -> Z Y X W",
            "As if defined by `:rotated [rotate] dip;`.")]
        private void Rotated_() =>
            this.Dipped("rotated", v => v.FourParameters(), this.Rotate_);

        [Builtin(
            "pop",
            "X ->",
            "Removes X from the top of the stack.")]
        private void Pop_()
        {
            new Validator("pop")
                .OneParameter()
                .Validate(this.stack);

            this.Pop();
        }

        [Builtin(
            "choice",
            "B T F -> X",
            "If B is true, then X = T else X = F.")]
        private void Choice_()
        {
            new Validator("choice")
                .ThreeParameters()
                .Validate(this.stack);

            var f = this.Pop();
            var t = this.Pop();
            var b = this.Pop();

            if (Value.IsTruthy(b))
            {
                this.Push(t);
            }
            else
            {
                this.Push(f);
            }
        }

        private void Or_() => AndOrXor("or");

        private void Xor_() => AndOrXor("xor");

        private void And_() => AndOrXor("and");

        private void Not_()
        {
            new Validator("not")
                .OneParameter()
                .Validate(this.stack);

            var x = this.Pop();
            this.Push(new Value.Bool(!Value.IsTruthy(x)));
        }

        [Builtin(
            "+",
            "M I -> N",
            "Numeric N is the result of adding integer I to numeric M.",
            "Also supports float.")]
        private void Add_() => AddSub("+");

        [Builtin(
            "-",
            "M I -> N",
            "Numeric N is the result of subtracting integer I from numeric M.",
            "Also supports float.")]
        private void Sub_() => AddSub("-");

        [Builtin(
            "*",
            "I J -> K",
            "Integer K is the product of integers I and J.",
            "Also supports float.")]
        private void Mul_()
        {
            new Validator("*")
                .TwoParameters()
                .TwoFloatsOrIntegers()
                .Validate(this.stack);

            if (TwoIntsOnTop())
            {
                var y = this.Pop<Value.Int>();
                var x = this.Pop<Value.Int>();
                this.Push(x * y);
            }
            else
            {
                var y = this.Pop<IFloatable>();
                var x = this.Pop<IFloatable>();
                this.Push(x.Mul(y));
            }
        }

        [Builtin(
            "/",
            "I J -> K",
            "Integer K is the (rounded) ratio of integers I and J.",
            "Also supports float.")]
        private void Divide_()
        {
            new Validator("/")
                .TwoParameters()
                .TwoFloatsOrIntegers()
                .NonZeroOnTop()
                .Validate(this.stack);

            if (TwoIntsOnTop())
            {
                var y = this.Pop<Value.Int>();
                var x = this.Pop<Value.Int>();
                this.Push(x / y);
            }
            else
            {
                var y = this.Pop<IFloatable>();
                var x = this.Pop<IFloatable>();
                this.Push(x.Divide(y));
            }
        }

        [Builtin(
            "rem",
            "I J -> K",
            "Integer K is the remainder of dividing I by J.",
            "Also supports float.")]
        private void Rem_()
        {
            void Remf()
            {
                var y = this.Pop<IFloatable>();
                var x = this.Pop<IFloatable>();
            }

            var floatp = new Validator("rem")
                .TwoParameters()
                .AddRule(Validator.Floatable2)
                .NonZeroOnTop();

            if (floatp.TryValidate(this.stack, out var ignored))
            {
                Remf();
                return;
            }

            new Validator("rem")
                .TwoParameters()
                .TwoIntegersOnTop()
                .NonZeroOnTop()
                .Validate(this.stack);

            var y = this.Pop<Value.Int>();
            var x = this.Pop<Value.Int>();
            this.Push(new Value.Int(x.Value % y.Value));
        }

        [Builtin(
            "div",
            "I J -> K L",
            "Integers K and L are the quotient and remainder of dividing I by J.")]
        private void Div_()
        {
            new Validator("div")
                .TwoParameters()
                .TwoIntegersOnTop()
                .NonZeroOnTop()
                .Validate(this.stack);

            var y = this.Pop<Value.Int>();
            var x = this.Pop<Value.Int>();
            var q = Math.DivRem(x.Value, y.Value, out var r);
            this.Push(new Value.Int(q));
            this.Push(new Value.Int(r));
        }

        [Builtin(
            "sign",
            "N1 -> N2",
            "Integer N2 is the sign (-1 or 0 or +1) of integer N1",
            "or float N2 is the sign (-1.0 or 0.0 or +1.0) of float N1.")]
        private void Sign_()
        {
            new Validator("sign")
                .OneParameter()
                .FloatOrInteger()
                .Validate(this.stack);

            var x = this.Pop();
            IValue y = x switch
            {
                Value.Int i => new Value.Int(M.Sign(i.Value)),
                Value.Float f => new Value.Float(M.Sign(f.Value)),
                _ => throw new NotSupportedException(),
            };

            this.Push(y);
        }

        [Builtin(
            "neg",
            "I -> J",
            "Integer J is the negative of integer I.",
            "Also supports float.")]
        private void Neg_()
        {
            new Validator("neg")
                .OneParameter()
                .FloatOrInteger()
                .Validate(this.stack);

            var x = this.Pop();
            IValue y = x switch
            {
                Value.Int i => new Value.Int(-i.Value),
                Value.Float f => new Value.Float(-f.Value),
                _ => throw new NotSupportedException(),
            };

            this.Push(y);
        }

        [Builtin(
            "ord",
            "C -> I",
            "Integer I is the ASCII value of character C (or logical or integer).")]
        private void Ord_() => OrdChr("ord");

        [Builtin(
            "chr",
            "I -> C",
            "C is the character whose ASCII value is integer I (or logical or character).")]
        private void Chr_() => OrdChr("chr");

        [Builtin(
            "abs",
            "N1 -> N2",
            "Integer N2 is the absolute value (0, 1, 2..) of integer N1",
            "or float N2 is the absolute value (0.0 ..) of float N1.")]
        private void Abs_()
        {
            new Validator("abs")
                .OneParameter()
                .FloatOrInteger()
                .Validate(this.stack);

            var x = this.Pop();
            IValue y = x switch
            {
                Value.Int i => new Value.Int(M.Abs(i.Value)),
                Value.Float f => new Value.Float(M.Abs(f.Value)),
                _ => throw new NotSupportedException(),
            };

            this.Push(y);
        }
    }
}