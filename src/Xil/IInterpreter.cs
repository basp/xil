namespace Xil
{
    /// <summary>
    /// Allows an object to act as an interpreter.
    /// </summary>
    public interface IInterpreter
    {
        /// <summary>
        /// Get the value on top of the stack without removing it.
        /// </summary>
        IValue Peek();

        /// <summary>
        /// Same as <c>Peek()</c> but used when a certain type is expected.
        /// </summary>
        T Peek<T>() where T : IValue;

        /// <summary>
        /// Remove the value on top of the stack and return it.
        /// </summary>
        IValue Pop();

        /// <summary>
        /// Same as <c>Pop()</c> but used when a certain type is expected.
        /// </summary>
        T Pop<T>() where T : IValue;

        /// <summary>
        /// Push a value onto the stack.
        /// </summary>
        void Push(IValue value);

        /// <summary>
        /// Get the stack as an array. The top value of the stack will 
        /// be the first element in the result.
        /// </summary>
        IValue[] GetStack();

        /// <summary>
        /// Execute a single term, represented as a <c>IValue</c> instance.
        /// </summary>
        void Exec(IValue value);

        /// <summary>
        /// Starts a new transactional block. This saves the stack until 
        /// either <c>Commit</c> or <c>Rollback</c> is called.
        /// </summary>
        /// <remarks>
        /// It is allowed to call <c>Begin</c> multiple times without 
        /// matching <c>Commit</c> or <c>Rollback</c> calls. In this
        /// case, each call to <c>Begin</c> will update the rollback stack
        /// with the current stack at the point of invocation.
        /// </remars>
        void Begin();

        /// <summary>
        /// Commits a transactional block that was started with 
        /// a call to <c>Begin</c>.
        /// </summary>
        /// <remarks>
        /// It is allowed to call <c>Commit</c> without a matching call
        /// to <c>Begin</c>. In this case, only the rollback stack will
        /// be cleared.
        /// </remarks>
        void Commit();        

        /// <summary>
        /// Rolls back a transactional block that was started with
        /// a call to <c>Begin</c>.
        /// </summary>
        /// <remarks>
        /// It is allowed to call <c>Rollback</c> without a matching call
        /// to <c>Begin</c>. In this case, <c>Rollback</c> does nothing.
        /// </remarks>
        void Rollback();

        /// <summary>
        /// Add a runtime definition to the interpreter userdefs.
        /// </summary>
        void AddDefinition(string name, Value.List body);
    }
}