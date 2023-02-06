﻿using System;
using System.IO;

namespace NRasterizer.IO
{
    public class ByteOrderSwappingBinaryReader: BinaryReader
    {
        public ByteOrderSwappingBinaryReader(Stream input): base(input)
        {
        }
        
        public ushort SwapBytes(ushort x)
        {
            return (ushort)((ushort)((x & 0xff) << 8) | ((x >> 8) & 0xff));
        }

        public uint SwapBytes(uint x)
        {
            return ((x & 0x000000ff) << 24) +
                ((x & 0x0000ff00) << 8) +
                ((x & 0x00ff0000) >> 8) +
                ((x & 0xff000000) >> 24);
        }

        public ulong SwapBytes(ulong value)
        {
            ulong uvalue = value;
            ulong swapped =
                ((0x00000000000000FF) & (uvalue >> 56)
                | (0x000000000000FF00) & (uvalue >> 40)
                | (0x0000000000FF0000) & (uvalue >> 24)
                | (0x00000000FF000000) & (uvalue >> 8)
                | (0x000000FF00000000) & (uvalue << 8)
                | (0x0000FF0000000000) & (uvalue << 24)
                | (0x00FF000000000000) & (uvalue << 40)
                | (0xFF00000000000000) & (uvalue << 56));
            return swapped;
        }

        public override Stream BaseStream { get { return base.BaseStream; } }
        //public override void Close() { base.Close(); }

        public override int PeekChar() { throw new NotImplementedException(); }
        public override int Read() { throw new NotImplementedException(); }
        public override int Read(byte[] buffer, int index, int count) { throw new NotImplementedException(); }
        public override int Read(char[] buffer, int index, int count) { throw new NotImplementedException(); }
        public override bool ReadBoolean() { throw new NotImplementedException(); }
        public override byte ReadByte() { return base.ReadByte(); }
        public override byte[] ReadBytes(int count) { return base.ReadBytes(count); }
        public override char ReadChar() { throw new NotImplementedException(); }
        public override char[] ReadChars(int count) { throw new NotImplementedException(); }
        public override decimal ReadDecimal() { throw new NotImplementedException(); }
        public override double ReadDouble() { throw new NotImplementedException(); }
        public override short ReadInt16() { return (short)SwapBytes(base.ReadUInt16()); }
        public override int ReadInt32() { throw new NotImplementedException(); }
        public override long ReadInt64() { throw new NotImplementedException(); }
        public override sbyte ReadSByte() { return base.ReadSByte(); }
        public override float ReadSingle() { throw new NotImplementedException(); }
        public override string ReadString() { throw new NotImplementedException(); }
        public override ushort ReadUInt16() { return SwapBytes(base.ReadUInt16()); }
        public override uint ReadUInt32() { return SwapBytes(base.ReadUInt32()); }
        public override ulong ReadUInt64() { return SwapBytes(base.ReadUInt64()); }

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
