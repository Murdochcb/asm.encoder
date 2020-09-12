using asm.encoder.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Formatter
{
    internal sealed class PythonFormatter : BaseFormatter
    {
        public const string FormatName = "Python";
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

            return $"{ops.Select(b => $"\\x{b:X2}").Aggregate((x, acc) => x + acc)}";
        }

        public override string Format(Transition transition, Endian endian)
        {
            string opCodeFormat = this.Format(transition.Delta, endian);
            return this.FormatTransitionWithComment(transition, opCodeFormat);
        }

        public override string Format(AsmEncoding encoding, Endian endian)
        {
            StringBuilder builder = new StringBuilder();
#if DEBUG
            builder.AppendLine($"# {this.Format(encoding.Source, Endian.Big)} -> {this.Format(encoding.Target, Endian.Big)}");
#endif
            for (int i = 0; i < encoding.Transitions.Count; i++)
            {
                builder.AppendLine($"stage += {this.Format(encoding.Transitions.ElementAt(i), endian)}");
            }

            RegisterCode pushRegister = encoding.Register.GetRegisterCode(Instruction.PushReg);

            builder.AppendLine($"stage += \"{this.Format(pushRegister)}\" # {pushRegister.Comment.Replace(RegisterCode.RegisterPlaceholder, encoding.Register.Type.ToString())}");

            return builder.ToString();
        }

        public override string Format(RegisterCode registerCode)
        {
            return $"{registerCode.Ops.Select(b => $"\\x{b:X2}").Aggregate((x, acc) => x + acc)}";
        }

        private string FormatTransitionWithComment(Transition transition, string opCodeFormat)
        {
            Instruction instruction;
            switch (transition.Operation)
            {
                case Operation.ADD:
                    instruction = Instruction.AddRegCon;
                    break;
                case Operation.SUB:
                    instruction = Instruction.SubRegCon;
                    break;
                case Operation.XOR:
                    instruction = Instruction.XorRegCon;
                    break;
                default:
                    throw new ArgumentException($"The operation {transition.Operation} is not supported.");
            }

            string bigEndianOpCode = $"0x{this.Format(transition.Delta, Endian.Big).Replace("\\x", string.Empty)}";
            RegisterCode register = transition.Register.GetRegisterCode(instruction);
            return $"\"{this.Format(register)}{opCodeFormat}\" # {register.Comment.Replace(RegisterCode.RegisterPlaceholder, transition.Register.Type.ToString()).Replace(RegisterCode.ValuePlaceholder, bigEndianOpCode)}";
        }
    }
}
