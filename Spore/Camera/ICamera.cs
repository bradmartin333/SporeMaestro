namespace Camera
{
    public interface ICamera
    {
        public Raylib_CsLo.Color[] ProcessedColors { get; internal set; }
        public void Start();
        public void Stop();
    }
}
