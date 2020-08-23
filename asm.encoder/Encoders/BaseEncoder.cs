using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Encoders
{
    internal abstract class BaseEncoder
    {
        protected const int transitionCountLimit = 10;
        protected readonly IEnumerable<Byte> allowedBytes;
        protected readonly Dictionary<int, Dictionary<Byte, IEnumerable<Byte>>> map;

        public BaseEncoder(IEnumerable<byte> allowedBytes)
        {
            this.allowedBytes = allowedBytes ?? throw new ArgumentNullException(nameof(allowedBytes));

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

        protected IEnumerable<Transition> ConvertMapTransitions(Operation operation, IEnumerable<IEnumerable<byte>> transitions)
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

                result.Add(new Transition(operation, step));
            }

            return result;
        }

        protected abstract IEnumerable<Transition> BuildTransitions(Operation operation, OpCode delta);
        public abstract AsmEncoding EncodeOperation(OpCode source, OpCode target, Operation operation);
        protected abstract Dictionary<int, Dictionary<Byte, IEnumerable<Byte>>> BuildTransitionMap();
    }
}
