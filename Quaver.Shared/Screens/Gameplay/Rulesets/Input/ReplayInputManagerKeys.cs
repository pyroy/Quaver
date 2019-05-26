/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Replays;
using Quaver.API.Replays.Virtual;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Input
{
    public class ReplayInputManagerKeys
    {
        /// <summary>
        ///     Reference to the actual gameplay screen.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     Reference to the hitobject manager.
        /// </summary>
        private HitObjectManagerKeys Manager => Screen.Ruleset.HitObjectManager as HitObjectManagerKeys;

        /// <summary>
        ///     The replay that is currently loaded.
        /// </summary>
        internal Replay Replay { get; }

        /// <summary>
        ///     The frame that we are currently on in the replay.
        /// </summary>
        internal int CurrentFrame { get; set; } = 1;

        /// <summary>
        ///     If there are unique key presses in the current frame, per lane.
        /// </summary>
        internal List<bool> UniquePresses { get; } = new List<bool>();

        /// <summary>
        ///     If there are unique key releases in the current frame, per lane.
        /// </summary>
        internal List<bool> UniqueReleases { get; } = new List<bool>();

        /// <summary>
        ///     Virtually plays replay frames
        /// </summary>
        public VirtualReplayPlayer VirtualPLayer { get; }

        /// <summary>
        ///     The current frame being played in the virtual replay player
        /// </summary>
        private int CurrentVirtualReplayStat { get; set; } = -1;

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        internal ReplayInputManagerKeys(GameplayScreen screen)
        {
            Screen = screen;
            Replay = Screen.LoadedReplay;

            VirtualPLayer = new VirtualReplayPlayer(Replay, Screen.Map);
            VirtualPLayer.PlayAllFrames();

            // Populate unique key presses/releases.
            for (var i = 0; i < screen.Map.GetKeyCount(); i++)
            {
                UniquePresses.Add(false);
                UniqueReleases.Add(false);
            }
        }

        /// <summary>
        ///     Determines which frame we are on in the replay and sets if it has unique key presses/releases.
        /// </summary>
        internal void HandleInput()
        {
            HandleScoring();

            if (CurrentFrame >= Replay.Frames.Count || !(Manager.CurrentAudioPosition >= Replay.Frames[CurrentFrame].Time) || !Screen.InReplayMode)
                return;

            var previousActive = Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame - 1].Keys);
            var currentActive = Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame].Keys);

            foreach (var activeLane in currentActive)
            {
                if (!previousActive.Contains(activeLane))
                    UniquePresses[activeLane] = true;
            }

            foreach (var activeLane in previousActive)
            {
                if (!currentActive.Contains(activeLane))
                    UniqueReleases[activeLane] = true;
            }

            CurrentFrame++;
        }

        private void HandleScoring()
        {
            for (var i = CurrentVirtualReplayStat + 1; i < VirtualPLayer.ScoreProcessor.Stats.Count; i++)
            {
                var hom = Screen.Ruleset.HitObjectManager as HitObjectManagerKeys;

                if (hom?.CurrentAudioPosition >= VirtualPLayer.ScoreProcessor.Stats[i].SongPosition)
                {
                    ((ScoreProcessorKeys)Screen.Ruleset.ScoreProcessor).CalculateScore(VirtualPLayer.ScoreProcessor.Stats[i].Judgement);

                    // Update Scoreboard
                    var view = (GameplayScreenView) Screen.View;
                    view.UpdateScoreAndAccuracyDisplays();

                    CurrentVirtualReplayStat++;
                }
                else
                    break;
            }
        }

        internal void HandleSkip()
        {
            var frame = Replay.Frames.FindLastIndex(x => x.Time <= Manager.CurrentAudioPosition);

            if (frame == -1)
                return;

            CurrentFrame = ModManager.IsActivated(ModIdentifier.Autoplay) ? frame + 1 : frame;
        }
    }
}
