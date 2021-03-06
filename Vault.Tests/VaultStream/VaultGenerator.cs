﻿using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using Vault.Core.Data;
using Vault.Core.Tools;

namespace Vault.Tests.VaultStream
{
    public class VaultGenerator
    {
        public VaultGenerator()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
        }

        public VaultGenerator InitializeVault(VaultConfiguration configuration, VaultInfo vaultInfo)
        {
            _configuration = configuration;
            var buffer = new byte[configuration.VaultMetadataSize];
            buffer.Write(w =>
            {
                w.Write((byte)vaultInfo.Flags);
                w.Write(vaultInfo.NumbersOfAllocatedBlocks);
                w.Write(vaultInfo.Mask.Bytes);
                w.WriteString2(vaultInfo.Name);
            });

            //for (int i = 0; i < buffer.Length; i++)
            //    buffer[i] = 1;

            _writer.Write(buffer);

            WriteBlockWithPattern(pattern: new byte[] {10, 11, 12}, isMasterBlock: true);

            return this;
        }

        public VaultGenerator WriteBlockWithPattern(ushort continuation = 0, int allocated = DefaultBlockCOntentSize, byte[] pattern = null, bool isFirstBlock = true, bool isMasterBlock = false, bool? isLastBlock = null)
        {
            if (pattern == null)
                pattern = new byte[] {1, 2, 3};


            var flags = BlockFlags.None;
            if(isFirstBlock)
                flags |= BlockFlags.IsFirstBlock;
            if(isMasterBlock)
                flags |= BlockFlags.IsMaserBlock;
            if(continuation == 0 && isLastBlock != false)
                flags |= BlockFlags.IsLastBlock;

            var blockInfo = new BlockInfo(_currentIndex, continuation, allocated, flags);

            var allocatedSize = allocated < DefaultBlockCOntentSize ? allocated : DefaultBlockCOntentSize;

            var buffer = Gc.GetByteBufferFromPattern(pattern, DefaultBlockCOntentSize, allocatedSize);

            _writer.Write(blockInfo.ToBinary());
            _writer.Write(buffer);

            _currentIndex++;

            return this;
        }

        public VaultGenerator WriteBlockWithContent(ushort continuation, int allocated = DefaultBlockCOntentSize, BlockFlags flags = BlockFlags.None, byte[] content = null)
        {
            Contract.Requires(content != null);
            Contract.Requires(content.Length == allocated);

            var blockInfo = new BlockInfo(_currentIndex, continuation, allocated, flags);

            var binary = blockInfo.ToBinary();
            var buffer = new byte[_configuration.BlockContentSize];

            // ReSharper disable once AssignNullToNotNullAttribute
            // ReSharper disable once PossibleNullReferenceException
            Array.Copy(content, 0, buffer, 0, content.Length);
            _writer.Write(binary);
            _writer.Write(buffer);

            _currentIndex++;

            return this;
        }

        public MemoryStream GetStream()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            return _stream;
        }

        public byte[] GetContentWithoutVaultInfo()
        {
            _stream.Seek(_configuration.VaultMetadataSize, SeekOrigin.Begin);
            var reader = new BinaryReader(_stream);

            var result = reader.ReadBytes(_configuration.BlockFullSize * (_currentIndex + 1));
            return result;
        }

        private ushort _currentIndex;

        private readonly MemoryStream _stream;
        private readonly BinaryWriter _writer;

        private VaultConfiguration _configuration;

        private const int DefaultBlockCOntentSize = 55;
    }
}
