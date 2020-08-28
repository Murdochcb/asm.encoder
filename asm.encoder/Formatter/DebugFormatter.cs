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
    }
}
