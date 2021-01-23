#region Header
//+ <source name="GameBoard.cs" language="C#" begin="14-Apr-2012">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2012">
//+ //+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlaXore.GameFramework;
using PlaXore.GameFramework.Controls;
using Wizzle.Engine;
using WizzleGame = Wizzle.Engine.Game;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Provides base functionality for any derived game board.
    /// </summary>
    abstract class GameBoard : Control
    {
        #region Fields
        internal const int MaxTileCount = 25;
        internal const int DisturbingElementsThreshold = 3;

        private bool disturbingElementsEnabled;
        private bool disturbingElementsTerminable;
        private bool tileNumbersEnabled;
        private Stack<DisturbingElement> disturbingElements;
        private List<Tile> tiles;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a <see cref="Tile"/> is moved.
        /// </summary>
        public event EventHandler<TileMovedEventArgs> TileMoved;

        /// <summary>
        /// Occurs when there are no <see cref="DisturbingElement"/> objects
        /// left.
        /// </summary>
        public event EventHandler<EventArgs> NoDisturbingElementsLeft;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GameBoard"/> class with the
        /// specified game host, position, source rectangle, and <see cref="WizzleGame"/>.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="position">The <see cref="GameBoard"/> position.</param>
        /// <param name="sourceRectangle">The <see cref="GameBoard"/> source rectangle.</param>
        /// <param name="game">
        /// The <see cref="WizzleGame"/> associated with this <see cref="GameBoard"/>.
        /// </param>
        public GameBoard(GameHost gameHost, Vector2 position, Rectangle sourceRectangle, WizzleGame game)
            : base(gameHost, position, sourceRectangle)
        {
            tiles = new List<Tile>(MaxTileCount);

            LayerDepth = 1f;
            Game = game;
            Game.InstanceReset += OnGameReset;
            Reset();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a <see cref="DisturbingElement"/> sample.
        /// </summary>
        /// <value>
        /// A reference to a loaded <see cref="DisturbingElement"/>, if any.
        /// </value>
        /// <remarks>
        /// Use <see cref="GameBoard.DisturbingElementSample"/> instead of looping
        /// through loaded disturbing elements to access <see cref="DisturbingElement"/>
        /// properties.
        /// </remarks>
        public DisturbingElement DisturbingElementSample
        {
            get {
                if (disturbingElements == null) { return null; }
                return disturbingElements.Count > 0 ? disturbingElements.Peek() : null;
            }
        }

        /// <summary>
        /// Gets the <see cref="DisturbingElement"/> left on the <see cref="GameBoard"/>
        /// surface.
        /// </summary>
        /// <value>
        /// The <see cref="DisturbingElement"/> left on the <see cref="GameBoard"/>
        /// surface.
        /// </value>
        public int DisturbingElementsLeft
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether or not disturbing elements are frozen.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if disturbing elements are frozen; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool DisturbingElementsFrozen
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not disturbing elements
        /// are enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if disturbing elements are enabled; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public virtual bool DisturbingElementsEnabled
        {
            get { return disturbingElementsEnabled; }
            set {
                if (value != disturbingElementsEnabled) {
                    if (value) {
                        ResetDisturbingElements();
                    } else {
                        ResetDisturbingElements(0);
                    }

                    disturbingElementsEnabled = value;
                    Game.Cheats[WizzleGameHost.DisturbingElementsHidingCheatId].IsActive = !value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not disturbing elements
        /// are terminable.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if disturbing elements are terminable; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public virtual bool DisturbingElementsTerminable
        {
            get { return disturbingElementsTerminable; }
            set {
                if (value != disturbingElementsTerminable) {
                    foreach (DisturbingElement disturbingElement in disturbingElements) {
                        disturbingElement.IsTerminable = value;
                    }

                    disturbingElementsTerminable = value;
                    Game.Cheats[WizzleGameHost.DisturbingElementsImmunityCheatId].IsActive = !value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not tile numbers
        /// are enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if tile numbers are enabled; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public virtual bool TileNumbersEnabled
        {
            get { return tileNumbersEnabled; }
            set {
                tileNumbersEnabled = value;
                Game.Cheats[WizzleGameHost.TileNumbersCheatId].IsActive = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not this <see cref="GameBoard"/>
        /// is active.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this <see cref="GameBoard"/> is active; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsActive
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="WizzleGame"/> associated with this <see cref="GameBoard"/>.
        /// </summary>
        /// <value>
        /// The <see cref="WizzleGame"/> associated with this <see cref="GameBoard"/>.
        /// </value>
        public WizzleGame Game
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the <see cref="Tile"/> width.
        /// </summary>
        /// <value>The <see cref="Tile"/> width.</value>
        public int TileWidth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="Tile"/> height.
        /// </summary>
        /// <value>The <see cref="Tile"/> height.</value>
        public int TileHeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="Tile"/> instances of this <see cref="GameBoard"/>.
        /// </summary>
        /// <value>The <see cref="Tile"/> instances of this <see cref="GameBoard"/>.</value>
        public ICollection<Tile> Tiles
        {
            get { return tiles.AsReadOnly(); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Activates this <see cref="GameBoard"/>.
        /// </summary>
        public virtual void Activate()
        {
            IsActive = true;
            DisturbingElementsFrozen = false;
        }

        /// <summary>
        /// Deactivates this <see cref="GameBoard"/>.
        /// </summary>
        public virtual void Deactivate()
        {
            Deactivate(true);
        }

        /// <summary>
        /// Deactivates this <see cref="GameBoard"/> and if <paramref name="freezeDisturbingElements"/>
        /// is <see langword="true"/> freezes active disturbing elements.
        /// </summary>
        /// <param name="freezeDisturbingElements">
        /// A boolean value indicating whether or not to freeze active disturbing elements.
        /// </param>
        public virtual void Deactivate(bool freezeDisturbingElements)
        {
            IsActive = false;
            DisturbingElementsFrozen = freezeDisturbingElements;
        }

        /// <summary>
        /// Draws this <see cref="GameBoard"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (Tile tile in Tiles) {
                tile.Draw(gameTime, spriteBatch);
            }

            if (DisturbingElementsEnabled) {
                foreach (DisturbingElement disturbingElement in disturbingElements) {
                    disturbingElement.Draw(gameTime, spriteBatch);
                }
            }
        }

        /// <summary>
        /// Updates this <see cref="GameBoard"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (DisturbingElementsEnabled) {
                if (DisturbingElementsLeft < 1) {
                    // When there are no disturbing elements left, scramble the puzzle
                    if (Game.Stopwatch.IsRunning && !Game.Puzzle.IsSolved) {
                        Game.Puzzle.Scramble();
                        ResetDisturbingElements();
                    } else {
                        OnNoDisturbingElementsLeft(new EventArgs());
                    }
                }
            }

            foreach (Tile tile in Tiles) {
                tile.Update(gameTime);
            }

            if (DisturbingElementsEnabled) {
                foreach (DisturbingElement disturbingElement in disturbingElements) {
                    disturbingElement.Update(gameTime);
                }
            }
        }
 
        /// <summary>
        /// Resets this <see cref="GameBoard"/>.
        /// </summary>
        protected virtual void Reset()
        {
            WizzleGameHost gameHost = GameHost as WizzleGameHost;

            // Initialize instance variables
            TileWidth = (int) Math.Round(BoundingBox.Width / (float) Game.Puzzle.Width);
            TileHeight = (int) Math.Round(BoundingBox.Height / (float) Game.Puzzle.Height);

            // Initialize game cheats according to current settings
            Game.Cheats[WizzleGameHost.DisturbingElementsHidingCheatId].IsActive = !DisturbingElementsEnabled;
            Game.Cheats[WizzleGameHost.DisturbingElementsImmunityCheatId].IsActive = !DisturbingElementsTerminable;
            Game.Cheats[WizzleGameHost.TileNumbersCheatId].IsActive = TileNumbersEnabled;

            // Initialize disturbing elements
            if (DisturbingElementsEnabled) {
                ResetDisturbingElements();
            }

            // Initialize game board's tiles
            ResetTiles();
        }

        /// <summary>
        /// Raises the <see cref="GameBoard.TileMoved"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        /// <remarks>
        /// The <see cref="GameBoard.OnTileMoved"/> method also allows derived
        /// classes to handle the event without attaching a delegate.
        /// </remarks>
        protected virtual void OnTileMoved(TileMovedEventArgs args)
        {
            if (TileMoved != null) {
                TileMoved(this, args);
            }
        }

        /// <summary>
        /// Raises the <see cref="GameBoard.NoDisturbingElementsLeft"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        /// <remarks>
        /// The <see cref="GameBoard.OnNoDisturbingElementsLeft"/> method also
        /// allows derived classes to handle the event without attaching a delegate.
        /// </remarks>
        protected virtual void OnNoDisturbingElementsLeft(EventArgs args)
        {
            if (NoDisturbingElementsLeft != null) {
                NoDisturbingElementsLeft(this, args);
            }
        }

        /// <summary>
        /// Handles the <see cref=" DisturbingElement.Terminated"/> event.
        /// </summary>set
        /// <param name="sender">
        /// The <see cref="DisturbingElement"/> that generated the event.
        /// </param>
        /// <param name="args">The event data.</param>
        protected virtual void OnDisturbingElementTerminated(object sender, EventArgs args)
        {
            DisturbingElementsLeft--;
        }

        /// <summary>
        /// Handles the <see cref="Wizzle.Engine.Matrix.Scrambled"/> event.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="Wizzle.Engine.Matrix"/> that generated the event.
        /// </param>
        /// <param name="args">The event data.</param>
        protected virtual void OnScrambled(object sender, EventArgs args)
        {
            for (int x = 0; x < Game.Puzzle.Width; x++) {
                for (int y = 0; y < Game.Puzzle.Height; y++) {
                    Tile tile = Game.Puzzle[x, y] as Tile;

                    if (tile != null) {
                        Vector2 newPosition = new Vector2(TileWidth * x, TileHeight * y + Position.Y);
                        tile.Position = newPosition;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="WizzleGame.InstanceReset"/> event.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="WizzleGame"/> that generated the event.
        /// </param>
        /// <param name="args">The event data.</param>
        protected virtual void OnGameReset(object sender, EventArgs args)
        {
            Game.Puzzle.Scrambled -= OnScrambled;
            Game.Puzzle.Scrambled += OnScrambled;
            Reset();
        }

        /// <summary>
        /// Resets the number of <see cref="DisturbingElement"/> instances of this
        /// <see cref="GameBoard"/> to the default.
        /// </summary>
        /// <remarks>
        /// The default number of <see cref="DisturbingElement"/> instances
        /// corresponds to the current number of <see cref="Tile"/> instances in
        /// this <see cref="GameBoard"/>.
        /// </remarks>
        public void ResetDisturbingElements()
        {
            ResetDisturbingElements(Game.Puzzle.Width * Game.Puzzle.Height);
        }

        /// <summary>
        /// Resets the number of <see cref="DisturbingElement"/> instances of this
        /// <see cref="GameBoard"/> to the specified value.
        /// </summary>
        /// <param name="count">
        /// The number of <see cref="DisturbingElement"/> instances.
        /// </param>
        public void ResetDisturbingElements(int count)
        {
            if (count < 1) {
                if (disturbingElements != null) {
                    disturbingElements.Clear();
                }
            } else {
                if (disturbingElements == null) {
                    disturbingElements = new Stack<DisturbingElement>(MaxTileCount);
                }

                if (count < disturbingElements.Count) {
                    while (count < disturbingElements.Count) {
                        disturbingElements.Pop();
                    }
                } else if (count > disturbingElements.Count) {
                    while (count > disturbingElements.Count) {
                        disturbingElements.Push(
                            new Scorpion(GameHost, this, Vector2.Zero,
                                new Rectangle((int) Position.X, (int) Position.Y, BoundingBox.Width, BoundingBox.Height)
                            )
                        );

                        disturbingElements.Peek().Terminated += OnDisturbingElementTerminated;
                    }
                }

                Vector2 position = new Vector2();

                foreach (DisturbingElement disturbingElement in disturbingElements) {
                    position.X = GameRandomHelper.GetRandomInt((int) Position.X, BoundingBox.Width - DisturbingElement.FrameWidth);
                    position.Y = GameRandomHelper.GetRandomInt((int) Position.Y, (int) Position.Y + BoundingBox.Height - DisturbingElement.FrameHeight);
                    disturbingElement.Position = position;
                    disturbingElement.IsTerminated = false;
                    disturbingElement.IsTerminable = DisturbingElementsTerminable;
                }
            }

            DisturbingElementsLeft = disturbingElements.Count;
        }

        /// <summary>
        /// Resets the <see cref="Tile"/> objects in this <see cref="GameBoard"/>.
        /// </summary>
        private void ResetTiles()
        {
            tiles.Clear();

            int order = 0;
            int maxOrder = (Game.Puzzle.Width * Game.Puzzle.Height) - 1;
            Vector2 position = new Vector2();
            Rectangle sourceRectangle = new Rectangle();
            Tile newTile = null;

            for (int y = 0; y < Game.Puzzle.Height; y++) {
                for (int x = 0; x < Game.Puzzle.Width; x++) {
                    if (order < maxOrder) {
                        sourceRectangle.X = TileWidth * x;
                        sourceRectangle.Y = TileHeight * y;
                        sourceRectangle.Width = TileWidth;
                        sourceRectangle.Height = TileHeight;

                        position.X = sourceRectangle.X;
                        position.Y = sourceRectangle.Y + Position.Y;

                        newTile = new Tile(GameHost, position, sourceRectangle, this, order++);
                        newTile.Color = Color.White;
                        newTile.AutoHitTestMode = AutoHitTestMode.Rectangle;

                        tiles.Add(newTile);
                        Game.Puzzle[x, y] = newTile;
                    } else {
                        Game.Puzzle[x, y] = null;
                    }
                }
            }
        }
        #endregion
    }
}
