using System.Diagnostics.CodeAnalysis;

namespace Kernel.Core.SoFuckingFastAlgorithms;

using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Numerics;

public sealed class HybridDictionary<TValue>
{
    // 短键使用 FrozenDictionary + 完美哈希
    private readonly FrozenDictionary<ulong, TValue> _shortKeyDict;

    // 长键使用 SIMD 优化字典
    private readonly SimdDictionary _longKeyDict;

    // 短键阈值（可调整）
    private const int ShortKeyThreshold = 16;

    public HybridDictionary(IEnumerable<KeyValuePair<string, TValue>> items)
    {
        // 分区处理
        var shortItems = new Dictionary<ulong, TValue>();
        var longItems = new Dictionary<string, TValue>();

        foreach (var (key, value) in items)
            if (key.Length <= ShortKeyThreshold)
            {
                var encoded = HybridDictionary<TValue>.EncodeShortKey(key);
                shortItems[encoded] = value;
            }
            else
            {
                longItems[key] = value;
            }

        // 构建优化结构
        _shortKeyDict = shortItems.ToFrozenDictionary();
        _longKeyDict = new SimdDictionary(longItems);
    }

    public TValue this[string key]
    {
        get
        {
            if (!TryGetValue(key, out var value))
                throw new IndexOutOfRangeException($"Key is {key}");
            return value;
        }
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
    {
        if (key.Length <= ShortKeyThreshold)
        {
            var encoded = HybridDictionary<TValue>.EncodeShortKey(key);
            return _shortKeyDict.TryGetValue(encoded, out value);
        }

        return _longKeyDict.TryGetValue(key, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong EncodeShortKey(string key)
    {
        // 将短字符串编码为ULONG进行快速比较
        fixed (char* ptr = key)
        {
            ulong result = 0;
            var len = Math.Min(key.Length, 4); // 8字节=4字符

            for (var i = 0; i < len; i++)
                result |= (ulong)ptr[i] << i * 16;
            return result;
        }
    }

    // SIMD 优化字典实现
    private sealed class SimdDictionary
    {
        private struct Entry
        {
            public string Key;
            public TValue Value;
            public uint Hash;
        }

        private readonly Entry[] _buckets;
        private readonly int _capacity;

        public SimdDictionary(Dictionary<string, TValue> source)
        {
            _capacity = SimdDictionary.GetPrime(source.Count * 2);
            _buckets = new Entry[_capacity];

            foreach (var (key, value) in source)
            {
                var hash = SimdDictionary.ComputeCrossPlatformHash(key);
                var index = (int)(hash % (uint)_capacity);

                // 线性探测解决冲突
                index = (index + 1) % _capacity;

                _buckets[index] = new Entry { Key = key, Value = value, Hash = hash };
            }
        }

        public bool TryGetValue(string key, out TValue value)
        {
            var hash = SimdDictionary.ComputeCrossPlatformHash(key);
            var index = (int)(hash % (uint)_capacity);
            var start = index;

            do
            {
                ref var entry = ref _buckets[index];

                // 先比较哈希值，再比较键内容
                if (entry.Hash == hash && key == entry.Key)
                {
                    value = entry.Value;
                    return true;
                }

                index = (index + 1) % _capacity;
            } while (index != start);

            value = default;
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint FinalizeHash(uint h)
        {
            // 改进的混合函数（基于MurmurHash3）
            h ^= h >> 16;
            h *= 0x85ebca6bu;
            h ^= h >> 13;
            h *= 0xc2b2ae35u;
            h ^= h >> 16;
            return h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint ComputeCrossPlatformHash(string key)
        {
            var hash = 2166136261u;
            var len = key.Length;
            var i = 0;
            var ptr = Unsafe.As<ushort[]>(key);
            // 使用 Vector<T> 跨平台 SIMD
            var vectorSize = Vector<uint>.Count * 2; // 每次处理的字符数
            while (len - i >= vectorSize)
            {
                // 加载字符块
                var chunk = new Vector<ushort>(ptr, i);

                // 拆分为两个向量处理
                var part1 = Vector.AsVectorUInt32(Vector.WidenLower(chunk));
                var part2 = Vector.AsVectorUInt32(Vector.WidenUpper(chunk));

                // 应用 FNV-1a
                var hashVec = new Vector<uint>(hash);
                hashVec ^= part1;
                hashVec *= 16777619u;
                hashVec ^= part2;
                hashVec *= 16777619u;

                // 合并结果
                for (var j = 0; j < Vector<uint>.Count; j++)
                    hash = hashVec[j];

                i += vectorSize;
            }

            // 处理剩余字符
            for (; i < len; i++)
            {
                hash ^= key[i];
                hash *= 16777619u;
            }

            return HybridDictionary<TValue>.SimdDictionary.FinalizeHash(hash);
        }

        // 获取大于等于 min 的最小质数（优化版本）
        private static int GetPrime(int min)
        {
            // 常见质数缓存
            int[] primes =
            {
                3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
                1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
                17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
                187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
                1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
            };

            // 检查缓存质数
            foreach (var prime in primes)
                if (prime >= min)
                    return prime;

            // 对于大于缓存的数，计算扩展质数
            for (var candidate = min | 1; ; candidate += 2)
            {
                if (candidate < 0) // 溢出保护
                    throw new OverflowException("Prime size overflow");

                if (HybridDictionary<TValue>.SimdDictionary.IsPrime(candidate))
                    return candidate;
            }
        }

        // 优化的素数检查
        private static bool IsPrime(int n)
        {
            if (n <= 1) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            // 快速检查小因子
            if (n % 3 == 0 || n % 5 == 0 || n % 7 == 0 ||
                n % 11 == 0 || n % 13 == 0 || n % 17 == 0)
                return false;

            // 只需检查到 sqrt(n)
            var limit = (int)Math.Sqrt(n);
            for (var i = 19; i <= limit; i += 2)
                if (n % i == 0)
                    return false;

            return true;
        }
    }
}