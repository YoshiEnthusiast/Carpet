namespace Carpet.Examples.Platforming
{
    public class Grapplable : Component
    {
        private Grapple _grapple;

        public event Grapple Grapple
        {
            add
            {
                _grapple += value;
            }

            remove
            {
                _grapple -= value;
            }
        }

        public void Grappled(Player by)
        {
            _grapple?.Invoke(this, by);
        }
    }
}
