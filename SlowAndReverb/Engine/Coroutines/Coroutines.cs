using System.Collections;

namespace Carpet
{
    public static class Coroutines
    {
        private static CoroutineRunner s_runner = new();

        public static int AliveCount => s_runner.AliveCount;

        public static void Update(float deltaTime)
        {
            s_runner.DoUpdate(deltaTime);
        }

        public static int Start(IEnumerator enumerator, float initialDelay)
        {
            return s_runner.StartCoroutine(enumerator, initialDelay);   
        }

        public static int Start(IEnumerator enumerator)
        {
            return Start(enumerator);
        }

        public static bool Stop(int id)
        {
            return s_runner.StopCoroutine(id);  
        }

        public static void StopAll()
        {
            s_runner.StopAllCoroutines();
        }

        public static bool IsAlive(int id)
        {
            return s_runner.IsAlive(id);
        }
    }
}
