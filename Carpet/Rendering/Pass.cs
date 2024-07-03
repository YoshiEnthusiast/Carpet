namespace Carpet
{
    public class Pass
    {
        private PassProcess _process;

        public event PassProcess Render
        {
            add
            {
                _process += value;
            }

            remove
            {
                _process -= value;
            }
        }

        public virtual void Process()
        {
            _process?.Invoke();
        }

        protected void InvokeProcessEvent()
        {
            _process?.Invoke();
        }
    }
}
