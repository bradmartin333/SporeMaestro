using SixLabors.ImageSharp.Memory;

namespace ImageProcessing
{
    internal static class BackgroundSubtract
    {
        private const int CLUSTER_SIZE = 5;
        private const float THRESHOLD = 250.0f;

        internal static float Process(ref Image<Rgba32> image, Rectangle r, Buffer2D<ulong> intImg, Buffer2D<ulong> lastIntImg)
        {
            float thisProcessingValue = -1f;
            image.ProcessPixelRows(accessor =>
            {
                for (int y = r.Top; y < r.Bottom; y += CLUSTER_SIZE)
                {
                    for (int x = r.Left; x < r.Right; x += CLUSTER_SIZE)
                    {
                        int maxX = r.Right - 1;
                        int maxY = r.Bottom - 1;
                        int x1 = Math.Clamp(x - CLUSTER_SIZE + 1, 0, maxX);
                        int x2 = Math.Min(x + CLUSTER_SIZE + 1, maxX);
                        int y1 = Math.Clamp(y - CLUSTER_SIZE + 1, 0, maxY);
                        int y2 = Math.Min(y + CLUSTER_SIZE + 1, maxY);
                        ulong sum = Math.Min(intImg[x2, y2] - intImg[x1, y2] - intImg[x2, y1] + intImg[x1, y1], ulong.MaxValue);
                        ulong lastSum = Math.Min(lastIntImg[x2, y2] - lastIntImg[x1, y2] - lastIntImg[x2, y1] + lastIntImg[x1, y1], ulong.MaxValue);
                        ulong diff = lastSum > sum ? lastSum - sum : sum - lastSum;
                        if (diff > THRESHOLD)
                        {
                            Color c = Colorizer.GetScoreColor(THRESHOLD / (diff - THRESHOLD));
                            if (c != Color.Transparent)
                            {
                                for (int j = 0; j < CLUSTER_SIZE; j++)
                                {
                                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y + j);
                                    for (int i = 0; i < CLUSTER_SIZE; i++)
                                    {
                                        ref Rgba32 pixel = ref pixelRow[x + i];
                                        pixel = c;
                                    }
                                }
                                thisProcessingValue++;
                            }
                        }
                    }
                }
            });
            return thisProcessingValue;
        }
    }
}
