namespace Xil
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions
    {
        public static T Pop<T>(this Stack<IValue> self)
            where T : IValue => (T)self.Pop();

        public static Stack<IValue> Clone(this Stack<IValue> self)
        {
            // cloning a stack is a bit awkward since we need to
            // reverse all the elements before we create a new one
            var xs = self.Select(x => x.Clone()).Reverse().ToArray();
            return new Stack<IValue>(xs);
        }
    }
}