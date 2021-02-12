namespace Xil
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using Superpower;
    using M = System.Math;

    public class Interpreter
    {
        private static readonly Random Rng = new Random();

        private static IDictionary<string, Func<IValue, IValue, IValue>> andOrXorOps =
            new Dictionary<string, Func<IValue, IValue, IValue>>
            {
                ["and"] = (x, y) => x.And(y),
                ["or"] = (x, y) => x.Or(y),
                ["xor"] = (x, y) => x.Xor(y),
            };

        private static IDictionary<string, Func<IFloatable, IValue>> floatUnaryOps =
            new Dictionary<string, Func<IFloatable, IValue>>
            {
                ["acos"] = f => f.Acos(),
                ["asin"] = f => f.Asin(),
                ["atan"] = f => f.Atan(),
                ["ceil"] = f => f.Ceil(),
                ["cos"] = f => f.Cos(),
                ["cosh"] = f => f.Cosh(),
                ["exp"] = f => f.Exp(),
                ["floor"] = f => f.Floor(),
                ["log"] = f => f.Log(),
                ["log10"] = f => f.Log10(),
                ["sin"] = f => f.Sin(),
                ["sinh"] = f => f.Sinh(),
                ["sqrt"] = f => f.Sqrt(),
                ["tan"] = f => f.Tan(),
                ["tanh"] = f => f.Tanh(),
            };

        private static IDictionary<string, Func<IFloatable, IFloatable, IValue>> floatBinaryOps =
            new Dictionary<string, Func<IFloatable, IFloatable, IValue>>
            {
                ["+"] = (x, y) => x.Add(y),
                ["-"] = (x, y) => x.Sub(y),
                ["min"] = (x, y) => x.Min(y),
                ["max"] = (x, y) => x.Max(y),
            };

        private static IDictionary<string, Func<IOrdinal, IOrdinal, IValue>> ordBinaryOps =
            new Dictionary<string, Func<IOrdinal, IOrdinal, IValue>>
            {
                ["min"] = (x, y) => x.Min(y),
                ["max"] = (x, y) => x.Max(y),
            };

        private static IDictionary<string, Func<IFloatable, IFloatable, IValue>> floatCmpOps =
            new Dictionary<string, Func<IFloatable, IFloatable, IValue>>
            {
                ["cmp"] = (x, y) => new Value.Int(x.CompareTo(y)),
                ["<"] = (x, y) => new Value.Bool(x.CompareTo(y) < 0),
                ["<="] = (x, y) => new Value.Bool(x.CompareTo(y) <= 0),
                [">"] = (x, y) => new Value.Bool(x.CompareTo(y) > 0),
                [">="] = (x, y) => new Value.Bool(x.CompareTo(y) >= 0),
                ["="] = (x, y) => new Value.Bool(x.CompareTo(y) == 0),
                ["!="] = (x, y) => new Value.Bool(x.CompareTo(y) != 0),
            };

        private static IDictionary<string, Func<IOrdinal, IOrdinal, IValue>> ordCmpOps =
            new Dictionary<string, Func<IOrdinal, IOrdinal, IValue>>
            {
                ["cmp"] = (x, y) => new Value.Int(x.CompareTo(y)),
                ["<"] = (x, y) => new Value.Bool(x.CompareTo(y) < 0),
                ["<="] = (x, y) => new Value.Bool(x.CompareTo(y) <= 0),
                [">"] = (x, y) => new Value.Bool(x.CompareTo(y) > 0),
                [">="] = (x, y) => new Value.Bool(x.CompareTo(y) >= 0),
                ["="] = (x, y) => new Value.Bool(x.CompareTo(y) == 0),
                ["!="] = (x, y) => new Value.Bool(x.CompareTo(y) != 0),
            };

        private static IDictionary<string, Func<IOrdinal, IValue>> ordChrOps =
            new Dictionary<string, Func<IOrdinal, IValue>>
            {
                ["chr"] = o => o.Chr(),
                ["ord"] = o => o.Ord(),
            };

        private static IDictionary<string, Func<IOrdinal, IValue>> predSuccOps =
            new Dictionary<string, Func<IOrdinal, IValue>>
            {
                ["pred"] = o => o.Pred(),
                ["succ"] = o => o.Succ(),
            };

        const int IntitialStackSize = 32;

        private Stack<IValue> stack = new Stack<IValue>(IntitialStackSize);

        private readonly IDictionary<string, Value.List> usrdefs =
            new Dictionary<string, Value.List>();

        private readonly IDictionary<string, Builtin> builtins;

        private readonly Action<int, string> @out;

        public Interpreter(Action<int, string> @out)
        {
            this.builtins = new Dictionary<string, Builtin>
            {
                ["id"] = Builtin.Id(this.Id_),
                ["stack"] = Builtin.Stack(this.Stack_),
                ["unstack"] = Builtin.Unstack(this.Unstack_),
                ["newstack"] = Builtin.Newstack(this.Newstack_),
                ["intern"] = Builtin.Intern(this.Intern_),
                ["pop"] = Builtin.Pop(this.Pop_),
                ["swap"] = Builtin.Swap(this.Swap_),
                ["rolldown"] = Builtin.Rolldown(this.Rolldown_),
                ["rollup"] = Builtin.Rollup(this.Rollup_),
                ["rotate"] = Builtin.Rotate(this.Rotate_),
                ["dup"] = Builtin.Dup(this.Dup_),
                ["popd"] = Builtin.Popd(this.Popd_),
                ["dupd"] = Builtin.Dupd(this.Dupd_),
                ["swapd"] = Builtin.Swapd(this.Swapd_),
                ["rolldownd"] = Builtin.Rolldownd(this.Rolldownd_),
                ["rollupd"] = Builtin.Rollupd(this.Rollupd_),
                ["rotated"] = Builtin.Rotated(this.Rotated_),
                ["not"] = Builtin.Not(this.Not_),
                ["and"] = Builtin.And(this.And_),
                ["or"] = Builtin.Or(this.Or_),
                ["xor"] = Builtin.Xor(this.Xor_),
                ["chr"] = Builtin.Chr(this.Chr_),
                ["ord"] = Builtin.Ord(this.Ord_),
                ["abs"] = Builtin.Abs(this.Abs_),
                ["sign"] = Builtin.Sign(this.Sign_),
                ["neg"] = Builtin.Neg(this.Neg_),
                ["*"] = Builtin.Mul(this.Mul_),
                ["/"] = Builtin.Ratio(this.Divide_),
                ["+"] = Builtin.Add(this.Add_),
                ["-"] = Builtin.Min(this.Sub_),
                ["rem"] = Builtin.Rem(this.Rem_),
                ["div"] = Builtin.Div(this.Div_),
                ["strtoi"] = Builtin.Strtoi(this.Strtoi_),
                ["strtof"] = Builtin.Strtof(this.Strtof_),
                ["acos"] = Builtin.Acos(this.Acos_),
                ["asin"] = Builtin.Asin(this.Asin_),
                ["atan"] = Builtin.Atan(this.Atan_),
                ["ceil"] = Builtin.Ceil(this.Ceil_),
                ["cos"] = Builtin.Cos(this.Cos_),
                ["cosh"] = Builtin.Cosh(this.Cosh_),
                ["exp"] = Builtin.Exp(this.Exp_),
                ["floor"] = Builtin.Floor(this.Floor_),
                ["log"] = Builtin.Log(this.Log_),
                ["log10"] = Builtin.Log10(this.Log10_),
                ["sin"] = Builtin.Sin(this.Sin_),
                ["sinh"] = Builtin.Sinh(this.Sinh_),
                ["sqrt"] = Builtin.Sqrt(this.Sqrt_),
                ["tan"] = Builtin.Tan(this.Tan_),
                ["tanh"] = Builtin.Tanh(this.Tanh_),
                ["pred"] = Builtin.Pred(this.Pred_),
                ["succ"] = Builtin.Succ(this.Succ_),
                ["max"] = Builtin.Max(this.Max_),
                ["min"] = Builtin.Min(this.Min_),
                ["cmp"] = Builtin.Cmp(this.Cmp_),
                ["="] = Builtin.Eq(this.Eq_),
                ["!="] = Builtin.Ne(this.Ne_),
                ["<"] = Builtin.Lt(this.Lt_),
                ["<="] = Builtin.Lte(this.Le_),
                [">"] = Builtin.Gt(this.Gt_),
                [">="] = Builtin.Gte(this.Ge_),
                ["first"] = Builtin.First(this.First_),
                ["rest"] = Builtin.Rest(this.Rest_),
                ["cons"] = Builtin.Cons(this.Cons_),
                ["swons"] = Builtin.Swons(this.Swons_),
                ["concat"] = Builtin.Concat(this.Concat_),
                ["enconcat"] = Builtin.Enconcat(this.Enconcat_),
                ["uncons"] = Builtin.Uncons(this.Uncons_),
                ["unswons"] = Builtin.Unswons(this.Unswons_),
                ["take"] = Builtin.Take(this.Take_),
                ["drop"] = Builtin.Drop(this.Drop_),
                ["at"] = Builtin.At(this.At_),
                ["of"] = Builtin.Of(this.Of_),
                ["size"] = Builtin.Size(this.Size_),
                ["null"] = Builtin.Null(this.Null_),
                ["equal"] = Builtin.Equal(this.Equal_),
                ["int"] = Builtin.Int(this.IsInt_),
                ["float"] = Builtin.Float(this.IsFloat_),
                ["char"] = Builtin.Char(this.IsChar_),
                ["string"] = Builtin.String(this.IsString_),
                ["list"] = Builtin.List(this.IsList_),
                ["choice"] = Builtin.Choice(this.Choice_),
                ["i"] = Builtin.I(this.I_),
                ["x"] = Builtin.X(this.X_),
                ["step"] = Builtin.Step(this.Step_),
                ["dip"] = Builtin.Dip(this.Dip_),
                ["infra"] = Builtin.Infra(this.Infra_),
                ["linrec"] = new Builtin(this.Linrec_),
                ["branch"] = new Builtin(this.Branch_),
                ["iflist"] = new Builtin(this.IfList_),
                ["ifte"] = Builtin.Ifte(this.Ifte_),
                ["case"] = Builtin.Case(this.Case_),
                ["opcase"] = Builtin.Opcase(this.Opcase_),
                ["nullary"] = Builtin.Nullary(this.Nullary_),
                ["unary"] = Builtin.Unary(this.Unary_),
                ["unary2"] = new Builtin(this.Unary2_),
                ["times"] = Builtin.Times(this.Times_),
                ["map"] = Builtin.Map(this.Map_),
                ["filter"] = new Builtin(this.Filter_),
                ["some"] = new Builtin(this.Some_),
                ["all"] = new Builtin(this.All_),
                ["body"] = Builtin.Body(this.Body_),
                ["peek"] = new Builtin(this.Peek_),
                ["put"] = Builtin.Put(this.Put_),
                ["puts"] = Builtin.Puts(this.Puts_),
                ["time"] = new Builtin(this.Time_),
                ["error"] = new Builtin(this.Error_),
                ["defs"] = new Builtin(this.Defined_),
                ["help"] = new Builtin(this.Help_),
                ["helpdetail"] = new Builtin(this.Helpdetail_),
                ["fopen"] = Builtin.Fopen(this.Fopen_),
                ["fclose"] = Builtin.Fclose(this.Fclose_),
                ["freads"] = Builtin.Freads(this.Freads_),
                ["import"] = new Builtin(this.Import_),
                ["rand"] = Builtin.Rand(this.Rand_),
            };

            this.@out = @out;
        }

        public IValue[] GetStack() => this.stack.ToArray();

        public void Exec(IValue value)
        {
            switch (value.Kind)
            {
                case ValueKind.Int:
                case ValueKind.Float:
                case ValueKind.Bool:
                case ValueKind.String:
                case ValueKind.Char:
                case ValueKind.List:
                    if (value is Value.List)
                    {
                        // if value is a list that is not empty and contains
                        // only character values then it will be pushed as 
                        // a string value instead (experimental)
                        var l = (Value.List)value;
                        if (l.Elements.Any() && l.Elements.All(x => x is Value.Char))
                        {
                            var chars = l.Elements
                                .Cast<Value.Char>()
                                .Select(x => x.Value)
                                .ToArray();

                            this.Push(new Value.String(new string(chars)));
                        }
                        else
                        {
                            this.Push(value);
                        }
                    }
                    else
                    {
                        this.Push(value);
                    }
                    break;
                case ValueKind.Def:
                    var def = (Value.Usr)value;
                    this.usrdefs[def.Id] = def.Body;
                    break;
                case ValueKind.Symbol:
                    var sym = (Value.Symbol)value;
                    if (this.builtins.TryGetValue(sym.Value, out var bi))
                    {
                        bi.Op();
                        return;
                    }

                    if (this.usrdefs.TryGetValue(sym.Value, out var usr))
                    {
                        this.Push(usr);
                        this.I_();
                        return;
                    }

                    var msg = $"undefined `{sym.Value}`";
                    throw new RuntimeException(msg);
                default:
                    throw new NotSupportedException();
            }
        }

        public void AddUsrdef(string name, Value.List body)
        {
            this.usrdefs[name] = body;
        }

        public void Rand_() =>
            this.Push(new Value.Float(Rng.NextDouble()));

        private void Id_()
        {
            /* do nothing */
        }

        private void Stack_()
        {
            var xs = this.stack.ToArray();
            this.Push(new Value.List(xs));
        }

        private void Unstack_()
        {
            new Validator("unstack")
                .OneParameter()
                .ListOnTop()
                .Validate(this.stack);

            var xs = this.Pop<Value.List>().Elements;
            this.stack = new Stack<IValue>(xs);
        }

        private void Newstack_()
        {
            this.stack = new Stack<IValue>(IntitialStackSize);
        }

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

        private void Pop_()
        {
            new Validator("pop")
                .OneParameter()
                .Validate(this.stack);

            this.Pop();
        }

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

        private void Dup_()
        {
            new Validator("dup")
                .OneParameter()
                .Validate(this.stack);

            this.Push(this.Peek().Clone());
        }

        private void Dipped(
            string name,
            Func<Validator, Validator> rules,
            Action arg)
        {
            rules(new Validator(name)).Validate(this.stack);
            var x = this.Pop();
            arg();
            this.Push(x);
        }

        private void Popd_() =>
            this.Dipped("popd", v => v.TwoParameters(), this.Pop_);

        private void Dupd_() =>
            this.Dipped("dupd", v => v.TwoParameters(), this.Dup_);

        private void Swapd_() =>
            this.Dipped("swapd", v => v.ThreeParameters(), this.Swap_);

        private void Rolldownd_() =>
            this.Dipped("rolldownd", v => v.FourParameters(), this.Rolldown_);

        private void Rollupd_() =>
            this.Dipped("rollupd", v => v.FourParameters(), this.Rollup_);

        private void Rotated_() =>
            this.Dipped("rotated", v => v.FourParameters(), this.Rotate_);

        private void Not_()
        {
            new Validator("not")
                .OneParameter()
                .Validate(this.stack);

            var x = this.Pop();
            this.Push(new Value.Bool(!Value.IsTruthy(x)));
        }

        private void AndOrXor(string op)
        {
            new Validator(op)
                .TwoParameters()
                .Validate(this.stack);

            var y = this.Pop<Value>();
            var x = this.Pop<Value>();
            this.Push(andOrXorOps[op](x, y));
        }

        private void And_() => AndOrXor("and");

        private void Or_() => AndOrXor("or");

        private void Xor_() => AndOrXor("xor");

        private void OrdChr(string op)
        {
            new Validator(op)
                .OneParameter()
                .OrdinalOnTop();

            var x = this.Pop<IOrdinal>();
            this.Push(ordChrOps[op](x));
        }

        private void Ord_() => OrdChr("ord");

        private void Chr_() => OrdChr("chr");

        private void Abs_()
        {
            new Validator("abs")
                .OneParameter()
                .FloatOrInteger()
                .Validate(this.stack);

            var x = this.Pop();
            IValue y = x switch
            {
                Value.Int i => new Value.Int(BigInteger.Abs(i.Value)),
                Value.Float f => new Value.Float(M.Abs(f.Value)),
                _ => throw new NotSupportedException(),
            };

            this.Push(y);
        }

        private void Sign_()
        {
            new Validator("sign")
                .OneParameter()
                .FloatOrInteger()
                .Validate(this.stack);

            var x = this.Pop();
            IValue y = x switch
            {
                Value.Int i => new Value.Int(i.Value.Sign),
                Value.Float f => new Value.Float(M.Sign(f.Value)),
                _ => throw new NotSupportedException(),
            };

            this.Push(y);
        }

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

        private void Mul_()
        {
            new Validator("*")
                .TwoParameters()
                .TwoFloatsOrIntegers()
                .Validate(this.stack);

            var y = this.Pop<IFloatable>();
            var x = this.Pop<IFloatable>();
            this.Push(x.Mul(y));
        }

        private void Divide_()
        {
            new Validator("/")
                .TwoParameters()
                .TwoFloatsOrIntegers()
                .NonZeroOnTop()
                .Validate(this.stack);

            var y = this.Pop<IFloatable>();
            var x = this.Pop<IFloatable>();
            this.Push(x.Divide(y));
        }

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

        private void Div_()
        {
            new Validator("div")
                .TwoParameters()
                .TwoIntegers()
                .NonZeroOnTop()
                .Validate(this.stack);

            var y = this.Pop<Value.Int>();
            var x = this.Pop<Value.Int>();
            var q = BigInteger.DivRem(x.Value, y.Value, out var r);
            this.Push(new Value.Int(q));
            this.Push(new Value.Int(r));
        }

        private void Strtoi_()
        {
            new Validator("strtoi")
                .TwoParameters()
                .IntegerOnTop()
                .StringAsSecond()
                .Validate(this.stack);

            var i = this.Pop<Value.Int>();
            var s = this.Pop<Value.String>();
            var v = new BigInteger(Convert.ToInt64(s.Value, (int)i.Value));
            this.Push(new Value.Int(v));
        }

        private void Strtof_()
        {
            new Validator("strtof")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var s = this.Pop<Value.String>();
            this.Push(new Value.Float(double.Parse(s.Value)));
        }

        private void FloatMath(string op)
        {
            new Validator(op)
                .OneParameter()
                .FloatOrInteger()
                .Validate(this.stack);

            var x = this.Pop<IFloatable>();
            this.stack.Push(floatUnaryOps[op](x));
        }

        private void Acos_() => FloatMath("acos");

        private void Asin_() => FloatMath("asin");

        private void Atan_() => FloatMath("atan");

        private void Ceil_() => FloatMath("ceil");

        private void Cos_() => FloatMath("cos");

        private void Cosh_() => FloatMath("cosh");

        private void Exp_() => FloatMath("exp");

        private void Floor_() => FloatMath("floor");

        private void Log_() => FloatMath("log");

        private void Log10_() => FloatMath("log10");

        private void Sin_() => FloatMath("sin");

        private void Sinh_() => FloatMath("sinh");

        private void Sqrt_() => FloatMath("sqrt");

        private void Tan_() => FloatMath("tan");

        private void Tanh_() => FloatMath("tanh");

        private void PredSucc(string op)
        {
            new Validator(op)
                .OneParameter()
                .OrdinalOnTop()
                .Validate(this.stack);

            var x = this.Pop<IOrdinal>();
            this.Push(predSuccOps[op](x));
        }

        private void Pred_() => PredSucc("pred");

        private void Succ_() => PredSucc("succ");

        private void AddSub(string op)
        {
            new Validator(op)
                .TwoParameters()
                .TwoFloatsOrIntegers()
                .Validate(this.stack);

            var y = this.Pop<IFloatable>();
            var x = this.Pop<IFloatable>();
            this.stack.Push(floatBinaryOps[op](x, y));
        }

        private void Add_() => AddSub("+");

        private void Sub_() => AddSub("-");

        private void MaxMin(string op)
        {
            void MaxMinf(string op)
            {
                var y = this.Pop<IFloatable>();
                var x = this.Pop<IFloatable>();
                this.Push(floatBinaryOps[op](x, y));
            }

            var validator = new Validator(op);
            var floatp = validator.TwoFloatsOrIntegers();
            if (floatp.TryValidate(this.stack, out var ignored))
            {
                MaxMinf(op);
                return;
            }

            validator
                .SameTwoTypes()
                .OrdinalOnTop()
                .Validate(this.stack);

            var y = this.Pop<IOrdinal>();
            var x = this.Pop<IOrdinal>();
            this.Push(ordBinaryOps[op](x, y));
        }

        private void Min_() => MaxMin("min");

        private void Max_() => MaxMin("max");

        private void Comprel_(string op)
        {
            void Comprelf()
            {
                var y = this.Pop<IFloatable>();
                var x = this.Pop<IFloatable>();
                this.Push(floatCmpOps[op](x, y));
            }

            var validator = new Validator(op).TwoParameters();
            var floatp = validator.TwoFloatsOrIntegers();
            if (floatp.TryValidate(this.stack, out var ignored))
            {
                Comprelf();
                return;
            }

            validator.TwoOrdinalsOnTop().Validate(this.stack);
            var y = this.Pop<IOrdinal>();
            var x = this.Pop<IOrdinal>();
            this.Push(ordCmpOps[op](x, y));
        }

        private void Cmp_() => Comprel_("cmp");

        private void Eq_() => Comprel_("=");

        private void Ne_() => Comprel_("!=");

        private void Lt_() => Comprel_("<");

        private void Le_() => Comprel_("<=");

        private void Gt_() => Comprel_(">");

        private void Ge_() => Comprel_(">=");

        private void First_()
        {
            var validator = new Validator("first")
                .OneParameter()
                .NonEmptyAggregateOnTop()
                .Validate(this.stack);

            var x = this.Pop<IAggregate>();
            this.Push(x.First());
        }

        private void Rest_()
        {
            new Validator("rest")
                .OneParameter()
                .AggregateOnTop()
                .Validate(this.stack);

            var x = this.Pop<IAggregate>();
            this.Push(x.Rest());
        }

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

        private void Swons_()
        {
            new Validator("swons")
                .TwoParameters()
                .AggregateAsSecond()
                .Validate(this.stack);

            this.Swap_();
            this.Cons_();
        }

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

        private void Unswons_()
        {
            new Validator("unswons")
                .OneParameter()
                .NonEmptyAggregateOnTop()
                .Validate(this.stack);

            this.Uncons_();
            this.Swap_();
        }

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

        private void At_()
        {
            new Validator("at")
                .TwoParameters()
                .IntegerOnTop()
                .NonEmptyAggregateAsSecond()
                .Validate(this.stack);

            var i = this.Pop<Value.Int>();
            var a = this.Pop<IAggregate>();
            this.Push(a.At((int)i.Value));
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

        private void Null_()
        {
            new Validator("null")
                .OneParameter()
                .Validate(this.stack);

            var x = this.Pop<IValue>();
            this.Push(new Value.Bool(Value.IsNull(x)));
        }

        private void Equal_()
        {
            new Validator("equal")
                .TwoParameters()
                .Validate(this.stack);

            var y = this.Pop<IValue>();
            var x = this.Pop<IValue>();
            this.Push(new Value.Bool(x.Equals(y)));
        }

        private void TypeCheck<T>(string op)
            where T : IValue
        {
            new Validator(op)
                .OneParameter()
                .Validate(this.stack);

            var x = this.Pop<IValue>();
            this.Push(new Value.Bool(Value.IsType<T>(x)));
        }

        private void IsInt_() => TypeCheck<Value.Int>("integer");

        private void IsFloat_() => TypeCheck<Value.Float>("float");

        private void IsBool_() => TypeCheck<Value.Bool>("bool");

        private void IsChar_() => TypeCheck<Value.Char>("char");

        private void IsList_() => TypeCheck<Value.List>("list");

        private void IsString_() => TypeCheck<Value.String>("stringg");

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

        private void Execterm(Value.List p)
        {
            foreach (var fac in p.Elements)
            {
                this.Exec(fac);
            }
        }

        private void I_()
        {
            new Validator("i")
                .OneParameter()
                .OneQuote()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            this.Execterm(p);
        }

        private void X_()
        {
            new Validator("x")
                .OneParameter()
                .OneQuote()
                .Validate(this.stack);

            var p = this.Peek<Value.List>();
            this.Execterm(p);
        }

        private void Step_()
        {
            new Validator("step")
                .TwoParameters()
                .OneQuote()
                .AggregateAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var a = this.Pop<IAggregate>();
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
            }
        }

        private void Dip_()
        {
            new Validator("dip")
                .TwoParameters()
                .OneQuote()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var x = this.Pop();
            this.Push(p);
            this.I_();
            this.Push(x);
        }

        private void Nullary_()
        {
            new Validator("nullary")
                .OneQuote()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            this.Execterm(p);
            var r = this.Pop();
            this.stack = saved;
            this.Push(r);
        }

        private void Unary_()
        {
            new Validator("unary")
                .TwoParameters()
                .OneQuote()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            this.Execterm(p);
            var r = this.Pop();
            this.stack = saved;
            this.Pop(); // consume single parameter
            this.Push(r);
        }

        // Y Z [P] -> Y' Z'
        private void Unary2_()
        {
            new Validator("unary2")
                .ThreeParameters()
                .OneQuote()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var z = this.Pop();
            var y = this.Pop();
            var saved = this.stack.Clone();
            this.Push(y);
            this.Execterm(p);
            var py = this.Pop();
            this.stack = saved;
            this.Push(z);
            this.Execterm(p);
            var pz = this.Pop();
            this.stack = saved;
            this.stack.Push(py);
            this.stack.Push(pz);
        }

        private void Branch_()
        {
            new Validator("branch")
                .ThreeParameters()
                .TwoQuotes()
                .Validate(this.stack);

            var f = this.Pop<Value.List>();
            var t = this.Pop<Value.List>();
            var b = this.Pop();
            if (Value.IsTruthy(b))
            {
                this.Execterm(t);
            }
            else
            {
                this.Execterm(f);
            }
        }

        private void IfList_()
        {
            new Validator("iflist")
                .TwoParameters()
                .TwoQuotes()
                .Validate(this.stack);

            var f = this.Pop<Value.List>();
            var t = this.Pop<Value.List>();
            var x = this.Peek();
            if (x.Kind == ValueKind.List)
            {
                this.Execterm(t);
            }
            else
            {
                this.Execterm(f);
            }
        }

        private void Ifte_()
        {
            new Validator("ifte")
                .ThreeParameters()
                .ThreeQuotes()
                .Validate(this.stack);

            var f = this.Pop<Value.List>();
            var t = this.Pop<Value.List>();
            var b = this.Pop<Value.List>();

            // var xs = this.stack
            //     .Reverse()
            //     .Select(x => x.Clone());

            // var temp = new Stack<IValue>(xs);
            var saved = this.stack.Clone();
            this.Execterm(b);
            var pred = Value.IsTruthy(this.Pop());
            this.stack = saved;
            if (pred)
            {
                this.Execterm(t);
            }
            else
            {
                this.Execterm(f);
            }
        }

        private void Case_()
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
                var v = y.At(0);
                if (v.Equals(x))
                {
                    this.Push(y.At(1));
                    this.I_();
                    return;
                }
            }
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
                var k = y.At(0);
                if (k.Kind == x.Kind)
                {
                    this.Push(y.At(1));
                    return;
                }
            }
        }

        private void Map_()
        {
            new Validator("map")
                .TwoParameters()
                .OneQuote()
                .ListAsSecond()
                .Validate(this.stack);

            var results = new List<IValue>();
            var p = this.Pop<Value.List>();
            var a = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
                results.Add(this.Pop());
                this.stack = saved.Clone();
            }

            this.Push(new Value.List(results.ToArray()));
        }

        private void Filter_()
        {
            new Validator("filter")
                .TwoParameters()
                .OneQuote()
                .ListAsSecond()
                .Validate(this.stack);

            var results = new List<IValue>();
            var p = this.Pop<Value.List>();
            var a = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
                var r = this.Pop();
                if (Value.IsTruthy(r))
                {
                    results.Add(x);
                }

                this.stack = saved.Clone();
            }

            this.Push(new Value.List(results.ToArray()));
        }

        private void Some_()
        {
            new Validator("some")
                .TwoParameters()
                .OneQuote()
                .ListAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var a = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            IValue result = new Value.Bool(false);
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
                if (Value.IsTruthy(this.Pop()))
                {
                    result = new Value.Bool(true);
                    break;
                }

                this.Push(result);
            }

            this.Push(result);
        }

        private void All_()
        {
            new Validator("all")
                .TwoParameters()
                .OneQuote()
                .ListAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var a = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            IValue result = new Value.Bool(true);
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
                this.stack = saved.Clone();
                if (!Value.IsTruthy(this.Pop()))
                {
                    result = new Value.Bool(false);
                    break;
                }
            }

            this.Push(result);
        }

        private void Infra_()
        {
            new Validator("infra")
                .TwoParameters()
                .OneQuote()
                .ListAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var l1 = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            this.stack = new Stack<IValue>(l1.Elements.Reverse().ToArray());
            this.Execterm(p);
            var l2 = new Value.List(this.stack.ToArray());
            this.stack = saved;
            this.Push(l2);
        }

        private void Times_()
        {
            new Validator("times")
                .TwoParameters()
                .OneQuote()
                .IntegerAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var n = this.Pop<Value.Int>();
            for (var i = 0; i < n.Value; i++)
            {
                this.Execterm(p);
            }
        }

        private void Linrec_()
        {
            new Validator("linrec")
                .FourParameters()
                .FourQuotes()
                .Validate(this.stack);

            var r2 = this.Pop<Value.List>();
            var r1 = this.Pop<Value.List>();
            var t = this.Pop<Value.List>();
            var p = this.Pop<Value.List>();

            void Linrecaux()
            {
                var saved = this.stack.Clone();
                this.Execterm(p);
                var result = this.Pop();
                this.stack = saved;
                if (Value.IsTruthy(result))
                {
                    this.Execterm(t);
                }
                else
                {
                    this.Execterm(r1);
                    Linrecaux();
                    this.Execterm(r2);
                }
            }

            Linrecaux();
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

            if (this.usrdefs.TryGetValue(name, out var usr))
            {
                this.Push(usr);
                return;
            }

            var msg = $"undefined `{name}`";
            throw new RuntimeException(msg);
        }

        private void Peek_()
        {
            new Validator("peek")
                .OneParameter()
                .Validate(this.stack);

            var x = this.Peek();
            this.@out(this.stack.Count, x.ToString());
        }

        private void Put_()
        {
            new Validator("put")
                .OneParameter()
                .Validate(this.stack);

            var x = this.Pop();
            this.@out(this.stack.Count, x.ToString());
        }

        private void Puts_()
        {
            new Validator("puts")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var x = this.Pop<Value.String>();
            this.@out(this.stack.Count, x.Value);
        }

        private void Import_()
        {
            var s = this.Pop();
            var stream = s switch
            {
                Value.String x => File.OpenRead(x.Value),
                Value.Stream x => x.Value,
                _ => throw new NotSupportedException(),
            };

            using (var reader = new StreamReader(stream))
            {
                var source = reader.ReadToEnd();
                var tokens = Parser.Tokenizer.Tokenize(source);
                var defs = Parser.Def.Many().Parse(tokens);
                Array.ForEach(
                    defs.ToArray(),
                    x => this.AddUsrdef(x.Id, x.Body));
            }
        }

        public void Fopen_()
        {
            new Validator("fopen")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var path = this.Pop<Value.String>();
            var s = Value.Stream.Open(path.Value);
            this.stack.Push(s);
        }

        public void Freads_()
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

        public void Fclose_()
        {
            new Validator("fclose")
                .OneParameter()
                .StreamOnTop()
                .Validate(this.stack);

            var s = this.Pop<Value.Stream>();
            s.Close();
        }

        private void Time_()
        {
            var sw = new Stopwatch();
            var p = this.Pop<Value.List>();
            sw.Start();
            this.Execterm(p);
            sw.Stop();
            this.Push(new Value.Float(sw.Elapsed.TotalSeconds));
        }

        private void Error_()
        {
            new Validator("error")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var x = this.Pop<Value.String>();
            throw new RuntimeException(x.Value);
        }

        private void Defined_()
        {
            if (this.usrdefs.Count == 0)
            {
                return;
            }

            var names = this.usrdefs.Keys.OrderBy(x => x);
            var w = names.Max(x => x.Length);
            foreach (var name in names)
            {
                var def = this.usrdefs[name];
                var ss = def.Elements.Select(x => x.ToString());
                var s = string.Join(" ", ss);
                var info = $": {name} {s} ;";
                // var fmt = $"{{0,{-w}}}  :  {s}";
                // var info = string.Format(fmt, name);
                Console.WriteLine(info);
            }
        }

        private void Help_()
        {
            if (this.builtins.Count == 0)
            {
                return;
            }

            var names = this.builtins.Keys.OrderBy(x => x);
            var w = this.builtins.Max(x => x.Key.Length);
            foreach (var name in names)
            {
                var bi = this.builtins[name];
                var fmt = $"{{0,{-w}}}  :  {bi.Effect}";
                var info = string.Format(fmt, name);
                Console.WriteLine(info);
            }
        }

        private void Helpdetail_()
        {
            new Validator("?").AggregateOnTop().Validate(this.stack);
            var q = this.Pop<IAggregate>();
            var names = q.Elements.Select(x => x.ToString()).ToHashSet();
            var w = names.Max(x => x.Length);
            foreach (var name in names)
            {
                if (this.builtins.TryGetValue(name, out var bi))
                {
                    var fmt = $"{{0,{-w}}}  :  {bi.Effect}";
                    var info = string.Format(fmt, name);
                    Console.WriteLine(info);
                    foreach (var note in bi.Notes)
                    {
                        Console.WriteLine(note);
                    }

                    Console.WriteLine();
                }

                if (this.usrdefs.TryGetValue(name, out var def))
                {
                    var ss = def.Elements.Select(x => x.ToString());
                    var s = string.Join(" ", ss);
                    var fmt = $"{{0,{-w}}}  :  {s}";
                    var info = string.Format(fmt, name);
                    Console.WriteLine(info);
                }
            }
        }

        private IValue Peek() => this.stack.Peek();

        private T Peek<T>() where T : Value => (T)this.stack.Peek();

        private IValue Pop() => this.stack.Pop();

        private T Pop<T>() where T : IValue => (T)this.stack.Pop();

        private void Push(IValue value) => this.stack.Push(value);
    }
}