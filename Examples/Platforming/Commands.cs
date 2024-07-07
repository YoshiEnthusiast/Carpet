namespace Carpet.Examples.Platforming
{
    public static class Commands
    {
        [Command("spawn")]
        public static void Spawn(Entity entity, float x, float y)
        {
            entity.Position = new Vector2(x, y);

            Game.CurrentScene.Add(entity);
        }

        [Command("mpos")]
        public static void MousePosition()
        {
            DebugConsole.Log(Game.ForegroundLayer.MousePosition);
        }
    }
}
