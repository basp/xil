namespace Xil
{
    public interface IValue : ILogical
    {
        ValueKind Kind { get; }

        // bool IsThruthy { get; }

        // bool IsZero { get; }

        // bool IsNull { get; }

        IValue Clone();

        object ToClrValue();
    }
}