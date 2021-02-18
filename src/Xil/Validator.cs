namespace Xil
{
    using System;
    using System.Collections.Generic;

    public class Validator
    {
        private readonly string name;

        private readonly List<ValidationRule> rules =
            new List<ValidationRule>();

        public Validator(string name)
        {
            this.name = name;
        }

        private Validator(string name, List<ValidationRule> rules)
            : this(name)
        {
            this.rules = rules;
        }

        public static string GetErrorMessage(string name, string message) =>
            $"{message} needed for `{name}`";

        public static void ThrowBadData(string name) =>
            throw new RuntimeException(
                GetErrorMessage(name, "different type"));

        public static void ThrowBadAggregate(string name) =>
            throw new RuntimeException(
                GetErrorMessage(name, "aggregate"));

        public static void InternalList(string name, IValue value)
        {
            if (value is Value.List)
            {
                return;
            }

            var msg = GetErrorMessage(name, "internal list");
            throw new RuntimeException(msg);
        }
        
        public static bool Floatable(IValue[] xs) =>
            xs[0].Kind == ValueKind.Int || xs[0].Kind == ValueKind.Float;

        public static bool Floatable2(IValue[] xs) =>
            (xs[0].Kind == ValueKind.Float && xs[1].Kind == ValueKind.Float) ||
            (xs[0].Kind == ValueKind.Float && xs[1].Kind == ValueKind.Int) ||
            (xs[0].Kind == ValueKind.Int && xs[1].Kind == ValueKind.Float);

        public Validator OneParameter() =>
            this.AddRule(
                xs => xs.Length > 0,
                "one parameter");

        public Validator TwoParameters() =>
            this.AddRule(
                xs => xs.Length > 1,
                "two parameters");

        public Validator ThreeParameters() =>
            this.AddRule(
                xs => xs.Length > 2,
                "three parameters");

        public Validator FourParameters() =>
            this.AddRule(
                xs => xs.Length > 3,
                "four parameters");

        public Validator FiveParameters() =>
            this.AddRule(
                xs => xs.Length > 4,
                "five parameters");

        public Validator QuoteOnTop() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.List,
                "quotation as top parameter");

        public Validator TwoQuotes() => this.QuoteOnTop()
            .AddRule(
                xs => xs[1].Kind == ValueKind.List,
                "quotation as second parameter");

        public Validator ThreeQuotes() => this.TwoQuotes()
            .AddRule(
                xs => xs[2].Kind == ValueKind.List,
                "quotation as third parameter");

        public Validator FourQuotes() => this.ThreeQuotes()
            .AddRule(
                xs => xs[3].Kind == ValueKind.List,
                "quotation as fourth parameter");

        public Validator SameTwoTypes() =>
            this.AddRule(
                xs => xs[0].Kind == xs[1].Kind,
                "two parameters of the same type");

        public Validator SymbolOrStringOnTop() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.Symbol ||
                      xs[0].Kind == ValueKind.String,
                "string or symbol");

        public Validator StringOnTop() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.String,
                "string");

        public Validator StringAsSecond() =>
            this.AddRule(
                xs => xs[1].Kind == ValueKind.String,
                "string as second parameter");

        public Validator StreamOnTop() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.Stream,
                "stream");

        public Validator IntegerOnTop() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.Int,
                "integer");

        public Validator IntegerAsSecond() =>
            this.AddRule(
                xs => xs[1].Kind == ValueKind.Int,
                "integer as second parameter");

        public Validator CharacterOnTop() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.Char,
                "character");

        public Validator TwoIntegers() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.Int &&
                      xs[1].Kind == ValueKind.Int,
                "two integers");

        public Validator OrdinalOnTop() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.Int ||
                      xs[0].Kind == ValueKind.Char ||
                      xs[0].Kind == ValueKind.Bool,
                "ordinal");

        public Validator OrdinalAsSecond() =>
            this.AddRule(
                xs => xs[1].Kind == ValueKind.Int ||
                      xs[1].Kind == ValueKind.Char,
                "ordinal second parameter");

        public Validator TwoOrdinals() =>
            this.AddRule(
                xs => xs[0] is IOrdinal &&
                      xs[1] is IOrdinal,
                "two ordinal parameters");

        public Validator FloatOrIntegerOnTop() =>
            this.AddRule(Floatable, "float or integer");

        public Validator TwoFloatsOrIntegers() =>
            this.AddRule(
                xs => Floatable2(xs) ||
                      (xs[0].Kind == ValueKind.Int &&
                       xs[1].Kind == ValueKind.Int),
                "two floats or integers");

        public Validator NonZeroOnTop() =>
            this.AddRule(xs => !Value.IsZero(xs[0]), "non-zero divisor");

        public Validator AggregateOnTop() =>
            this.AddRule(xs => xs[0] is IAggregate, "aggregate");

        public Validator AggregateAsSecond() =>
            this.AddRule(
                xs => xs[1] is IAggregate,
                "aggregate as second parameter");

        public Validator TwoAggregates() =>
            this.AddRule(
                xs => xs[0] is IAggregate &&
                      xs[1] is IAggregate,
                "two aggregate parameters");

        public Validator NonEmptyAggregateOnTop() =>
            NonEmptyAggregateOnTop<IAggregate>();

        public Validator NonEmptyAggregateOnTop<T>()
            where T : IAggregate =>
                this.AddRule(xs =>
                {
                    if (xs[0] is T x)
                    {
                        return x.Size > 0;
                    }

                    return false;
                },
                "non-empty aggregate");

        public Validator NonEmptyAggregateAsSecond() =>
            NonEmptyAggregateAsSecond<IAggregate>();

        public Validator NonEmptyAggregateAsSecond<T>()
            where T : IAggregate =>
                this.AddRule(xs =>
                {
                    if (xs[1] is T x)
                    {
                        return x.Size > 0;
                    }

                    return false;
                },
                "non-empty aggregate as second parameter");

        public Validator NonEmptyListOnTop() =>
            this.AddRule(xs =>
            {
                if (xs[0] is Value.List x)
                {
                    return x.Size > 0;
                }

                return false;
            });

        public Validator NonEmptyStringOnTop() =>
            this.AddRule(xs =>
            {
                if (xs[0] is Value.String x)
                {
                    return x.Size > 0;
                }

                return false;
            });

        public Validator ListOnTop() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.List,
                "list");

        public Validator ListAsSecond() =>
            this.AddRule(
                xs => xs[1].Kind == ValueKind.List,
                "list as second parameter");

        public Validator ClrTypeOnTop() =>
            this.AddRule(
                xs => xs[0].Kind == ValueKind.ClrType,
                "clrtype");

        public Validator ClrTypeAsSecond() =>
            this.AddRule(
                xs => xs[1].Kind == ValueKind.ClrType,
                "clrtype as second parameter");

        public Validator AddRule(Func<IValue[], bool> p, string message = null)
        {
            var clone = new Validator(this.name, this.rules);
            clone.rules.Add(new ValidationRule(p, message));
            return clone;
        }

        public bool TryValidate(Stack<IValue> stack, out string error)
        {
            error = null;
            var xs = stack.ToArray();
            foreach (var rule in this.rules)
            {
                if (!rule.Apply(xs, out var type))
                {
                    error = GetErrorMessage(type);
                    return false;
                }
            }

            return true;
        }

        public Validator Validate(Stack<IValue> stack)
        {
            if (!this.TryValidate(stack, out var error))
            {
                throw new RuntimeException(error);
            }

            return this;
        }

        private string GetErrorMessage(string message) =>
            GetErrorMessage(this.name, message);
    }
}