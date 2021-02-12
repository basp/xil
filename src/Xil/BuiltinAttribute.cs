namespace Xil
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class BuiltinAttribute : Attribute
    {
        public BuiltinAttribute(string name)
            : this(name, string.Empty)
        {
        }

        public BuiltinAttribute(
            string name,
            string effect,
            params string[] notes)
        {
            this.Name = name;
            this.Effect = effect;
            this.Notes = notes;
        }

        public string Name { get; }

        public string Effect { get; }

        public string[] Notes { get; }
    }
}