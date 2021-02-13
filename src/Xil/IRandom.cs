namespace Xil
{
    public interface IRandom
    {
        void Seed(int value);
        
        int Next();
    }
}