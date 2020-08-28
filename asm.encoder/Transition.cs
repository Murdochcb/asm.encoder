using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder
{
    internal sealed class Transition
    {
        public Operation Operation { get; }
        public OpCode Delta { get; private set; }
        public Transition(Operation operation, OpCode delta)
        {
            this.Operation = operation;
            this.Delta = delta;
        }

        public override Int32 GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + this.Operation.GetHashCode();
                hash = hash * 23 + this.Delta.GetHashCode();
                return hash;
            }
        }

        public bool Equals(Transition other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Operation == other.Operation &&
                Equals(this.Delta, other.Delta);
        }

        public override Boolean Equals(Object obj)
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
