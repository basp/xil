namespace Xil
{
    using System;

    public class Builtin
    {
        public Builtin(Action op)
            : this(op, string.Empty, string.Empty)
        {
        }

        public Builtin(Action op, string effect, params string[] notes)
        {
            this.Op = op;
            this.Effect = effect;
            this.Notes = notes;
        }

        public Action Op { get; }

        public string Effect { get; }

        public string[] Notes { get; }
    }
}