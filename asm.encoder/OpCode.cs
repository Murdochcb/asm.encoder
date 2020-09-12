using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder
{
    internal sealed class OpCode
    {
        public static readonly OpCode Zero = new OpCode(0x0);
        public byte[] Ops;
        public uint Code => BitConverter.ToUInt32(this.Ops, 0);

        public OpCode(uint code)
        {
            this.Ops = BitConverter.GetBytes(code);
        }

        public void ModifyOpCode(int bytePosition, byte value)
        {
            this.Ops[bytePosition] = value;
        }

        public static bool operator >(OpCode left, OpCode right)
        {
            return left.Code > right.Code;
        }

        public static bool operator <(OpCode left, OpCode right)
        {
            return left.Code < right.Code;
        }

        public static OpCode operator -(OpCode left, OpCode right)
        {
            return new OpCode(left.Code - right.Code);
        }

        public static OpCode operator ^(OpCode left, OpCode right)
        {
            return new OpCode(left.Code ^ right.Code);
        }

        public override Int32 GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + this.Code.GetHashCode();
                return hash;
            }
        }

        public bool Equals(OpCode other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Code == other.Code;
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
