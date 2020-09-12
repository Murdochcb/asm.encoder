using asm.encoder.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Formatter
{
    internal abstract class BaseFormatter : IFormatter
    {
        protected const Endian defaultEndian = Endian.Little;
        public virtual string Format(AsmEncoding encoding)
        {
            return this.Format(encoding, defaultEndian);
        }

        public virtual string Format(AsmEncoding encoding, Endian endian)
        {
            StringBuilder builder = new StringBuilder();
#if DEBUG
            builder.AppendLine($"{this.Format(encoding.Source, Endian.Big)} -> {this.Format(encoding.Target, Endian.Big)}");
#endif
            for (int i = 0; i < encoding.Transitions.Count; i++)
            {
                builder.AppendLine(this.Format(encoding.Transitions.ElementAt(i), endian));
            }

            builder.AppendLine($"{this.Format(encoding.Register.GetRegisterCode(Instruction.PushReg))}");

            return builder.ToString();
        }

        public virtual string Format(OpCode opCode)
        {
            return this.Format(opCode, defaultEndian);
        }

        public abstract string Format(OpCode opCode, Endian endian);

        public virtual string Format(Transition transition)
        {
            return this.Format(transition, defaultEndian);
        }

        public abstract string Format(Transition transition, Endian endian);

        public abstract string Format(RegisterCode registerCode);
    }
}
