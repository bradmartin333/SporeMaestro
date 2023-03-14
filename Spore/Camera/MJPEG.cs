namespace Camera
{
    /// <summary>
    /// An amazingly simple implementation by Larry57 from 2015
    /// </summary>
    internal class MJPEG : ICamera
    {
        public Raylib_CsLo.Color[] ProcessedColors { get; set; } = new Raylib_CsLo.Color[Config.WID * Config.HGT];

        // JPEG delimiters
        const byte PicMarker = 0xFF;
        const byte PicStart = 0xD8;
        const byte PicEnd = 0xD9;

        private readonly string MJPEGAddress = ":8000/stream.mjpg";
        private readonly CancellationTokenSource CTS = new();

        internal MJPEG()
        {
            //if (workspace == null) return;
            //MJPEGAddress = workspace.RPiAddress + MJPEGAddress;
            //Workspace = workspace;
            //Workspace?.SourceNames.Clear();
        }

        public void Start()
        {
            new Thread(async () => { await StartAsync(MJPEGAddress, CTS.Token); }).Start();
        }

        public void Stop()
        {
            CTS.Cancel();
        }

        private async Task StartAsync(string url, CancellationToken token, int chunkMaxSize = 1024)
        {
            try
            {
                using var cli = new HttpClient();
                using var stream = await cli.GetStreamAsync(url).ConfigureAwait(false);
                var streamBuffer = new byte[1024];         // Stream chunk read
                var frameBuffer = new byte[1280 * 960];    // Frame buffer
                var frameIdx = 0;       // Last written byte location in the frame buffer
                var inPicture = false;  // Are we currently parsing a picture ?
                byte current = 0x00;    // The last byte read
                byte previous = 0x00;   // The byte before
                while (!token.IsCancellationRequested) // Continuously pump the stream. The cancellationtoken is used to get out of there
                {
                    var streamLength = await stream.ReadAsync(streamBuffer.AsMemory(0, chunkMaxSize), token).ConfigureAwait(false);
                    ParseStreamBuffer(frameBuffer, ref frameIdx, streamLength, streamBuffer, ref inPicture, ref previous, ref current);
                };
            }
            catch (Exception)
            {
                Console.WriteLine($"Could not connect to MJPEG stream at {MJPEGAddress}");
            }
        }

        private void ParseStreamBuffer(byte[] frameBuffer, ref int frameIdx, int streamLength, byte[] streamBuffer, ref bool inPicture, ref byte previous, ref byte current)
        {
            var idx = 0;
            while (idx < streamLength)
            {
                if (inPicture)
                    ParsePicture(frameBuffer, ref frameIdx, ref streamLength, streamBuffer, ref idx, ref inPicture, ref previous, ref current);
                else
                    SearchPicture(frameBuffer, ref frameIdx, ref streamLength, streamBuffer, ref idx, ref inPicture, ref previous, ref current);
            }
        }

        // While we are looking for a picture, look for a FFD8 (end of JPEG) sequence.
        private static void SearchPicture(byte[] frameBuffer, ref int frameIdx, ref int streamLength, byte[] streamBuffer, ref int idx, ref bool inPicture, ref byte previous, ref byte current)
        {
            do
            {
                previous = current;
                current = streamBuffer[idx++];
                if (previous == PicMarker && current == PicStart) // JPEG picture start ?
                {
                    frameIdx = 2;
                    frameBuffer[0] = PicMarker;
                    frameBuffer[1] = PicStart;
                    inPicture = true;
                    return;
                }
            } while (idx < streamLength);
        }

        // While we are parsing a picture, fill the frame buffer until a FFD9 is reach.
        private void ParsePicture(byte[] frameBuffer, ref int frameIdx, ref int streamLength, byte[] streamBuffer, ref int idx, ref bool inPicture, ref byte previous, ref byte current)
        {
            do
            {
                previous = current;
                current = streamBuffer[idx++];
                frameBuffer[frameIdx++] = current;
                if (previous == PicMarker && current == PicEnd) // JPEG picture end ?
                {
                    //Utilities.Imaging.NewMJPEG(frameBuffer, Workspace);
                    //Workspace.ColorsHaveUpdated = true;
                    inPicture = false;
                    return;
                }
            } while (idx < streamLength);
        }
    }
}
