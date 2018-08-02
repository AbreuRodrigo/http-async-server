using System;

namespace HttpAsyncServer
{
    public static class ServerRandomizer
    {
        private static readonly int seed = DateTime.Now.Millisecond;
        private static Random rand = new Random(seed);

        public static int Seed
        {
            get
            {
                return seed;
            }
        }

        public static int NextInt()
        {
            return rand.Next();
        }
    }
}