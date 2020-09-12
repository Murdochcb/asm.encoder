using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using asm.encoder;
using asm.encoder.Encoders;
using System.Linq;
using asm.encoder.Formatter;
using asm.encoder.Registers;

namespace asm.test
{
    [TestClass]
    public class EncodersTest
    {
        const int tenThousandTestCases = 10000;
        private IFormatter formatter;

        [TestInitialize]
        public void InitializeTest()
        {
            this.formatter = new DebugFormatter();
        }

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
            ICollection<byte> allowedBytes = new List<byte>() { 0x53 }; // Add support for EBX register
            for (int i = 0x80; i < byte.MaxValue + 1; i++)
            {
                allowedBytes.Add((byte)i);
            }

            BaseEncoder addEncoder = new AddSubEncoder(Operation.ADD, new EbxRegister(allowedBytes), allowedBytes);

            OpCode source = new OpCode((uint)0x40A96593);
            OpCode target = new OpCode((uint)0x41058ABE);

            AsmEncoding addEncoding = addEncoder.EncodeOperation(source, target);
            Assert.AreEqual(addEncoding.Intermediate.Code, addEncoding.Target.Code, $"{Operation.ADD} :: {this.formatter.Format(source, Endian.Big)} --> {this.formatter.Format(target, Endian.Big)} == {addEncoding.Transitions.Select(op => this.formatter.Format(op.Delta, Endian.Big)).Aggregate((x, acc) => x + "," + acc)}");
        }

        [TestMethod]
        public void SubOperationWithBorrowSucceeds()
        {
            ICollection<byte> allowedBytes = new List<byte>() { 0x53 }; // Add support for EBX register
            for (int i = 0x80; i < byte.MaxValue + 1; i++)
            {
                allowedBytes.Add((byte)i);
            }

            BaseEncoder subEncoder = new AddSubEncoder(Operation.SUB, new EbxRegister(allowedBytes), allowedBytes);

            OpCode source = new OpCode(0x7E16DFC6);
            OpCode target = new OpCode(0x7D8BE34F);

            AsmEncoding subEncoding = subEncoder.EncodeOperation(source, target);
            Assert.AreEqual(subEncoding.Intermediate.Code, subEncoding.Target.Code, $"{Operation.SUB} :: {this.formatter.Format(source, Endian.Big)} --> {this.formatter.Format(target, Endian.Big)} == {subEncoding.Transitions.Select(op => this.formatter.Format(op.Delta, Endian.Big)).Aggregate((x, acc) => x + "," + acc)}");
        }

        [TestMethod]
        public void OperationReturnsNullWhenNoEncodingExists()
        {
            ICollection<byte> allowedBytes = new List<byte>() { 0x0 };

            BaseEncoder addEncoder = new AddSubEncoder(Operation.ADD, new EaxRegister(allowedBytes), allowedBytes);
            BaseEncoder subEncoder = new AddSubEncoder(Operation.SUB, new EaxRegister(allowedBytes), allowedBytes);
            BaseEncoder xorEncoder = new XorEncoder(new EbxRegister(allowedBytes), allowedBytes);

            OpCode source = OpCode.Zero;
            OpCode target = new OpCode(0x01010101);

            AsmEncoding addEncoding = addEncoder.EncodeOperation(source, target);
            AsmEncoding subEncoding = addEncoder.EncodeOperation(source, target);
            AsmEncoding xorEncoding = xorEncoder.EncodeOperation(source, target);

            Assert.AreEqual(null, addEncoding);
            Assert.AreEqual(null, subEncoding);
            Assert.AreEqual(null, xorEncoding);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddEncoderThrowsArgumentExceptionWithNoAllowedBytesSpecified()
        {
            ICollection<byte> allowedBytes = Enumerable.Range(0, 255).Select(i => Convert.ToByte(i)).ToList();

            _ = new AddSubEncoder(Operation.ADD, new EbxRegister(allowedBytes), Enumerable.Empty<byte>().ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SubEncoderThrowsArgumentExceptionWithNoAllowedBytesSpecified()
        {
            ICollection<byte> allowedBytes = Enumerable.Range(0, 255).Select(i => Convert.ToByte(i)).ToList();

            _ = new AddSubEncoder(Operation.SUB, new EbxRegister(allowedBytes), Enumerable.Empty<byte>().ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void XorEncoderThrowsArgumentExceptionWithNoAllowedBytesSpecified()
        {
            ICollection<byte> allowedBytes = Enumerable.Range(0, 255).Select(i => Convert.ToByte(i)).ToList();

            _ = new XorEncoder(new EbxRegister(allowedBytes), Enumerable.Empty<byte>().ToList());
        }

        private void PerformTestLoop(IEnumerable<byte> allowedBytes, int testCases)
        {
            BaseEncoder addEncoder = new AddSubEncoder(Operation.ADD, new EbxRegister(allowedBytes), allowedBytes);
            BaseEncoder subEncoder = new AddSubEncoder(Operation.SUB, new EbxRegister(allowedBytes), allowedBytes);
            BaseEncoder xorEncoder = new XorEncoder(new EbxRegister(allowedBytes), allowedBytes);

            Random random = new Random();

            for (int i = 0; i < testCases; i++)
            {
                OpCode source = new OpCode((uint)random.Next());
                OpCode target = new OpCode((uint)random.Next());

                AsmEncoding addEncoding = addEncoder.EncodeOperation(source, target);
                if (addEncoding != null)
                {
                    Assert.AreEqual(addEncoding.Intermediate.Code, addEncoding.Target.Code, $"{Operation.ADD} :: {this.formatter.Format(source, Endian.Big)} --> {this.formatter.Format(target, Endian.Big)} == {addEncoding.Transitions.Select(op => this.formatter.Format(op.Delta, Endian.Big)).Aggregate((x, acc) => x + "," + acc)}");
                }

                AsmEncoding subEncoding = subEncoder.EncodeOperation(source, target);
                if (subEncoding != null)
                {
                    Assert.AreEqual(subEncoding.Intermediate.Code, subEncoding.Target.Code, $"{Operation.SUB} :: {this.formatter.Format(source, Endian.Big)} --> {this.formatter.Format(target, Endian.Big)} == {subEncoding.Transitions.Select(op => this.formatter.Format(op.Delta, Endian.Big)).Aggregate((x, acc) => x + "," + acc)}");
                }

                AsmEncoding xorEncoding = xorEncoder.EncodeOperation(source, target);
                if (xorEncoding != null)
                {
                    Assert.AreEqual(xorEncoding.Intermediate.Code, xorEncoding.Target.Code, $"{Operation.XOR} :: {this.formatter.Format(source, Endian.Big)} --> {this.formatter.Format(target, Endian.Big)} == {xorEncoding.Transitions.Select(op => this.formatter.Format(op.Delta, Endian.Big)).Aggregate((x, acc) => x + "," + acc)}");
                }
            }
        }
    }
}
