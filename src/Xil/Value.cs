namespace Xil
{
    public abstract partial class Value : IValue
    {
        public static bool IsZero(IValue value) =>
            value switch
            {
                Value.Bool x => x.Value ? false : true,
                Value.Int x => x.Value == 0,
                Value.Float x => x.Value == 0,
                _ => false,
            };

        public static bool IsTruthy(IValue value) =>
            value switch
            {
                Value.Bool x => x.Value,
                Value.Int x => !IsZero(x),
                Value.Float x => !IsZero(x),
                Value.List x => x.Elements.Count > 0,
                _ => true,
            };

        public static bool IsNull(IValue value) =>
            value switch
            {
                IAggregate a => a.Size == 0,
                IValue v => Value.IsZero(v),
            };

        public static bool IsType<T>(IValue v)
            where T : IValue => v is T;

        public abstract ValueKind Kind { get; }

        public abstract IValue Clone();

        public virtual IValue Not() =>
            new Value.Bool(!IsTruthy(this));

        public virtual IValue And(IValue value) =>
            new Value.Bool(IsTruthy(this) && IsTruthy(value));

        public virtual IValue Or(IValue value) =>
            new Value.Bool(IsTruthy(this) || IsTruthy(value));

        public virtual IValue Xor(IValue value) =>
            new Value.Bool(IsTruthy(this) ^ IsTruthy(value));
    }
}