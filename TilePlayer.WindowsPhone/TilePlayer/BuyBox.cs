#region Header
//+ <source name="BuyBox.cs" language="C#" begin="23-Jun-2012">
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
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Shows a dialog box that prompts the <see cref="LocalGamer"/> to buy
    /// the game on the marketplace.
    /// </summary>
    public class BuyBox : Window
    {
        #region Fields
        private bool gameplayInterrupted;
        private TextObject[] title;
        private TextObject[] message;
        private Button okButton;
        private Button cancelButton;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BuyBox"/> class with
        /// the specified game host.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        public BuyBox(GameHost gameHost)
            : this(gameHost, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyBox"/> class with
        /// the specified game host and a value indicating whether
        /// or not the gameplay was interrupted due to an attempt to access
        /// functionality not enabled.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="gameplayInterrupted">
        /// <see langword="true"/> if the gameplay was interrupted; otherwise,
        /// <see langword="false"/>.
        /// </param>
        public BuyBox(GameHost gameHost, bool gameplayInterrupted)
            : base(gameHost, gameHost.Textures["BuyBoxBackground"])
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

            this.gameplayInterrupted = gameplayInterrupted;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets value indicating whether or not the gameplay was
        /// interrupted due to an attempt to access functionality not enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the gameplay was interrupted; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool GameplayInterrupted
        {
            get { return gameplayInterrupted; }
            set {
                gameplayInterrupted = value;
                message = null;
            }
        }

        /// <summary>
        /// Gets the message title.
        /// </summary>
        /// <value>The message title.</value>
        private TextObject[] Title
        {
            get {
                if (title == null) {
                    string text = Resources.Buy;
                    SpriteFont font = GameHost.Fonts["DefaultBold22"];

                    int x = BoundingBox.Left + Margin.Left + 90;
                    int y = BoundingBox.Top + Margin.Top + 20;
                    int height = (int) font.MeasureString(text).Y;

                    title = ArrangeText(text, new Rectangle(x, y, 0, height),
                        font, Color.Black, TextAlignment.Near, TextAlignment.Near);
                }

                return title;
            }
        }
        /// <summary>
        /// Gets the buy message.
        /// </summary>
        /// <value>The buy message, one entry per text line.</value>
        private TextObject[] Message
        {
            get {
                if (message == null) {
                    string text = String.Format(GameplayInterrupted
                            ? Resources.EditionFunctionalityWarning
                            : Resources.EditionWarning, Resources.TrialEdition);
                    text = String.Format(Resources.EditionWantToBuy, text, Resources.FullEdition);

                    SpriteFont font = GameHost.Fonts["DefaultRegular18"];
                    int x = BoundingBox.Left + Margin.Left;
                    int y = BoundingBox.Top + (Margin.Top * 2) + 80;
                    int width = BoundingBox.Width - Margin.Left - Margin.Right;
                    int height = BoundingBox.Height - Margin.Top - Margin.Bottom - okButton.BoundingBox.Height;

                    message = ArrangeText(text, new Rectangle(x, y, width, height),
                        font, Color.Black, TextAlignment.Center, TextAlignment.Near);
                }

                return message;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Releases the unmanaged resources used by this <see cref="BuyBox"/>
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
                    foreach (TextObject textObject in title) { textObject.Dispose(); }
                    title = null;

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
        /// Draws this <see cref="BuyBox"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Title[0].Draw(gameTime, spriteBatch);
            foreach (TextObject item in Message) { item.Draw(gameTime, spriteBatch); }

            base.Draw(gameTime, spriteBatch);
        }

        /// <summary>
        /// Handles the <see cref="Window.Shown"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnShown(EventArgs args)
        {
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
            switch ((sender as Button).Tag) {
                case "OkButton":
                    WizzleGameHost gameHost = GameHost as WizzleGameHost;
                    gameHost.License.Buy();
                    break;
                case "CancelButton":
                    break;
            }

            Close();
        }
        #endregion
    }
}
