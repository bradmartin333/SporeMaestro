using SixLabors.ImageSharp.Memory;

namespace Camera
{
    internal class Acquire
    {
        private static bool BUSY = false;
        private static bool HandleImage = false;
        private const int TargetFPS = 60;
        private const int FPSWindow = 5; // For moving average calculation
        internal static int ActualCamFPS = TargetFPS / 2;
        private static int NumFrames = 0;
        private static int BufferIDX = 0;
        private static readonly int[] NumFramesBuffer = Enumerable.Repeat(ActualCamFPS, FPSWindow).ToArray();
#pragma warning disable IDE0052 // Remove unread private members
        private static Timer FPSTrigger = new(new TimerCallback(TriggerTick), null, 0, (int)(1000.0 / TargetFPS));
        private static readonly Timer FPSTracker = new(new TimerCallback(TrackerTick), null, 0, 1000);
#pragma warning restore IDE0052 // Remove unread private members

        private static Buffer2D<ulong>? IntImage = null;
        private static Buffer2D<ulong>? LastIntImage = null;

        private static void TriggerTick(object? state)
        {
            HandleImage = !BUSY;
        }

        private static void TrackerTick(object? state)
        {
            if (NumFrames == 0) return;
            NumFramesBuffer[BufferIDX] = NumFrames;
            BufferIDX = (BufferIDX + 1) % FPSWindow;
            ActualCamFPS = (int)((double)NumFramesBuffer.Sum() / FPSWindow);
            NumFrames = 0;
        }

        internal static void NewUSB2(ArraySegment<byte> arraySegment)
        {
            if (!HandleImage) return;
            BUSY = true;
            Image<Rgba32> image = Image.Load<Rgba32>(arraySegment);
            NewImage(image);
        }

        internal static void NewMJPEG(byte[] frameBuffer)
        {
            if (!HandleImage) return;
            BUSY = true;
            Image<Rgba32> image = Image.Load<Rgba32>(frameBuffer);
            NewImage(image);
        }

        internal static void NewFLIR(byte[] bytes, uint wid, uint hgt)
        {
            if (!HandleImage) return;
            BUSY = true;
            Rgba32[] data = new Rgba32[wid * hgt];
            for (int i = 0; i < bytes.Length; i += 3)
                data[i / 3] = new Rgba32() { R = bytes[i], G = bytes[i + 1], B = bytes[i + 2], A = byte.MaxValue };
            Image<Rgba32> image = new(Configuration.Default, 10, 10); //Image.LoadPixelData(data, (int)wid, (int)hgt);
            NewImage(image);
        }

        private static void NewImage(Image<Rgba32> image)
        {
            UpdateImage(image);

            //if (workspace.SaveFrameCount > 0)
            if (true)
            {
                //string path = workspace.SaveFrameCount > 1 ?
                    //workspace.SaveFramePath.Replace(".png", $"_{workspace.SaveFrameCount}.png") : workspace.SaveFramePath;
                try
                {
                    //Task.Factory.StartNew(() => image.Clone().SaveAsPngAsync(path));
                    //Console.WriteLine($"Frame saved to {path.Split('\\').Last()}");
                    //workspace.SaveFrameCount--;
                }
                catch (Exception)
                {
                    //Console.WriteLine($"Failed to save frame saved to {path}");
                }
            }

            NumFrames++;
            BUSY = false;
            FPSTrigger = new(new TimerCallback(TriggerTick), null, 0, (int)(1000.0 / TargetFPS)); // Reset trigger
        }

        private static void UpdateImage(Image<Rgba32> image)
        {
            //if (workspace.ResizeOptions == null) return;

            // Apply user transformations
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

            //// Apply custom processor within ROI regions
            //try
            //{
            //    IntImage = IntegralImage.CalculateIntegralImage(image);
            //    foreach (ROI roi in workspace.ROIs)
            //    {
            //        Rectangle? r = SafeRect(roi.Rect.x, roi.Rect.y, roi.Rect.width, roi.Rect.height, workspace.Width, workspace.Height);
            //        if (r != null && LastIntImage != null)
            //        {
            //            switch (roi.InputType)
            //            {
            //                case ROI.InputTypes.Null:
            //                    break;
            //                case ROI.InputTypes.Motion:
            //                    roi.Value = BackgroundSubtract.Process(ref image, r.Value, IntImage, LastIntImage);
            //                    break;
            //                case ROI.InputTypes.Focus:
            //                    roi.Value = Focus.Process(ref image, r.Value);
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //    LastIntImage = IntImage;
            //}
            //catch (Exception) { }

            // Extract pixel array
            Rgba32[] pixelArray = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(pixelArray);

            // Update window colors
            int pixelIdx = 0;
            for (int j = 0; j < image.Height; j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    int idx = (j * image.Width) + i;
                    Rgba32 pixel = pixelArray[pixelIdx];
                    //workspace.Colors[idx] = new Color(pixel.R, pixel.G, pixel.B, pixel.A);
                    pixelIdx++;
                }
            }
        }

        private static Rectangle? SafeRect(float x, float y, float wid, float hgt, int frameWid, int frameHgt)
        {
            if (x > frameWid || y > frameHgt) return null;
            if (x < 0)
            {
                wid += x;
                x = 0;
            }
            if (y < 0)
            {
                hgt += y;
                y = 0;
            }
            wid = (x + wid < frameWid) ? wid : frameWid - x;
            hgt = (y + hgt < frameHgt) ? hgt : frameHgt - y;
            if (wid < 0 || hgt < 0) return null;
            return new((int)x, (int)y, (int)wid, (int)hgt);
        }
    }
}
