namespace Xil
{
    using System;

    /// <summary>
    /// Specifies a builtin operation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BuiltinAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <c>BuiltinAttribute</c>
        /// class with the specified operation name.
        /// </summary>
        public BuiltinAttribute(string name)
            : this(name, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>BuiltinAttribute</c>
        /// class with the specified name and meta data.
        /// </summary>
        public BuiltinAttribute(
            string name,
            string effect,
            params string[] notes)
        {
            this.Name = name;
            this.Effect = effect;
            this.Notes = notes;
        }

        /// <summary>
        /// Gets the builtin operation name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the builtin operation stack effect description.
        /// </summary>
        public string Effect { get; }

        /// <summary>
        /// Gets the help notes on the builtin operation.
        /// </summary>
        public string[] Notes { get; }
    }
}