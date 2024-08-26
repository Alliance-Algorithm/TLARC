using System.Buffers;
using Microsoft.Toolkit.HighPerformance;

namespace Tlarc.DataStructure;

partial class RingBuffer<THead>(int lengthOfByte) where THead : unmanaged
{
    Memory<byte> _buffer = new byte[lengthOfByte * 2];
    int _writeOffset = 0;
    int _readOffset = 0;
    int _usedSizeOfByte = 0;
    public readonly int BufferSizeOfByte = lengthOfByte;
    public int UsedSizeOfByte => _usedSizeOfByte;
    public unsafe readonly int SizeOfHeader = sizeof(THead);

    public MemoryHandle HeadOfBuffer => _buffer[_writeOffset..].Pin();
    public void UpdateWritePointer(int dataLength) => _writeOffset = (_writeOffset + dataLength) % BufferSizeOfByte;
    public ReadOnlyMemory<TTo> CastDataTo<TTo>(int dataLength, bool movePointerToNext = true, bool ignoreHeader = true) where TTo : unmanaged
    {
        var offset = _readOffset + (ignoreHeader ? SizeOfHeader : 0);
        using var begin = _buffer.Pin();
        using var end = _buffer[BufferSizeOfByte..].Pin();
        if (dataLength > BufferSizeOfByte)
            throw new OutOfMemoryException(
                string.Format("RingBuffer: Length of Output Data Length : {0} is Larger then BufferSizeOfByte : {1}"
                , dataLength, BufferSizeOfByte));

        unsafe
        {
            if (offset + dataLength > BufferSizeOfByte)
                Buffer.MemoryCopy(begin.Pointer, end.Pointer,
                 offset + dataLength - BufferSizeOfByte, offset + dataLength - BufferSizeOfByte);
        }
        if (movePointerToNext)
            _readOffset = (offset + dataLength) % BufferSizeOfByte;

        return _buffer[offset..dataLength].Cast<byte, TTo>();
    }

    public THead GetHead() => CastDataTo<THead>(SizeOfHeader, false, false).Span[0];
}