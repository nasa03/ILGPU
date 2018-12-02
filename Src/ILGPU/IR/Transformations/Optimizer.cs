﻿// -----------------------------------------------------------------------------
//                                    ILGPU
//                     Copyright (c) 2016-2018 Marcel Koester
//                                www.ilgpu.net
//
// File: Optimizer.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details
// -----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

namespace ILGPU.IR.Transformations
{
    /// <summary>
    /// Represetns an optimization level.
    /// </summary>
    public enum OptimizationLevel
    {
        /// <summary>
        /// Lightweight (required) transformations only.
        /// </summary>
        Debug,

        /// <summary>
        /// All transformation passes with seveal optimization
        /// iterations.
        /// </summary>
        Release,
    }

    /// <summary>
    /// Realizes utility helpers to perform and initialize transformations
    /// based on an <see cref="OptimizationLevel"/>.
    /// </summary>
    public static class Optimizer
    {
        /// <summary>
        /// Returns the number of known optimization levels.
        /// </summary>
        public const int NumOptimizationLevels = 2;

        /// <summary>
        /// Populates the given transformation manager with the required
        /// optimization transformations.
        /// </summary>
        /// <typeparam name="TInliningConfiguration">The inlining configuration type.</typeparam>
        /// <param name="builder">The transformation manager to populate.</param>
        /// <param name="inliningConfiguration">The desired inlining configuration.</param>
        /// <param name="level">The desired optimization level.</param>
        /// <returns>The maximum number of iterations.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddOptimizations<TInliningConfiguration>(
            this Transformer.Builder builder,
            in TInliningConfiguration inliningConfiguration,
            OptimizationLevel level)
            where TInliningConfiguration : IInliningConfiguration
        {
            switch (level)
            {
                case OptimizationLevel.Debug:
                    AddDebugOptimizations(builder, inliningConfiguration);
                    break;
                case OptimizationLevel.Release:
                    AddReleaseOptimizations(builder, inliningConfiguration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }

        /// <summary>
        /// Populates the given transformation manager with the required
        /// debug optimization transformations.
        /// </summary>
        /// <typeparam name="TInliningConfiguration">The inlining configuration type.</typeparam>
        /// <param name="builder">The transformation manager to populate.</param>
        /// <param name="inliningConfiguration">The desired inlining configuration.</param>
        public static void AddDebugOptimizations<TInliningConfiguration>(
            this Transformer.Builder builder,
            in TInliningConfiguration inliningConfiguration)
            where TInliningConfiguration : IInliningConfiguration
        {
            builder.Add(new Inliner<TInliningConfiguration>(inliningConfiguration));
            builder.Add(new SimplifyControlFlow());
            builder.Add(new SSAConstruction());
            builder.Add(new DeadCodeElimination());
        }

        /// <summary>
        /// Populates the given transformation manager with the required
        /// release optimization transformations.
        /// </summary>
        /// <typeparam name="TInliningConfiguration">The inlining configuration type.</typeparam>
        /// <param name="builder">The transformation manager to populate.</param>
        /// <param name="inliningConfiguration">The desired inlining configuration.</param>
        public static void AddReleaseOptimizations<TInliningConfiguration>(
            this Transformer.Builder builder,
            in TInliningConfiguration inliningConfiguration)
            where TInliningConfiguration : IInliningConfiguration
        {
            builder.Add(new Inliner<TInliningConfiguration>(inliningConfiguration));
            builder.Add(new SimplifyControlFlow());
            builder.Add(new InferAddressSpaces());
            builder.Add(new SSAConstruction());
            builder.Add(new DeadCodeElimination());

            builder.Add(new SimplifyControlFlow());
            builder.Add(new Inliner<TInliningConfiguration>(inliningConfiguration));
            builder.Add(new SSAConstruction());
        }

        /// <summary>
        /// Creates a transformer for the given optimization level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="configuration">The transformer configuration.</param>
        /// <param name="inliningConfiguration">The inlining configuration.</param>
        /// <returns>The created transformer.</returns>
        public static Transformer CreateTransformer<TInliningConfiguration>(
            this OptimizationLevel level,
            TransformerConfiguration configuration,
            in TInliningConfiguration inliningConfiguration)
            where TInliningConfiguration : IInliningConfiguration
        {
            var builder = Transformer.CreateBuilder(configuration);
            builder.AddOptimizations(inliningConfiguration, level);
            return builder.ToTransformer();
        }
    }
}
