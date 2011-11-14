using System;

namespace KinectAMole
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (KinectAMoleGame game = new KinectAMoleGame())
            {
                game.Run();
            }
        }
    }
#endif
}

