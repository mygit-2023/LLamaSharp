﻿using System;

namespace LLama.Native;

using llama_token = Int32;

/// <summary>
/// Input data for llama_decode. A llama_batch object can contain input about one or many sequences.
/// </summary>
public sealed class LLamaBatchSafeHandle
    : SafeLLamaHandleBase
{
    private readonly int _embd;

    /// <summary>
    /// Get the native llama_batch struct
    /// </summary>
    public LLamaNativeBatch NativeBatch { get; private set; }

    /// <summary>
    /// the token ids of the input (used when embd is NULL)
    /// </summary>
    public Span<llama_token> Token
    {
        get
        {
            unsafe
            {
                if (_embd != 0)
                    return new Span<int>(null, 0);
                else
                    return new Span<int>(NativeBatch.token, NativeBatch.n_tokens);
            }
        }
    }

    /// <summary>
    /// token embeddings (i.e. float vector of size n_embd) (used when token is NULL)
    /// </summary>
    public Span<llama_token> Embed
    {
        get
        {
            unsafe
            {
                // If embd != 0, llama_batch.embd will be allocated with size of n_tokens *embd * sizeof(float)
                // Otherwise, llama_batch.token will be allocated to store n_tokens llama_token

                if (_embd != 0)
                    return new Span<llama_token>(NativeBatch.embd, NativeBatch.n_tokens * _embd);
                else
                    return new Span<llama_token>(null, 0);
            }
        }
    }

    /// <summary>
    /// the positions of the respective token in the sequence
    /// </summary>
    public Span<LLamaPos> Pos
    {
        get
        {
            unsafe
            {
                return new Span<LLamaPos>(NativeBatch.pos, NativeBatch.n_tokens);
            }
        }
    }

    /// <summary>
    /// the sequence to which the respective token belongs
    /// </summary>
    public Span<LLamaSeqId> Sequence_ID
    {
        get
        {
            unsafe
            {
                return new Span<LLamaSeqId>(NativeBatch.seq_id, NativeBatch.n_tokens);
            }
        }
    }

    /// <summary>
    /// if zero, the logits for the respective token will not be output
    /// </summary>
    public Span<byte> Logits
    {
        get
        {
            unsafe
            {
                return new Span<byte>(NativeBatch.logits, NativeBatch.n_tokens);
            }
        }
    }

    /// <summary>
    /// Create a safe handle owning a `LLamaNativeBatch`
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="embd"></param>
    public LLamaBatchSafeHandle(LLamaNativeBatch batch, int embd)
        : base((nint)1)
    {
        _embd = embd;
        NativeBatch = batch;
    }

    /// <summary>
    /// Call `llama_batch_init` and create a new batch
    /// </summary>
    /// <param name="n_tokens"></param>
    /// <param name="embd"></param>
    /// <returns></returns>
    public static LLamaBatchSafeHandle Create(int n_tokens, int embd)
    {
        var batch = NativeApi.llama_batch_init(n_tokens, embd);
        return new LLamaBatchSafeHandle(batch, embd);
    }

    /// <inheritdoc />
    protected override bool ReleaseHandle()
    {
        NativeApi.llama_batch_free(NativeBatch);
        NativeBatch = default;
        SetHandle(IntPtr.Zero);
        return true;
    }
}