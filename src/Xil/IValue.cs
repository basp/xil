namespace Xil
{
    public interface IValue
    {
        ValueKind Kind { get; }

        IValue Clone();

        IValue Not();

        IValue And(IValue value);

        IValue Or(IValue value);

        IValue Xor(IValue value);
    }
}