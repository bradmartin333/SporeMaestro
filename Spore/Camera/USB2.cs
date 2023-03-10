using FlashCap;

namespace Camera
{
    internal class USB2Cam : ICamera
    {
        internal List<(CaptureDeviceDescriptor, VideoCharacteristics)> Sources = new();
        private CaptureDeviceDescriptor? Capture;
        internal VideoCharacteristics? Characteristics;
        private CaptureDevice? Device;

        internal USB2Cam()
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
                //Capture = Sources[workspace.CamIdx].Item1;
                //Characteristics = Sources[workspace.CamIdx].Item2;

                //Workspace?.SourceNames.AddRange(Sources.Select(x => x.Item1.Name).Where(x => !x.ToLower().Contains("default")));
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
                Device?.Stop();
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
                Device.Start();
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
            //if (Workspace == null) return;
            //Utilities.Imaging.NewUSB2(bufferScope.Buffer.ReferImage(), Workspace);
            //Workspace.ColorsHaveUpdated = true;
        }
    }
}
