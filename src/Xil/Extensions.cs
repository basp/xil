namespace Xil
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class Extensions
    {
        public static T Pop<T>(this Stack<IValue> self)
            where T : IValue => (T)self.Pop();

        public static Stack<IValue> Clone(this Stack<IValue> self)
        {
            // cloning a stack is a awkward since we need to
            // reverse all the elements before we create a new one;
            // if we don't our clone will end up reversed from our
            // original
            var xs = self.Select(x => x.Clone()).Reverse().ToArray();
            return new Stack<IValue>(xs);
        }
    }
}