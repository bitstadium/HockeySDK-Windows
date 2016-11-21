namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    
    /// <summary>
    /// This class will read some basic information from a loaded native binary.
    /// </summary>
    internal class PEImageReader
    {
        /// <summary>
        /// The offset for the binaries signature.
        /// </summary>
        public const int PESignatureOffsetLocation = 0x3C;

        /// <summary>
        /// The size of the COFF header.
        /// </summary>
        public const int SizeofCOFFFileHeader = 20;

        /// <summary>
        /// Size of the optional fields in the standard header (32-bit mode)
        /// </summary>
        public const int SizeofOptionalHeaderStandardFields32 = 28;

        /// <summary>
        /// Size of the optional fields in the standard header (64-bit mode)
        /// </summary>
        public const int SizeofOptionalHeaderStandardFields64 = 24;

        /// <summary>
        /// Size of the optional fields for the NT header (32-bit mode)
        /// </summary>
        public const int SizeofOptionalHeaderNTAdditionalFields32 = 68;

        /// <summary>
        /// Size of the optional fields for the NT header (64-bit mode)
        /// </summary>
        public const int SizeofOptionalHeaderNTAdditionalFields64 = 88;

        /// <summary>
        /// The offset to the debug table.
        /// </summary>
        public const int DebugTableDirectoryOffset = 48;

        /// <summary>
        /// The size of the debug directory.
        /// </summary>
        public const int SizeofDebugDirectory = 28;

        /// <summary>
        /// Signature to identify a code view.
        /// </summary>
        public const int CodeViewSignature = 0x53445352; // "RSDS" in little endian format

        /// <summary>
        /// SizeOfImage is a 4 byte unsigned value at offset 56 of the COFF optional header, 
        /// section 3.4.2 in http://download.microsoft.com/download/e/b/a/eba1050f-a31d-436b-9281-92cdfeae4b45/pecoff.doc
        /// </summary>
        public const int SizeOfImageOffset = 56;

        /// <summary>
        /// The image base pointer
        /// </summary>
        private IntPtr imageBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="PEImageReader"/> class.
        /// </summary>
        /// <param name="imageBase"></param>
        public PEImageReader(IntPtr imageBase)
        {
            this.imageBase = imageBase;
        }

        /// <summary>
        /// Parse the image file and extract the code view debug data information.
        /// </summary>
        /// <returns>The code view debug information if found, null otherwise.</returns>
        public CodeViewDebugData Parse()
        {
            int peHeaderOffset = this.ReadDwordAtFileOffset(PEImageReader.PESignatureOffsetLocation);
            int peOptionalHeaderOffset = peHeaderOffset + 4 + PEImageReader.SizeofCOFFFileHeader;
            int optionalHeaderDirectoryEntriesOffset = 0;
            int sizeOfImage = ReadDwordAtFileOffset(peOptionalHeaderOffset + 56);
            IntPtr endAddress = imageBase + sizeOfImage;

            PEMagic magic = (PEMagic)this.ReadWordAtFileOffset(peOptionalHeaderOffset);
            if (magic == PEMagic.PEMagic32)
            {
                optionalHeaderDirectoryEntriesOffset = peOptionalHeaderOffset +
                                                       PEImageReader.SizeofOptionalHeaderStandardFields32 +
                                                       PEImageReader.SizeofOptionalHeaderNTAdditionalFields32;
            }
            else
            {
                optionalHeaderDirectoryEntriesOffset = peOptionalHeaderOffset +
                                                       PEImageReader.SizeofOptionalHeaderStandardFields64 +
                                                       PEImageReader.SizeofOptionalHeaderNTAdditionalFields64;
            }

            DirectoryEntry debugDirectoryEntry = this.ReadDebugDirectoryEntry(optionalHeaderDirectoryEntriesOffset);
            DebugDirectory[] debugDirectories = ReadDebugDirectories(debugDirectoryEntry);
            return this.GetCodeViewDebugData(debugDirectories, endAddress);
        }

        /// <summary>
        /// Reads a set of bytes from the image pointer at the specified offset.
        /// </summary>
        /// <param name="fileBytes">The byte array to fill with the bytes at the sepcified offset.</param>
        /// <param name="offset">The offset at which to start the copy counting from the address specified in imageBase.</param>
        private void ReadAtOffset(byte[] fileBytes, int offset)
        {
            Marshal.Copy(this.imageBase + offset, fileBytes, 0, fileBytes.Length);
        }

        /// <summary>
        /// Reads an unsigned short value (16 bits) from the specified offset.
        /// </summary>
        /// <param name="offset">The offset at which to read the unsigned short.</param>
        /// <returns>The read value.</returns>
        private ushort ReadWordAtFileOffset(int offset)
        {
            byte[] word = new byte[2];
            ReadAtOffset(word, offset);
            return BitConverter.ToUInt16(word, 0);
        }

        /// <summary>
        /// Reads a dword value (32 bits) from the specified offset.
        /// </summary>
        /// <param name="offset">The offset at which to read the dword.</param>
        /// <returns>The read value.</returns>
        private int ReadDwordAtFileOffset(int offset)
        {
            byte[] dword = new byte[4];
            ReadAtOffset(dword, offset);
            return BitConverter.ToInt32(dword, 0);
        }

        /// <summary>
        /// Returns a binary reader object at the specified relative virtual address and spanning size.
        /// </summary>
        /// <param name="rva">The relative virtual address.</param>
        /// <param name="size">The size of the reader.</param>
        /// <returns>The binary reader </returns>
        private BinaryReader ReadAtRelativeVirtualAddress(int rva, int size)
        {
            byte[] data = new byte[size];
            this.ReadAtOffset(data, rva);
            return new BinaryReader(new MemoryStream(data));
        }

        /// <summary>
        /// Read a null terminated UTF-8 string.
        /// </summary>
        /// <param name="reader">The binary reader we are going to use.</param>
        /// <returns>The read string.</returns>
        private string ReadNullTerminatedUTF8String(BinaryReader reader)
        {
            List<byte> stringBytes = new List<byte>();
            byte nextByte = reader.ReadByte();
            while (nextByte != 0)
            {
                stringBytes.Add(nextByte);
                nextByte = reader.ReadByte();
            }

            return Encoding.UTF8.GetString(stringBytes.ToArray(), 0, stringBytes.Count);
        }

        /// <summary>
        /// Read the debug entry at the specified offset.
        /// </summary>
        /// <param name="optionalHeaderDirectoryEntriesOffset">The offset information from imageBase.</param>
        /// <returns>The parsed directory entry info (will contain RVA and the size of the entry).</returns>
        private DirectoryEntry ReadDebugDirectoryEntry(int optionalHeaderDirectoryEntriesOffset)
        {
            DirectoryEntry entry = new DirectoryEntry();
            entry.RelativeVirtualAddress = this.ReadDwordAtFileOffset(optionalHeaderDirectoryEntriesOffset + PEImageReader.DebugTableDirectoryOffset);
            entry.Size = this.ReadDwordAtFileOffset(optionalHeaderDirectoryEntriesOffset + PEImageReader.DebugTableDirectoryOffset + 4);
            return entry;
        }

        /// <summary>
        /// Reads all directory entries from the passed in debug directory entry.
        /// </summary>
        /// <param name="debugDirectoryEntry">The debug directory entry.</param>
        /// <returns>The parsed debug directory entries.</returns>
        private DebugDirectory[] ReadDebugDirectories(DirectoryEntry debugDirectoryEntry)
        {
            // there might not be a debug directory, in which case return an empty list
            if (debugDirectoryEntry.RelativeVirtualAddress == 0)
            {
                return new DebugDirectory[0];
            }

            BinaryReader reader = this.ReadAtRelativeVirtualAddress(debugDirectoryEntry.RelativeVirtualAddress, debugDirectoryEntry.Size);
            int countDirectories = (int)(debugDirectoryEntry.Size / PEImageReader.SizeofDebugDirectory);
            DebugDirectory[] debugDirectories = new DebugDirectory[countDirectories];
            for (int i = 0; i < countDirectories; i++)
            {
                debugDirectories[i] = new DebugDirectory();
                debugDirectories[i].Characteristics = reader.ReadUInt32();
                debugDirectories[i].TimeDateStamp = reader.ReadUInt32();
                debugDirectories[i].MajorVersion = reader.ReadUInt16();
                debugDirectories[i].MinorVersion = reader.ReadUInt16();
                debugDirectories[i].Type = (ImageDebugType)reader.ReadUInt32();
                debugDirectories[i].SizeOfData = reader.ReadUInt32();
                debugDirectories[i].AddressOfRawData = reader.ReadUInt32();
                debugDirectories[i].PointerToRawData = reader.ReadUInt32();
            }

            return debugDirectories;
        }

        /// <summary>
        /// Reads the code view debug data from the specified set of debug directories.
        /// </summary>
        /// <param name="debugDirectories">The set of debug directories.</param>
        /// <param name="endAddress">End Address of the image.</param>
        /// <returns>The code view if found, null otherwise.</returns>
        private CodeViewDebugData GetCodeViewDebugData(DebugDirectory[] debugDirectories, IntPtr endAddress)
        {
            foreach (DebugDirectory debugDirectory in debugDirectories)
            {
                if (debugDirectory.Type != ImageDebugType.CodeView)
                {
                    continue;
                }

                if (debugDirectory.SizeOfData > 1000)
                {
                    return null;
                }

                BinaryReader reader = this.ReadAtRelativeVirtualAddress((int)debugDirectory.AddressOfRawData, (int)debugDirectory.SizeOfData);
                int signature = reader.ReadInt32();
                if (signature != PEImageReader.CodeViewSignature)
                {
                    return null;
                }

                CodeViewDebugData codeView = new CodeViewDebugData(
                                        new Guid(reader.ReadBytes(16)),
                                        (int)reader.ReadUInt32(),
                                        this.ReadNullTerminatedUTF8String(reader),
                                        endAddress);
                return codeView;
            }

            return null;
        }

        /// <summary>
        /// Constants.
        /// </summary>
        private enum PEMagic : 
            ushort
        {
            /// <summary>
            /// 32-bit magic
            /// </summary>
            PEMagic32 = 0x010B,

            /// <summary>
            /// 64-bit magic.
            /// </summary>
            PEMagic64 = 0x020B,
        }

        /// <summary>
        /// The type of the image.
        /// </summary>
        private enum ImageDebugType : 
            uint
        {
            Unknown = 0,
            Coff = 1,
            CodeView = 2,
            Fpo = 3,
            Misc = 4,
            Exception = 5,
            Fixup = 6,
            Borland = 9
        }

        /// <summary>
        /// Represents a single PE directory entry.
        /// </summary>
        private struct DirectoryEntry
        {
            /// <summary>
            /// The relative virtual address for the entry.
            /// </summary>
            public int RelativeVirtualAddress;

            /// <summary>
            /// The size of the entry.
            /// </summary>
            public int Size;
        }

        /// <summary>
        /// A single debug directory info.
        /// </summary>
        private struct DebugDirectory
        {
            /// <summary>
            /// The characteristic flags.
            /// </summary>
            public uint Characteristics;

            /// <summary>
            /// Timestamp for the directory.
            /// </summary>
            public uint TimeDateStamp;

            /// <summary>
            /// The major version field.
            /// </summary>
            public ushort MajorVersion;

            /// <summary>
            /// The minor version field.
            /// </summary>
            public ushort MinorVersion;

            /// <summary>
            /// The image type.
            /// </summary>
            public ImageDebugType Type;

            /// <summary>
            /// The size of the data.
            /// </summary>
            public uint SizeOfData;

            /// <summary>
            /// Memory address of the data.
            /// </summary>
            public uint AddressOfRawData;

            /// <summary>
            /// A pointer to the data.
            /// </summary>
            public uint PointerToRawData;
        }

        /// <summary>
        /// Data representing identifying information for a single symbol file.
        /// </summary>
        public class CodeViewDebugData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CodeViewDebugData"/> class.
            /// </summary>
            /// <param name="signature">The signature for this symbol.</param>
            /// <param name="age">The age for this symbol.</param>
            /// <param name="pdbPath">The symbol path for this build.</param>
            /// <param name="endAddress">EndAddress of the image.</param>
            public CodeViewDebugData(Guid signature, int age, string pdbPath, IntPtr endAddress)
            {
                this.Signature = signature;
                this.Age = age;
                this.PdbPath = pdbPath;
                this.EndAddress = endAddress;
            }

            /// <summary>
            /// Gets the signature of the binary. This together with Age forms a unique identity for the symbol file.
            /// </summary>
            public Guid Signature { get; private set; }

            /// <summary>
            /// Gets the age of the binary. This together with Signature forms a unique identity for the symbol file.
            /// </summary>
            public int Age { get; private set; }

            /// <summary>
            /// Gets the path to the symbol file.
            /// </summary>
            public string PdbPath { get; private set; }

            /// <summary>
            /// Gets image end address.
            /// </summary>
            public IntPtr EndAddress { get; private set; }
        }
    }
}
