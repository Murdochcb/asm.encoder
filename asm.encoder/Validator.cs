using asm.encoder.Formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm.encoder
{
    internal sealed class Validator
    {
        const string AddFlag = "-add";
        const string SubFlag = "-sub";
        const string XorFlag = "-xor";
        const string SourceFlag = "-source";
        const string TargetFlag = "-target";
        const string AllowedFlag = "-allowed";
        const string BadFlag = "-bad";
        const string FormatFlag = "-format";
        const string EndianFlag = "-endian";
        static readonly string[] formats = new string[] { BinaryAsmFormatter.FormatName, PythonFormatter.FormatName, DebugFormatter.FormatName };

        public static void ValidateArgs(string[] args, out byte[] sourceBytes, out byte[] targetBytes, out byte[] allowedBytes, out bool useAddEncoder, out bool useSubEncoder, out bool useXorEncoder, out IFormatter formatter, out Endian endian)
        {
            string usageMessage = $"Usage: {nameof(asm.encoder)} -source <value> -target <value> [(-allowed | -bad) <value>] [-add | -sub | -xor] {{-format <value>}} {{-endian <value>}}";
            string byteFormatMessage = $"<value> must be binary string format: \\x00\\x01\\x02";
            string formatFormatMessage = $"<value> must be any of the following (case-insensitive): [{formats.Aggregate((f, acc) => f + " | " + acc)}]";
            string endianFormatMessage = $"<value> must be any of the following (case-insensitive): [{Endian.Little} | {Endian.Big}]";
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
            formatter = new PythonFormatter();
            endian = Endian.Little;

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
                else if (string.Equals(args[i], FormatFlag, StringComparison.OrdinalIgnoreCase))
                {
                    string formatValue = args[i + 1];

                    if (string.Equals(formatValue, BinaryAsmFormatter.FormatName, StringComparison.OrdinalIgnoreCase))
                    {
                        formatter = new BinaryAsmFormatter();
                    }
                    else if (string.Equals(formatValue, PythonFormatter.FormatName, StringComparison.OrdinalIgnoreCase))
                    {
                        formatter = new PythonFormatter();
                    }
                    else if (string.Equals(formatValue, DebugFormatter.FormatName, StringComparison.OrdinalIgnoreCase))
                    {
                        formatter = new DebugFormatter();
                    }
                    else
                    {
                        throw new ArgumentException(formatFormatMessage);
                    }
                }
                else if (string.Equals(args[i], EndianFlag, StringComparison.OrdinalIgnoreCase))
                {
                    string endianValue = args[i + 1];

                    if (string.Equals(endianValue, Endian.Little.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        endian = Endian.Little;
                    }
                    else if (string.Equals(endianValue, Endian.Big.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        endian = Endian.Big;
                    }
                    else
                    {
                        throw new ArgumentException(endianFormatMessage);
                    }
                }
            }
        }
    }
}
