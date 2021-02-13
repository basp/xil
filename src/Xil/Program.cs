namespace Xil
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Superpower;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine($"Xil 0.8 on {RuntimeInformation.FrameworkDescription}");
            var interpreter = Interpreter.Create(Print);
            while (true)
            {
                var stack = interpreter.GetStack();
                Prompt(stack.Length);
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
                        interpreter.AddDefinition(x.Id, x.Body);
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
                    var msg = $"Runtime error: {ex.Message}";
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

        private static void Prompt(int stackSize)
        {
            Console.Write("[");
            Console.Write(stackSize);
            Console.Write("]");
            Console.Write(" < ");
        }

        private static void Print(int stackSize, string value)
        {
            Console.Write("[");
            Console.Write(stackSize);
            Console.Write("]");
            Console.Write(" > ");
            Console.WriteLine(value);
        }
    }
}