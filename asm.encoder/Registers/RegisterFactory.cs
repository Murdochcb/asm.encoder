using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Registers
{
    internal static class RegisterFactory
    {
        private static IEnumerable<IRegister> GetRegisters(IEnumerable<byte> allowedBytes)
        {
            yield return new EaxRegister(allowedBytes);
            yield return new EbpRegister(allowedBytes);
            yield return new EbxRegister(allowedBytes);
            yield return new EcxRegister(allowedBytes);
            yield return new EdiRegister(allowedBytes);
            yield return new EdxRegister(allowedBytes);
            yield return new EsiRegister(allowedBytes);
        }

        public static IEnumerable<IRegister> GetEncodingRegisters(Operation operationFlags, IEnumerable<byte> allowedBytes)
        {
            IEnumerable<IRegister> encodingRegisters = RegisterFactory.GetRegisters(allowedBytes);
            foreach (var register in encodingRegisters)
            {
                if (!register.SupportedInstructions().Where(s => Equals(s, Instruction.PushReg)).Any())
                {
                    continue;
                }

                if (operationFlags.HasFlag(Operation.ADD))
                {
                    if (!register.SupportedInstructions().Where(s => Equals(s, Instruction.AddRegCon)).Any())
                    {
                        continue;
                    }
                }

                if (operationFlags.HasFlag(Operation.SUB))
                {
                    if (!register.SupportedInstructions().Where(s => Equals(s, Instruction.SubRegCon)).Any())
                    {
                        continue;
                    }
                }

                if (operationFlags.HasFlag(Operation.XOR))
                {
                    if (!register.SupportedInstructions().Where(s => Equals(s, Instruction.XorRegCon)).Any())
                    {
                        continue;
                    }
                }

                yield return register;
            }
        }
    }
}
