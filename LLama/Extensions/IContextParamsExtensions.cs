﻿using System;
using System.IO;
using LLama.Abstractions;
using LLama.Native;

namespace LLama.Extensions
{
    /// <summary>
    /// Extention methods to the IContextParams interface
    /// </summary>
    public static class IContextParamsExtensions
    {
        /// <summary>
        /// Convert the given `IModelParams` into a `LLamaContextParams`
        /// </summary>
        /// <param name="params"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void ToLlamaContextParams(this IContextParams @params, out LLamaContextParams result)
        {
            result = NativeApi.llama_context_default_params();
            result.n_ctx = @params.ContextSize;
            result.n_batch = @params.BatchSize;
            result.seed = @params.Seed;
            result.f16_kv = @params.UseFp16Memory;
            result.logits_all = @params.Perplexity;
            result.embedding = @params.EmbeddingMode;
            result.rope_freq_base = @params.RopeFrequencyBase;
            result.rope_freq_scale = @params.RopeFrequencyScale;
            result.mul_mat_q = @params.MulMatQ;

            result.n_threads = Threads(@params.Threads);
            result.n_threads_batch = Threads(@params.BatchThreads);
        }

        private static uint Threads(uint? value)
        {
            if (value is > 0)
                return (uint)value;

            return (uint)Math.Max(Environment.ProcessorCount / 2, 1);
        }
    }
}
