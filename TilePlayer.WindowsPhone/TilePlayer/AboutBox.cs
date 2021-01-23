#region Header
//+ <source name=AboutBox.cs" language="C#" begin="05-Mar-2012">
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
using PlaXore.Properties;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Shows a dialog box with game and copyright information.
    /// </summary>
    public class AboutBox : Window
    {
        #region Fields
        private TextObject[] edition;
        private TextObject[] version;
        private TextObject[] legend;
        private TextObject[] legalNotice;
        private TextObject[] copyright;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutBox"/> class
        /// with the specified game host.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        public AboutBox(GameHost gameHost)
            : base(gameHost, gameHost.Textures["AboutBoxBackground"])
        {
            Click += OnClick;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the game edition.
        /// </summary>
        /// <value>The game edition.</value>
        private TextObject[] Edition
        {
            get {
                if (edition == null) {
                    WizzleGameHost gameHost = GameHost as WizzleGameHost;
                    string text = gameHost.License.IsTrial ? Resources.TrialEdition : Resources.FullEdition;
                    SpriteFont font = gameHost.Fonts["DefaultRegular18"];

                    edition = ArrangeText(text,
                        new Rectangle(0, BoundingBox.Top + 55, 0, (int) font.MeasureString(text).Y),
                        font, Color.Black, TextAlignment.Far, TextAlignment.Center);
                }

                return edition;
            }
        }

        /// <summary>
        /// Gets the game version.
        /// </summary>
        /// <value>The game version.</value>
        private TextObject[] Version
        {
            get {
                if (version == null) {
                    string text = String.Format(Resources.Version, new AssemblyInfoHelper().Version);
                    SpriteFont font = GameHost.Fonts["DefaultRegular12"];

                    version = ArrangeText(text,
                        new Rectangle(0, BoundingBox.Top + 80, 0, (int) font.MeasureString(text).Y),
                        font, Color.Black, TextAlignment.Far, TextAlignment.Center);
                }

                return version;
            }
        }

        /// <summary>
        /// Gets the icon legend.
        /// </summary>
        /// <value>A short description of each menu icon, one entry per icon.</value>
        private TextObject[] Legend
        {
            get {
                if (legend == null) {
                    WizzleGameHost gameHost = GameHost as WizzleGameHost;

                    string[] items = {
                        Resources.IconMatrix, Resources.IconDisturbingElement,
                        Resources.IconStart, gameHost.License.IsTrial ? Resources.Buy : Resources.IconHighScores,
                        Resources.IconPause, Resources.IconSettings,
                        Resources.IconStop, Resources.IconAbout
                    };

                    legend = new TextObject[items.Length];

                    SpriteFont font = GameHost.Fonts["DefaultSemibold12"];
                    Vector2 position = Vector2.Zero;
                    Vector2 textSize = Vector2.Zero;
                    float currentY = BoundingBox.Top + 108;

                    for (int i = 0; i < legend.Length; i++) {
                        textSize = font.MeasureString(items[i]);

                        if (i % 2 == 0) {
                            position.X = BoundingBox.Left + 60;
                            position.Y = currentY += textSize.Y + 10;
                        } else {
                            position.X = BoundingBox.Left + 220;
                            position.Y = currentY;
                        }

                        legend[i] = ArrangeText(items[i],
                            new Rectangle((int) position.X, (int) position.Y, (int) textSize.X, (int) textSize.Y + 10),
                            font, Color.Black, TextAlignment.Near, TextAlignment.Center)[0];
                    }
                }

                return legend;
            }
        }

        /// <summary>
        /// Gets the legal notice.
        /// </summary>
        /// <value>The legal notice, one entry per text line.</value>
        private TextObject[] LegalNotice
        {
            get {
                if (legalNotice == null) {
                    string text = Resources.LegalNotice;
                    SpriteFont font = GameHost.Fonts["DefaultRegular10"];

                    legalNotice = ArrangeText(text,
                        new Rectangle(0, BoundingBox.Top + 275, 0, 85),
                        font, Color.Black, TextAlignment.Center, TextAlignment.Center);
                }

                return legalNotice;
            }
        }

        /// <summary>
        /// Gets the copyright info.
        /// </summary>
        /// <value>The copyright info.</value>
        private TextObject[] Copyright
        {
            get {
                if (copyright == null) {
                    string text = String.Format(Resources.Copyright, DateTime.UtcNow.Year);
                    SpriteFont font = GameHost.Fonts["DefaultBold10"];

                    copyright = ArrangeText(text,
                        new Rectangle(0, BoundingBox.Top + 355, 0, (int) font.MeasureString(text).Y),
                        font, Color.Black, TextAlignment.Center, TextAlignment.Center);
                }

                return copyright;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Releases the unmanaged resources used by this <see cref="AboutBox"/>
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
                    foreach (TextObject textObject in edition) { textObject.Dispose(); }
                    edition = null;

                    foreach (TextObject textObject in version) { textObject.Dispose(); }
                    version = null;

                    foreach (TextObject textObject in legend) { textObject.Dispose(); }
                    legend = null;

                    foreach (TextObject textObject in legalNotice) { textObject.Dispose(); }
                    legalNotice = null;

                    foreach (TextObject textObject in copyright) { textObject.Dispose(); }
                    copyright = null;
                }

                // Release unmanaged resources
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Draws this <see cref="AboutBox"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Edition[0].Draw(gameTime, spriteBatch);
            Version[0].Draw(gameTime, spriteBatch);
            foreach (TextObject item in Legend) { item.Draw(gameTime, spriteBatch); }
            foreach (TextObject item in LegalNotice) { item.Draw(gameTime, spriteBatch); }
            Copyright[0].Draw(gameTime, spriteBatch);

            base.Draw(gameTime, spriteBatch);
        }

        /// <summary>
        /// Handles the <see cref="Window.Shown"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnShown(EventArgs args)
        {
            if (GameHost.License.IsTrial) {
                Texture = GameHost.Textures["AboutBoxTrialBackground"];
            }

            base.OnShown(args);
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Window"/> that raised the event.</param>
        /// <param name="args">The event data.</param>
        protected virtual void OnClick(object sender, GameInputEventArgs args)
        {
            Close();
        }
        #endregion
    }
}
