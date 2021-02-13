namespace Xil
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public partial class Interpreter : IInterpreter
    {
        [Builtin(
            "id",
            "->",
            "Identity function does nothing.",
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

        [Builtin(
            "or",
            "X Y -> Z",
            "Z is the union of sets X and Y,",
            "logical disjunction for truth values.")]
        private void Or_() => AndOrXor("or");

        [Builtin(
            "xor",
            "X Y -> Z",
            "Z is the symmetric difference of sets X and Y,",
            "logical exclusive disjunction for truth values.")]
        private void Xor_() => AndOrXor("xor");

        [Builtin(
            "and",
            "X Y -> Z",
            "Z is the intersection of sets X and Y,",
            "logical conjunction for truth values.")]
        private void And_() => AndOrXor("and");

        [Builtin(
            "not",
            "X -> Y",
            "Y is the complement of set X,",
            "logical negation for truth values.")]
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
                .TwoIntegers()
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
                .TwoIntegers()
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
                .FloatOrIntegerOnTop()
                .Validate(this.stack);

            var x = this.Pop();
            IValue y = x switch
            {
                Value.Int i => new Value.Int(Math.Sign(i.Value)),
                Value.Float f => new Value.Float(Math.Sign(f.Value)),
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
                .FloatOrIntegerOnTop()
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
                .FloatOrIntegerOnTop()
                .Validate(this.stack);

            var x = this.Pop();
            IValue y = x switch
            {
                Value.Int i => new Value.Int(Math.Abs(i.Value)),
                Value.Float f => new Value.Float(Math.Abs(f.Value)),
                _ => throw new NotSupportedException(),
            };

            this.Push(y);
        }

        [Builtin(
            "acos",
            "F -> G",
            "G is the arc cosine of F.")]
        private void Acos_() => FloatMath("acos");

        [Builtin(
            "asin",
            "F -> G",
            "G is the arc sine of F.")]
        private void Asin_() => FloatMath("asin");

        [Builtin(
            "atan",
            "F -> G",
            "G is the arc tangent of F.")]
        private void Atan_() => FloatMath("atan");

        // TODO: Atan2_

        [Builtin(
            "ceil",
            "F -> G",
            "G is the float ceiling of F.")]
        private void Ceil_() => FloatMath("ceil");

        [Builtin(
            "cos",
            "F -> G",
            "G is the cosine of F.")]
        private void Cos_() => FloatMath("cos");

        [Builtin(
            "cosh",
            "F -> G",
            "G is the hyperbolic cosine of F.")]
        private void Cosh_() => FloatMath("cosh");

        [Builtin(
            "exp",
            "F -> G",
            "G is e raised to the Fth power.")]
        private void Exp_() => FloatMath("exp");

        [Builtin(
            "floor",
            "F -> G",
            "G is floor of F.")]
        private void Floor_() => FloatMath("floor");

        // TODO: frexp, ldexp

        [Builtin(
            "log",
            "F -> G",
            "G is the natural logarithm of F.")]
        private void Log_() => FloatMath("log");

        [Builtin(
            "log10",
            "F -> G",
            "G is the common logarithm of F.")]
        private void Log10_() => FloatMath("log10");

        // TODO: modf, pow

        [Builtin(
            "sin",
            "F -> G",
            "G is the sine of F.")]
        private void Sin_() => FloatMath("sin");

        [Builtin(
            "sinh",
            "F -> G",
            "G is the hyperbolic sine of F.")]
        private void Sinh_() => FloatMath("sinh");

        [Builtin(
            "sqrt",
            "F -> G",
            "G is the square root of F.")]
        private void Sqrt_() => FloatMath("sqrt");

        [Builtin(
            "tan",
            "F -> G",
            "G is the tangent of F.")]
        private void Tan_() => FloatMath("tan");

        [Builtin(
            "tanh",
            "F -> G",
            "G is the hyperbolic tangent of F.")]
        private void Tanh_() => FloatMath("tanh");

        [Builtin(
            "strtoi",
            "S I -> J",
            "String S is converted to the integer J using base I.")]
        private void Strtoi_()
        {
            new Validator("strtoi")
                .TwoParameters()
                .IntegerOnTop()
                .StringAsSecond()
                .Validate(this.stack);

            var i = this.Pop<Value.Int>();
            var s = this.Pop<Value.String>();
            var v = Convert.ToInt32(s.Value, (int)i.Value);
            this.Push(new Value.Int(v));
        }

        [Builtin(
            "strtof",
            "S -> R",
            "String S in converted to the float R.")]
        private void Strtof_()
        {
            new Validator("strtof")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var s = this.Pop<Value.String>();
            this.Push(new Value.Float(double.Parse(s.Value)));
        }

        [Builtin(
            "srand",
            "I ->",
            "Sets the random integer seed to integer I.")]
        private void Srand_()
        {
            new Validator("srand")
                .OneParameter()
                .IntegerOnTop()
                .Validate(this.stack);

            var seed = this.Pop<Value.Int>();
            this.rng = new Random(seed.Value);
        }

        [Builtin(
            "pred",
            "M -> N",
            "Numeric N is the predecessor of numeric M.")]
        private void Pred_() => PredSucc("pred");

        [Builtin(
            "succ",
            "M -> N",
            "Numeric N is the successor of numeric M.")]
        private void Succ_() => PredSucc("succ");

        [Builtin(
            "max",
            "N1 N2 -> N",
            "N is the maximum of numeric values N1 and N2.",
            "Also supports float.")]
        private void Max_() => MaxMin("max");

        [Builtin(
            "min",
            "N1 N2 -> N",
            "N is the minimum of numeric values N1 and N2.",
            "Also supports float.")]
        private void Min_() => MaxMin("min");

        [Builtin(
            "fclose",
            "S ->",
            "Stream S is closed and removed from the stack.")]
        private void Fclose_()
        {
            new Validator("fclose")
                .OneParameter()
                .StreamOnTop()
                .Validate(this.stack);

            var s = this.Pop<Value.Stream>();
            s.Close();
        }

        [Builtin(
            "feof",
            "S -> S B",
            "B is the end-of-file status of stream S.")]
        private void Feof_()
        {
            throw new NotImplementedException();
        }

        [Builtin(
            "ferror",
            "S -> S B",
            "B is the error status of stream S.")]
        private void Ferror_()
        {
            throw new NotImplementedException();
        }

        [Builtin(
            "fflush",
            "S -> S",
            "Flush stream S, forcing all buffered output to be written.")]
        private void Fflush_()
        {
            throw new NotImplementedException();
        }

        [Builtin(
            "fgetch",
            "S -> S C",
            "C is the next available character from stream S.")]
        private void Fgetch_()
        {
            throw new NotImplementedException();
        }

        [Builtin(
            "fgets",
            "S -> S L",
            "L is the next available line (as a string) from stream S.")]
        private void Fgets_()
        {
            throw new NotImplementedException();
        }

        [Builtin(
            "fopen",
            "P -> S",
            "The file object at path P is opened and a stream object S is pushed.")]
        private void Fopen_()
        {
            new Validator("fopen")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var path = this.Pop<Value.String>();
            var s = Value.Stream.Open(path.Value);
            this.stack.Push(s);
        }

        [Builtin(
            "freads",
            "S -> T",
            "The contents of stream S are read (as a string) and pushed.")]
        private void Freads_()
        {
            new Validator("freads")
                .OneParameter()
                .StreamOnTop()
                .Validate(this.stack);

            var stream = this.Pop<Value.Stream>();
            using (var reader = new StreamReader(stream.Value))
            {
                var s = reader.ReadToEnd();
                this.stack.Push(new Value.String(s));
            }
        }

        [Builtin(
            "unstack",
            "[X Y ..] -> .. Y X",
            "The list [X Y ..] becomes the new stack.")]
        private void Unstack_()
        {
            new Validator("unstack")
                .OneParameter()
                .ListOnTop()
                .Validate(this.stack);

            var xs = this.Pop<Value.List>().Elements;
            this.stack = new Stack<IValue>(xs);
        }

        [Builtin(
            "cons",
            "X A -> B",
            "Aggregate B is A with a new member X (first member for sequences).")]
        private void Cons_()
        {
            new Validator("cons")
                .TwoParameters()
                .AggregateOnTop()
                .Validate(this.stack);

            var a = this.Pop<IAggregate>();
            var x = this.Pop();
            this.Push(a.Cons(x));
        }

        [Builtin(
            "swons",
            "A X -> B",
            "Aggregate B is A with a new member X (first member for sequences).")]
        private void Swons_()
        {
            new Validator("swons")
                .TwoParameters()
                .AggregateAsSecond()
                .Validate(this.stack);

            this.Swap_();
            this.Cons_();
        }

        [Builtin(
            "first",
            "A -> F",
            "F is the first member of of the non-empty aggregate A.")]
        private void First_()
        {
            var validator = new Validator("first")
                .OneParameter()
                .NonEmptyAggregateOnTop()
                .Validate(this.stack);

            var x = this.Pop<IAggregate>();
            this.Push(x.First());
        }

        [Builtin(
            "rest",
            "A -> R",
            "R is the non-empty aggregate A with its first member removed.")]
        private void Rest_()
        {
            new Validator("rest")
                .OneParameter()
                .AggregateOnTop()
                .Validate(this.stack);

            var x = this.Pop<IAggregate>();
            this.Push(x.Rest());
        }

        [Builtin(
            "cmp",
            "A B -> I",
            "I (-1, 0, +1) is the comparison of A and B.",
            "The values correspond to the predicates <=, =, >=")]
        private void Cmp_() => Comprel("cmp");

        private void At_()
        {
            new Validator("at")
                .TwoParameters()
                .IntegerOnTop()
                .NonEmptyAggregateAsSecond()
                .Validate(this.stack);

            var i = this.Pop<Value.Int>();
            var a = this.Pop<IAggregate>();
            this.Push(a.Index((int)i.Value));
        }

        private void Of_()
        {
            new Validator("of")
                .TwoParameters()
                .NonEmptyAggregateOnTop()
                .IntegerAsSecond()
                .Validate(this.stack);

            this.Swap_();
            this.At_();
        }

        private void Size_()
        {
            new Validator("size")
                .OneParameter()
                .AggregateOnTop()
                .Validate(this.stack);

            var a = this.Pop<IAggregate>();
            this.Push(new Value.Int(a.Size));
        }


        private void Opcase_()
        {
            new Validator("opcase")
                .TwoParameters()
                .AggregateOnTop()
                .Validate(this.stack);

            var a = this.Pop<IAggregate>();
            var x = this.Pop<IValue>();
            Array.ForEach(
                a.Elements.ToArray(),
                e => Validator.InternalList("opcase", e));

            foreach (var y in a.Elements.Cast<IAggregate>())
            {
                var k = y.Index(0);
                if (k.Kind == x.Kind)
                {
                    this.Push(y.Index(1));
                    return;
                }
            }
        }

        private void Case_()
        {
            new Validator("case")
                .TwoParameters()
                .AggregateOnTop()
                .Validate(this.stack);

            var a = this.Pop<IAggregate>();
            var x = this.Pop<IValue>();
            Array.ForEach(
                a.Elements.ToArray(),
                e => Validator.InternalList("opcase", e));

            foreach (var y in a.Elements.Cast<IAggregate>())
            {
                var v = y.Index(0);
                if (v.Equals(x))
                {
                    this.Push(y.Index(1));
                    this.I_();
                    return;
                }
            }
        }

        [Builtin("uncons")]
        private void Uncons_()
        {
            new Validator("uncons")
                .OneParameter()
                .NonEmptyAggregateOnTop()
                .Validate(this.stack);

            var x = this.Pop<IAggregate>();
            var first = x.Uncons(out var rest);
            this.Push(first);
            this.Push(rest);
        }

        [Builtin("unswons")]
        private void Unswons_()
        {
            new Validator("unswons")
                .OneParameter()
                .NonEmptyAggregateOnTop()
                .Validate(this.stack);

            this.Uncons_();
            this.Swap_();
        }

        [Builtin("drop")]
        private void Drop_()
        {
            new Validator("drop")
                .TwoParameters()
                .IntegerOnTop()
                .AggregateAsSecond()
                .Validate(this.stack);

            var n = this.Pop<Value.Int>();
            var a = this.Pop<IAggregate>();
            this.Push(a.Drop((int)n.Value));
        }

        [Builtin("take")]
        private void Take_()
        {
            new Validator("take")
                .TwoParameters()
                .IntegerOnTop()
                .AggregateAsSecond()
                .Validate(this.stack);

            var n = this.Pop<Value.Int>();
            var a = this.Pop<IAggregate>();
            this.Push(a.Take((int)n.Value));
        }

        private void Concat_()
        {
            new Validator("concat")
                .TwoParameters()
                .TwoAggregates()
                .SameTwoTypes()
                .Validate(this.stack);

            var y = this.Pop<IAggregate>();
            var x = this.Pop<IAggregate>();
            this.Push(x.Concat(y));
        }

        private void Enconcat_()
        {
            new Validator("enconcat")
                .ThreeParameters()
                .SameTwoTypes()
                .Validate(this.stack);

            this.Swapd_();
            this.Cons_();
            this.Concat_();
        }

        [Builtin(
            "name",
            "sym -> \"sym\"",
            "For operators and combinators, the string \"sym\" is the name of item sym,",
            "For literals sym the result string is its type.")]
        private void Name_()
        {
            new Validator("name")
                .OneParameter()
                .Validate(this.stack);

            var sym = this.Pop();
            var name = sym switch
            {
                Value.Int x => "int",
                Value.Float x => "float",
                Value.Bool x => "bool",
                Value.Char x => "char",
                Value.String x => "string",
                Value.List x => "list",
                Value.Set x => "set",
                Value.Stream x => "stream",
                Value.Symbol x => x.Value,
                _ => throw new NotSupportedException(),
            };

            this.Push(new Value.String(name));
        }

        [Builtin(
            "intern",
            "\"sym\" -> sym",
            "Pushes the item whose name is \"sym\".")]
        private void Intern_()
        {
            new Validator("intern")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var s = this.Pop<Value.String>();
            var sym = new Value.Symbol(s.Value);
            this.Push(sym);
        }

        private void Body_()
        {
            new Validator("body")
                .OneParameter()
                .SymbolOrStringOnTop()
                .Validate(this.stack);

            var x = this.Pop();
            var name = x switch
            {
                Value.String s => s.Value,
                Value.Symbol s => s.Value,
                _ => throw new NotSupportedException(),
            };

            if (this.usrDefs.TryGetValue(name, out var usr))
            {
                this.Push(usr);
                return;
            }

            var msg = $"undefined `{name}`";
            throw new RuntimeException(msg);
        }
    }
}