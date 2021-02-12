namespace Xil
{
    public partial class Interpreter : IInterpreter
    {
        [Builtin(
            "dip",
            "X [P] -> ... X",
            "Saves X, executes P, pushes X back.")]     
        private void Dip_()
        {
            new Validator("dip")
                .TwoParameters()
                .QuoteOnTop()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var x = this.Pop();
            this.Push(p);
            this.I_();
            this.Push(x);
        }   
    }
}