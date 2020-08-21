using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Encoders
{
    internal abstract class BaseEncoder
    {
        protected abstract IEnumerable<byte> BuildPartialTransition(int transitionCount, byte delta);
        protected abstract IEnumerable<Transition> BuildTransitions(Operation operation, OpCode delta, int transitionCount);
        public abstract AsmEncoding EncodeOperation(OpCode source, OpCode target, Operation operation);
        protected abstract Dictionary<Byte, IEnumerable<Byte>> BuildMap();
    }
}
