﻿using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.Timeline
{
    public class TimelineSnapLine : Sprite
    {
        private EditorScrollContainerKeys Container { get; }

        /// <summary>
        ///     The timing point this snap line belongs to.
        /// </summary>
        public TimingPointInfo TimingPoint { get; }

        /// <summary>
        ///     The time in the song the line is located.
        /// </summary>
        public float Time { get; }

        /// <summary>
        ///     The index of the timing point this snap line is.
        /// </summary>
        public int Index { get; }

        /// <summary>
        ///     If the object is currently on-screen.
        /// </summary>
        public bool IsInView { get; set; }

        /// <summary>
        ///     The text that displays the measure in the song.
        /// </summary>
        private SpriteTextBitmap TextMeasure { get; set; }

        /// <summary>
        ///     Determines if this line is for a measure.
        /// </summary>
        private bool IsMeasureLine => Index / Container.Ruleset.Screen.BeatSnap.Value % 4 == 0
                                      && Index % Container.Ruleset.Screen.BeatSnap.Value == 0 && Time >= TimingPoint.StartTime;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="tp"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <param name="measureCount"></param>
        public TimelineSnapLine(EditorScrollContainerKeys container, TimingPointInfo tp, float time, int index, int measureCount)
        {
            Container = container;
            TimingPoint = tp;
            Index = index;
            Time = time;

            if (!IsMeasureLine)
                return;

            TextMeasure = new SpriteTextBitmap(FontsBitmap.MuliBold, measureCount.ToString())
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                FontSize = 28
            };

            TextMeasure.X = -TextMeasure.Width - 15;
            Y = -2;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            DrawToSpriteBatch();

            if (IsMeasureLine)
                TextMeasure.DrawToSpriteBatch();
        }

        /// <summary>
        ///     Checks if the timing line is on-screen.
        /// </summary>
        /// <returns></returns>
        public bool CheckIfOnScreen() => Time * Container.TrackSpeed >= Container.TrackPositionY - Container.Height &&
                                         Time * Container.TrackSpeed <= Container.TrackPositionY + Container.Height;
    }
}