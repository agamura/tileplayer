#region Header
//+ <source name="LocalGamer.cs" language="C#" begin="17-Feb-2012">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2012">
//+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
using System.Collections.Generic;
using Wizzle.Engine;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Represents a local game player that takes part in a <see cref="GameSession"/>.
    /// </summary>
    /// <seealso cref="GameSession"/>
    class LocalGamer : Gamer
    {
        #region Fields
        private Queue<Position> moves;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalGamer"/> class
        /// with the specified globally unique identifier and gamertag.
        /// </summary>
        /// <param name="id">
        /// A globally unique identifier that identifies this <see cref="LocalGamer"/>.
        /// </param>
        /// <param name="gamertag">
        /// A string that uniquely identifies the <see cref="LocalGamer"/>.
        /// </param>
        public LocalGamer(Guid id, string gamertag)
            : base(id, gamertag)
        {
            moves = new Queue<Position>();
            base.OnGameOver = _OnGameOver;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether or not this
        /// <see cref="LocalGamer"/> is signed up.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this <see cref="LocalGamer"/> is signed
        /// up; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsSignedUp
        {
            get;
            set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the specified start <see cref="Position"/> of the next move.
        /// </summary>
        /// <param name="position">
        /// The start <see cref="Position"/> of the next move.
        /// </param>
        public void AddMove(Position position)
        {
            moves.Enqueue(position);
        }

        /// <summary>
        /// Gets the start <see cref="Position"/> of the next move issued by this
        /// <see cref="LocalGamer"/>.
        /// </summary>
        /// <value>
        /// The start <see cref="Position"/> of the next move, or <see cref="Position.Undefined"/>
        /// if no moves are available.
        /// </value>
        public override Position GetNextMove()
        {
            if (moves.Count == 0) {
                return Position.Undefined;
            }

            return moves.Dequeue();
        }

        /// <summary>
        /// Handles the <c>GameOver</c> event.
        /// </summary>
        /// <param name="sender">The <see cref="GameSession"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void _OnGameOver(object sender, GameOverEventArgs args)
        {
            // Add your code here
        }
        #endregion
    }
}
