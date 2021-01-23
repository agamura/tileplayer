#region Header
//+ <source name="Scorpion.cs" language="C#" begin="31-Mar-2012">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2012">
//+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using PlaXore.GameFramework;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Represents a disturbing scorpion ghost that walks around on the
    /// <see cref="LocalGameBoard"/> surface while the <see cref="Wizzle.Engine.Gamer"/>
    /// is trying to solving the puzzle.
    /// </summary>
    class Scorpion : DisturbingElement
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Scorpion"/> class with
        /// the specified game host, position, and moving area.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="gameBoard">The <see cref="GameBoard"/> this <see cref="Scorpion"/> belongs to.</param>
        /// <param name="position">The <see cref="Scorpion"/> position.</param>
        /// <param name="movingArea">The moving area.</param>
        public Scorpion(GameHost gameHost, GameBoard gameBoard, Vector2 position, Rectangle movingArea)
            : base(gameHost, gameBoard, position, movingArea)
        {
            MovingTexture = gameHost.Textures["WalkingScorpionGhost"];
            TerminationTexture = gameHost.Textures["BurstingScorpionGhost"];
            TerminationSound = gameHost.SoundEffects["ScorpionBurstSound"];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the <i>moving</i> texture of this
        /// <see cref="Scorpion"/>.
        /// </summary>
        /// <value>
        /// The <i>moving</i> texture of this <see cref="Scorpion"/>.
        /// </value>
        public override Texture2D MovingTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <i>termination</i> texture of this
        /// <see cref="Scorpion"/>.
        /// </summary>
        /// <value>
        /// The <i>termination</i> texture of this <see cref="Scorpion"/>.
        /// </value>
        public override Texture2D TerminationTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <i>termination</i> sound effect of this
        /// <see cref="Scorpion"/>.
        /// </summary>
        /// <value>
        /// The <i>termination</i> sound effect of this <see cref="Scorpion"/>.
        /// </value>
        public override SoundEffect TerminationSound
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of this <see cref="DisturbingElement"/>.
        /// </summary>
        public override string Name
        {
            get { return Resources.Scorpion; }
        }

        /// <summary>
        /// Gets the plural name of this <see cref="DisturbingElement"/>.
        /// </summary>
        public override string PluralName
        {
            get { return Resources.Scorpions; }
        }
        #endregion
    }
}
