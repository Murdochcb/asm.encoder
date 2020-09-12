using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Registers
{
    internal sealed class EdiRegister : BaseRegister
    {
        static readonly byte[] _AddRegisterConstant = new byte[] { 0x81, 0xC7 };
        static readonly byte[] _SubRegisterConstant = new byte[] { 0x81, 0xEF };
        static readonly byte[] _XorRegisterConstant = new byte[] { 0x81, 0xF7 };
        static readonly byte[] _PushRegister = new byte[] { 0x57 };
        static readonly byte[] _PopRegister = new byte[] { 0x5F };
        static readonly byte[] _PushRegisterWithMovSub = new byte[] { 0x83, 0xEC, 0x04, 0x89, 0x3C, 0x24 };
        static readonly byte[] _PushRegisterWithMovDec = new byte[] { 0x4C, 0x4C, 0x4C, 0x4C, 0x89, 0x3C, 0x24 };
        static readonly byte[] _PopRegisterWithMovAdd = new byte[] { 0x8B, 0x3C, 0x24, 0x83, 0xC4, 0x04 };
        static readonly byte[] _PopRegisterWithMovInc = new byte[] { 0x8B, 0x3C, 0x24, 0x44, 0x44, 0x44, 0x44 };

        protected override byte[] AddRegisterConstant => _AddRegisterConstant;

        protected override byte[] SubRegisterConstant => _SubRegisterConstant;

        protected override byte[] XorRegisterConstant => _XorRegisterConstant;

        protected override byte[] PushRegister => _PushRegister;

        protected override byte[] PushRegisterWithMovSub => _PushRegisterWithMovSub;

        protected override byte[] PushRegisterWithMovDec => _PushRegisterWithMovDec;

        protected override byte[] PopRegister => _PopRegister;

        protected override byte[] PopRegisterWithMovAdd => _PopRegisterWithMovAdd;

        protected override byte[] PopRegisterWithMovInc => _PopRegisterWithMovInc;

        public EdiRegister(IEnumerable<byte> allowedBytes) : base(allowedBytes, RegisterType.EDI) { }
    }
}
