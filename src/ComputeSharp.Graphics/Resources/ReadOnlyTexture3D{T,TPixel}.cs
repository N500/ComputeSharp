﻿using System.Diagnostics;
using ComputeSharp.__Internals;
using ComputeSharp.Exceptions;
using ComputeSharp.Graphics.Resources.Enums;
using ComputeSharp.Resources;
using ComputeSharp.Resources.Views;

#pragma warning disable CS0618

namespace ComputeSharp
{
    /// <summary>
    /// A <see langword="class"/> representing a typed readonly 3D texture stored on GPU memory.
    /// </summary>
    /// <typeparam name="T">The type of items stored on the texture.</typeparam>
    /// <typeparam name="TPixel">The type of pixels used on the GPU side.</typeparam>
    [DebuggerTypeProxy(typeof(Texture3DDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public sealed class ReadOnlyTexture3D<T, TPixel> : Texture3D<T>, IReadOnlyTexture3D<TPixel>
        where T : unmanaged, IUnorm<TPixel>
        where TPixel : unmanaged
    {
        /// <summary>
        /// Creates a new <see cref="ReadOnlyTexture3D{T,TPixel}"/> instance with the specified parameters.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/> associated with the current instance.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="depth">The depth of the texture.</param>
        internal ReadOnlyTexture3D(GraphicsDevice device, int width, int height, int depth)
            : base(device, width, height, depth, ResourceType.ReadOnly)
        {
        }

        /// <inheritdoc/>
        public TPixel this[int x, int y, int z] => throw new InvalidExecutionContextException($"{nameof(ReadOnlyTexture3D<T, TPixel>)}<T>[int,int,int]");

        /// <inheritdoc/>
        public TPixel this[Int3 xyz] => throw new InvalidExecutionContextException($"{nameof(ReadOnlyTexture3D<T, TPixel>)}<T>[Int3]");

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ComputeSharp.ReadOnlyTexture3D<{typeof(T)},{nameof(TPixel)}>[{Width}, {Height}, {Depth}]";
        }
    }
}
