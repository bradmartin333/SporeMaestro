using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using System.Buffers;

// From /ImageSharp/Processing/Extensions/ProcessingExtensions.IntegralImage.cs
namespace ImageProcessing
{
    /// <summary>
    /// Apply an image integral. <See href="https://en.wikipedia.org/wiki/Summed-area_table"/>
    /// </summary>
    internal static partial class IntegralImage
    {
        /// <param name="source">The image on which to apply the integral.</param>
        /// <typeparam name="TPixel">The type of the pixel.</typeparam>
        /// <returns>The <see cref="Buffer2D{T}"/> containing all the sums.</returns>
        public static Buffer2D<ulong> CalculateIntegralImage<TPixel>(this Image<TPixel> source)
            where TPixel : unmanaged, IPixel<TPixel>
            => source.Frames.RootFrame.CalculateIntegralImage();

        /// <param name="source">The image on which to apply the integral.</param>
        /// <param name="bounds">The bounds within the image frame to calculate.</param>
        /// <typeparam name="TPixel">The type of the pixel.</typeparam>
        /// <returns>The <see cref="Buffer2D{T}"/> containing all the sums.</returns>
        public static Buffer2D<ulong> CalculateIntegralImage<TPixel>(this Image<TPixel> source, Rectangle bounds)
            where TPixel : unmanaged, IPixel<TPixel>
            => source.Frames.RootFrame.CalculateIntegralImage(bounds);

        /// <param name="source">The image frame on which to apply the integral.</param>
        /// <typeparam name="TPixel">The type of the pixel.</typeparam>
        /// <returns>The <see cref="Buffer2D{T}"/> containing all the sums.</returns>
        public static Buffer2D<ulong> CalculateIntegralImage<TPixel>(this ImageFrame<TPixel> source)
            where TPixel : unmanaged, IPixel<TPixel>
            => source.CalculateIntegralImage(source.Bounds());

        /// <param name="source">The image frame on which to apply the integral.</param>
        /// <param name="bounds">The bounds within the image frame to calculate.</param>
        /// <typeparam name="TPixel">The type of the pixel.</typeparam>
        /// <returns>The <see cref="Buffer2D{T}"/> containing all the sums.</returns>
        public static Buffer2D<ulong> CalculateIntegralImage<TPixel>(this ImageFrame<TPixel> source, Rectangle bounds)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Configuration configuration = source.GetConfiguration();

            var interest = Rectangle.Intersect(bounds, source.Bounds());
            int startY = interest.Y;
            int startX = interest.X;
            int endY = interest.Height;

            Buffer2D<ulong> intImage = configuration.MemoryAllocator.Allocate2D<ulong>(interest.Width, interest.Height);
            ulong sumX0 = 0;
            Buffer2D<TPixel> sourceBuffer = source.PixelBuffer;

            using (IMemoryOwner<L8> tempRow = configuration.MemoryAllocator.Allocate<L8>(interest.Width))
            {
                Span<L8> tempSpan = tempRow.Memory.Span;
                Span<TPixel> sourceRow = sourceBuffer.DangerousGetRowSpan(startY).Slice(startX, tempSpan.Length);
                Span<ulong> destRow = intImage.DangerousGetRowSpan(0);

                PixelOperations<TPixel>.Instance.ToL8(configuration, sourceRow, tempSpan);

                // First row
                for (int x = 0; x < tempSpan.Length; x++)
                {
                    sumX0 += tempSpan[x].PackedValue;
                    destRow[x] = sumX0;
                }

                Span<ulong> previousDestRow = destRow;

                // All other rows
                for (int y = 1; y < endY; y++)
                {
                    sourceRow = sourceBuffer.DangerousGetRowSpan(y + startY).Slice(startX, tempSpan.Length);
                    destRow = intImage.DangerousGetRowSpan(y);

                    PixelOperations<TPixel>.Instance.ToL8(configuration, sourceRow, tempSpan);

                    // Process first column
                    sumX0 = tempSpan[0].PackedValue;
                    destRow[0] = sumX0 + previousDestRow[0];

                    // Process all other colmns
                    for (int x = 1; x < tempSpan.Length; x++)
                    {
                        sumX0 += tempSpan[x].PackedValue;
                        destRow[x] = sumX0 + previousDestRow[x];
                    }

                    previousDestRow = destRow;
                }
            }

            return intImage;
        }
    }
}
