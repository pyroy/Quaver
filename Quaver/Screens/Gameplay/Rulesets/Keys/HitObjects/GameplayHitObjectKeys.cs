using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects
{
    public class GameplayHitObjectKeys : GameplayHitObject
    {
        /// <summary>
        ///     Reference to the Keys ruleset.
        /// </summary>
        private GameplayRulesetKeys Ruleset { get; set; }

        /// <summary>
        ///     Reference to the actual playfield.
        /// </summary>
        private GameplayPlayfieldKeys Playfield { get; set; }

        /// <summary>
        ///     Is determined by whether the player is holding the key that this hit object is binded to
        /// </summary>
        private bool CurrentlyBeingHeld { get; set; }

        /// <summary>
        ///     If the note is a long note.
        ///     In .qua format, long notes are defined as if the end time is greater than 0.
        /// </summary>
        public bool IsLongNote { get; private set; }

        /// <summary>
        ///     The Y position of the HitObject Sprites.
        /// </summary>
        private float SpritePosition { get; set; }

        /// <summary>
        ///     The width of the object.
        /// </summary>
        private float Width { get; set; }

        /// <summary>
        ///     The Y-Offset from the receptor.
        /// </summary>
        public long TrackPosition { get; set; }

        /// <summary>
        ///     The long note Y offset from the receptor.
        /// </summary>
        public long LongNoteTrackPosition { get; set; }

        /// <summary>
        ///     The initial size of this object's long note.
        /// </summary>
        private long InitialLongNoteSize { get; set; }

        /// <summary>
        ///     The current size of this object's long note.
        /// </summary>
        private long CurrentLongNoteSize { get; set; }

        /// <summary>
        ///      The offset of the long note body from the hit object.
        /// </summary>
        private float LongNoteBodyOffset { get; set; }

        /// <summary>
        ///     The offset of the hold end from hold body.
        /// </summary>
        private float LongNoteEndOffset { get; set; }

        /// <summary>
        ///     The actual HitObject sprite.
        /// </summary>
        private Sprite HitObjectSprite { get; set; }

        /// <summary>
        ///     The hold body sprite for long notes.
        /// </summary>
        public AnimatableSprite LongNoteBodySprite { get; set; }

        /// <summary>
        ///     The hold end sprite for long notes.
        /// </summary>
        private Sprite LongNoteEndSprite { get; set; }

        /// <summary>
        ///     The SpriteEffects. Flips the image horizontally if we are using upscroll.
        /// </summary>
        private static SpriteEffects Effects => !ConfigManager.DownScroll4K.Value &&
                                                SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].FlipNoteImagesOnUpscroll
            ? SpriteEffects.FlipVertically
            : SpriteEffects.None;

        /// <summary>
        ///     Y offset from the receptor. Calculated from hit body size and global offset
        /// </summary>
        public static float HitPositionOffset { get; set; } = 0;

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="info"></param>
        public GameplayHitObjectKeys(HitObjectInfo info, GameplayRulesetKeys ruleset) : base(info)
        {
            // Set References to other classes
            Playfield = (GameplayPlayfieldKeys)ruleset.Playfield;
            Ruleset = ruleset;
            Info = info;

            // Reference variables
            var posX = Playfield.Stage.Receptors[info.Lane - 1].X;
            var laneIndex = info.Lane - 1;

            // Create the base HitObjectSprite
            HitObjectSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                SpriteEffect = Effects,
                Image = GetHitObjectTexture(laneIndex)
            };

            // Update hit body's size to match image ratio
            HitObjectSprite.Size = new ScalableVector2(Playfield.LaneSize, Playfield.LaneSize * HitObjectSprite.Image.Height / HitObjectSprite.Image.Width);
            LongNoteBodyOffset = HitObjectSprite.Height / 2;

            // Create Hold Body
            var bodies = SkinManager.Skin.Keys[Ruleset.Mode].NoteHoldBodies[laneIndex];
            LongNoteBodySprite = new AnimatableSprite(bodies)
            {
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Playfield.LaneSize, 0),
                Position = new ScalableVector2(posX, 0),
                Parent = Playfield.Stage.HitObjectContainer
            };

            // Create the Hold End
            LongNoteEndSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                Size = new ScalableVector2(Playfield.LaneSize, 0),
                Parent = Playfield.Stage.HitObjectContainer,
                SpriteEffect = Effects
            };

            // Set long note end properties.
            LongNoteEndSprite.Image = SkinManager.Skin.Keys[Ruleset.Mode].NoteHoldEnds[laneIndex];
            LongNoteEndSprite.Height = Playfield.LaneSize * LongNoteEndSprite.Image.Height / LongNoteEndSprite.Image.Width;
            LongNoteEndOffset = LongNoteEndSprite.Height / 2f;

            // We set the parent of the HitObjectSprite **AFTER** we create the long note
            // so that the body of the long note isn't drawn over the object.
            HitObjectSprite.Parent = Playfield.Stage.HitObjectContainer;

            // Initialize
            Initialize(info);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="playfield"></param>
        public override void Initialize(HitObjectInfo info)
        {
            // Update Hit Object State
            Info = info;
            HitObjectSprite.Tint = Color.White; // todo: reference this in Colors class
            IsLongNote = Info.EndTime > 0;
            TrackPosition = Ruleset.Screen.Positioning.GetPositionFromTime(info.StartTime);
            CurrentlyBeingHeld = false;

            // Update Hit Object State depending if its an LN or not
            if (!IsLongNote)
            {
                LongNoteEndSprite.Visible = false;
                LongNoteBodySprite.Visible = false;
                LongNoteTrackPosition = 0;
            }
            else
            {
                LongNoteBodySprite.Tint = Color.White; // todo: reference this in Colors class
                LongNoteEndSprite.Tint = Color.White; // todo: reference this in Colors class
                LongNoteEndSprite.Visible = true;
                LongNoteBodySprite.Visible = true;
                LongNoteTrackPosition = Ruleset.Screen.Positioning.GetPositionFromTime(info.EndTime);
                //todo: make this a float instead?
                InitialLongNoteSize = (long)((LongNoteTrackPosition - TrackPosition) * HitObjectManagerKeys.ScrollSpeed);
                CurrentLongNoteSize = InitialLongNoteSize;
            }

            // Update Positions
            UpdateSpritePositions(Ruleset.Screen.Positioning.Position);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            HitObjectSprite.Destroy();
            LongNoteBodySprite.Destroy();
            LongNoteEndSprite.Destroy();
        }

        /// <summary>
        ///     Gets the correct HitObject texture also based on if we have note snapping and if
        ///     the note is a long note or note.
        ///
        ///     If the user has ColourObjectsBySnapDistance enabled in their skin, we load the one with their
        ///     specified color.
        ///
        ///     If not, we default it to the first beat snap in the list.
        /// </summary>
        /// <returns></returns>
        private Texture2D GetHitObjectTexture(int laneIndex)
        {
            var skin = SkinManager.Skin.Keys[Ruleset.Mode];

            if (skin.ColorObjectsBySnapDistance)
                return IsLongNote ? skin.NoteHoldHitObjects[laneIndex][SnapIndex] : skin.NoteHitObjects[laneIndex][SnapIndex];

            return IsLongNote ? skin.NoteHoldHitObjects[laneIndex][0] : skin.NoteHitObjects[laneIndex][0];
        }

        /// <summary>
        ///     Calculates the position of the Hit Object with a position offset.
        /// </summary>
        /// <returns></returns>
        public float GetPosFromOffset(long offset = 0)
        {
            // If the object is not being held, use proper position
            if (!CurrentlyBeingHeld)
            {
                var speed = GameplayRulesetKeys.IsDownscroll ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed;
                // (OLD) return (float) (manager.HitPositionOffset + (offset - ((int)Ruleset.Screen.Timing.Time - ConfigManager.GlobalAudioOffset.Value + MapManager.Selected.Value.LocalOffset)) * speed) - HitObjectSprite.Height;
                return (float)(HitPositionOffset + (TrackPosition - offset) * speed) - HitObjectSprite.Height;
            }

            // If the object is being held, use receptor position
            return HitPositionOffset - HitObjectSprite.Height;
        }

        /// <summary>
        ///     Updates the HitObject sprite positions
        /// </summary>
        public void UpdateSpritePositions(long offset)
        {
            if (CurrentlyBeingHeld)
            {
                if (offset > TrackPosition)
                {
                    // (OLD) hitObject.CurrentLongNoteSize = (ulong)((hitObject.LongNoteOffsetYFromReceptor - Ruleset.Screen.Timing.Time) * ScrollSpeed);
                    CurrentLongNoteSize = (long)((LongNoteTrackPosition - Ruleset.Screen.Positioning.Position) * HitObjectManagerKeys.ScrollSpeed);
                    //CurrentlyBeingHeld = true;
                    SpritePosition = GetPosFromOffset();
                }
                else
                {
                    CurrentLongNoteSize = InitialLongNoteSize;
                    SpritePosition = GetPosFromOffset();
                }
            }
            else
            {
                SpritePosition = GetPosFromOffset(offset);
            }

            // Only update note if it's inside the window
            //if ((!GameplayRulesetKeys.IsDownscroll || PositionY + HitObjectSprite.Height <= 0) && (GameplayRulesetKeys.IsDownscroll || !(PositionY < WindowManager.Height)))
            //    return;

            // Update HitBody
            HitObjectSprite.Y = SpritePosition;

            // Disregard the rest if it isn't a long note.
            if (!IsLongNote)
                return;

            // It will ignore the rest of the code after this statement if long note size is equal/less than 0
            if (CurrentLongNoteSize <= 0)
            {
                LongNoteBodySprite.Visible = false;
                LongNoteEndSprite.Visible = false;
                return;
            }

            //Update HoldBody Position and Size
            LongNoteBodySprite.Height = CurrentLongNoteSize;

            if (GameplayRulesetKeys.IsDownscroll)
            {
                LongNoteBodySprite.Y = -(float) CurrentLongNoteSize + LongNoteBodyOffset + SpritePosition;
                LongNoteEndSprite.Y = SpritePosition - CurrentLongNoteSize - LongNoteEndOffset + LongNoteBodyOffset;
            }
            else
            {
                LongNoteBodySprite.Y = SpritePosition + LongNoteBodyOffset;
                LongNoteEndSprite.Y = SpritePosition + CurrentLongNoteSize - LongNoteEndOffset + LongNoteBodyOffset;
            }
        }

        /// <summary>
        ///     When the object iself dies, we want to change it to a dead color.
        /// </summary>
        public void ChangeSpriteColorToDead()
        {
            if (IsLongNote)
            {
                LongNoteBodySprite.Tint = Colors.DeadLongNote;
                LongNoteEndSprite.Tint = Colors.DeadLongNote;
            }

            HitObjectSprite.Tint = Colors.DeadLongNote;
        }

        /// <summary>
        ///     Fades out the object. Usually used for failure.
        /// </summary>
        /// <param name="dt"></param>
        public void FadeOut(double dt)
        {
            // HitObjectSprite.FadeOut(dt, 240);

            if (!IsLongNote)
                return;

            // LongNoteBodySprite.FadeOut(dt, 240);
            // LongNoteEndSprite.FadeOut(dt, 240);
        }
    }
}
