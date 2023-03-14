using ImageProcessing;
using SixLabors.ImageSharp.Memory;

namespace Camera
{
    /// <summary>
    /// TODO
    /// </summary>
    internal class Acquire
    {
        private static Buffer2D<ulong>? IntImage = null;
        private static Buffer2D<ulong>? LastIntImage = null;

        internal static void UpdateImage(Image<Rgba32> image)
        {
            //switch (workspace.RotationFlip)
            //{
            //    case 1: // Rotate
            //    case 6: // Flip XY
            //        image.Mutate(x => x.Resize(workspace.ResizeOptions).Rotate(RotateMode.Rotate180));
            //        break;
            //    case 2: // FLip X
            //    case 5: // Rotate, Flip Y
            //        image.Mutate(x => x.Resize(workspace.ResizeOptions).Flip(FlipMode.Horizontal));
            //        break;
            //    case 4: // Flip Y
            //    case 3: // Rotate, Flip X
            //        image.Mutate(x => x.Resize(workspace.ResizeOptions).Flip(FlipMode.Vertical));
            //        break;
            //    case 0: // None
            //    case 7: // Rotate, Flip XY
            //    default:
            //        image.Mutate(x => x.Resize(workspace.ResizeOptions));
            //        break;
            //}

            try
            {
                IntImage = IntegralImage.CalculateIntegralImage(image);
                if (LastIntImage != null)
                {
                    BackgroundSubtract.Process(ref image, new Rectangle(100, 100, 300, 300), IntImage, LastIntImage);
                    Focus.Process(ref image, new Rectangle(200, 200, 300, 300));
                }
                LastIntImage = IntImage;
            }
            catch (Exception) { }

            // Extract pixel array
            Rgba32[] pixelArray = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(pixelArray);
        }
    }
}
