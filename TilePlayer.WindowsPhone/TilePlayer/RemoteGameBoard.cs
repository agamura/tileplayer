#region Header
//+ <source name="RemoteGameBoard.cs" language="C#" begin="15-Apr-2012">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2012">
//+ //+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
using Microsoft.Xna.Framework;
using PlaXore.GameFramework;
using Microsoft.Xna.Framework.Graphics;
using WizzleGame = Wizzle.Engine.Game;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Represents a remote <see cref="GameBoard"/>.
    /// </summary>
    class RemoteGameBoard : GameBoard
    {
        #region Fields
        #endregion

        #region Events
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteGameBoard"/> class with
        /// the specified game host, position, source rectangle, and <see cref="WizzleGame"/>.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="position">The <see cref="RemoteGameBoard"/> position.</param>
        /// <param name="sourceRectangle">The <see cref="RemoteGameBoard"/> source rectangle.</param>
        /// <param name="game">
        /// The <see cref="WizzleGame"/> associated with this <see cref="RemoteGameBoard"/>.
        /// </param>
        public RemoteGameBoard(GameHost gameHost, Vector2 position, Rectangle sourceRectangle, WizzleGame game)
            : base(gameHost, position, sourceRectangle, game)
        {
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="RemoteGameBoard"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets this <see cref="RemoteGameBoard"/>.
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
        }

        /// <summary>
        /// Raises the <see cref="GameBoard.TileMoved"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnTileMoved(TileMovedEventArgs args)
        {
            base.OnTileMoved(args);
        }

        /// <summary>
        /// Handles the <see cref="Wizzle.Engine.Matrix.Scrambled"/> event.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="Wizzle.Engine.Matrix"/> that generated the event.
        /// </param>
        /// <param name="args">The event data.</param>
        protected override void OnScrambled(object sender, EventArgs args)
        {
            base.OnScrambled(sender, args);
        }
        #endregion
    }
}
