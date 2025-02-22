﻿using System.Buffers;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using LLama.Extensions;
using LLama.Native;

namespace LLama;

/// <summary>
/// Decodes a stream of tokens into a stream of characters
/// </summary>
public sealed class StreamingTokenDecoder
{
    private readonly SafeLlamaModelHandle _weights;
    private readonly Decoder _decoder;

    private readonly List<char> _characters = new();

    /// <summary>
    /// The number of decoded characters waiting to be read
    /// </summary>
    public int AvailableCharacters => _characters.Count;

    #region constructors
    /// <summary>
    /// Create a new decoder
    /// </summary>
    /// <param name="encoding">Text encoding to use</param>
    /// <param name="weights">Model weights</param>
    public StreamingTokenDecoder(Encoding encoding, LLamaWeights weights)
        : this(encoding, weights.NativeHandle)
    {
    }

    /// <summary>
    /// Create a new decoder
    /// </summary>
    /// <param name="context">Context to retrieve encoding and model weights from</param>
    public StreamingTokenDecoder(LLamaContext context)
        : this(context.Encoding, context.NativeHandle)
    {
    }

    /// <summary>
    /// Create a new decoder
    /// </summary>
    /// <param name="encoding">Text encoding to use</param>
    /// <param name="context">Context to retrieve model weights from</param>
    public StreamingTokenDecoder(Encoding encoding, SafeLLamaContextHandle context)
        : this(encoding, context.ModelHandle)
    {
    }

    /// <summary>
    /// Create a new decoder
    /// </summary>
    /// <param name="encoding">Text encoding to use</param>
    /// <param name="weights">Models weights to use</param>
    public StreamingTokenDecoder(Encoding encoding, SafeLlamaModelHandle weights)
    {
        _weights = weights;
        _decoder = encoding.GetDecoder();
    }
    #endregion

    /// <summary>
    /// Add a single token to the decoder
    /// </summary>
    /// <param name="token"></param>
    public void Add(int token)
    {
        var charsArr = ArrayPool<char>.Shared.Rent(16);
        var bytesArr = ArrayPool<byte>.Shared.Rent(16);
        try
        {
            // Convert this token into bytes
            var bytesAvailable = TokenToBytes(ref bytesArr, token, _weights).Length;

            // Convert those bytes into characters
            var bytesOffset = 0;
            var completed = false;
            while (!completed)
            {
                // Decode some of the bytes into the temp char buffer. Keep doing this
                // until all bytes have been consumed
                _decoder.Convert(
                    bytesArr, bytesOffset, bytesAvailable,
                    charsArr, 0, charsArr.Length,
                    false,
                    out var bytesUsed, out var charsUsed, out completed
                );
                bytesOffset += bytesUsed;
                bytesAvailable -= bytesUsed;

                // Add the decoded characters to the output buffer
                _characters.AddSpan(charsArr.AsSpan(0, charsUsed));
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(charsArr);
            ArrayPool<byte>.Shared.Return(bytesArr);
        }

        return;

        // Converts a single token into bytes, using the `bytes` array as temporary storage.
        // If the `bytes` array is too small it will get a larger one from the ArrayPool.
        static Span<byte> TokenToBytes(ref byte[] bytes, int token, SafeLlamaModelHandle model)
        {
            // Try to get bytes
            var l = model.TokenToSpan(token, bytes);

            // Negative length indicates that the output was too small. Expand it to twice that size and try again.
            if (l < 0)
            {
                // Return the old array to the pool and get a new one
                ArrayPool<byte>.Shared.Return(bytes);
                bytes = ArrayPool<byte>.Shared.Rent(-l * 2);

                // Get bytes, this time it can't fail
                l = model.TokenToSpan(token, bytes);
            }

            Debug.Assert(l >= 0);
            return new Span<byte>(bytes, 0, l);
        }
    }

    /// <summary>
    /// Add all tokens in the given enumerable
    /// </summary>
    /// <param name="tokens"></param>
    public void AddRange(IEnumerable<int> tokens)
    {
        foreach (var item in tokens)
            Add(item);
    }

    /// <summary>
    /// Read all decoded characters and clear the buffer
    /// </summary>
    /// <param name="dest"></param>
    public void Read(List<char> dest)
    {
        dest.AddRange(_characters);
        _characters.Clear();
    }

    /// <summary>
    /// Read all decoded characters as a string and clear the buffer
    /// </summary>
    /// <returns></returns>
    public string Read()
    {
        if (_characters.Count == 0)
            return "";

        var str = string.Join("", _characters);
        _characters.Clear();
        return str;
    }

    /// <summary>
    /// Set the decoder back to its initial state
    /// </summary>
    public void Reset()
    {
        _decoder.Reset();
        _characters.Clear();
    }
}