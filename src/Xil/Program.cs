namespace Xil
{
    using System;
    using System.Linq;
    using Superpower;

    internal class Program
    {
        private static void In(int size)
        {
            Console.Write("[");
            Console.Write(size);
            Console.Write("]");
            Console.Write(" < ");
        }

        private static void Out(int size, string value)
        {
            Console.Write("[");
            Console.Write(size);
            Console.Write("]");
            Console.Write(" > ");
            Console.WriteLine(value);
        }

        private static void Main(string[] args)
        {
            var interpreter = new Interpreter(Out);
            while (true)
            {
                var stack = interpreter.GetStack();
                In(stack.Length);
                var source = Console.ReadLine();
                try
                {
                    var tokens = Parser.Tokenizer.Tokenize(source);
                    if (!tokens.Any())
                    {
                        continue;
                    }

                    if (tokens.First().Kind == TokenKind.Colon)
                    {
                        var x = Parser.Def.Parse(tokens);
                        interpreter.AddUsrdef(x.Id, x.Body);
                        continue;
                    }

                    var term = Parser.Term.Parse(tokens);
                    foreach (var fac in term)
                    {
                        interpreter.Exec(fac);
                    }
                }
                catch (RuntimeException ex)
                {
                    var msg = $"Runtime error: {ex.Message}.";
                    Console.WriteLine(msg);
                }
                catch (ParseException ex)
                {
                    var msg = ex.Message;
                    Console.WriteLine(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}