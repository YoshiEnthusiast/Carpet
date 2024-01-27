namespace Carpet
{
    public abstract class OpenALObject
    {
        public int Handle { get; protected init; }
        public bool Deleted { get; protected set; }

        public virtual void Delete()
        {
            Deleted = true;
        }
    }
}
