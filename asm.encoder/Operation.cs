using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder
{
    [Flags]
    internal enum Operation
    {
        NONE = 0x0,
        ADD = 0x1,
        SUB = 0x2,
        XOR = 0x4
    }
}
