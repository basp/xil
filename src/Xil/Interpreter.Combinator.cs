namespace Xil
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class Interpreter : IInterpreter
    {
        [Builtin(
            "i",
            "[P] -> ...",
            "Executes P. So, [P] i == P.")]
        private void I_()
        {
            new Validator("i")
                .OneParameter()
                .QuoteOnTop()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            this.Execterm(p);
        }

        [Builtin(
            "x",
            "[P] -> ...",
            "Executes P without popping [P]. So, [P] x == [P] P.")]
        private void X_()
        {
            new Validator("x")
                .OneParameter()
                .QuoteOnTop()
                .Validate(this.stack);

            var p = this.Peek<Value.List>();
            this.Execterm(p);
        }

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


        private void Nullary_()
        {
            new Validator("nullary")
                .QuoteOnTop()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            this.Execterm(p);
            var r = this.Pop();
            this.stack = saved;
            this.Push(r);
        }

        private void Unary_()
        {
            new Validator("unary")
                .TwoParameters()
                .QuoteOnTop()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            this.Execterm(p);
            var r = this.Pop();
            this.stack = saved;
            this.Pop(); // consume single parameter
            this.Push(r);
        }

        // Y Z [P] -> Y' Z'
        private void Unary2_()
        {
            new Validator("unary2")
                .ThreeParameters()
                .QuoteOnTop()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var z = this.Pop();
            var y = this.Pop();
            var saved = this.stack.Clone();
            this.Push(y);
            this.Execterm(p);
            var py = this.Pop();
            this.stack = saved;
            this.Push(z);
            this.Execterm(p);
            var pz = this.Pop();
            this.stack = saved;
            this.stack.Push(py);
            this.stack.Push(pz);
        }

        private void Branch_()
        {
            new Validator("branch")
                .ThreeParameters()
                .TwoQuotes()
                .Validate(this.stack);

            var f = this.Pop<Value.List>();
            var t = this.Pop<Value.List>();
            var b = this.Pop();
            if (Value.IsTruthy(b))
            {
                this.Execterm(t);
            }
            else
            {
                this.Execterm(f);
            }
        }

        private void Ifte_()
        {
            new Validator("ifte")
                .ThreeParameters()
                .ThreeQuotes()
                .Validate(this.stack);

            var f = this.Pop<Value.List>();
            var t = this.Pop<Value.List>();
            var b = this.Pop<Value.List>();

            // var xs = this.stack
            //     .Reverse()
            //     .Select(x => x.Clone());

            // var temp = new Stack<IValue>(xs);
            var saved = this.stack.Clone();
            this.Execterm(b);
            var pred = Value.IsTruthy(this.Pop());
            this.stack = saved;
            if (pred)
            {
                this.Execterm(t);
            }
            else
            {
                this.Execterm(f);
            }
        }        

        private void IfList_()
        {
            new Validator("iflist")
                .TwoParameters()
                .TwoQuotes()
                .Validate(this.stack);

            var f = this.Pop<Value.List>();
            var t = this.Pop<Value.List>();
            var x = this.Peek();
            if (x.Kind == ValueKind.List)
            {
                this.Execterm(t);
            }
            else
            {
                this.Execterm(f);
            }
        }

        private void Linrec_()
        {
            new Validator("linrec")
                .FourParameters()
                .FourQuotes()
                .Validate(this.stack);

            var r2 = this.Pop<Value.List>();
            var r1 = this.Pop<Value.List>();
            var t = this.Pop<Value.List>();
            var p = this.Pop<Value.List>();

            void Linrecaux()
            {
                var saved = this.stack.Clone();
                this.Execterm(p);
                var result = this.Pop();
                this.stack = saved;
                if (Value.IsTruthy(result))
                {
                    this.Execterm(t);
                }
                else
                {
                    this.Execterm(r1);
                    Linrecaux();
                    this.Execterm(r2);
                }
            }

            Linrecaux();
        }

        private void Step_()
        {
            new Validator("step")
                .TwoParameters()
                .QuoteOnTop()
                .AggregateAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var a = this.Pop<IAggregate>();
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
            }
        }

        private void Map_()
        {
            new Validator("map")
                .TwoParameters()
                .QuoteOnTop()
                .ListAsSecond()
                .Validate(this.stack);

            var results = new List<IValue>();
            var p = this.Pop<Value.List>();
            var a = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
                results.Add(this.Pop());
                this.stack = saved.Clone();
            }

            this.Push(new Value.List(results.ToArray()));
        }

        private void Times_()
        {
            new Validator("times")
                .TwoParameters()
                .QuoteOnTop()
                .IntegerAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var n = this.Pop<Value.Int>();
            for (var i = 0; i < n.Value; i++)
            {
                this.Execterm(p);
            }
        }

        private void Infra_()
        {
            new Validator("infra")
                .TwoParameters()
                .QuoteOnTop()
                .ListAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var l1 = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            this.stack = new Stack<IValue>(l1.Elements.Reverse().ToArray());
            this.Execterm(p);
            var l2 = new Value.List(this.stack.ToArray());
            this.stack = saved;
            this.Push(l2);
        }

        private void Filter_()
        {
            new Validator("filter")
                .TwoParameters()
                .QuoteOnTop()
                .ListAsSecond()
                .Validate(this.stack);

            var results = new List<IValue>();
            var p = this.Pop<Value.List>();
            var a = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
                var r = this.Pop();
                if (Value.IsTruthy(r))
                {
                    results.Add(x);
                }

                this.stack = saved.Clone();
            }

            this.Push(new Value.List(results.ToArray()));
        }


        private void Some_()
        {
            new Validator("some")
                .TwoParameters()
                .QuoteOnTop()
                .ListAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var a = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            IValue result = new Value.Bool(false);
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
                if (Value.IsTruthy(this.Pop()))
                {
                    result = new Value.Bool(true);
                    break;
                }

                this.Push(result);
            }

            this.Push(result);
        }

        private void All_()
        {
            new Validator("all")
                .TwoParameters()
                .QuoteOnTop()
                .ListAsSecond()
                .Validate(this.stack);

            var p = this.Pop<Value.List>();
            var a = this.Pop<Value.List>();
            var saved = this.stack.Clone();
            IValue result = new Value.Bool(true);
            foreach (var x in a.Elements)
            {
                this.Push(x);
                this.Execterm(p);
                this.stack = saved.Clone();
                if (!Value.IsTruthy(this.Pop()))
                {
                    result = new Value.Bool(false);
                    break;
                }
            }

            this.Push(result);
        }
    }
}