namespace Xil
{
    using System;

    public class SystemRandom : IRandom
    {
        private Random rng = new Random();

        public int Next() => this.rng.Next();

        public void Seed(int value) => 
            this.rng = new Random(value);
    }
}