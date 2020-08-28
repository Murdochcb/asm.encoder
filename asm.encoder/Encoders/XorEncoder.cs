using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Encoders
{
    internal sealed class XorEncoder : BaseEncoder
    {
        private readonly Operation xorOperation;

        public XorEncoder(IEnumerable<byte> allowedBytes) : base (allowedBytes)
        {
            this.xorOperation = Operation.XOR;
        }

        public override AsmEncoding EncodeOperation(OpCode source, OpCode target, Operation operation)
        {
            if (operation != this.xorOperation)
            {
                throw new ArgumentException($"{operation} is not a supported option. {nameof(XorEncoder)} can only perform {this.xorOperation} operations.");
            }

            AsmEncoding encoding = new AsmEncoding(source, target);

            OpCode delta = encoding.Target ^ encoding.Intermediate;

            if (!this.TransitionExists(delta))
            {
                return null;
            }

            IEnumerable<Transition> transitions = this.BuildTransitions(operation, delta);

            foreach (var transition in transitions)
            {
                encoding.Transitions.Add(transition);
            }

            return encoding;
        }

        protected override IEnumerable<Transition> BuildTransitions(Operation operation, OpCode delta)
        {
            if (OpCode.Zero.Equals(delta.Code))
            {
                return Enumerable.Empty<Transition>();
            }

            ICollection<List<byte>> transitions = new List<List<byte>>();

            for (int transitionCount = 1; transitionCount <= this.map.Keys.Max(); transitionCount++)
            {
                if (this.TransitionCountExists(transitionCount, delta))
                {
                    for (int i = 0; i < delta.Ops.Length; i++)
                    {
                        transitions.Add(this.map[transitionCount][delta.Ops[i]].ToList());
                    }

                    break;
                }
            }

            return this.ConvertMapTransitions(operation, transitions);
        }

        protected override Dictionary<int, Dictionary<byte, IEnumerable<byte>>> BuildTransitionMap()
        {
            var result = new Dictionary<int, Dictionary<byte, IEnumerable<byte>>>();

            int transitionCount = 1;
            result[transitionCount] = new Dictionary<byte, IEnumerable<byte>>();
            foreach (byte allowed in this.allowedBytes)
            {
                result[transitionCount][allowed] = new List<byte>() { allowed };
            }

            Dictionary<byte, IEnumerable<byte>> tempMap = new Dictionary<byte, IEnumerable<byte>>();
            for (transitionCount = 2; transitionCount <= transitionCountLimit; transitionCount++)
            {
                tempMap.Clear();

                void BuildTransitionMap()
                {
                    foreach (byte allowed in this.allowedBytes)
                    {
                        foreach (var pair in result[transitionCount - 1])
                        {
                            if (tempMap.Count == byte.MaxValue + 1)
                            {
                                return;
                            }

                            byte xor = (byte)(allowed ^ pair.Key);

                            if (!tempMap.ContainsKey(xor))
                            {
                                tempMap[xor] = new List<byte>() { allowed }.Concat(pair.Value).ToList();
                            }
                        }
                    }
                }

                BuildTransitionMap();

                if (tempMap.Any())
                {
                    result[transitionCount] = tempMap.ToDictionary(pair => pair.Key, pair => pair.Value);
                }
            }

            return result;
        }
    }
}
