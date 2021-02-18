namespace Xil
{
    using System;

    public class ConsolePrinter : IPrinter
    {
        public void Error(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void Print(int stackSize, string s) =>
            Console.WriteLine($"[{stackSize}] > {s}");

        public void Print(int stackSize, IValue value)
        {
            Console.WriteLine($"[{stackSize}] > {value}");
        }
    }
}