using System;

namespace SlowAndReverb
{
    public class GrapplableCoin : Coin
    {
        public GrapplableCoin(float x, float y) : base(x, y)
        {
            SpriteName = "grapplableCoin";
            Color = new Color(103, 14, 171);

            var grapplable = new Grapplable();
            grapplable.Grapple += OnGrappled;

            Add(grapplable);
        }

        private void OnGrappled(Grapplable grapplable, Player player)
        {
            Collect();
        }
    }
}
