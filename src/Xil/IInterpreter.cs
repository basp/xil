namespace Xil
{
    public interface IInterpreter
    {
        IValue Peek();

        T Peek<T>() where T : IValue;

        IValue Pop();

        T Pop<T>() where T : IValue;

        void Push(IValue value);

        IValue[] GetStack();

        void Exec(IValue value);

        void Exec2(IValue value);

        void AddDefinition(string name, Value.List body);
    }
}