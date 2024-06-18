using System.IO;
using System.Threading;
using TK.MSTS.Tokens;

namespace KE.MSTS.TsUnpack
{
    /// <summary>
    /// Handles loading of .trk and .tdb files for a given route in a separate thread.
    /// </summary>
    internal class SimisFileLoader
    {
        private readonly Thread trkLoadingThread;
        private readonly Thread tdbLoadingThread;

        private TokenFile? trkFile;
        private TokenFile? tdbFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimisFileLoader"/> class and starts loading the .trk and .tdb files for the specified route.
        /// </summary>
        /// <param name="mstsPath">The base path of the MSTS installation.</param>
        /// <param name="routeDirectory">The directory of the route being loaded.</param>
        public SimisFileLoader(string mstsPath, string routeDirectory)
        {
            trkLoadingThread = new Thread(() =>
            {
                trkFile = LoadFile(Path.Combine(mstsPath, "ROUTES", routeDirectory, routeDirectory + ".trk"));
            })
            {
                Name = "Loading .trk file",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            trkLoadingThread.Start();

            tdbLoadingThread = new Thread(() =>
            {
                tdbFile = LoadFile(Path.Combine(mstsPath, "ROUTES", routeDirectory, routeDirectory + ".tdb"));
            })
            {
                Name = "Loading .tdb file",
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            tdbLoadingThread.Start();
        }

        /// <summary>
        /// Gets the loaded .trk file. If the file is still being loaded, this method will block until the loading is complete.
        /// </summary>
        /// <returns>The loaded <see cref="TokenFile"/> representing the .trk file, or null if the file could not be loaded.</returns>
        public TokenFile? GetTrkFile()
        {
            if (trkFile == null && trkLoadingThread.IsAlive)
            {
                trkLoadingThread.Join();
            }

            return trkFile;
        }

        /// <summary>
        /// Gets the loaded .tdb file. If the file is still being loaded, this method will block until the loading is complete.
        /// </summary>
        /// <returns>The loaded <see cref="TokenFile"/> representing the .tdb file, or null if the file could not be loaded.</returns>
        public TokenFile? GetTdbFile()
        {
            if (tdbFile == null && tdbLoadingThread.IsAlive)
            {
                tdbLoadingThread.Join();
            }

            return tdbFile;
        }

        /// <summary>
        /// Loads a token file from the specified file path.
        /// </summary>
        /// <param name="fileName">The full path of the file to load.</param>
        /// <returns>A <see cref="TokenFile"/> object representing the loaded file.</returns>
        private static TokenFile LoadFile(string fileName)
        {
            return TokenFile.ReadFile(fileName);
        }
    }
}
