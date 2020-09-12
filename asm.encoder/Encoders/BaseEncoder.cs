using asm.encoder.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Encoders
{
    internal abstract class BaseEncoder
    {
        protected const int transitionCountLimit = 10;
        protected readonly Operation operation;
        protected readonly IRegister register;
        protected readonly IEnumerable<byte> allowedBytes;
        protected readonly Dictionary<int, Dictionary<byte, IEnumerable<byte>>> map;

        public BaseEncoder(Operation operation, IRegister register, IEnumerable<byte> allowedBytes)
        {
            this.operation = operation;
            this.allowedBytes = allowedBytes ?? throw new ArgumentNullException(nameof(allowedBytes));
            this.register = register ?? throw new ArgumentNullException(nameof(register));

            if (!this.allowedBytes.Any())
            {
                throw new ArgumentException($"{nameof(this.allowedBytes)} cannot be empty.");
            }

            this.map = this.BuildTransitionMap();
        }

        protected bool TransitionExists(OpCode delta)
        {
            for (int transitionCount = 1; transitionCount <= this.map.Keys.Max(); transitionCount++)
            {
                if (this.TransitionCountExists(transitionCount, delta))
                {
                    return true;
                }
            }

            return false;
        }

        protected bool TransitionCountExists(int transitionCount, OpCode delta)
        {
            return delta.Ops.All(b => this.map[transitionCount].ContainsKey(b));
        }

        protected bool TransitionCountByteExists(int transitionCount, int index, OpCode delta)
        {
            return this.map[transitionCount].ContainsKey(delta.Ops[index]);
        }

        protected IEnumerable<Transition> ConvertMapTransitions(IEnumerable<IEnumerable<byte>> transitions)
        {
            if (!transitions.Any())
            {
                throw new InvalidOperationException($"Cannot produce encoding with current restrictions.");
            }

            ICollection<Transition> result = new List<Transition>();
            for (int i = 0; i < transitions.Select(t => t.Count()).Max(); i++)
            {
                OpCode step = new OpCode(BitConverter.ToUInt32(new byte[]
                {
                        transitions.ElementAt(0).ElementAt(i),
                        transitions.ElementAt(1).ElementAt(i),
                        transitions.ElementAt(2).ElementAt(i),
                        transitions.ElementAt(3).ElementAt(i)
                }, 0));

                result.Add(new Transition(this.operation, this.register, step));
            }

            return result;
        }

        protected abstract IEnumerable<Transition> BuildTransitions(OpCode delta);
        public abstract AsmEncoding EncodeOperation(OpCode source, OpCode target);
        protected abstract Dictionary<int, Dictionary<byte, IEnumerable<byte>>> BuildTransitionMap();
    }
}
