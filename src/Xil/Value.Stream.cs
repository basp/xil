namespace Xil
{
    using System.IO;

    public abstract partial class Value
    {
        public class Stream : Value
        {
            private readonly string path;

            public Stream(string path, System.IO.Stream stream)
            {
                this.path = path;
                this.Value = stream;
            }

            public override ValueKind Kind => ValueKind.Stream;

            public System.IO.Stream Value { get; }

            public static Stream Open(string path) =>
                new Stream(path, File.Open(path, FileMode.Open));

            public override IValue Clone() =>
                new Value.Stream(this.path, this.Value);

            public void Close() => this.Value.Dispose();

            public override string ToString() => $"<IO: {this.path}>";
        }
    }
}