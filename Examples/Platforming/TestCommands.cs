using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet.Platforming
{
    public static class TestCommands
    {
        [Command("method")]
        public static void MethodCommand(string amogus, Key key, float number = 5f)
        {
            DebugConsole.Log(amogus, LogType.Warning);
            DebugConsole.Log(number);
        }

        [Command("spawn")]
        public static void Spawn(Entity entity, float x, float y)
        {
            entity.Position = new Vector2(x, y);

            Game.CurrentScene.Add(entity);
        }
    }
}
