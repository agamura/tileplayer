#region Header
//+ <source name="Tile.cs" language="C#" begin="03-Dec-2011">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2011">
//+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using PlaXore.GameFramework;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Represents a square sliding tile.
    /// </summary>
    class Tile : SpriteObject
    {
        #region Fields
        private static readonly TimeSpan VibrateTime;
        private TextObject text;
        private Vector2 scalePosition;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes static members.
        /// </summary>
        static Tile()
        {
            VibrateTime = new TimeSpan(111111);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class with the
        /// specified game host, position, source rectangle, <see cref="GameBoard"/>,
        /// and order.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="position">The <see cref="Tile"/> position.</param>
        /// <param name="sourceRectangle">The <see cref="Tile"/> source rectangle.</param>
        /// <param name="gameBoard">The <see cref="GameBoard"/> this <see cref="Tile"/> belongs to.</param>
        /// <param name="order">The <see cref="Tile"/> order.</param>
        public Tile(GameHost gameHost, Vector2 position, Rectangle sourceRectangle, GameBoard gameBoard, int order)
            : base(gameHost, position, sourceRectangle)
        {
            LayerDepth = 1f;
            Color = Color.White;
            MoveSoundEffect = gameHost.SoundEffects["MoveSound"];
            GameBoard = gameBoard;
            Order = order;

            text = new TextObject(GameHost, gameHost.Fonts["DefaultRegular22"]);
            text.Text = (order + 1).ToString();
            text.Color = Color.Black;
            text.LayerDepth = 0.9f;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="GameBoard"/> this <see cref="Tile"/> belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="GameBoard"/> this <see cref="Tile"/> belongs to.
        /// </value>
        public GameBoard GameBoard
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the order of this <see cref="Tile"/>.
        /// </summary>
        /// <value>The order of this <see cref="Tile"/>.</value>
        public int Order
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <i>move</i> sound effect for this <see cref="Tile"/>.
        /// </summary>
        public SoundEffect MoveSoundEffect
        {
            set;
            get;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws this <see cref="Tile"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (GameBoard.TileNumbersEnabled) {
                text.Draw(gameTime, spriteBatch);
            }

            if (scalePosition != Vector2.Zero) {
                if (Texture != null) {
                    spriteBatch.Draw(Texture, scalePosition, SourceRectangle, Color,
                        Angle, Origin, Scale, SpriteEffects.None, LayerDepth);
                }
            } else {
                base.Draw(gameTime, spriteBatch);
            }
        }

        /// <summary>
        /// Updates this <see cref="Tile"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        public override void Update(GameTime gameTime)
        {
            text.PositionX = Position.X + 5f;
            text.PositionY = Position.Y - 5f;

            if (SourceRectangle.Width < 100) {
                ScaleX = 0.98f;
                ScaleY = 0.98f;
            } else {
                ScaleX = 0.99f;
                ScaleY = 0.99f;
            }

            scalePosition.X = Position.X + ((SourceRectangle.Width - (SourceRectangle.Width * Scale.X)) / 2);
            scalePosition.Y = Position.Y + ((SourceRectangle.Height - (SourceRectangle.Height * Scale.Y)) / 2);
        }
        #endregion
    }
}
