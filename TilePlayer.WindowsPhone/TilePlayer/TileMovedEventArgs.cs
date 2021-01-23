#region Header
//+ <source name="TileMovedEventArgs.cs" language="C#" begin="08-Apr-2012">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2012">
//+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Provides data for the <see cref="GameBoard.TileMoved"/> event.
    /// </summary>
    class TileMovedEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TileMovedEventArgs"/>
        /// class with the specified <see cref="Tile"/>.
        /// </summary>
        /// <param name="tile">The <see cref="Tile"/> that has been moved.</param>
        internal TileMovedEventArgs(Tile tile)
        {
            Tile = tile;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Tile"/> that has been moved.
        /// </summary>
        /// <value>
        /// The <see cref="Tile"/> that has been moved.
        /// </value>
        public Tile Tile
        {
            get;
            private set;
        }
        #endregion
    }
}
