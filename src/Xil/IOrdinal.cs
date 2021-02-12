namespace Xil
{
    public interface IOrdinal : IValue
    {
        int OrdinalValue { get; }

        int CompareTo(IOrdinal value);

        IValue Ord();

        IValue Chr();

        IValue Succ();

        IValue Pred();

        IValue Min(IOrdinal value);

        IValue Max(IOrdinal value);
    }
}