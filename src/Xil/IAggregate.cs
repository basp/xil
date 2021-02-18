namespace Xil
{
    using System.Collections.Generic;
    
    /// <summary>
    /// Provides operations that are applicable to aggregates
    /// which include <see cref="Value.List"/> and
    /// <see cref="Value.String"/> for example.
    /// </summary>
    public interface IAggregate : IValue, IEnumerable<IValue>
    {
        /// <summary>
        /// Gets the number of elements in the aggregate.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets the elements of the aggreate as a list.
        /// </summary>
        IList<IValue> Elements { get; }

        /// <summary>
        /// Returns the first element of the aggregate.
        /// </summary>
        IValue First();

        /// <summary>
        /// Returns everything except the first element of the aggregate.
        /// </summary>
        IAggregate Rest();

        /// <summary>
        /// Insert a value in the aggregate.
        /// </summary>
        /// <remarks>
        /// In the case of lists and strings the value will be placed 
        /// in front of the rest of the aggregate.
        /// </remarks>
        IValue Cons(IValue value);

        /// <summary>
        /// Obtain the first and rest of an aggregate in one operation.
        /// </summary>
        IValue Uncons(out IAggregate rest);

        /// <summary>
        /// Concatenates two aggregates into one.
        /// </summary>
        IValue Concat(IAggregate value);

        /// <summary>
        /// Returns a new aggregate with the first `n` elements omitted.
        /// </summary>
        IAggregate Drop(int n);

        /// <summary>
        /// Returns a new aggregate with only the first `n` elements.
        /// </summary>
        IAggregate Take(int n);

        /// <summary>
        /// Returns the element at index `i`.
        /// </summary>
        IValue Index(int i);
    }
}