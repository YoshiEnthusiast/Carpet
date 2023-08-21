namespace SlowAndReverb
{
    public class TestBackground : Background
    {
        public TestBackground() : base("testBackgroundMain")
        {
            PushParallaxLayer("testBackground0", 0.4f);
            PushParallaxLayer("testBackground1", 0.8f);
        }
    }
}
