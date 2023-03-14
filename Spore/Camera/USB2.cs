using FlashCap;

namespace Camera
{
    public class USB2Cam : ICamera
    {
        public Raylib_CsLo.Color[] ProcessedColors { get; set; } = new Raylib_CsLo.Color[Config.WID * Config.HGT];
        private readonly List<(CaptureDeviceDescriptor, VideoCharacteristics)> Sources = new();
        private CaptureDeviceDescriptor? Capture;
        private readonly VideoCharacteristics? Characteristics;
        private CaptureDevice? Device;

        public USB2Cam()
        {
            CaptureDevices devices = new();
            foreach (CaptureDeviceDescriptor device in devices.EnumerateDescriptors())
                if (device.Characteristics.Any())
                    foreach (VideoCharacteristics c in device.Characteristics)
                    {
                        Sources.Add((device, c));
                        break;
                    }

            if (Sources.Any())
            {
                Capture = Sources[0].Item1;
                Characteristics = Sources[0].Item2;
                Start();
            }
            else Console.WriteLine("No USB2 camera sources found");
        }

        public async void Start()
        {
            await StartDevice();
        }

        public void Stop()
        {
            try
            {
                Device?.StopAsync();
                Capture = null;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to stop USB2Cam");
            }

        }

        private async Task<bool> StartDevice()
        {
            if (Capture == null || Characteristics == null) return false;
            try
            {
                Device = await Capture.OpenAsync(Characteristics, OnPixelBufferArrivedAsync);
                await Device.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not start camera device: {ex.Message}");
                return false;
            }
            return true;
        }

        private void OnPixelBufferArrivedAsync(PixelBufferScope bufferScope)
        {
            // Unpack and resize image
            Image<Rgba32> image = Image.Load<Rgba32>(bufferScope.Buffer.ReferImage());
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
}
