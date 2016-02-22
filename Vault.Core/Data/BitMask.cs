﻿using System;

namespace Vault.Core.Data
{
    public class BitMask
    {

        public BitMask(byte[] byteArray, int maskLength = 0)
        {
            if (maskLength == 0 || byteArray.Length*NumberOfBitsInByte > maskLength)
                maskLength = byteArray.Length*NumberOfBitsInByte;
            _byteArray = byteArray;
            _maskLength = maskLength;
        }

        public byte[] Bytes => _byteArray;

        public bool GetValueOf(int indexOfBit)
        {
            if (indexOfBit > _maskLength - 1)
                throw new ArgumentException(nameof(indexOfBit));

            var indexOfByte = GetNumberOfByteWithBitIndex(indexOfBit);

            var mask = (byte) (1 << indexOfBit%NumberOfBitsInByte);
            return (_byteArray[indexOfByte] & mask) == mask;
        }

        public BitMask SetValueTo(int indexOfBit, bool value)
        {
            if (indexOfBit > _maskLength - 1)
                throw new ArgumentException(nameof(indexOfBit));

            var indexOfByte = GetNumberOfByteWithBitIndex(indexOfBit);

            var @byte = (byte) (1 << indexOfBit%NumberOfBitsInByte);

            _byteArray[indexOfByte] =
                value
                    ? _byteArray[indexOfByte] |= @byte
                    : _byteArray[indexOfByte] &= (byte) ~@byte;

            return this;
        }

        public BitMask SetValuesTo(bool value, params int[] indexes)
        {
            foreach (var index in indexes)
                SetValueTo(index, value);

            return this;
        }

        public bool this[int index]
        {
            get { return GetValueOf(index); }
            set { SetValueTo(index, value); }
        }

        public int GetFirstIndexOf(bool value)
        {
            int index = 0;
            while (index < _maskLength)
            {
                if (this[index] == value)
                    return index;
                index ++;
            }
            return -1;
        }

        // private functions

        private int GetNumberOfByteWithBitIndex(int bitIndex)
        {
            return bitIndex/NumberOfBitsInByte;
        }

        // fields

        private readonly byte[] _byteArray;
        private readonly int _maskLength;

        // constants

        private const int NumberOfBitsInByte = 8;
    }
}
