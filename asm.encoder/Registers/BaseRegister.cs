using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Registers
{
    internal abstract class BaseRegister : IRegister
    {
        protected const string AddRegisterComment = "ADD " + RegisterCode.RegisterPlaceholder + ", " + RegisterCode.ValuePlaceholder;
        protected const string SubRegisterComment = "SUB " + RegisterCode.RegisterPlaceholder + ", " + RegisterCode.ValuePlaceholder;
        protected const string XorRegisterComment = "XOR " + RegisterCode.RegisterPlaceholder + ", " + RegisterCode.ValuePlaceholder;
        protected const string PushRegisterComment = "PUSH " + RegisterCode.RegisterPlaceholder;
        protected const string PushRegisterWithMovSubComment = "SUB ESP, 0x4; MOV DWORD PTR SS:[ESP], " + RegisterCode.RegisterPlaceholder;
        protected const string PushRegisterWithMovDecComment = "DEC ESP (*4); MOV DWORD PTR SS:[ESP], " + RegisterCode.RegisterPlaceholder;
        protected const string PopRegisterComment = "POP " + RegisterCode.RegisterPlaceholder;
        protected const string PopRegisterWithMovAddComment = "MOV " + RegisterCode.RegisterPlaceholder + ", DWORD PTR SS:[ESP]; ADD ESP, 0x4";
        protected const string PopRegisterWithMovIncComment = "MOV " + RegisterCode.RegisterPlaceholder + ", DWORD PTR SS:[ESP]; INC ESP (*4)";
        public RegisterType Type { get; }

        protected readonly IEnumerable<byte> allowedBytes;
        protected readonly ISet<RegisterCode> registerCodes;
        protected abstract byte[] AddRegisterConstant { get; }
        protected abstract byte[] SubRegisterConstant { get; }
        protected abstract byte[] XorRegisterConstant { get; }
        protected abstract byte[] PushRegister { get; }
        protected abstract byte[] PushRegisterWithMovSub { get; }
        protected abstract byte[] PushRegisterWithMovDec { get; }
        protected abstract byte[] PopRegister { get; }
        protected abstract byte[] PopRegisterWithMovAdd { get; }
        protected abstract byte[] PopRegisterWithMovInc { get; }

        public BaseRegister(IEnumerable<byte> allowedBytes, RegisterType type)
        {
            this.allowedBytes = allowedBytes ?? throw new ArgumentNullException(nameof(allowedBytes));

            if (!this.allowedBytes.Any())
            {
                throw new ArgumentException($"{nameof(this.allowedBytes)} cannot be empty.");
            }

            this.Type = type;

            this.registerCodes = new HashSet<RegisterCode>();
            this.UpdateRegisterCodes();
        }

        public IEnumerable<Instruction> SupportedInstructions()
        {
            return this.registerCodes.Select(r => r.Instruction);
        }

        public RegisterCode GetRegisterCode(Instruction instruction)
        {
            RegisterCode registerCode = this.registerCodes.SingleOrDefault(r => Equals(r.Instruction, instruction));

            if (registerCode == null)
            {
                throw new ArgumentException($"The {instruction} is not supported.");
            }

            return registerCode;
        }

        private void UpdateRegisterCodes()
        {
            if (this.AddRegisterConstant.All(b => allowedBytes.Contains(b)))
            {
                this.registerCodes.Add(new RegisterCode(Instruction.AddRegCon, this.AddRegisterConstant, AddRegisterComment));
            }

            if (this.SubRegisterConstant.All(b => allowedBytes.Contains(b)))
            {
                this.registerCodes.Add(new RegisterCode(Instruction.SubRegCon, this.SubRegisterConstant, SubRegisterComment));
            }

            if (this.XorRegisterConstant.All(b => allowedBytes.Contains(b)))
            {
                this.registerCodes.Add(new RegisterCode(Instruction.XorRegCon, this.XorRegisterConstant, XorRegisterComment));
            }

            if (this.PushRegister.All(b => allowedBytes.Contains(b)))
            {
                this.registerCodes.Add(new RegisterCode(Instruction.PushReg, this.PushRegister, PushRegisterComment));
            }
            else if (this.PushRegisterWithMovSub.All(b => allowedBytes.Contains(b)))
            {
                this.registerCodes.Add(new RegisterCode(Instruction.PushReg, this.PushRegisterWithMovSub, PushRegisterWithMovSubComment));
            }
            else if (this.PushRegisterWithMovDec.All(b => allowedBytes.Contains(b)))
            {
                this.registerCodes.Add(new RegisterCode(Instruction.PushReg, this.PushRegisterWithMovDec, PushRegisterWithMovDecComment));
            }

            if (this.PopRegister.All(b => allowedBytes.Contains(b)))
            {
                this.registerCodes.Add(new RegisterCode(Instruction.PopReg, this.PopRegister, PopRegisterComment));
            }
            else if (this.PopRegisterWithMovAdd.All(b => allowedBytes.Contains(b)))
            {
                this.registerCodes.Add(new RegisterCode(Instruction.PopReg, this.PopRegisterWithMovAdd, PopRegisterWithMovAddComment));
            }
            else if (this.PopRegisterWithMovInc.All(b => allowedBytes.Contains(b)))
            {
                this.registerCodes.Add(new RegisterCode(Instruction.PopReg, this.PopRegisterWithMovInc, PopRegisterWithMovIncComment));
            }
        }

        public override Int32 GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + this.Type.GetHashCode();
                hash = hash * 23 + this.allowedBytes.GetHashCode();
                hash = hash * 23 + this.registerCodes.GetHashCode();
                return hash;
            }
        }

        public bool Equals(BaseRegister other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.registerCodes.Count != other.registerCodes.Count)
            {
                return false;
            }

            return this.Type == other.Type &&
                this.registerCodes.SequenceEqual(other.registerCodes) &&
                this.allowedBytes.SequenceEqual(other.allowedBytes);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return this.Equals(obj as BaseRegister);
        }
    }
}
