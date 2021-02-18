namespace Xil
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Superpower;

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
            this.printer.Print(this.stack.Count, x);
        }


        [Builtin(
            "defined",
            "->",
            "Prints all user definitions.")]
        private void Defined_()
        {
            if (this.usrDefs.Count == 0)
            {
                return;
            }

            var names = this.usrDefs.Keys.OrderBy(x => x);
            var w = names.Max(x => x.Length);
            foreach (var name in names)
            {
                var def = this.usrDefs[name];
                var ss = def.Elements.Select(x => x.ToString());
                var s = string.Join(" ", ss);
                var info = $": {name} {s} ;";
                // var fmt = $"{{0,{-w}}}  :  {s}";
                // var info = string.Format(fmt, name);
                Console.WriteLine(info);
            }
        }

        [Builtin(
            "newstack",
            "... ->",
            "Clears the current stack.")]
        private void Newstack_()
        {
            this.stack = new Stack<IValue>(IntitialStackSize);
        }

        [Builtin(
            "help",
            "->",
            "List builtin definitions.")]
        private void Help_()
        {
            if (this.biOps.Count == 0)
            {
                return;
            }

            var names = this.biOps.Keys.OrderBy(x => x);
            var w = this.biOps.Max(x => x.Key.Length);
            foreach (var name in names)
            {
                var bi = this.biOps[name];
                var fmt = $"{{0,{-w}}}  :  {bi.Effect}";
                var info = string.Format(fmt, name);
                Console.WriteLine(info);
            }
        }

        [Builtin(
            "?",
            "Q ->",
            "Show help on symbols in quotation Q.")]
        private void Helpdetail_()
        {
            new Validator("?").AggregateOnTop().Validate(this.stack);
            var q = this.Pop<IAggregate>();
            var names = q.Elements.Select(x => x.ToString()).ToHashSet();
            var w = names.Max(x => x.Length);
            foreach (var name in names)
            {
                if (this.biOps.TryGetValue(name, out var bi))
                {
                    var fmt = $"{{0,{-w}}}  :  {bi.Effect}";
                    var info = string.Format(fmt, name);
                    Console.WriteLine(info);
                    foreach (var note in bi.Notes)
                    {
                        Console.WriteLine(note);
                    }

                    Console.WriteLine();
                }

                if (this.usrDefs.TryGetValue(name, out var def))
                {
                    var ss = def.Elements.Select(x => x.ToString());
                    var s = string.Join(" ", ss);
                    var fmt = $"{{0,{-w}}}  :  {s}";
                    var info = string.Format(fmt, name);
                    Console.WriteLine(info);
                }
            }
        }

        [Builtin(
            "peek",
            "->",
            "Prints the string representation of the top of",
            "the stack without popping it.")]
        private void Peek_()
        {
            new Validator("peek")
                .OneParameter()
                .Validate(this.stack);

            var x = this.Peek();
            this.printer.Print(this.stack.Count, x);
        }

        [Builtin(
            "puts",
            "S ->",
            "S is a string on top of the stack.",
            "Prints the string S to the standard output.")]
        private void Puts_()
        {
            new Validator("puts")
                .OneParameter()
                .StringOnTop()
                .Validate(this.stack);

            var x = this.Pop<Value.String>();
            this.printer.Print(this.stack.Count, x.Value);
        }

        [Builtin(
            "import",
            "F ->",
            "F is either a path string or a stream.",
            "Reads file F as a string and parses it as a set of definitions.")]
        private void Import_()
        {
            var s = this.Pop();
            var stream = s switch
            {
                Value.String x => File.OpenRead(x.Value),
                Value.Stream x => x.Value,
                _ => throw new NotSupportedException(),
            };

            using (var reader = new StreamReader(stream))
            {
                var source = reader.ReadToEnd();
                var tokens = Parser.Tokenizer.Tokenize(source);
                var defs = Parser.Def.Many().Parse(tokens);
                Array.ForEach(
                    defs.ToArray(),
                    x => this.AddDefinition(x.Id, x.Body));
            }
        }
    }
}