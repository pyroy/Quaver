using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Config;
using Quaver.Online;
using Wobble.Logging;

namespace Quaver
{
    public static class Program
    {
        /// <summary>
        ///     The path of the current executable.
        /// </summary>
        public static string ExecutablePath => System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", "");

        /// <summary>
        ///     The current working directory of the executable.
        /// </summary>
        public static string WorkingDirectory => Path.GetDirectoryName(ExecutablePath).Replace(@"file:\", "");

        [STAThread]
        public static void Main()
        {
            // Log all unhandled exceptions.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exception = args.ExceptionObject as Exception;
                Logger.Error(exception, LogType.Runtime);
            };

            // Change the working directory to where the executable is.
            Directory.SetCurrentDirectory(WorkingDirectory);
            Environment.CurrentDirectory = WorkingDirectory;

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            ConfigManager.Initialize();
            SteamManager.Initialize();

            using (var game = new QuaverGame())
                game.Run();
        }
    }
}
