using SpinnakerNET;
using SpinnakerNET.GenApi;

namespace Camera
{
    internal class FLIR : ICamera
    {
        class ImageEventListener : ManagedImageEventHandler
        {
            readonly IManagedImageProcessor? Processor;

            public ImageEventListener()
            {
                Processor = new ManagedImageProcessor();
                Processor.SetColorProcessing(ColorProcessingAlgorithm.HQ_LINEAR);
            }

            override protected void OnImageEvent(ManagedImage raw)
            {
                try
                {
                    if (!raw.IsIncomplete && Processor != null)
                    {
                        using IManagedImage convertedImage = Processor.Convert(raw, PixelFormatEnums.RGB8);
                        byte[] bytes = convertedImage.ManagedData;
                        //Utilities.Imaging.NewFLIR(bytes, convertedImage.Width, convertedImage.Height, Workspace);
                        raw.Release();
                        //Workspace.ColorsHaveUpdated = true;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error on FLIR image event");
                }
            }
        }

        private readonly ManagedSystem System = new();
        private readonly ManagedCameraList? Sources;
        private readonly IManagedCamera? Capture;

        internal FLIR()
        {
            //Workspace = workspace;
            //Workspace?.SourceNames.Clear();

            Sources = System.GetCameras();
            if (Sources.Count == 0) return;
            //Capture = Sources.GetByIndex((uint)workspace.CamIdx);

            //Workspace?.SourceNames.AddRange(Enumerable.Range(1, Sources.Count).Select(x => "FLIR " + x.ToString()));
        }

        public void Start()
        {
            if (Capture == null) return;
            Capture.Init();
            INodeMap nodeMap = Capture.GetNodeMap();
            ConfigureCamera(nodeMap);
            ImageEventListener imageEventListener = new();
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
}
