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

        void Begin();

        void Commit();        

        void Rollback();

        void AddDefinition(string name, Value.List body);
    }
}