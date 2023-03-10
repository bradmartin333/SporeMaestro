namespace ImageProcessing
{
    internal static class Focus
    {
        private const int CLUSTER_SIZE = 10;

        internal static float Process(ref Image<Rgba32> image, Rectangle r)
        {
            float thisProcessingValue = -1f;
            int scanArea = CLUSTER_SIZE * CLUSTER_SIZE;
            int thisRight = Math.Min(image.Width, r.Right);
            int thisBottom = Math.Min(image.Height, r.Bottom);
            image.ProcessPixelRows(accessor =>
            {
                for (int y = r.Top; y < thisBottom; y += CLUSTER_SIZE)
                {
                    int thisClusterY = Math.Min(thisBottom - y, CLUSTER_SIZE);
                    for (int x = r.Left; x < thisRight; x += CLUSTER_SIZE)
                    {
                        int thisClusterX = Math.Min(thisRight - x, CLUSTER_SIZE);
                        double pDiff = 0f;
                        double p1 = -1;
                        for (int j = 0; j < thisClusterY; j++)
                        {
                            Span<Rgba32> pixelRow = accessor.GetRowSpan(y + j);
                            for (int i = 0; i < thisClusterX; i++)
                            {
                                ref Rgba32 pixel = ref pixelRow[x + i];
                                double p2 = pixel.R;
                                if (p1 == -1) p1 = p2;
                                else if (p1 != 0 && p2 != 0)
                                    pDiff += Math.Abs((p1 - p2) / ((p1 + p2) / 2));
                            }
                        }

                        pDiff /= scanArea;
                        Color c = Colorizer.GetScoreColor(pDiff);
                        if (pDiff > 0.1) thisProcessingValue++;
                        for (int j = 0; j < thisClusterY; j++)
                        {
                            Span<Rgba32> pixelRow = accessor.GetRowSpan(y + j);
                            for (int i = 0; i < thisClusterX; i++)
                            {
                                ref Rgba32 pixel = ref pixelRow[x + i];
                                pixel = c;
                            }
                        }
                    }
                }
            });
            return thisProcessingValue;
        }
    }
}
