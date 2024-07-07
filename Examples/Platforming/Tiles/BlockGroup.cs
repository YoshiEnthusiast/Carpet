﻿using System.Collections;

namespace Carpet.Examples.Platforming
{
    public class BlockGroup : Entity
    {
        private readonly LightOccluder _occluder;
        private readonly CoroutineRunner _coroutineRunner;

        private float _alpha = 1f;
        private bool _fading;

        public BlockGroup(float x, float y, IEnumerable<AutoTile> blocks) : base(x, y)
        {
            Blocks = blocks;

            _coroutineRunner = Add(new CoroutineRunner());
            _occluder = Add(new LightOccluder()
            {
                Mode = OcclusionMode.EntityRectangle
            });
        }

        public IEnumerable<AutoTile> Blocks { get; private init; }

        public void FadeAway()
        {
            if (_fading)
                return;

            Remove(_occluder);
            _coroutineRunner.StartCoroutine(UpdateFadeAway());

            _fading = true;
        }

        private IEnumerator UpdateFadeAway()
        {
            while (true)
            {
                _alpha -= 0.05f;

                foreach (AutoTile block in Blocks)
                    block.Get<Sprite>().Color = Color.White * _alpha;

                if (_alpha <= 0f)
                {
                    foreach (AutoTile block in Blocks)
                        Scene.Remove(block);

                    Scene.Remove(this);

                    yield break;
                }

                yield return 2f;
            }
        }
    }
}
