namespace Xil
{
    public interface IFloatable : IValue
    {
        Value.Float AsFloat();

        IValue Rem(IValue value);

        IValue Add(IValue value);

        IValue Sub(IValue value);

        IValue Mul(IValue value);

        IValue Divide(IValue value);

        IValue Acos();

        IValue Asin();

        IValue Atan();

        IValue Ceil();

        IValue Cos();

        IValue Cosh();

        IValue Exp();

        IValue Floor();

        IValue Log();

        IValue Log10();

        IValue Sin();

        IValue Sinh();

        IValue Sqrt();

        IValue Tan();

        IValue Tanh();

        IValue Min(IValue value);

        IValue Max(IValue value);

        int CompareTo(IValue value);
    }
}