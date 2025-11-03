using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using RileyCache.MultiLevel.Options;

namespace RileyCache.MultiLevel.Strategies;

/// <summary>
/// 简单的内存型布隆过滤器实现。
/// 通过多哈希函数映射到位数组，提供“可能存在 / 一定不存在”的快速判断。
/// </summary>
public sealed class InMemoryBloomFilter : IBloomFilter
{
    private readonly int _numBits;
    private readonly int _numHashes;
    private readonly uint[] _bitArray;

    /// <summary>
    /// 基于容量与误判率（FPP）初始化布隆过滤器参数。
    /// </summary>
    public InMemoryBloomFilter(IOptions<MultiLevelCacheOptions> options)
    {
        // Derive bit size from capacity and error rate using standard formula.
        var capacity = Math.Max(1, options.Value.BloomFilterCapacity);
        var errorRate = Math.Clamp(options.Value.BloomFilterErrorRate, 0.0001, 0.2);

        var m = (int)Math.Ceiling(-(capacity * Math.Log(errorRate)) / (Math.Pow(Math.Log(2), 2)));
        _numBits = Math.Max(512, m);
        _numHashes = Math.Max(2, (int)Math.Round((_numBits / (double)capacity) * Math.Log(2)));

        var numUInts = (_numBits + 31) / 32;
        _bitArray = new uint[numUInts];
    }

    /// <summary>
    /// 添加元素到过滤器。
    /// </summary>
    public void Add(string item)
    {
        Span<byte> hash = stackalloc byte[32];
        Hash(item, hash);
        var (h1, h2) = SplitHash(hash);

        for (var i = 0; i < _numHashes; i++)
        {
            var combined = (int)((h1 + (ulong)i * h2) % (ulong)_numBits);
            SetBit(combined);
        }
    }

    /// <summary>
    /// 判断元素可能存在。
    /// </summary>
    public bool MightContain(string item)
    {
        Span<byte> hash = stackalloc byte[32];
        Hash(item, hash);
        var (h1, h2) = SplitHash(hash);

        for (var i = 0; i < _numHashes; i++)
        {
            var combined = (int)((h1 + (ulong)i * h2) % (ulong)_numBits);
            if (!GetBit(combined)) return false;
        }
        return true;
    }

    /// <summary>
    /// 计算元素的 SHA256 哈希。
    /// </summary>
    private static void Hash(string item, Span<byte> destination)
    {
        using var sha = SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(item);
        sha.TryComputeHash(bytes, destination, out _);
    }

    /// <summary>
    /// 将哈希拆分为两个 64 位子哈希，用于双重哈希法。
    /// </summary>
    private static (ulong h1, ulong h2) SplitHash(ReadOnlySpan<byte> hash)
    {
        var h1 = BinaryPrimitives.ReadUInt64LittleEndian(hash.Slice(0, 8));
        var h2 = BinaryPrimitives.ReadUInt64LittleEndian(hash.Slice(8, 8));
        if (h2 == 0) h2 = 0x9E3779B97F4A7C15UL; // golden ratio if degenerate
        return (h1, h2);
    }

    /// <summary>
    /// 设置位数组中的指定比特。
    /// </summary>
    private void SetBit(int bitIndex)
    {
        var idx = bitIndex >> 5;
        var offset = bitIndex & 31;
        _bitArray[idx] |= 1u << offset;
    }

    /// <summary>
    /// 读取位数组中的指定比特。
    /// </summary>
    private bool GetBit(int bitIndex)
    {
        var idx = bitIndex >> 5;
        var offset = bitIndex & 31;
        return ((_bitArray[idx] >> offset) & 1u) == 1u;
    }
}


