namespace Xil
{
    public interface IValue
    {
        ValueKind Kind { get; }

        // bool IsThruthy { get; }

        // bool IsZero { get; }

        // bool IsNull { get; }

        IValue Clone();

        IValue Not();

        IValue And(IValue value);

        IValue Or(IValue value);

        IValue Xor(IValue value);
    }
}