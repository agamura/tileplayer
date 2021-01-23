#region Header
//+ <source name=SplashScreen.cs" language="C#" begin="7-Jul-2012">
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
    /// Shows the game's splash screen.
    /// </summary>
    public class SplashScreen : Window
    {
        #region Fields
        private TimeSpan elapsedTime;
        private bool autoClose;
        private bool close;
        private TextObject[] legalNotice;
        private TextObject[] copyright;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashScreen"/> class
        /// with the specified game host.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        public SplashScreen(GameHost gameHost)
            : this(gameHost, null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplashScreen"/> class
        /// with the specified game host and minimum show time.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="minShowTime">The minimum show time of this <see cref="SplashScreen"/>, in milliseconds.</param>
        /// <param name="autoClose">
        /// A boolean value indicating whether or not to close this <see cref="SplashScreen"/>
        /// after <paramref name="minShowTime"/> expires.
        /// </param>
        public SplashScreen(GameHost gameHost, int? minShowTime, bool autoClose)
            : base(gameHost, gameHost.Textures["SplashScreenBackground"])
        {
            MinShowTime = minShowTime;
            AutoClose = autoClose;
            elapsedTime = TimeSpan.Zero;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether or not to close this <see cref="SplashScreen"/>
        /// automatically after <see cref="SplashScreen.MinShowTime"/> expires.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to close this <see cref="SplashScreen"/> automatically after
        /// <see cref="SplashScreen.MinShowTime"/> expires; otherwise, <see langword="false"/>.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// Attempted to set <see cref="SplashScreen.AutoClose"/> to <see langword="true"/>
        /// even if <see cref="SplashScreen.MinShowTime"/> is unspecified.
        /// </exception>
        public bool AutoClose
        {
            get { return autoClose; }
            set
            {
                if (value && !MinShowTime.HasValue) {
                    throw new InvalidOperationException("No minimum show time was specified.");
                }

                autoClose = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not this <see cref="SplashScreen"/>
        /// can close.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this <see cref="SplashScreen"/> can close; 
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool CanClose
        {
            get {
                if (MinShowTime.HasValue) {
                    return elapsedTime.TotalMilliseconds >= MinShowTime.Value;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets or sets the minimum show time of this <see cref="SplashScreen"/>.
        /// </summary>
        /// <value>
        /// The minimum show time of this <see cref="SplashScreen"/>, in milliseconds.
        /// </value>
        public int? MinShowTime
        {
            get;
            set;
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
                    SpriteFont font = GameHost.Fonts["DefaultRegular14"];

                    legalNotice = ArrangeText(text,
                        new Rectangle(0, BoundingBox.Top + 600, 0, 90),
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
                    SpriteFont font = GameHost.Fonts["DefaultBold14"];
                    Vector2 fontSize = font.MeasureString(text);

                    copyright = ArrangeText(text,
                        new Rectangle(0, BoundingBox.Bottom - Margin.Bottom - (int) fontSize.Y, 0, (int) fontSize.Y),
                        font, Color.Black, TextAlignment.Center, TextAlignment.Center);
                }

                return copyright;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Closes this <see cref="SplashScreen"/>.
        /// </summary>
        public override void Close()
        {
            if (MinShowTime.HasValue) {
                close = true;
            } else {
                Dispose();
            }
        }
        /// <summary>
        /// Draws this <see cref="SplashScreen"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (TextObject item in LegalNotice) { item.Draw(gameTime, spriteBatch); }
            Copyright[0].Draw(gameTime, spriteBatch);

            base.Draw(gameTime, spriteBatch);
        }

        /// <summary>
        /// Updates this <see cref="SplashScreen"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            elapsedTime += gameTime.ElapsedGameTime;

            if (MinShowTime.HasValue) {
                if (elapsedTime.TotalMilliseconds >= MinShowTime.Value) {
                    if (AutoClose || close) {
                        Dispose();
                    }
                }
            }
        }
        #endregion
    }
}
