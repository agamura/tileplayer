#region Header
//+ <source name="GameStateId.cs" language="C#" begin="10-Apr-2013">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2013">
//+ //+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Specifies the game state.
    /// </summary>
    internal static class GameStateId
    {
        /// <summary>
        /// The game has no state.
        /// </summary>
        public const int Undefined = 0;

        /// <summary>
        /// The game is loading.
        /// </summary>
        public const int Loading = 1;

        /// <summary>
        /// The game has been loaded.
        /// </summary>
        public const int Loaded = 2;

        /// <summary>
        /// The game is starting.
        /// </summary>
        public const int Starting = 3;

        /// <summary>
        /// The game has been started.
        /// </summary>
        public const int Started = 4;

        /// <summary>
        /// The game is ready.
        /// </summary>
        public const int Ready = 5;

        /// <summary>
        /// The game is running.
        /// </summary>
        public const int Running = 6;

        /// <summary>
        /// The game is over.
        /// </summary>
        public const int GameOver = 7;

        /// <summary>
        /// The game has been paused.
        /// </summary>
        public const int Paused = 8;
    }
}
