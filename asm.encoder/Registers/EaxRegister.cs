using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Registers
{
    internal sealed class EaxRegister : BaseRegister
    {
        static readonly byte[] _AddRegisterConstant = new byte[] { 0x05 };
        static readonly byte[] _SubRegisterConstant = new byte[] { 0x2D };
        static readonly byte[] _XorRegisterConstant = new byte[] { 0x35 };
        static readonly byte[] _PushRegister = new byte[] { 0x50 };
        static readonly byte[] _PushRegisterWithMovSub = new byte[] { 0x83, 0xEC, 0x04, 0x89, 0x04, 0x24 };
        static readonly byte[] _PushRegisterWithMovDec = new byte[] { 0x4C, 0x4C, 0x4C, 0x4C, 0x89, 0x04, 0x24 };
        static readonly byte[] _PopRegister = new byte[] { 0x58 };
        static readonly byte[] _PopRegisterWithMovAdd = new byte[] { 0x8B, 0x04, 0x24, 0x83, 0xC4, 0x04 };
        static readonly byte[] _PopRegisterWithMovInc = new byte[] { 0x8B, 0x04, 0x24, 0x44, 0x44, 0x44, 0x44 };

        protected override byte[] AddRegisterConstant => _AddRegisterConstant;

        protected override byte[] SubRegisterConstant => _SubRegisterConstant;

        protected override byte[] XorRegisterConstant => _XorRegisterConstant;

        protected override byte[] PushRegister => _PushRegister;

        protected override byte[] PushRegisterWithMovSub => _PushRegisterWithMovSub;

        protected override byte[] PushRegisterWithMovDec => _PushRegisterWithMovDec;

        protected override byte[] PopRegister => _PopRegister;

        protected override byte[] PopRegisterWithMovAdd => _PopRegisterWithMovAdd;

        protected override byte[] PopRegisterWithMovInc => _PopRegisterWithMovInc;

        public EaxRegister(IEnumerable<byte> allowedBytes) : base(allowedBytes, RegisterType.EAX) { }
    }
}
