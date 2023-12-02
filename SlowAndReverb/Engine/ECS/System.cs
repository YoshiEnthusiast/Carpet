namespace SlowAndReverb
{
    public abstract class System
    {
        public System(Scene scene)
        {
            Scene = scene;
        }

        protected Scene Scene { get; private init; }

        public virtual void Initialize()
        {

        }

        public virtual void Update(float deltaTime)
        {

        }

        public virtual void OnBeforeDraw()
        {
            
        }

        public virtual void Draw()
        {

        }

        public virtual void OnLateDraw()
        {

        }

        public virtual void Terminate()
        {

        } 
    }
}
