using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FasmDebug
{
    struct FasHeader
    {
        public uint Signature;
        public byte MajorVersion;
        public byte MinorVersion;
        public ushort HeaderLength;
        public uint InputNameOffset;
        public uint OutputNameOffset;
        public uint StringTableOffset;
        public uint StringTableLength;
        public uint SymbolTableOffset;
        public uint SymbolTableLength;
        public uint PreprocessedSourceOffset;
        public uint PreprocessedSourceLength;
        public uint AssemblyOffset;
        public uint AssemblyLength;
        public uint SectionNamesOffset;
        public uint SectionNamesLength;
        public uint SymbolReferencesOffset;
        public uint SymbolReferencesLength;
    }

    struct FasSymbol
    {
        public ulong Value;
        public FasSymbolFlags Flags;
        public byte DataSize;
        public byte ValueType;
        public uint ExtendedSIB;
        public ushort DefinedPassCount;
        public ushort UsedPassCount;
        public uint RelocatableJunk;
        public uint NameOffset;
        public uint DefinitionOffset;
    }

    struct FasPreprocessedLine
    {
        public uint FileNameOffset;
        public int LineNumber;
        public uint SourceFileLineOffset;
        public uint MacroPreprocessedLineOffset;
        public string Tokens;
    }

    struct FasAssemblyEntry
    {
        public uint OutputOffset;
        public uint PreprocessedSourceOffset;
        public ulong Address;
        public uint ExtendedSIB;
        public uint RelocatableJunk;
        public byte AddressType;
        public byte CodeType;
        public byte FlagStuff;
        public byte UpperAddressBits;
    }

    [Flags]
    enum FasSymbolFlags
    {
        Defined = 1,
        AssemblyTimeVariable = 2,
        CannotForwardReference = 4,
        Used = 8,
        PredictionNeededForUse = 16,
        LastPredictedResultForUse = 32,
        PredictionNeededForDefine = 64,
        LastPredictedResultForDefine = 128,
        OptimizationAdjustment = 256,
        Negative = 512,
        SpecialMarker = 1024
    }

    class Symbol
    {
        public string Name;

        public int NameOffset;
        public int Address;
    }

    class Line
    {
        public string FileName;

        public int FileNameOffset;
        public int LineNumber;
        public int Address;
    }

    public class Program
    {
        private static List<Symbol> _symbols = new List<Symbol>(); 
        private static List<Line> _lines = new List<Line>();

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Converts FASM symbol files into a nicer format");
                Console.WriteLine("Usage: FasmDebug <input.fas>");
                return;
            }

            var input = args[0];
            var output = Path.GetFileNameWithoutExtension(input) + ".dbg";

            try
            {
                ReadFasmDebug(input);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read input file: " + e.Message);
                return;
            }

            try
            {
                WriteDebug(output);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to write output file: " + e.Message);
                return;
            }
        }

        private static void WriteDebug(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Create))
            using (var writer = new BinaryWriter(file))
            {
                const int headerSize = 24;
                var symbolsSize = _symbols.Count * 8;
                var linesSize = _lines.Count * 12;

                writer.Write(0x30474244);                           // header - DBG0
                writer.Write(headerSize);                           // symbol offset
                writer.Write(_symbols.Count);                       // symbol count
                writer.Write(headerSize + symbolsSize);             // line offset
                writer.Write(_lines.Count);                         // line count
                writer.Write(headerSize + symbolsSize + linesSize); // string table offset

                file.Seek(_symbols.Count * 8, SeekOrigin.Current);
                file.Seek(_lines.Count * 12, SeekOrigin.Current);

                #region Write String Table
                var stringTable = new List<Tuple<string, int>>();

                foreach (var s in _symbols)
                {
                    var line = stringTable.FirstOrDefault(str => str.Item1 == s.Name);

                    if (line != null)
                    {
                        s.NameOffset = line.Item2;
                        continue;
                    }

                    var offset = (int)file.Position;
                    s.NameOffset = offset;
                    WriteNullTerminated(writer, s.Name);
                    stringTable.Add(Tuple.Create(s.Name, offset));
                }

                foreach (var l in _lines)
                {
                    var line = stringTable.FirstOrDefault(str => str.Item1 == l.FileName);

                    if (line != null)
                    {
                        l.FileNameOffset = line.Item2;
                        continue;
                    }

                    var offset = (int)file.Position;
                    l.FileNameOffset = offset;
                    WriteNullTerminated(writer, l.FileName);
                    stringTable.Add(Tuple.Create(l.FileName, offset));
                }
                #endregion

                file.Seek(headerSize, SeekOrigin.Begin);

                #region Write Symbols
                foreach (var s in _symbols)
                {
                    writer.Write(s.NameOffset);
                    writer.Write(s.Address);
                }
                #endregion

                #region Write Lines
                foreach (var l in _lines)
                {
                    writer.Write(l.FileNameOffset);
                    writer.Write(l.LineNumber);
                    writer.Write(l.Address);
                }
                #endregion
            }
        }

        private static void ReadFasmDebug(string fileName)
        {
            using (var file = File.OpenRead(fileName))
            using (var reader = new BinaryReader(file))
            {
                var header = ReadFasHeader(reader);
                var fasSymbols = new List<FasSymbol>();
                var runningLength = 0;

                if (header.Signature != 0x1A736166)
                    throw new Exception("Not a FASM symbol file");

                file.Seek(header.SymbolTableOffset, SeekOrigin.Begin);
                while (runningLength < header.SymbolTableLength)
                {
                    fasSymbols.Add(ReadSymbol(reader));
                    runningLength += 32; // sizeof(FasSymbol)
                }

                string parent = "";
                int anonCount = 0;

                foreach (var s in fasSymbols.OrderBy(s => s.Value))
                {
                    string name;

                    if (s.Flags.HasFlag(FasSymbolFlags.AssemblyTimeVariable))
                        continue;

                    if (s.NameOffset == 0)
                    {
                        name = string.Format("{0}.@@_{1}", parent, anonCount);
                        anonCount++;
                    }
                    else if ((s.NameOffset & 0x80000000) != 0)
                    {
                        var offset = s.NameOffset & 0x7FFFFFFF;
                        file.Seek(header.StringTableOffset + offset, SeekOrigin.Begin);
                        name = ReadNullTerminated(reader);
                    }
                    else
                    {
                        file.Seek(header.PreprocessedSourceOffset + s.NameOffset, SeekOrigin.Begin);
                        name = reader.ReadString();
                    }

                    if (name[0] == '.')
                    {
                        name = parent + name;
                    }

                    if (s.NameOffset != 0 && !name.Contains('.'))
                    {
                        parent = name;
                        anonCount = 0;
                    }

                    _symbols.Add(new Symbol { Name = name, Address = (int)s.Value });
                }

                var assemblyEntries = new List<FasAssemblyEntry>();

                file.Seek(header.AssemblyOffset, SeekOrigin.Begin);
                while (file.Position - header.AssemblyOffset <= header.AssemblyLength)
                {
                    assemblyEntries.Add(ReadAssemblyEntry(reader));
                }

                var preprocessedLines = new Dictionary<uint, FasPreprocessedLine>();

                file.Seek(header.PreprocessedSourceOffset, SeekOrigin.Begin);
                while (file.Position - header.PreprocessedSourceOffset <= header.PreprocessedSourceLength)
                {
                    var offset = file.Position - header.PreprocessedSourceOffset;
                    var line = ReadPreprocessedLine(reader);
                    preprocessedLines.Add((uint)offset, line);
                }

                uint prevLine = uint.MaxValue;

                foreach (var entry in assemblyEntries)
                {
                    if (entry.Address >= int.MaxValue)
                        break;

                    var line = preprocessedLines[entry.PreprocessedSourceOffset];
                    uint lineOffset = line.SourceFileLineOffset;

                    if ((line.LineNumber & 0x80000000) != 0)
                    {
                        while ((line.LineNumber & 0x80000000) != 0)
                        {
                            line = preprocessedLines[line.SourceFileLineOffset];
                            lineOffset = line.SourceFileLineOffset;
                        }

                        if (lineOffset == prevLine)
                            continue;

                        prevLine = lineOffset;
                    }

                    uint fileNameOffset;
                    if (line.FileNameOffset == 0)
                        fileNameOffset = header.StringTableOffset + header.InputNameOffset;
                    else
                        fileNameOffset = header.PreprocessedSourceOffset + line.FileNameOffset;

                    file.Seek(fileNameOffset, SeekOrigin.Begin);
                    var lineFile = ReadNullTerminated(reader);

                    _lines.Add(new Line { FileName = lineFile, LineNumber = line.LineNumber, Address = (int)entry.Address });
                }
            }
        }

        private static void WriteNullTerminated(BinaryWriter writer, string str)
        {
            foreach (var c in str)
            {
                writer.Write((byte)c);
            }

            writer.Write((byte)0);
        }

        private static string ReadNullTerminated(BinaryReader reader)
        {
            var result = "";
            byte value;

            while ((value = reader.ReadByte()) != 0)
            {
                result += (char)value;
            }

            return result;
        }

        private static string ReadLength(BinaryReader reader, uint count)
        {
            var result = "";

            while (count-- > 0)
            {
                result += (char)reader.ReadByte();
            }

            return result;
        }

        private static FasAssemblyEntry ReadAssemblyEntry(BinaryReader reader)
        {
            var result = new FasAssemblyEntry
            {
                OutputOffset = reader.ReadUInt32(),
                PreprocessedSourceOffset = reader.ReadUInt32(),
                Address = reader.ReadUInt64(),
                ExtendedSIB = reader.ReadUInt32(),
                RelocatableJunk = reader.ReadUInt32(),
                AddressType = reader.ReadByte(),
                CodeType = reader.ReadByte(),
                FlagStuff = reader.ReadByte(),
                UpperAddressBits = reader.ReadByte()
            };

            return result;
        }

        private static string ReadTokenizedLine(BinaryReader reader)
        {
            var result = "";

            while (true)
            {
                var id = reader.ReadByte();

                if (id == 0) // eol
                    break;

                switch (id)
                {
                    case 0x1A: // byte prefixed text
                    case 0x3B: // fuck knows
                        result += ReadLength(reader, reader.ReadByte());
                        break;
                    case 0x22: // dword prefixed text
                        result += "'" + ReadLength(reader, reader.ReadUInt32()) + "'";
                        break;
                    default:
                        result += (char)id;
                        break;
                }

                result += " ";
            }

            return result;
        }

        private static FasPreprocessedLine ReadPreprocessedLine(BinaryReader reader)
        {
            var result = new FasPreprocessedLine
            {
                FileNameOffset = reader.ReadUInt32(),
                LineNumber = reader.ReadInt32(),
                SourceFileLineOffset = reader.ReadUInt32(),
                MacroPreprocessedLineOffset = reader.ReadUInt32(),
                Tokens = ReadTokenizedLine(reader)
            };

            return result;
        }

        private static FasSymbol ReadSymbol(BinaryReader reader)
        {
            var result = new FasSymbol
            {
                Value = reader.ReadUInt64(),
                Flags = (FasSymbolFlags)reader.ReadUInt16(),
                DataSize = reader.ReadByte(),
                ValueType = reader.ReadByte(),
                ExtendedSIB = reader.ReadUInt32(),
                DefinedPassCount = reader.ReadUInt16(),
                UsedPassCount = reader.ReadUInt16(),
                RelocatableJunk = reader.ReadUInt32(),
                NameOffset = reader.ReadUInt32(),
                DefinitionOffset = reader.ReadUInt32()
            };

            return result;
        }

        private static FasHeader ReadFasHeader(BinaryReader reader)
        {
            var result = new FasHeader
            {
                Signature = reader.ReadUInt32(),
                MajorVersion = reader.ReadByte(),
                MinorVersion = reader.ReadByte(),
                HeaderLength = reader.ReadUInt16(),
                InputNameOffset = reader.ReadUInt32(),
                OutputNameOffset = reader.ReadUInt32(),
                StringTableOffset = reader.ReadUInt32(),
                StringTableLength = reader.ReadUInt32(),
                SymbolTableOffset = reader.ReadUInt32(),
                SymbolTableLength = reader.ReadUInt32(),
                PreprocessedSourceOffset = reader.ReadUInt32(),
                PreprocessedSourceLength = reader.ReadUInt32(),
                AssemblyOffset = reader.ReadUInt32(),
                AssemblyLength = reader.ReadUInt32(),
                SectionNamesOffset = reader.ReadUInt32(),
                SectionNamesLength = reader.ReadUInt32(),
                SymbolReferencesOffset = reader.ReadUInt32(),
                SymbolReferencesLength = reader.ReadUInt32()
            };

            return result;
        }
    }
}
