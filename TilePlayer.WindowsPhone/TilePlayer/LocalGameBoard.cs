#region Header
//+ <source name="LocalGameBoard.cs" language="C#" begin="03-Dec-2011">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2011">
//+ //+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Devices;
using PlaXore.GameFramework;
using PlaXore.GameFramework.Input;
using PlaXore.GameFramework.Controls;
using PlaXore.Media;
using Wizzle.Engine;
using WizzleGame = Wizzle.Engine.Game;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Represents a local <see cref="GameBoard"/>.
    /// </summary>
    class LocalGameBoard : GameBoard
    {
        #region Fields
        private const float KineticFriction = 0.80f;
        private const float MaxMoveRatio = 0.50f;
        private static readonly TimeSpan VibrateTime;

        private IDecoder decoder;
        private GameInfo gameInfo;
        private Tile selectedTile;
        private Vector2 delta;
        // private byte[] lastFrame;
        private Texture2D currentFrame;
        private Object lockObject;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes read-only members.
        /// </summary>
        static LocalGameBoard()
        {
            VibrateTime = new TimeSpan(222222);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalGameBoard"/> class with the
        /// specified game host, position, source rectangle, and <see cref="WizzleGame"/>.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="position">The <see cref="LocalGameBoard"/> position.</param>
        /// <param name="sourceRectangle">The <see cref="LocalGameBoard"/> source rectangle.</param>
        /// <param name="game">
        /// The <see cref="WizzleGame"/> associated with this <see cref="LocalGameBoard"/>.
        /// </param>
        public LocalGameBoard(GameHost gameHost, Vector2 position, Rectangle sourceRectangle, WizzleGame game)
            : base(gameHost, position, sourceRectangle, game)
        {
            Pressed += OnPressed;
            Released += OnReleased;
            Flick += OnFlick;
            HorizontalDrag += OnHorizontalDrag;
            VerticalDrag += OnVerticalDrag;
            lockObject = new Object();

            ICodecFactory codecFactory = new MiwaCodecFactory();
            decoder = codecFactory.GetDecoder(TitleContainer.OpenStream("Content/Video/desert_riders.miwa"), true);
            decoder.IsLooped = true;
            decoder.FrameAvailable += OnFrameAvailable;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the currently selected <see cref="Tile"/>.
        /// </summary>
        /// <value>The currently selected <see cref="Tile"/>.</value>
        public Tile SelectedTile
        {
            get
            {
                return selectedTile;
            }
            set
            {
                selectedTile = value;
                if (value == null) {
                    delta = Vector2.Zero;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Activates this <see cref="LocalGameBoard"/>.
        /// </summary>
        public override void Activate()
        {
            decoder.Start();
            base.Activate();
        }

        /// <summary>
        /// Deactivates this <see cref="LocalGameBoard"/> and if <paramref name="freezeDisturbingElements"/>
        /// is <see langword="true"/> freezes active disturbing elements.
        /// </summary>
        /// <param name="freezeDisturbingElements">
        /// A boolean value indicating whether or not to freeze active disturbing elements.
        /// </param>
        public override void Deactivate(bool freezeDisturbingElements)
        {
            decoder.Stop();
            base.Deactivate(freezeDisturbingElements);
        }

        /// <summary>
        /// Handles the <see cref="Control.Pressed"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Control"/> that raised the event.</param>
        /// <param name="args">The event data.</param>
        protected virtual void OnPressed(object sender, GameInputEventArgs args)
        {
            if (!Game.Puzzle.IsSolved) {
                // Before selecting another tile, complete the current movement
                if (SelectedTile != null && delta != Vector2.Zero) {
                    return;
                }

                SelectTile(args.TouchPosition);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Released"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Control"/> that raised the event.</param>
        /// <param name="args">The event data.</param>
        protected virtual void OnReleased(object sender, GameInputEventArgs args)
        {
            if (!Game.Puzzle.IsSolved) {
                if (delta == Vector2.Zero) {
                    SelectedTile = null;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Flick"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Control"/> that raised the event.</param>
        /// <param name="args">The event data.</param>
        protected virtual void OnFlick(object sender, GameInputEventArgs args)
        {
            if (IsActive && !Game.Puzzle.IsSolved) {
                Vector2 delta = args.GestureDelta * (float) GameHost.TargetElapsedTime.TotalSeconds;

                if (Math.Abs(delta.X) > Math.Abs(delta.Y)) {
                    delta.Y = 0;
                } else {
                    delta.X = 0;
                }

                MoveTile(delta, 0.5f, 1);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.HorizontalDrag"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Control"/> that raised the event.</param>
        /// <param name="args">The event data.</param>
        protected virtual void OnHorizontalDrag(object sender, GameInputEventArgs args)
        {
            if (IsActive && !Game.Puzzle.IsSolved) {
                MoveTile(args.GestureDelta, 0.1f, 4);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.VerticalDrag"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Control"/> that raised the event.</param>
        /// <param name="args">The event data.</param>
        protected virtual void OnVerticalDrag(object sender, GameInputEventArgs args)
        {
            if (IsActive && !Game.Puzzle.IsSolved) {
                MoveTile(args.GestureDelta, 0.1f, 4);
            }
        }

        /// <summary>
        /// Updates this <see cref="LocalGameBoard"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // UpdateTiles(gameTime);

            Texture2D currentFrame = null, previousFrame = null;

            if (this.currentFrame != null) {
                lock (lockObject) {
                    currentFrame = this.currentFrame;
                    this.currentFrame = null;
                }

                foreach (Tile tile in Tiles) {
                    previousFrame = tile.Texture;
                    tile.Texture = currentFrame;
                }

                if (previousFrame != null && !previousFrame.IsDisposed) {
                    previousFrame.Dispose();
                }
            }

            if (SelectedTile != null) {
                Position position = Game.Puzzle.GetPosition(SelectedTile.Order);
                int positionX = position.X;
                int positionY = position.Y;

                if (delta != Vector2.Zero) {
                    Wizzle.Engine.Position blank = Game.Puzzle.BlankPosition;
                    Direction direction = GetDirection(delta);
                    bool stopTile = false;

                    switch (direction) {
                        case Direction.Left:
                            for (int i = blank.X; i < positionX; i++) {
                                stopTile |= StopTile(i + 1, positionY, i, positionY, delta);
                            }
                            break;
                        case Direction.Right:
                            for (int i = blank.X; i > positionX; i--) {
                                stopTile |= StopTile(i - 1, positionY, i, positionY, delta);
                            }
                            break;
                        case Direction.Up:
                            for (int i = blank.Y; i < positionY; i++) {
                                stopTile |= StopTile(positionX, i + 1, positionX, i, delta);
                            }
                            break;
                        case Direction.Down:
                            for (int i = blank.Y; i > positionY; i--) {
                                stopTile |= StopTile(positionX, i - 1, positionX, i, delta);
                            }
                            break;
                    }

                    if (stopTile) {
                        delta = Vector2.Zero;
                    } else {
                        Vector2 blankCorner = GetUpLeftCorner(blank.X, blank.Y);

                        // Ensure that the friction value is within range; apply
                        // friction to the vector so that movement slows and stops.
                        delta *= MathHelper.Clamp(KineticFriction, 0, 1);

                        // Set delta movement to 0 when < 0.9
                        if (direction == Direction.Left || direction == Direction.Right) {
                            if (Math.Abs(delta.X) < 0.9f) {
                                switch (direction) {
                                    case Direction.Left:
                                        for (int i = blank.X; i < positionX; i++) {
                                            StickTile(i + 1, positionY, i, positionY);
                                        }
                                        break;
                                    case Direction.Right:
                                        for (int i = blank.X; i > positionX; i--) {
                                            StickTile(i - 1, positionY, i, positionY);
                                        }
                                        break;
                                }

                                delta.X = 0;
                            }
                        } else if (direction == Direction.Up || direction == Direction.Down) {
                            if (Math.Abs(delta.Y) < 0.9f) {
                                switch (direction) {
                                    case Direction.Up:
                                        for (int i = blank.Y; i < positionY; i++) {
                                            StickTile(positionX, i + 1, positionX, i);
                                        }
                                        break;
                                    case Direction.Down:
                                        for (int i = blank.Y; i > positionY; i--) {
                                            StickTile(positionX, i - 1, positionX, i);
                                        }
                                        break;
                                }

                                delta.Y = 0;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the <see cref="Tile"/> instances of this <see cref="LocalGameBoard"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>UpdateTiles</b>.</param>
        private void UpdateTiles(GameTime gameTime)
        {
            /*
            TimeSpan elapsedTime = gameTime != null ? gameTime.ElapsedGameTime : TimeSpan.Zero;
            byte[] frame = decoder[elapsedTime];

            if (frame != null) {
                if (frame != lastFrame) {
                    lastFrame = frame;
                    Texture2D texture = Texture2D.FromStream(GameHost.GraphicsDevice, new MemoryStream(frame));

                    foreach (Tile tile in Tiles) {
                        if (tile.Texture != null && !tile.Texture.IsDisposed) {
                            tile.Texture.Dispose();
                        }

                        tile.Texture = texture;
                    }
                }
            }
            */
        }

        /// <summary>
        /// Resets this <see cref="LocalGameBoard"/>.
        /// </summary>
        protected override void Reset()
        {
            base.Reset();

            if (gameInfo == null) {
                gameInfo = new GameInfo(GameHost, this, new Vector2(1, 80));
                gameInfo.Color = Color.White;
                gameInfo.Show();
                GameHost.GameObjects.Add(gameInfo);
            }

            gameInfo.Reset();
        }

        /// <summary>
        /// Selects the <see cref="Tile"/> at the specified delta, if any.
        /// </summary>
        /// <param name="delta">
        /// A vector that represents the difference between the location of the
        /// first touchpoint and the location of the last touchpoint.
        /// </param>
        private void SelectTile(Vector2 delta)
        {
            foreach (Tile tile in Tiles) {
                if (tile.IsPointInObject(delta)) {
                    SelectedTile = tile;
                    break;
                }
            }
        }

        /// <summary>
        /// Determines whether or not the <see cref="Tile"/> at the specified 
        /// position can be moved towards the specified delta.
        /// </summary>
        /// <param name="startPosition">The <see cref="Tile"/> position.</param>
        /// <param name="delta">
        /// A vector that represents the difference between the location of the
        /// first touchpoint and the location of the last touchpoint.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="Tile"/> can be moved towards
        /// <paramref name="delta"/>; otherwise, <see langword="false"/>.
        /// </returns>
        private bool CanMove(Position startPosition, Vector2 delta)
        {
            bool canMove = false;

            Wizzle.Engine.Matrix puzzle = Game.Puzzle;
            Wizzle.Engine.Position blank = puzzle.BlankPosition;

            switch (GetDirection(delta)) {
                case Direction.Up:
                    canMove = (blank.X == startPosition.X && blank.Y < startPosition.Y);
                    break;
                case Direction.Down:
                    canMove = (blank.X == startPosition.X && blank.Y > startPosition.Y);
                    break;
                case Direction.Left:
                    canMove = (blank.Y == startPosition.Y && blank.X < startPosition.X);
                    break;
                case Direction.Right:
                    canMove = (blank.Y == startPosition.Y && blank.X > startPosition.X);
                    break;
                case Direction.None:
                    canMove = (blank.X == startPosition.X || blank.Y == startPosition.Y);
                    break;
            }

            return canMove;
        }

        /// <summary>
        /// Stops the <see cref="Tile"/> at the specified coordinates at the
        /// nearest allowed position.
        /// </summary>
        /// <param name="x">The <see cref="Tile"/> x-coordinate.</param>
        /// <param name="y">The <see cref="Tile"/> y-coordinate.</param>
        /// <param name="newX">The x-coordinate of the nearest allowed position.</param>
        /// <param name="newY">The y-coordinate of the nearest allowed position.</param>
        /// <param name="delta">
        /// A vector that represents the difference between the location of the
        /// first touchpoint and the location of the last touchpoint.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="Tile"/> was stopped;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private bool StopTile(int x, int y, int newX, int newY, Vector2 delta)
        {
            Tile tile = Game.Puzzle[x, y] as Tile;
            tile.Position += delta;
            Vector2 nextCorner = GetUpLeftCorner(newX, newY);
            bool stopTile = false;

            switch (GetDirection(delta)) {
                case Direction.Left:
                    if (tile.Position.X <= nextCorner.X) {
                        tile.PositionX = nextCorner.X;
                        stopTile = true;
                    }
                    break;
                case Direction.Right:
                    if (tile.Position.X >= nextCorner.X) {
                        tile.PositionX = nextCorner.X;
                        stopTile = true;
                    }
                    break;
                case Direction.Up:
                    if (tile.Position.Y <= nextCorner.Y) {
                        tile.PositionY = nextCorner.Y;
                        stopTile = true;
                    }
                    break;
                case Direction.Down:
                    if (tile.Position.Y >= nextCorner.Y) {
                        tile.PositionY = nextCorner.Y;
                        stopTile = true;
                    }
                    break;
            }

            if (stopTile) {
                if (tile == SelectedTile) {
                    OnTileMoved(new TileMovedEventArgs(tile));
                }
            }

            return stopTile;
        }

        /// <summary>
        /// Sticks the <see cref="Tile"/> at the specified coordinates to the
        /// nearest allowed position.
        /// </summary>
        /// <param name="x">The <see cref="Tile"/> x-coordinate.</param>
        /// <param name="y">The <see cref="Tile"/> y-coordinate.</param>
        /// <param name="newX">The x-coordinate of the nearest allowed position.</param>
        /// <param name="newY">The y-coordinate of the nearest allowed position.</param>
        private void StickTile(int x, int y, int newX, int newY)
        {
            Tile tile = Game.Puzzle[x, y] as Tile;
            tile.Position = GetUpLeftCorner(newX, newY);

            if (tile == SelectedTile) {
                OnTileMoved(new TileMovedEventArgs(tile));
            }
        }

        /// <summary>
        /// Returns the upper-left corner of the <see cref="Tile"/> at the
        /// specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate</param>
        /// <returns>
        /// The upper-left corner of the <see cref="Tile"/> at <paramref name="x"/>,
        /// <paramref name="y"/>.
        /// </returns>
        private Vector2 GetUpLeftCorner(int x, int y)
        {
            Vector2 position = new Vector2(0, 160);
            position.X += x * TileWidth;
            position.Y += y * TileWidth;

            return position;
        }

        /// <summary>
        /// Returns the <see cref="PlaXore.GameFramework.Input.Direction"/> of the
        /// specified delta.
        /// </summary>
        /// <param name="delta">
        /// A vector that represents the difference between the location of the
        /// first touchpoint and the location of the last touchpoint.
        /// </param>
        /// <returns>
        /// One of the <see cref="PlaXore.GameFramework.Input.Direction"/> values.
        /// </returns>
        private Direction GetDirection(Vector2 delta)
        {
            Direction direction = Direction.None;

            if (delta.X != 0) {
                direction = (delta.X > 0 ? Direction.Right : Direction.Left);
            } else if (delta.Y != 0) {
                direction = (delta.Y > 0 ? Direction.Down : Direction.Up);
            }

            return direction;
        }

        /// <summary>
        /// Moves the currently <see cref="SelectedTile"/> towards the specified delta.
        /// </summary>
        /// <param name="delta">
        /// A vector that represents the difference between the location of the
        /// first touchpoint and the location of the last touchpoint.
        /// </param>
        /// <param name="tileSizeRatio">The <see cref="Tile"/> size ratio.</param>
        /// <param name="kineticFactor">The kinetic factor.</param>
        private void MoveTile(Vector2 delta, float tileSizeRatio, float kineticFactor)
        {
            if (SelectedTile != null) {
                if (this.delta != Vector2.Zero) {
                    return;
                }

                if (delta.X != 0) {
                    delta.X = (delta.X > 0 ? 1 : -1) * (SelectedTile.SourceRectangle.Width * tileSizeRatio);
                    delta.X = MathHelper.Clamp(delta.X, -TileWidth * MaxMoveRatio, TileWidth * MaxMoveRatio);
                    delta.Y = 0;
                } else if (delta.Y != 0) {
                    delta.Y = (delta.Y > 0 ? 1 : -1) * (SelectedTile.SourceRectangle.Height * tileSizeRatio);
                    delta.Y = MathHelper.Clamp(delta.Y, -TileHeight * MaxMoveRatio, TileHeight * MaxMoveRatio);
                }

                if (CanMove(Game.Puzzle.GetPosition(SelectedTile.Order), delta)) {
                    this.delta = delta * kineticFactor;
                } else {
                    this.delta = Vector2.Zero;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="GameBoard.TileMoved"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnTileMoved(TileMovedEventArgs args)
        {
            base.OnTileMoved(args);

            if ((GameHost as WizzleGameHost).IsSoundEnabled) {
                SelectedTile.MoveSoundEffect.Play();
            }

            if ((GameHost as WizzleGameHost).IsVibrateEnabled) {
                VibrateController.Default.Start(VibrateTime);
            }
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
            if ((GameHost as WizzleGameHost).IsSoundEnabled) {
                GameHost.SoundEffects["ScrambleSound"].Play();
            }

            base.OnScrambled(sender, args);
        }

        /// <summary>
        /// Handles the <see cref="WizzleGame.InstanceReset"/> event.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="WizzleGame"/> that generated the event.
        /// </param>
        /// <param name="args">The event data.</param>
        protected override void OnGameReset(object sender, EventArgs args)
        {
            SelectedTile = null;
            decoder.Reset();
            base.OnGameReset(sender, args);
        }

        /// <summary>
        /// Handles the <see cref="MiwaCodec.FrameAvailable"/> event.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="MiwaCodec"/> that generated the event.
        /// </param>
        /// <param name="args">The event data.</param>
        private void OnFrameAvailable(object sender, MediaEventArgs args)
        {
            lock (lockObject) {
                currentFrame = Texture2D.FromStream(GameHost.GraphicsDevice, new MemoryStream(args.Data));
            }

            /*
            foreach (Tile tile in Tiles) {
                tile.Texture = currentFrame;
            }

            if (this.currentFrame != null && !this.currentFrame.IsDisposed) {
                this.currentFrame.Dispose();
                this.currentFrame = currentFrame;
            }
            */
        }
        #endregion
    }
}
