using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder.Registers
{
    internal sealed class RegisterCode
    {
        public const string MemoryPlaceholder = "[MEMORY]";
        public const string RegisterPlaceholder = "[REGISTER]";
        public const string ValuePlaceholder = "[VALUE]";
        public string Comment { get; }
        public Instruction Instruction { get; }
        public IEnumerable<byte> Ops { get; }

        public RegisterCode(Instruction instruction, byte[] ops, string comment)
        {
            this.Instruction = instruction;

            if (ops == null)
            {
                throw new ArgumentNullException(nameof(ops));
            }

            if (!ops.Any())
            {
                throw new ArgumentException($"{nameof(this.Ops)} cannot be empty.");
            }

            this.Ops = ops.ToList();
            this.Comment = comment;
        }

        public override Int32 GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + this.Instruction.GetHashCode();
                hash = hash * 23 + this.Ops.GetHashCode();
                return hash;
            }
        }

        public bool Equals(RegisterCode other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.Ops.Count() != other.Ops.Count())
            {
                return false;
            }

            return Equals(this.Instruction, other.Instruction) &&
                this.Ops.SequenceEqual(other.Ops);
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

            return this.Equals(obj as OpCode);
        }
    }
}
