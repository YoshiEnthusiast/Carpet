namespace SlowAndReverb
{
    public abstract class OpenGLObject
    {
        public int Handle { get; protected init; }
        public bool Deleted { get; protected set; }

        public virtual void Bind()
        {
            if (Deleted)
                return;

            Bind(Handle);
        }

        public virtual void Unbind()
        {
            if (Deleted)
                return;

            Bind(0);
        }

        public virtual void Delete()
        {
            if (Deleted)
                return;

            Delete(Handle);
        }

        protected virtual void Bind(int handle)
        {

        }

        protected virtual void Delete(int handle)
        {

        }
    }
}
