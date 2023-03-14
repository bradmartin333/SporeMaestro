using SpinnakerNET;
using SpinnakerNET.GenApi;
using System.Security.Cryptography;

namespace Camera
{
    /// <summary>
    /// TODO
    /// </summary>
    public class FLIR : ICamera
    {
        public Raylib_CsLo.Color[] ProcessedColors { get; set; } = new Raylib_CsLo.Color[Config.WID * Config.HGT];

        private readonly ManagedSystem System = new();
        private readonly ManagedCameraList? Sources;
        private readonly IManagedCamera? Capture;

        public FLIR()
        {
            Sources = System.GetCameras();
            if (Sources.Count == 0) return;
            Capture = Sources.GetByIndex(0);
            Start();
        }

        public void Start()
        {
            if (Capture == null) return;
            Capture.Init();
            INodeMap nodeMap = Capture.GetNodeMap();
            ConfigureCamera(nodeMap);
            ImageEventListener imageEventListener = new(ProcessedColors);
            Capture.RegisterEventHandler(imageEventListener);
            Capture.BeginAcquisition();
        }

        public void Stop()
        {
            try
            {
                Capture?.EndAcquisition();
                Capture?.DeInit();
                System.Dispose();
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to stop FLIR");
            }
        }

        private void ConfigureCamera(INodeMap nodeMap)
        {
            // Set acquisition mode to continuous
            IEnum iAcquisitionMode = nodeMap.GetNode<IEnum>("AcquisitionMode");
            if (iAcquisitionMode == null || !iAcquisitionMode.IsWritable)
            {
                Console.WriteLine("Unable to set acquisition mode to continuous (node retrieval). Aborting...\n");
                return;
            }
            IEnumEntry iAcquisitionModeContinuous = iAcquisitionMode.GetEntryByName("Continuous");
            if (iAcquisitionModeContinuous == null || !iAcquisitionModeContinuous.IsReadable)
            {
                Console.WriteLine("Unable to set acquisition mode to continuous (enum entry retrieval). Aborting...\n");
                return;
            }
            iAcquisitionMode.Value = iAcquisitionModeContinuous.Symbolic;

            // Get newest frame every time
            if (Capture == null) return;
            INodeMap sNodeMap = Capture.GetTLStreamNodeMap();
            IEnum handlingMode = sNodeMap.GetNode<IEnum>("StreamBufferHandlingMode");
            if (handlingMode == null || !handlingMode.IsWritable)
            {
                Console.WriteLine("Unable to set Buffer Handling mode (node retrieval). Aborting...");
                return;
            }
            IEnumEntry handlingModeEntry = handlingMode.GetEntryByName("NewestOnly");
            handlingMode.Value = handlingModeEntry.Value;
        }
    }

    internal class ImageEventListener : ManagedImageEventHandler
    {
        private readonly Raylib_CsLo.Color[]? ProcessedColors = null;
        private readonly IManagedImageProcessor? Processor;

        public ImageEventListener(Raylib_CsLo.Color[] processedColors)
        {
            ProcessedColors = processedColors;
            Processor = new ManagedImageProcessor();
            Processor.SetColorProcessing(ColorProcessingAlgorithm.HQ_LINEAR);
        }

        override protected void OnImageEvent(ManagedImage raw)
        {
            try
            {
                if (!raw.IsIncomplete && Processor != null && ProcessedColors != null)
                {
                    using IManagedImage convertedImage = Processor.Convert(raw, PixelFormatEnums.RGB8);
                    byte[] bytes = convertedImage.ManagedData;
                    Rgba32[] data = new Rgba32[convertedImage.Width * convertedImage.Height];
                    for (int i = 0; i < bytes.Length; i += 3)
                        data[i / 3] = new Rgba32() { R = bytes[i], G = bytes[i + 1], B = bytes[i + 2], A = byte.MaxValue };
                    raw.Release();

                    // Unpack and resize image
                    Image<Rgba32> image = Image.LoadPixelData<Rgba32>(data.AsSpan(), (int)convertedImage.Width, (int)convertedImage.Height);
                    Acquire.UpdateImage(image); // TESTING
                    image.Mutate(x => x.Resize(new ResizeOptions()
                    {
                        Size = new(Config.WID, Config.HGT),
                        Mode = ResizeMode.Pad,
                    }));

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
                            ProcessedColors[idx] = new Raylib_CsLo.Color(pixel.R, pixel.G, pixel.B, pixel.A);
                            pixelIdx++;
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error on FLIR image event");
            }
        }
    }
}
