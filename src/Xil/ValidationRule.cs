namespace Xil
{
    using System;

    internal class ValidationRule
    {
        private readonly Func<IValue[], bool> predicate;

        private readonly string message;

        public ValidationRule(Func<IValue[], bool> predicate, string message)
        {
            this.predicate = predicate;
            this.message = message;
        }

        public bool Apply(IValue[] stack, out string error)
        {
            error = null;
            if (!this.predicate(stack))
            {
                error = this.message;
                return false;
            }

            return true;
        }
    }
}