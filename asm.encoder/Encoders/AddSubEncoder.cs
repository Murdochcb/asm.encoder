using asm.encoder.Registers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Encoders
{
    internal sealed class AddSubEncoder : BaseEncoder
    {
        public AddSubEncoder(Operation operation, IRegister register, IEnumerable<byte> allowedBytes) : base(operation, register, allowedBytes) { }

        public override AsmEncoding EncodeOperation(OpCode source, OpCode target)
        {
            if (!(Equals(this.operation, Operation.ADD) || Equals(this.operation, Operation.SUB)))
            {
                throw new ArgumentException($"{operation} is not a supported option. {nameof(AddSubEncoder)} can only perform {Operation.ADD} or {Operation.SUB} operations.");
            }

            if (this.register == null)
            {
                return null;
            }

            AsmEncoding encoding = new AsmEncoding(this.register, source, target);

            OpCode delta;
            switch (this.operation)
            {
                case Operation.ADD:
                    delta = encoding.Target - encoding.Intermediate;
                    break;
                case Operation.SUB:
                    delta = encoding.Intermediate - encoding.Target;
                    break;
                default:
                    throw new ArgumentException($"Unsupported operation: {this.operation}");
            }

            if (!this.TransitionExists(delta))
            {
                return null;
            }

            IEnumerable<Transition> transitions = this.BuildTransitions(delta);

            foreach (var transition in transitions)
            {
                encoding.Transitions.Add(transition);
            }

            return encoding;
        }

        protected override IEnumerable<Transition> BuildTransitions(OpCode delta)
        {
            if (OpCode.Zero.Equals(delta))
            {
                return Enumerable.Empty<Transition>();
            }

            ICollection<List<byte>> transitions = new List<List<byte>>();

            for (int transitionCount = 1; transitionCount <= this.map.Keys.Max(); transitionCount++)
            {
                if (this.TransitionCountExists(transitionCount, delta))
                {
                    OpCode deltaCarryAdjusted = new OpCode(delta.Code);

                    for (int i = 0; i < delta.Ops.Length; i++)
                    {
                        byte carry = 0;
                        for (int j = 0; j < i; j++)
                        {
                            int transitionSum = transitions.ElementAt(j).Sum(b => b) + carry;
                            carry = (byte)(transitionSum / (byte.MaxValue + 1));
                        }

                        deltaCarryAdjusted.Ops[i] = (byte)(deltaCarryAdjusted.Ops[i] - carry);
                        if (!this.TransitionCountByteExists(transitionCount, i, deltaCarryAdjusted))
                        {
                            break;
                        }

                        transitions.Add(this.map[transitionCount][deltaCarryAdjusted.Ops[i]].ToList());
                    }

                    if (this.TransitionCountExists(transitionCount, deltaCarryAdjusted))
                    {
                        break;
                    }
                    else
                    {
                        transitions.Clear();
                        continue;
                    }
                }
            }

            return this.ConvertMapTransitions(transitions);
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

                            byte sum = (byte)(allowed + pair.Key);

                            if (!tempMap.ContainsKey(sum))
                            {
                                tempMap[sum] = new List<byte>() { allowed }.Concat(pair.Value).ToList();
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
