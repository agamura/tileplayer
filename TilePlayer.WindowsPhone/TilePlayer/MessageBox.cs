#region Header
//+ <source name="MessageBox.cs" language="C#" begin="11-Jul-2012">
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
    /// Shows a generic message box.
    /// </summary>
    public class MessageBox : Window
    {
        #region Fields
        private string caption;
        private TextObject internalCaption;
        private string message;
        private TextObject[] internalMessage;
        private Button okButton;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBox"/> class
        /// with the specified game host.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        public MessageBox(GameHost gameHost)
            : this(gameHost, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBox"/> class
        /// with the specified game host, caption, and message.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="caption">The caption of this <see cref="MessageBox"/>.</param>
        /// <param name="message">The message of this <see cref="MessageBox"/>.</param>
        public MessageBox(GameHost gameHost, string caption, string message)
            : base(gameHost, gameHost.Textures["MessageBoxBackground"])
        {
            Texture2D texture = gameHost.Textures["OkButton"];
            Vector2 position = new Vector2();
            position.X = BoundingBox.Left + ((BoundingBox.Width / 2) - (texture.Width / 2));
            position.Y = BoundingBox.Bottom - Margin.Bottom - texture.Height;

            okButton = new Button(gameHost, position, texture);
            okButton.Click += OnClick;

            Caption = caption;
            Message = message;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the caption of this <see cref="MessageBox"/>.
        /// </summary>
        /// <value>The caption of this <see cref="MessageBox"/>.</value>
        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                caption = value;

                if (internalCaption != null) {
                    internalCaption.Dispose();
                    internalCaption = null;
                }
            }
        }

        private TextObject InternalCaption
        {
            get
            {
                if (internalCaption == null) {
                    SpriteFont font = GameHost.Fonts["DefaultRegular22"];

                    internalCaption = ArrangeText(caption,
                        new Rectangle(0, BoundingBox.Top, 0, (int) font.MeasureString(caption).Y),
                        font, Color.Black, TextAlignment.Center, TextAlignment.Center)[0];
                }

                return internalCaption;
            }
        }

        /// <summary>
        /// Gets or sets the message of this <see cref="MessageBox"/>.
        /// </summary>
        /// <value>The message of this <see cref="MessageBox"/>.</value>
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;

                if (internalMessage != null) {
                    foreach (TextObject textObject in internalMessage) {
                        textObject.Dispose();
                    }

                    internalMessage = null;
                }
            }
        }

        private TextObject[] InternalMessage
        {
            get
            {
                if (internalMessage == null) {
                    SpriteFont font = GameHost.Fonts["DefaultRegular18"];
                    int areaHeight = okButton.BoundingBox.Top - InternalCaption.BoundingBox.Bottom - Margin.Top;
                    int separatorHeight = Margin.Top / 2;

                    Rectangle area = new Rectangle(
                        BoundingBox.Left + Margin.Left, InternalCaption.BoundingBox.Bottom + separatorHeight,
                        BoundingBox.Width - Margin.Left - Margin.Right, areaHeight);

                    internalMessage = ArrangeText(message, area,
                        font, Color.Black, TextAlignment.Center, TextAlignment.Near);
                }

                return internalMessage;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Releases the unmanaged resources used by this <see cref="MessageBox"/>
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
                    okButton.Dispose();
                    okButton = null;

                    Caption = null;
                    Message = null;
                }

                // Release unmanaged resources
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Draws this <see cref="MessageBox"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        /// <exception cref="ObjectDisposedException">
        /// This <see cref="MessageBox"/> has already been disposed of.
        /// </exception>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            InternalCaption.Draw(gameTime, spriteBatch);
            foreach (TextObject item in InternalMessage) {
                item.Draw(gameTime, spriteBatch);
            }

            base.Draw(gameTime, spriteBatch);
        }

        /// <summary>
        /// Handles the <see cref="Window.Shown"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnShown(EventArgs args)
        {
            okButton.Show();
            base.OnShown(args);
        }

        /// <summary>
        /// Handles the <see cref="Window.Hidden"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnHidden(EventArgs args)
        {
            okButton.Hide();
            base.OnHidden(args);
        }

        /// <summary>
        /// Handles the <see cref="Window.Closed"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnClosed(EventArgs args)
        {
            okButton.Dispose();
            base.OnClosed(args);
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Button"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick(object sender, EventArgs args)
        {
            Close();
        }
        #endregion
    }
}
