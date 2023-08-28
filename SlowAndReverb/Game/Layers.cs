namespace SlowAndReverb
{
    public static class Layers
    {
        public static SmoothCameraLayer Foreground { get; private set; }
        public static Layer Background { get; private set; }
        public static Layer UI { get; private set; }

        internal static void Initialize()
        {
            Background = new Layer(1280, 720, 0f);

            Foreground = new SmoothCameraLayer(324, 184, 320, 180, 1f)
            {
                Material = new ForegroundMaterial(),
                ClearColor = new Color(40, 40, 40)
            };

            Foreground.Camera.SetCenterOrigin();

            UI = new Layer(320, 180, 2f);
        }
    }
}
