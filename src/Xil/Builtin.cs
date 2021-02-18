namespace Xil
{
    using System;

    /// <summary>
    /// Container for built-in operations and their
    /// associated meta data.
    /// </summary>
    public class Builtin
    {
        /// <summary>
        /// Constructs a new <c>Builtin</c> instance
        /// for the given operation.
        /// </summary>
        public Builtin(Action op)
            : this(op, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Constructs a new <c>Builtin</c> instance
        /// for the given operation including supplied meta data.
        /// </summary>
        public Builtin(Action op, string effect, params string[] notes)
        {
            this.Op = op;
            this.Effect = effect;
            this.Notes = notes;
        }

        /// <summary>
        /// Gets the built-in operation.
        /// </summary>
        public Action Op { get; }

        /// <summary>
        /// Gets the effect description.
        /// </summary>
        public string Effect { get; }

        /// <summary>
        /// Gets the notes on this operation.
        /// </summary>
        public string[] Notes { get; }
    }
}