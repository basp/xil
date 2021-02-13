namespace Xil
{
    using System;

    public class SystemTime : ITime
    {
        public long GetUnixTimeSeconds() =>
            DateTimeOffset.Now.ToUnixTimeSeconds();
    }
}