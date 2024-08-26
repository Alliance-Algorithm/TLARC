using System.Buffers;
using Microsoft.Toolkit.HighPerformance;

namespace Tlarc.DataStructure;

partial class RingBuffer(int lengthOfByte)
{
    Memory<byte> _buffer = new byte[lengthOfByte * 2];

    int _writeOffset = 0;
    int _readOffset = 0;
    int _usedSizeOfByte = 0;
    public readonly int BufferSizeOfByte = lengthOfByte;
    public int UsedSizeOfByte => _usedSizeOfByte;
    public int FreedSizeOfByte => BufferSizeOfByte - _usedSizeOfByte;
    public MemoryHandle HeadOfBuffer => _buffer[_writeOffset..].Pin();
    public void UpdateHeadPointer(int dataLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dataLength, FreedSizeOfByte, nameof(dataLength));
        _usedSizeOfByte += dataLength;
        _writeOffset = (_writeOffset + dataLength) % BufferSizeOfByte;
    }
    public ReadOnlyMemory<TTo> CastDataTo<TTo>(int dataLength, bool movePointerToNext = true) where TTo : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(UsedSizeOfByte, dataLength, nameof(dataLength));
        _usedSizeOfByte -= dataLength;
        var offset = _readOffset;
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
            offset = (offset + dataLength) % BufferSizeOfByte;

        return _buffer[offset..dataLength].Cast<byte, TTo>();
    }
}