namespace Xil
{
    using System;

    [Flags]
    public enum ValueKind
    {
        None = 0,

        Symbol = (1 << 0),

        Int = (1 << 1),

        Float = (1 << 2),

        Bool = (1 << 3),

        Char = (1 << 4),

        String = (1 << 5),

        List = (1 << 6),

        Def = (1 << 7),

        Stream = (1 << 8),

        ClrType = (1 << 9),

        ClrMethod = (1 << 10),

        ClrMember = (1 << 11),

        ClrKind = ClrType | ClrMember | ClrMethod
    }
}