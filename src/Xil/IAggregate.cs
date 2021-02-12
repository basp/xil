namespace Xil
{
    using System.Collections.Generic;
    
    public interface IAggregate : IValue, IEnumerable<IValue>
    {
        int Size { get; }

        IList<IValue> Elements { get; }

        IValue First();

        IAggregate Rest();

        IValue Cons(IValue value);

        IValue Uncons(out IAggregate rest);

        IValue Concat(IAggregate value);

        IAggregate Drop(int n);

        IAggregate Take(int n);

        IValue Index(int i);
    }
}