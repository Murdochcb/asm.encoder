using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Registers
{
    internal interface IRegister
    {
        RegisterType Type { get; }
        IEnumerable<Instruction> SupportedInstructions();
        RegisterCode GetRegisterCode(Instruction instruction);
    }
}
