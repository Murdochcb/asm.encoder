using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder
{
    internal sealed class AsmEncoding
    {
        public OpCode Source { get; }
        public OpCode Target { get; }
        public ICollection<Transition> Transitions { get; }
        public OpCode Intermediate
        {
            get
            {
                uint current = this.Source.Code;
                foreach (Transition transition in this.Transitions)
                {
                    switch (transition.Operation)
                    {
                        case Operation.ADD:
                            current += transition.Delta.Code;
                            break;
                        case Operation.SUB:
                            current -= transition.Delta.Code;
                            break;
                        case Operation.XOR:
                            current ^= transition.Delta.Code;
                            break;
                    }
                }

                return new OpCode(current);
            }
        }

        public AsmEncoding(OpCode source, OpCode target)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Target = target ?? throw new ArgumentNullException(nameof(target));
            this.Transitions = new List<Transition>();
        }

        public AsmEncoding DeepCopy()
        {
            AsmEncoding copy = new AsmEncoding(this.Source, this.Target);
            for (int i = 0; i < this.Transitions.Count; i++)
            {
                Transition transition = this.Transitions.ElementAt(i);
                copy.Transitions.Add(new Transition(transition.Operation, transition.Delta));
            }

            return copy;
        }

        public void ModifyEncodingTransition(int index, int bytePosition, byte value)
        {
            Transition transition = this.Transitions.ElementAt(index);
            transition.Delta.ModifyOpCode(bytePosition, value);
        }

        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
#if DEBUG
            builder.AppendLine($"{this.Source.ToString(false)} -> {this.Target.ToString(false)}");
#endif
            for (int i = 0; i < this.Transitions.Count; i++)
            {
                builder.AppendLine(this.Transitions.ElementAt(i).ToString());
            }

            builder.AppendLine("PUSH <REG>");

            return builder.ToString();
        }
    }
}
