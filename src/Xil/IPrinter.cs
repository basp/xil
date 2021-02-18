namespace Xil
{
    using System;

    public interface IPrinter
    {
        void Error(Exception ex);

        void Info(string message);

        void Print(int stackSize, IValue value);

        void Print(int stackSize, string s);
    }
}