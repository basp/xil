namespace Xil
{
    public partial class Interpreter : IInterpreter
    {
        
        [Builtin(
            "put",
            "X ->",
            "Writes X to output, pops X off the stack.")]
        private void Put_()
        {
            new Validator("put")
                .OneParameter()
                .Validate(this.stack);

            var x = this.Pop();
            this.@out(this.stack.Count, x.ToString());
        }
    }
}