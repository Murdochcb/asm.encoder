using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Formatter
{
    internal interface IFormatter
    {
        string Format(OpCode opCode);
        string Format(OpCode opCode, Endian endian);
        string Format(Transition transition);
        string Format(Transition transition, Endian endian);
        string Format(AsmEncoding encoding);
        string Format(AsmEncoding encoding, Endian endian);
    }
}
