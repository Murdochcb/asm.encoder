using asm.encoder;
using asm.encoder.Formatter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.test
{
    [TestClass]
    public class FormattersTest
    {
        [TestMethod]
        public void BinaryFormatterOutputsLittleEndianOpCode()
        {
            IFormatter formatter = new BinaryAsmFormatter();

            OpCode opCode = new OpCode(0x12345678);

            string formatterResult = formatter.Format(opCode, Endian.Little);

            Assert.AreEqual(formatterResult, "78 56 34 12");
        }

        [TestMethod]
        public void BinaryFormatterOutputsBigEndianOpCode()
        {
            IFormatter formatter = new BinaryAsmFormatter();

            OpCode opCode = new OpCode(0x12345678);

            string formatterResult = formatter.Format(opCode, Endian.Big);

            Assert.AreEqual(formatterResult, "12 34 56 78");
        }

        [TestMethod]
        public void PythonFormatterOutputsLittleEndianOpCode()
        {
            IFormatter formatter = new PythonFormatter();

            OpCode opCode = new OpCode(0x12345678);

            string formatterResult = formatter.Format(opCode, Endian.Little);

            Assert.AreEqual(formatterResult, "\\x78\\x56\\x34\\x12");
        }

        [TestMethod]
        public void PythonFormatterOutputsBigEndianOpCode()
        {
            IFormatter formatter = new PythonFormatter();

            OpCode opCode = new OpCode(0x12345678);

            string formatterResult = formatter.Format(opCode, Endian.Big);

            Assert.AreEqual(formatterResult, "\\x12\\x34\\x56\\x78");
        }

        [TestMethod]
        public void DebugFormatterOutputsLittleEndianOpCode()
        {
            IFormatter formatter = new DebugFormatter();

            OpCode opCode = new OpCode(0x12345678);

            string formatterResult = formatter.Format(opCode, Endian.Little);

            Assert.AreEqual(formatterResult, "0x78563412");
        }

        [TestMethod]
        public void DebugFormatterOutputsBigEndianOpCode()
        {
            IFormatter formatter = new DebugFormatter();

            OpCode opCode = new OpCode(0x12345678);

            string formatterResult = formatter.Format(opCode, Endian.Big);

            Assert.AreEqual(formatterResult, "0x12345678");
        }
    }
}
