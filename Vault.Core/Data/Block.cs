﻿using System;
using Vault.Core.Tools;

namespace Vault.Core.Data
{
    public class BlockInfo
    {
        public BlockInfo()
        {
            
        }

        public BlockInfo(byte[] binary)
        {
            if(binary == null)
                throw new ArgumentNullException(nameof(binary));

            if(binary.Length != 9)
                throw new ArgumentException($"Length of binary array expected 9, but now is {binary.Length}.");

            binary.Read(r =>
            {
                Index = r.ReadUInt16();
                Continuation = r.ReadUInt16();
                Allocated = r.ReadInt32();
                Flags = (BlockFlags) r.ReadByte();
            });
        }

        public BlockInfo(ushort index, ushort continuation, int allocated, BlockFlags flags)
        {
            Index = index;
            Continuation = continuation;
            Allocated = allocated;
            Flags = flags;
        }


        public ushort Index { get; set; }

        public ushort Continuation { get; set; }

        public int Allocated { get; set; }

        public byte[] ToBinary()
        {
            var buffer = new byte[9];

            buffer.Write(w =>
            {
                w.Write(Index);
                w.Write(Continuation);
                w.Write(Allocated);
                w.Write((byte)Flags);
            });
            return buffer;
        }

        public BlockFlags Flags { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as BlockInfo);
        }

        protected bool Equals(BlockInfo other)
        {
            return Index == other.Index
                   && Continuation == other.Continuation
                   && Allocated == other.Allocated
                   && Flags == other.Flags;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Index.GetHashCode();
                hashCode = (hashCode*397) ^ Continuation.GetHashCode();
                hashCode = (hashCode*397) ^ Allocated;
                hashCode = (hashCode*397) ^ (int) Flags;
                return hashCode;
            }
        }
    }

    [Flags]
    public enum BlockFlags : byte
    {
        None = 0,

        IsMaserBlock = 1,

        IsFirstBlock = 2,

        IsLastBlock = 4
    }
}
