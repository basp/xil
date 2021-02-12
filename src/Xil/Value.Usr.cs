namespace Xil
{
    public abstract partial class Value
    {
        public class Usr : Value
        {
            private readonly string id;

            private readonly Value.List body;

            public Usr(string id, params IValue[] body)
            {
                this.id = id;
                this.body = new Value.List(body);
            }

            public override ValueKind Kind => ValueKind.Def;

            public string Id => this.id;

            public Value.List Body => this.body;

            public override IValue Clone() => new Usr(this.id, this.body);
        }
    }
}