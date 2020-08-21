using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using asm.encoder;
using asm.encoder.Encoders;
using System.Linq;

namespace asm.test
{
    [TestClass]
    public class EncodersTest
    {
        const int tenThousandTestCases = 10000;

        [TestMethod]
        public void IntermediateEncodingEqualsTargetAllAllowedBytesTenThousandRandomChanges()
        {
            ICollection<byte> allowedBytes = new List<byte>();
            for (int i = 0; i < byte.MaxValue + 1; i++)
            {
                allowedBytes.Add((byte)i);
            }

            this.PerformTestLoop(allowedBytes, tenThousandTestCases);
        }

        [TestMethod]
        public void IntermediateEncodingEqualsTargetHighAllowedBytesTenThousandRandomChanges()
        {
            ICollection<byte> allowedBytes = new List<byte>();
            for (int i = 0x80; i < byte.MaxValue + 1; i++)
            {
                allowedBytes.Add((byte)i);
            }

            this.PerformTestLoop(allowedBytes, tenThousandTestCases);
        }

        [TestMethod]
        public void IntermediateEncodingEqualsTargetLowAllowedBytesTenThousandRandomChanges()
        {
            
            ICollection<byte> allowedBytes = new List<byte>();
            for (int i = 0x00; i < 0x80; i++)
            {
                allowedBytes.Add((byte)i);
            }

            this.PerformTestLoop(allowedBytes, tenThousandTestCases);
        }

        [TestMethod]
        public void AddOperationWithCarrySucceeds()
        {
            ICollection<byte> allowedBytes = new List<byte>();
            for (int i = 0x80; i < byte.MaxValue + 1; i++)
            {
                allowedBytes.Add((byte)i);
            }

            BaseEncoder addSubEncoder = new AddSubEncoder(allowedBytes);

            OpCode source = new OpCode((uint)0x40A96593);
            OpCode target = new OpCode((uint)0x41058ABE);

            AsmEncoding addEncoding = addSubEncoder.EncodeOperation(source, target, Operation.ADD);
            Assert.AreEqual(addEncoding.Intermediate.Code, addEncoding.Target.Code, $"{Operation.ADD} :: {source.ToString(false)} --> {target.ToString(false)} == {addEncoding.Transitions.Select(op => op.Delta.ToString(false)).Aggregate((x, acc) => x + "," + acc)}");
        }

        [TestMethod]
        public void SubOperationWithBorrowSucceeds()
        {
            ICollection<byte> allowedBytes = new List<byte>();
            for (int i = 0x80; i < byte.MaxValue + 1; i++)
            {
                allowedBytes.Add((byte)i);
            }

            BaseEncoder addSubEncoder = new AddSubEncoder(allowedBytes);

            OpCode source = new OpCode((uint)0x7E16DFC6);
            OpCode target = new OpCode((uint)0x7D8BE34F);

            AsmEncoding subEncoding = addSubEncoder.EncodeOperation(source, target, Operation.SUB);
            Assert.AreEqual(subEncoding.Intermediate.Code, subEncoding.Target.Code, $"{Operation.SUB} :: {source.ToString(false)} --> {target.ToString(false)} == {subEncoding.Transitions.Select(op => op.Delta.ToString(false)).Aggregate((x, acc) => x + "," + acc)}");
        }

        private void PerformTestLoop(IEnumerable<byte> allowedBytes, int testCases)
        {
            BaseEncoder addSubEncoder = new AddSubEncoder(allowedBytes);
            BaseEncoder xorEncoder = new XorEncoder(allowedBytes);

            Random random = new Random();

            for (int i = 0; i < testCases; i++)
            {
                OpCode source = new OpCode((uint)random.Next());
                OpCode target = new OpCode((uint)random.Next());

                try
                {
                    AsmEncoding addEncoding = addSubEncoder.EncodeOperation(source, target, Operation.ADD);
                    if (addEncoding != null)
                    {
                        Assert.AreEqual(addEncoding.Intermediate.Code, addEncoding.Target.Code, $"{Operation.ADD} :: {source.ToString(false)} --> {target.ToString(false)} == {addEncoding.Transitions.Select(op => op.Delta.ToString(false)).Aggregate((x, acc) => x + "," + acc)}");
                    }
                }
                catch (InvalidOperationException) { }

                try
                {
                    AsmEncoding subEncoding = addSubEncoder.EncodeOperation(source, target, Operation.SUB);
                    if (subEncoding != null)
                    {
                        Assert.AreEqual(subEncoding.Intermediate.Code, subEncoding.Target.Code, $"{Operation.SUB} :: {source.ToString(false)} --> {target.ToString(false)} == {subEncoding.Transitions.Select(op => op.Delta.ToString(false)).Aggregate((x, acc) => x + "," + acc)}");
                    }
                }
                catch (InvalidOperationException) { }

                try
                {
                    AsmEncoding xorEncoding = xorEncoder.EncodeOperation(source, target, Operation.XOR);
                    if (xorEncoding != null)
                    {
                        Assert.AreEqual(xorEncoding.Intermediate.Code, xorEncoding.Target.Code, $"{Operation.XOR} :: {source.ToString(false)} --> {target.ToString(false)} == {xorEncoding.Transitions.Select(op => op.Delta.ToString(false)).Aggregate((x, acc) => x + "," + acc)}");
                    }
                }
                catch (InvalidOperationException) { }
            }
        }
    }
}
