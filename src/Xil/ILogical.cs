namespace Xil
{
    public interface ILogical
    {
        IValue Not();

        IValue And(IValue value);

        IValue Or(IValue value);

        IValue Xor(IValue value);        
    }
}