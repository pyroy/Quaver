﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Commands;
using Quaver.Config;
using Quaver.Database;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Discord;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Replays;

namespace Quaver
{
    /// <summary>
    ///     The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Set UTF-8 encoding for console outputs
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Initialize Config
            ConfigManager.InitializeConfig();

            // Delete Temp Files
            DeleteTemporaryFiles();
 
            // Set up the game
            SetupGame();
      
            // Start game
            using (var game = new QuaverGame())
            {
                game.Run();
            }
        }

        /// <summary>
        ///     Deletes all temporary files if there are any.
        /// </summary>
        private static void DeleteTemporaryFiles()
        {
            try
            {
                foreach (var file in new DirectoryInfo(ConfigManager.DataDirectory + "/temp/").GetFiles("*", SearchOption.AllDirectories))
                    file.Delete();

                foreach (var dir in new DirectoryInfo(ConfigManager.DataDirectory + "/temp/").GetDirectories("*", SearchOption.AllDirectories))
                    dir.Delete(true);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Responsible for initializing and setting the map database and setting the loaded maps
        /// </summary>
        private static void SetupGame()
        {
            // Create now playing folder
            Directory.CreateDirectory(ConfigManager.DataDirectory + "/temp/Now Playing/");

            // Set the build version
            GameBase.BuildVersion = MapsetHelper.GetMd5Checksum(ConfigManager.GameDirectory + "/" + "Quaver.exe");

            // After initializing the configuration, we want to sync the map database, and load the dictionary of mapsets.
            var loadGame = Task.Run(async () =>
            {
                await MapCache.LoadAndSetMapsets();

                // Create the local scores database if it doesn't already exist
                await LocalScoreCache.CreateScoresDatabase();
                
                // Force garbage collection
                GC.Collect();
            });
            Task.WaitAll(loadGame);
        }
    }
}