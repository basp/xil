namespace Xil
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Superpower;

    public partial class Interpreter : IInterpreter
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

        private static IDictionary<string, Func<Value.Int, Value.Int, IValue>> intBinaryOps =
            new Dictionary<string, Func<Value.Int, Value.Int, IValue>>
            {
                ["+"] = (x, y) => x + y,
                ["-"] = (x, y) => x - y,
                ["*"] = (x, y) => x * y,
                ["/"] = (x, y) => x / y,
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

        private const int IntitialStackSize = 128;

        private Stack<IValue> stack = new Stack<IValue>(IntitialStackSize);

        private readonly IDictionary<string, Value.List> usrdefs =
            new Dictionary<string, Value.List>();

        private IDictionary<string, Builtin> builtins;

        private Action<int, string> print;

        private Interpreter(Action<int, string> print)
        {
            this.print = print;
        }

        public static IInterpreter Create(Action<int, string> @out)
        {
            var interpreter = new Interpreter(@out);

            Action CreateAction(MethodInfo method) =>
                (Action)Delegate.CreateDelegate(
                    typeof(Action),
                    interpreter,
                    method);

            BuiltinAttribute GetAttribute(MethodInfo method) =>
                method
                    .GetCustomAttributes(typeof(BuiltinAttribute), false)
                    .Cast<BuiltinAttribute>()
                    .FirstOrDefault();

            var ops = typeof(Interpreter)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(x => new
                {
                    Method = x,
                    Attr = GetAttribute(x),
                })
                .Where(x => x.Attr != null);

            interpreter.builtins = ops
                .Select(x => new
                {
                    Attr = x.Attr,
                    Action = CreateAction(x.Method),
                })
                .ToDictionary(
                    keySelector: x => x.Attr.Name,
                    elementSelector: x => new Builtin(
                        x.Action,
                        x.Attr.Effect,
                        x.Attr.Notes));

            return interpreter;
        }

        public IValue Peek() => this.stack.Peek();

        public T Peek<T>() where T : IValue => (T)this.stack.Peek();

        public IValue Pop() => this.stack.Pop();

        public T Pop<T>() where T : IValue => (T)this.stack.Pop();

        public void Push(IValue value) => this.stack.Push(value);

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
                        if (l.Any() && l.All(x => x is Value.Char))
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
                    var def = (Value.Definition)value;
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

        public void AddDefinition(string name, Value.List body) =>
            this.usrdefs[name] = body;

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

        private void AndOrXor(string op)
        {
            new Validator(op)
                .TwoParameters()
                .Validate(this.stack);

            var y = this.Pop<Value>();
            var x = this.Pop<Value>();
            this.Push(andOrXorOps[op](x, y));
        }

        private void OrdChr(string op)
        {
            new Validator(op)
                .OneParameter()
                .OrdinalOnTop();

            var x = this.Pop<IOrdinal>();
            this.Push(ordChrOps[op](x));
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
            var v = Convert.ToInt32(s.Value, (int)i.Value);
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
                .FloatOrIntegerOnTop()
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
                .TwoFloatsOrIntegersOnTop()
                .Validate(this.stack);

            var s = this.GetStack();
            if (TwoIntsOnTop())
            {
                var y = this.Pop<Value.Int>();
                var x = this.Pop<Value.Int>();
                this.stack.Push(intBinaryOps[op](x, y));
            }
            else
            {
                var y = this.Pop<IFloatable>();
                var x = this.Pop<IFloatable>();
                this.stack.Push(floatBinaryOps[op](x, y));
            }
        }

        private void MaxMin(string op)
        {
            void MaxMinf(string op)
            {
                var y = this.Pop<IFloatable>();
                var x = this.Pop<IFloatable>();
                this.Push(floatBinaryOps[op](x, y));
            }

            var validator = new Validator(op);
            var floatp = validator.TwoFloatsOrIntegersOnTop();
            if (floatp.TryValidate(this.stack, out var ignored))
            {
                MaxMinf(op);
                return;
            }

            validator
                .SameTwoTypesOnTop()
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
            var floatp = validator.TwoFloatsOrIntegersOnTop();
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

        private void Concat_()
        {
            new Validator("concat")
                .TwoParameters()
                .TwoAggregatesOnTop()
                .SameTwoTypesOnTop()
                .Validate(this.stack);

            var y = this.Pop<IAggregate>();
            var x = this.Pop<IAggregate>();
            this.Push(x.Concat(y));
        }

        private void Enconcat_()
        {
            new Validator("enconcat")
                .ThreeParameters()
                .SameTwoTypesOnTop()
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
                .QuoteOnTop()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            this.Execterm(p);
        }

        private void X_()
        {
            new Validator("x")
                .OneParameter()
                .QuoteOnTop()
                .Validate(this.stack);

            var p = this.Peek<Value.List>();
            this.Execterm(p);
        }

        private void Step_()
        {
            new Validator("step")
                .TwoParameters()
                .QuoteOnTop()
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

        private void Nullary_()
        {
            new Validator("nullary")
                .QuoteOnTop()
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
                .QuoteOnTop()
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
                .QuoteOnTop()
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
                .TwoQuotesOnTop()
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
                .TwoQuotesOnTop()
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
                .ThreeQuotesOnTop()
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
                var v = y.Index(0);
                if (v.Equals(x))
                {
                    this.Push(y.Index(1));
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
                var k = y.Index(0);
                if (k.Kind == x.Kind)
                {
                    this.Push(y.Index(1));
                    return;
                }
            }
        }

        private void Map_()
        {
            new Validator("map")
                .TwoParameters()
                .QuoteOnTop()
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
                .QuoteOnTop()
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
                .QuoteOnTop()
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
                .QuoteOnTop()
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
                .QuoteOnTop()
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
                .QuoteOnTop()
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
                .FourQuotesOnTop()
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
            this.print(this.stack.Count, x.ToString());
        }

        private void Puts_()
        {
            new Validator("puts")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var x = this.Pop<Value.String>();
            this.print(this.stack.Count, x.Value);
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
                    x => this.AddDefinition(x.Id, x.Body));
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


        [Builtin("stack")]
        private void Stack_()
        {
            var xs = this.stack.ToArray();
            this.Push(new Value.List(xs));
        }

        [Builtin("unstack")]
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

        [Builtin(
            "help",
            "->",
            "List builtin definitions.")]
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

        [Builtin(
            "?",
            "Q ->",
            "Show help on symbols in quotation Q.")]
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

        private bool TwoIntsOnTop()
        {
            var s = this.GetStack();
            return Value.IsType<Value.Int>(s[0])
                && Value.IsType<Value.Int>(s[1]);
        }
    }
}