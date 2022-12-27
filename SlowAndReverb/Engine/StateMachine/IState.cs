using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public interface IState
    {
        void Update(float deltaTime);

        void Draw();

        void OnStart();

        void OnTerminate();
    }
}
