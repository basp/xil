namespace Xil
{
    public abstract partial class Value
    {
        public class Definition : Value
        {
            private readonly string id;

            private readonly Value.List body;

            public Definition(string id, params IValue[] body)
            {
                this.id = id;
                this.body = new Value.List(body);
            }

            public override ValueKind Kind => ValueKind.Def;

            public string Id => this.id;

            public Value.List Body => this.body;

            public override IValue Clone() => new Definition(this.id, this.body);
        }
    }
}