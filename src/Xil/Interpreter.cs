namespace Xil
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Superpower;

    public partial class Interpreter : IInterpreter
    {
        private Random rng = new Random();

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

        private static IDictionary<Value.Symbol, IValue> wkSyms =
            new Dictionary<Value.Symbol, IValue>
            {
                [new Value.Symbol("PI")] = new Value.Float(Math.PI),
                [new Value.Symbol("E")] = new Value.Float(Math.E),
                [new Value.Symbol("true")] = new Value.Bool(true),
                [new Value.Symbol("false")] = new Value.Bool(false),
            };

        private readonly IDictionary<string, Value.List> usrDefs =
            new Dictionary<string, Value.List>();

        private readonly IPrinter printer;

        private const int IntitialStackSize = 128;

        private Stack<IValue> stack = new Stack<IValue>(IntitialStackSize);

        private Stack<IValue> saved;

        // will be initialized using reflection
        private IDictionary<string, Builtin> biOps;

        // abstract time functions for unit tests
        private ITime time;

        // abstract random functions for unit tests
        private IRandom random;

        // print action gets stack size and a repr string and
        // must be supplied by the interpreter host environment
        // private Action<int, string> print;

        private Interpreter(
            ITime time,
            IRandom random,
            IPrinter printer)
        {
            this.time = time;
            this.random = random;
            this.printer = printer;
        }

        /// <summary>
        /// Creates a new <see cref="IInterpreter"/> instance using
        /// system time and random functions.
        /// </summary>
        public static IInterpreter Create(IPrinter printer) =>
            Create(new SystemTime(), new SystemRandom(), printer);

        /// <summary>
        /// Creats a new <see cref="IInterpreter"/> instance with
        /// given <see cref="ITime"/> and <see cref="IRandom"/> implementations.
        /// </summary>
        public static IInterpreter Create(
            ITime time,
            IRandom random,
            IPrinter printer)
        {
            // we need this intrepeter instance to bind delegates later
            var interpreter = new Interpreter(time, random, printer);

            // creates an `Action` to hook into the bi dict
            Action CreateAction(MethodInfo method) =>
                (Action)Delegate.CreateDelegate(
                    typeof(Action),
                    interpreter,
                    method);

            // gets the builtin attribute that contains op info
            BuiltinAttribute GetBuiltinAttribute(MethodInfo method) =>
                method
                    .GetCustomAttributes(typeof(BuiltinAttribute), false)
                    .Cast<BuiltinAttribute>()
                    .FirstOrDefault();

            // find all ops decorated with the builtin attribute
            var ops = typeof(Interpreter)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(x => new
                {
                    Method = x,
                    Attr = GetBuiltinAttribute(x),
                })
                .Where(x => x.Attr != null);

            // initialize the builtin ops dictionary
            interpreter.biOps = ops
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

        public void Exec2(IValue value)
        {
            try
            {
                this.saved = this.stack.Clone();
                this.Exec(value);
            }
            catch(RuntimeException)
            {
                this.stack = this.saved;
            }
        }

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
                    this.usrDefs[def.Id] = def.Body;
                    break;
                case ValueKind.Symbol:
                    var sym = (Value.Symbol)value;

                    // first, try lookup in well-known symbols
                    if (wkSyms.TryGetValue(sym, out var x))
                    {
                        this.Push(x);
                        return;
                    }

                    // next, try builtin operations
                    if (this.biOps.TryGetValue(sym.Value, out var bi))
                    {
                        bi.Op();
                        return;
                    }

                    // finally, try user definitions
                    if (this.usrDefs.TryGetValue(sym.Value, out var usr))
                    {
                        this.Push(usr);
                        this.I_();
                        return;
                    }

                    var msg = $"undefined symbol `{sym.Value}`";
                    throw new RuntimeException(msg);
                default:
                    throw new NotSupportedException();
            }
        }

        public void AddDefinition(string name, Value.List body) =>
            this.usrDefs[name] = body;

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

        private void FloatMath(string op)
        {
            new Validator(op)
                .OneParameter()
                .FloatOrIntegerOnTop()
                .Validate(this.stack);

            var x = this.Pop<IFloatable>();
            this.stack.Push(floatUnaryOps[op](x));
        }

        private void PredSucc(string op)
        {
            new Validator(op)
                .OneParameter()
                .OrdinalOnTop()
                .Validate(this.stack);

            var x = this.Pop<IOrdinal>();
            this.Push(predSuccOps[op](x));
        }

        private void AddSub(string op)
        {
            new Validator(op)
                .TwoParameters()
                .TwoFloatsOrIntegers()
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

        private void Comprel(string op)
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

            validator.TwoOrdinals().Validate(this.stack);
            var y = this.Pop<IOrdinal>();
            var x = this.Pop<IOrdinal>();
            this.Push(ordCmpOps[op](x, y));
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

        private void Execterm(Value.List p)
        {
            foreach (var fac in p.Elements)
            {
                this.Exec(fac);
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