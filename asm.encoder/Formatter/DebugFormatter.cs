using asm.encoder.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Formatter
{
    internal sealed class DebugFormatter : BaseFormatter
    {
        public const string FormatName = "Debug";
        public override string Format(OpCode opCode, Endian endian)
        {
            IEnumerable<byte> ops = opCode.Ops;
            switch (endian)
            {
                case Endian.Little:
                    if (!BitConverter.IsLittleEndian)
                    {
                        ops = ops.Reverse();
                    }
                    break;
                case Endian.Big:
                    if (BitConverter.IsLittleEndian)
                    {
                        ops = ops.Reverse();
                    }
                    break;
                default:
                    throw new ArgumentException($"Unsupported {nameof(Endian)} Type: {endian}");
            }

            return $"0x{ops.Select(b => $"{b:X2}").Aggregate((x, acc) => x + acc)}";
        }

        public override string Format(Transition transition, Endian endian)
        {
            string opCodeFormat = this.Format(transition.Delta, endian);

            switch (transition.Operation)
            {
                case Operation.ADD:
                    return $"{this.Format(transition.Register.GetRegisterCode(Instruction.AddRegCon))} {opCodeFormat}";
                case Operation.SUB:
                    return $"{this.Format(transition.Register.GetRegisterCode(Instruction.SubRegCon))} {opCodeFormat}";
                case Operation.XOR:
                    return $"{this.Format(transition.Register.GetRegisterCode(Instruction.XorRegCon))} {opCodeFormat}";
                default:
                    throw new ArgumentException($"The operation {transition.Operation} is not supported.");
            }
        }

        public override string Format(RegisterCode registerCode)
        {
            return $"{registerCode.Ops.Select(b => $"{b:X2}").Aggregate((x, acc) => x + acc)}";
        }
    }
}
