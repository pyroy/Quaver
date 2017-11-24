﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Replays
{
    internal class ReplayFrame
    {
        /// <summary>
        ///     Holds the game time the frame was captured at
        /// </summary>
        internal long GameTime { get; set; }

        /// <summary>
        ///     The time in the song this replay occurred at.
        /// </summary>
        internal int SongTime { get; set; }

        /// <summary>
        ///     The difference in song time of the current and last frames taken in milliseconds
        /// </summary>
        internal int TimeSinceLastFrame { get; set; }

        /// <summary>
        ///     The state of the keys during this frame.
        /// </summary>
        internal KeyPressState KeyPressState { get; set; }
    }
}