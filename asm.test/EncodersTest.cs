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

            OpCode source = new OpCode(0x7E16DFC6);
            OpCode target = new OpCode(0x7D8BE34F);

            AsmEncoding subEncoding = addSubEncoder.EncodeOperation(source, target, Operation.SUB);
            Assert.AreEqual(subEncoding.Intermediate.Code, subEncoding.Target.Code, $"{Operation.SUB} :: {source.ToString(false)} --> {target.ToString(false)} == {subEncoding.Transitions.Select(op => op.Delta.ToString(false)).Aggregate((x, acc) => x + "," + acc)}");
        }

        [TestMethod]
        public void OperationReturnsNullWhenNoEncodingExists()
        {
            ICollection<byte> allowedBytes = new List<byte>() { 0x0 };

            BaseEncoder addSubEncoder = new AddSubEncoder(allowedBytes);
            BaseEncoder xorEncoder = new XorEncoder(allowedBytes);

            OpCode source = OpCode.Zero;
            OpCode target = new OpCode(0x01010101);

            AsmEncoding addEncoding = addSubEncoder.EncodeOperation(source, target, Operation.ADD);
            AsmEncoding subEncoding = addSubEncoder.EncodeOperation(source, target, Operation.SUB);
            AsmEncoding xorEncoding = xorEncoder.EncodeOperation(source, target, Operation.XOR);

            Assert.AreEqual(null, addEncoding);
            Assert.AreEqual(null, subEncoding);
            Assert.AreEqual(null, xorEncoding);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void XorEncodingFailsWithNonXorOperationCode()
        {
            BaseEncoder xorEncoder = new XorEncoder(new List<byte>() { 0x0 });

            xorEncoder.EncodeOperation(OpCode.Zero, OpCode.Zero, Operation.ADD);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddSubEncoderThrowsArgumentExceptionWithNoAllowedBytesSpecified()
        {
            _ = new AddSubEncoder(Enumerable.Empty<byte>().ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void XorEncoderThrowsArgumentExceptionWithNoAllowedBytesSpecified()
        {
            _ = new XorEncoder(Enumerable.Empty<byte>().ToList());
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

                AsmEncoding addEncoding = addSubEncoder.EncodeOperation(source, target, Operation.ADD);
                if (addEncoding != null)
                {
                    Assert.AreEqual(addEncoding.Intermediate.Code, addEncoding.Target.Code, $"{Operation.ADD} :: {source.ToString(false)} --> {target.ToString(false)} == {addEncoding.Transitions.Select(op => op.Delta.ToString(false)).Aggregate((x, acc) => x + "," + acc)}");
                }

                AsmEncoding subEncoding = addSubEncoder.EncodeOperation(source, target, Operation.SUB);
                if (subEncoding != null)
                {
                    Assert.AreEqual(subEncoding.Intermediate.Code, subEncoding.Target.Code, $"{Operation.SUB} :: {source.ToString(false)} --> {target.ToString(false)} == {subEncoding.Transitions.Select(op => op.Delta.ToString(false)).Aggregate((x, acc) => x + "," + acc)}");
                }

                AsmEncoding xorEncoding = xorEncoder.EncodeOperation(source, target, Operation.XOR);
                if (xorEncoding != null)
                {
                    Assert.AreEqual(xorEncoding.Intermediate.Code, xorEncoding.Target.Code, $"{Operation.XOR} :: {source.ToString(false)} --> {target.ToString(false)} == {xorEncoding.Transitions.Select(op => op.Delta.ToString(false)).Aggregate((x, acc) => x + "," + acc)}");
                }
            }
        }
    }
}
