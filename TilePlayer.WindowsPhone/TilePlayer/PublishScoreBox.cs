#region Header
//+ <source name="PublishScoreBox.cs" language="C#" begin="30-Apr-2012">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2012">
//+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlaXore.GameFramework;
using PlaXore.GameFramework.Controls;
using Wizzle.Engine;
using WizzleGame = Wizzle.Engine.Game;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Shows a dialog box that prompts for publishing game score.
    /// </summary>
    public class PublishScoreBox : Window
    {
        #region Fields
        private TextObject[] message;
        private Button okButton;
        private Button cancelButton;
        private Gamer gamer;
        private WizzleGame game;
        private HighScoreTable highScoreTable;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishScoreBox"/> class
        /// with the specified game host.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        public PublishScoreBox(GameHost gameHost)
            : this(gameHost, (gameHost as WizzleGameHost).LocalGamer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishScoreBox"/> class
        /// with the specified game host and <see cref="Gamer"/>.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="gamer">The <see cref="Gamer"/> to publish the score for.</param>
        public PublishScoreBox(GameHost gameHost, Gamer gamer)
            : base(gameHost, gameHost.Textures["PublishScoreBoxBackground"])
        {
            Texture2D texture = gameHost.Textures["OkButton"];
            Vector2 position = new Vector2();
            position.X = BoundingBox.Left + Margin.Left;
            position.Y = BoundingBox.Bottom - Margin.Bottom - texture.Height;

            okButton = new Button(gameHost, position, texture);
            okButton.Tag = "OkButton";
            okButton.Click += OnClick;

            texture = gameHost.Textures["CancelButton"];
            position.X = BoundingBox.Right - Margin.Right - texture.Width;
            position.Y = BoundingBox.Bottom - Margin.Bottom - texture.Height;

            cancelButton = new Button(gameHost, position, texture);
            cancelButton.Tag = "CancelButton";
            cancelButton.Click += OnClick;

            Gamer = gamer;

            if (GameHost.License.IsTrial) {
                GameHost.License.Bought += OnBought;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the <see cref="Gamer"/> to publish the score for.
        /// </summary>
        /// <value>The <see cref="Gamer"/> to publish the score for.</value>
        public Gamer Gamer
        {
            get { return gamer; }
            set {
                gamer = value;
                game = (GameHost as WizzleGameHost).GameSession[value];
                message = null;
            }
        }

        /// <summary>
        /// Gets the publish score message.
        /// </summary>
        /// <value>The publish score message, one entry per text line.</value>
        private TextObject[] Message
        {
            get {
                if (message == null) {
                    string text = String.Format(Resources.PublishScore, (int) game.Score);
                    SpriteFont font = GameHost.Fonts["DefaultRegular18"];
                    int width = BoundingBox.Width - (Margin.Left + Margin.Right + 80);

                    message = ArrangeText(text,
                        new Rectangle(BoundingBox.Right - Margin.Right - width, 0, width, 100),
                        font, Color.Black, TextAlignment.Center, TextAlignment.Near);
                }

                return message;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Releases the unmanaged resources used by this <see cref="PublishScoreBox"/>
        /// and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release unmanaged resources only.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed) {
                if (disposing) {
                    // Release managed resources
                    foreach (TextObject textObject in message) { textObject.Dispose(); }
                    message = null;

                    okButton.Dispose();
                    okButton = null;

                    cancelButton.Dispose();
                    cancelButton = null;
                }

                // Release unmanaged resources
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Draws this <see cref="PublishScoreBox"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (TextObject item in Message) { item.Draw(gameTime, spriteBatch); }

            base.Draw(gameTime, spriteBatch);
        }

        /// <summary>
        /// Handles the <see cref="Window.Shown"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnShown(EventArgs args)
        {
            string tableName = String.Format(Resources.MatrixSize, game.Puzzle.Width, game.Puzzle.Height);
            highScoreTable = GameHost.HighScores.GetTable(tableName);

            okButton.Show();
            cancelButton.Show();
            base.OnShown(args);
        }

        /// <summary>
        /// Handles the <see cref="Window.Hidden"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnHidden(EventArgs args)
        {
            okButton.Hide();
            cancelButton.Hide();
            base.OnHidden(args);
        }

        /// <summary>
        /// Handles the <see cref="Window.Closed"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnClosed(EventArgs args)
        {
            okButton.Dispose();
            cancelButton.Dispose();
            base.OnClosed(args);
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Button"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick(object sender, EventArgs args)
        {
            WizzleGameHost gameHost = GameHost as WizzleGameHost;

            switch ((sender as Button).Tag) {
                case "OkButton":
                    if (!gameHost.License.IsTrial) {
                        PublishScore();
                    }

                    Close();

                    if (gameHost.License.IsTrial) {
                        (gameHost.WindowManager["TilePlayer.BuyBox"] as BuyBox).GameplayInterrupted = true;
                        gameHost.WindowManager["TilePlayer.BuyBox"].Show();
                    }
                    break;
                case "CancelButton":
                    Close();
                    break;
            }
        }

        /// <summary>
        /// Handles the <see cref="License.Bought"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="License"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnBought(object sender, EventArgs args)
        {
            PublishScore();
            GameHost.License.Bought -= OnBought;
        }

        /// <summary>
        /// Publishes the current score.
        /// </summary>
        private void PublishScore()
        {
            WizzleGameHost gameHost = GameHost as WizzleGameHost;
            HighScoreEntry highScoreEntry = highScoreTable.AddEntry(gamer.Gamertag, (int) game.Score);

            if (highScoreEntry != null) {
                gameHost.HighScores.Save();
            }

            gameHost.LastScore = highScoreEntry;
        }
        #endregion
    }
}
