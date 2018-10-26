using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Resources;
using Quaver.Online;
using Quaver.Server.Client.Handlers;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;

namespace Quaver.Graphics.Overlays.Chat.Components.Users
{
    public class OnlineUsersHeader : Sprite
    {
        /// <summary>
        ///     Reference to the parent chat overlay.
        /// </summary>
        public ChatOverlay Overlay { get; }

        /// <summary>
        ///     Shows the text "Online users"
        /// </summary>
        public SpriteText TextHeader { get; private set; }

        /// <summary>
        ///    Displays the amount of users currently online.
        /// </summary>
        public SpriteText TextOnlineCount { get; private set; }

        /// <summary>
        ///     The divider line at the bottom of the header
        /// </summary>
        private Sprite DividerLine { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        public OnlineUsersHeader(ChatOverlay overlay)
        {
            Overlay = overlay;
            Parent = Overlay.OnlineUsersHeaderContainer;
            Size = Overlay.OnlineUsersHeaderContainer.Size;
            Tint = Color.Black;
            Alpha = 0.85f;

            CreateTextHeader();
            CreateTextOnlineUserCount();
            CreateDividerLine();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public override void Destroy()
        {
            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnUserConnected -= OnUserConnected;
                OnlineManager.Client.OnUserDisconnected -= OnUserDisconnected;
                OnlineManager.Client.OnUsersOnline -= OnUsersOnline;
            }

            base.Destroy();
        }

        /// <summary>
        ///    Creates the header text that says "Online Users"
        /// </summary>
        private void CreateTextHeader()
        {
            TextHeader = new SpriteText(BitmapFonts.Exo2BoldItalic, "Online Users", 13)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 10,
            };
        }

        /// <summary>
        ///     Creates the text that displays the total online user count.
        /// </summary>
        private void CreateTextOnlineUserCount()
        {
            TextOnlineCount = new SpriteText(BitmapFonts.Exo2MediumItalic, " ", 10)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = TextHeader.Y + TextHeader.Height - 2,
                ForceDrawAtSize = false
            };

            UpdateOnlineUserCount();
        }

        /// <summary>
        ///     Makes sure the online user count is up to date.
        /// </summary>
        private void UpdateOnlineUserCount()
        {
            var count = OnlineManager.Connected ? OnlineManager.OnlineUsers.Count : 0;
            TextOnlineCount.Text = $"Total Online: {count:n0}";
        }

        /// <summary>
        ///     Creates the divider line at the bottom.
        /// </summary>
        private void CreateDividerLine() => DividerLine = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.BotLeft,
            Size = new ScalableVector2(Width, 2),
            Alpha = 0.35f
        };

        public void OnUsersOnline(object sender, UsersOnlineEventArgs e) => UpdateOnlineUserCount();
        public void OnUserConnected(object sender, UserConnectedEventArgs e) => UpdateOnlineUserCount();
        public void OnUserDisconnected(object sender, UserDisconnectedEventArgs e) => UpdateOnlineUserCount();
    }
}