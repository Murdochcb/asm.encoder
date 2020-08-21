using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x86.asm.encoder.Encoders
{
    internal sealed class XorEncoder : BaseEncoder
    {
        private readonly Dictionary<Byte, IEnumerable<Byte>> map;
        private readonly IEnumerable<Byte> allowedBytes;
        private readonly Operation xorOperation;

        public XorEncoder(IEnumerable<Byte> allowedBytes)
        {
            this.xorOperation = Operation.XOR;

            this.allowedBytes = allowedBytes ?? throw new ArgumentNullException(nameof(allowedBytes));

            if (!this.allowedBytes.Any())
            {
                throw new ArgumentException($"{nameof(this.allowedBytes)} cannot be empty.");
            }

            this.map = this.BuildMap();
        }

        public override AsmEncoding EncodeOperation(OpCode source, OpCode target, Operation operation)
        {
            AsmEncoding encoding = new AsmEncoding(source, target);

            OpCode delta = encoding.Target ^ encoding.Intermediate;

            if (!delta.Ops.All(b => this.map.ContainsKey(b)))
            {
                return null;
            }

            int transitionCount = delta.Ops.Select(b => this.map[b].Count()).Max();
            IEnumerable<Transition> transitions = this.BuildTransitions(operation, delta, transitionCount);

            if (!transitions.Any())
            {
                throw new InvalidOperationException($"Cannot produce encoding with current restrictions.");
            }

            foreach (var transition in transitions)
            {
                encoding.Transitions.Add(transition);
            }

            return encoding;
        }

        protected override IEnumerable<Byte> BuildPartialTransition(int transitionCount, byte delta)
        {
            Dictionary<byte, IEnumerable<byte>> filteredMap = this.map.Where(pair => pair.Value.Count() < transitionCount).ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var pairOne in filteredMap)
            {
                foreach (var pairTwo in filteredMap)
                {
                    byte xor = (byte)(pairOne.Key ^ pairTwo.Key);
                    if (xor == delta && (pairOne.Value.Count() + pairTwo.Value.Count()) == transitionCount)
                    {
                        return pairOne.Value.Concat(pairTwo.Value).ToList();
                    }
                }
            }

            return Enumerable.Empty<byte>();
        }

        protected override IEnumerable<Transition> BuildTransitions(Operation operation, OpCode delta, int transitionCount)
        {
            if (transitionCount > 10)
            {
                return Enumerable.Empty<Transition>();
            }

            ICollection<List<byte>> transitions = new List<List<byte>>();

            for (int i = 0; i < delta.Ops.Length; i++)
            {
                if (this.map[delta.Ops[i]].Count() == transitionCount)
                {
                    transitions.Add(this.map[delta.Ops[i]].ToList());
                }
                else
                {
                    IEnumerable<byte> deltaOperation = this.BuildPartialTransition(transitionCount, delta.Ops[i]);
                    if (deltaOperation.Any())
                    {
                        transitions.Add(deltaOperation.ToList());
                    }
                    else
                    {
                        return this.BuildTransitions(this.xorOperation, delta, transitionCount + 1);
                    }
                }
            }

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

                result.Add(new Transition(this.xorOperation, step));
            }

            return result;
        }

        protected override Dictionary<Byte, IEnumerable<Byte>> BuildMap()
        {
            var result = new Dictionary<Byte, IEnumerable<Byte>>();

            foreach (byte allowed in this.allowedBytes)
            {
                result[allowed] = new List<byte>() { allowed };
            }

            Dictionary<byte, IEnumerable<byte>> tempMap = new Dictionary<byte, IEnumerable<byte>>();
            do
            {
                tempMap.Clear();
                foreach (byte allowed in this.allowedBytes)
                {
                    foreach (var pair in result)
                    {
                        byte xor = (byte)(allowed ^ pair.Key);
                        if (!result.ContainsKey(xor) && !tempMap.ContainsKey(xor))
                        {
                            tempMap[xor] = new List<byte>() { allowed }.Concat(pair.Value).ToList();
                        }
                    }
                }

                result = result.Concat(tempMap).ToDictionary(pair => pair.Key, pair => pair.Value);
            } while (tempMap.Any());

            return result;
        }
    }
}
