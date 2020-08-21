using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86.asm.encoder.Encoder
{
    internal interface IEncoder
    {
        IEnumerable<byte> BuildPartialTransition(int transitionCount, byte delta);
        IEnumerable<Transition> BuildTransitions(Operation operation, OpCode delta, int transitionCount);
        AsmEncoding EncodeOperation(OpCode source, OpCode target, Operation operation);
    }
}
