#region Header
//+ <source name="DisturbingElement.cs" language="C#" begin="05-Jan-2012">
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using PlaXore.GameFramework;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// This is the base class for all disturbing elements moving around on the
    /// <see cref="LocalGameBoard"/> surface.
    /// </summary>
    abstract class DisturbingElement : SpriteObject
    {
        #region Fields
        private const int DefaultSkinAlpha = 210;
        private const int MaxShineAlpha = 150;

        internal const int FrameWidth = 64;
        internal const int FrameHeight = 64;

        private Vector2 movement;
        private Vector2 screenMin;
        private Vector2 screenMax;
        private TimeSpan frameElapsedTime;
        private TimeSpan shineElapsedTime;
        private bool isOverSafeArea;
        private bool isGoingToTerminate;
        private int shineAlpha;
        private float minSpeed;
        private float maxSpeed;
        private float increaseFactor;
        private float targetAngle;
        private int moveFrameX;
        private int moveFrameY;
        private int terminateFrameX;
        private int terminateFrameY;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the <see cref="DisturbingElement"/> is terminated.
        /// </summary>
        public event EventHandler Terminated;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DisturbingElement"/>
        /// class with the specified game host, position, and moving area.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="gameBoard">The <see cref="GameBoard"/> this <see cref="DisturbingElement"/> belongs to.</param>
        /// <param name="position">The <see cref="DisturbingElement"/> position.</param>
        /// <param name="movingArea">The moving area.</param>
        public DisturbingElement(GameHost gameHost, GameBoard gameBoard, Vector2 position, Rectangle movingArea)
            : base(gameHost, position)
        {
            LayerDepth = 0.4f;
            GameBoard = gameBoard;

            frameElapsedTime = TimeSpan.Zero;
            screenMin.X = movingArea.X;
            screenMin.Y = movingArea.Y;
            screenMax.X = movingArea.X + movingArea.Width;
            screenMax.Y = movingArea.Y + movingArea.Height;
            minSpeed = 0.5f;
            maxSpeed = 2f;
            increaseFactor = 0.001f;
            isOverSafeArea = true;

            IsTerminable = true;
            IsTerminated = false;
            IsVisible = true;

            // Generate random movement and direction
            float directionX = GameRandomHelper.GetRandomFloat(1f, 10f);
            if (directionX < 6f) { directionX = -1f; } else { directionX = 1f; }
            float directionY = GameRandomHelper.GetRandomFloat(1f, 10f);
            if (directionY < 6f) { directionY = -1f; } else { directionY = 1f; }

            movement = GameRandomHelper.GetRandomVector2(1f, 3f);
            movement.X *= directionX;
            movement.Y *= directionY;

            shineAlpha = DefaultSkinAlpha;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="GameBoard"/> this <see cref="DisturbingElement"/>
        /// belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="GameBoard"/> this <see cref="DisturbingElement"/> belongs to.
        /// </value>
        public GameBoard GameBoard
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this
        /// <see cref="DisturbingElement"/> is terminated.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this <see cref="DisturbingElement"/> is
        /// terminated; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsTerminated
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this
        /// <see cref="DisturbingElement"/> is terminable.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this <see cref="DisturbingElement"/> is
        /// terminable; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsTerminable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <i>moving</i> texture of this
        /// <see cref="DisturbingElement"/>.
        /// </summary>
        /// <value>
        /// The <i>moving</i> texture of this <see cref="DisturbingElement"/>.
        /// </value>
        public abstract Texture2D MovingTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <i>termination</i> texture of this
        /// <see cref="DisturbingElement"/>.
        /// </summary>
        /// <value>
        /// The <i>termination</i> texture of this <see cref="DisturbingElement"/>.
        /// </value>
        public abstract Texture2D TerminationTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <i>termination</i> sound effect of this
        /// <see cref="DisturbingElement"/>.
        /// </summary>
        /// <value>
        /// The <i>termination</i> sound effect of this <see cref="DisturbingElement"/>.
        /// </value>
        public abstract SoundEffect TerminationSound
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of this <see cref="DisturbingElement"/>.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Gets the plural name of this <see cref="DisturbingElement"/>.
        /// </summary>
        public abstract string PluralName
        {
            get;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws this <see cref="DisturbingElement"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!SourceRectangle.IsEmpty && IsVisible) {
                Color color = Color;

                if (!IsTerminable) {
                    color = new Color(255, 215, 0);
                } else if ((GameHost as WizzleGameHost).LocalGameBoard.DisturbingElementsLeft <= GameBoard.DisturbingElementsThreshold) {
                    color = new Color(Color.Red.R, Color.Red.G, Color.Red.B);
                }

                if (color != Color) {
                    shineElapsedTime += gameTime.ElapsedGameTime;

                    if (shineElapsedTime.TotalMilliseconds > 250) {
                        shineElapsedTime = TimeSpan.Zero;
                        shineAlpha = shineAlpha < DefaultSkinAlpha ? DefaultSkinAlpha : MaxShineAlpha;
                    }

                    color.A = (byte) shineAlpha;
                }

                spriteBatch.Draw(Texture, Position, SourceRectangle,
                    color, Angle, Origin, Scale, SpriteEffects.None, LayerDepth);
            }
        }

        /// <summary>
        /// Updates this <see cref="DisturbingElement"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        public override void Update(GameTime gameTime)
        {
            if (IsTerminated) { return; }
            base.Update(gameTime);

            if (!isGoingToTerminate) {
                Texture = MovingTexture;

                if (!GameBoard.DisturbingElementsFrozen) {
                    SourceRectangle = GetNextMoveFrame();
                    Move();
                }

                isOverSafeArea = false;

                if (PositionX <= screenMax.X
                    && PositionX >= screenMin.X
                    && PositionY <= screenMax.Y
                    && PositionY >= screenMin.Y) {
                    foreach (Tile tile in GameBoard.Tiles) {
                        Rectangle tileRect = tile.BoundingBox;
                        if (BoundingBox.Intersects(tileRect)) {
                            isOverSafeArea = true;
                            break;
                        }
                    }
                } else { isOverSafeArea = true; }
            }

            if ((!isOverSafeArea && IsTerminable
                && (GameHost as WizzleGameHost).GameState.Current >= GameStateId.Ready
                && (GameHost as WizzleGameHost).GameState.Current != GameStateId.GameOver)
                || isGoingToTerminate) {

                // Terminate the disturbing element
                frameElapsedTime += gameTime.ElapsedGameTime;

                if (frameElapsedTime.TotalSeconds > 0.1d) {
                    Texture = TerminationTexture;

                    if (!GameBoard.DisturbingElementsFrozen) {
                        SourceRectangle = GetNextTerminateFrame();
                    }

                    foreach (GameObject gameObject in GameHost.GameObjects) {
                        // Increase all other disturbing elements' speed
                        if (gameObject is DisturbingElement) {
                            DisturbingElement disturbingElement = gameObject as DisturbingElement;
                            if (disturbingElement != this || disturbingElement.IsTerminated == false) {
                                disturbingElement.minSpeed += increaseFactor;
                                disturbingElement.maxSpeed += increaseFactor;
                            }
                        }
                    }

                    isGoingToTerminate = true;
                    frameElapsedTime = TimeSpan.Zero;
                }
            }

            // Rotate towards the target angle
            if (Angle != targetAngle) {
                Angle += (targetAngle - Angle) * 0.3f;
            }

            if (SourceRectangle.IsEmpty) {
                Color = Color.White;
                isGoingToTerminate = false;

                IsTerminated = true;
                OnTerminated(null);
            }
        }

        /// <summary>
        /// Calculates the new postion of this <see cref="DisturbingElement"/>.
        /// </summary>
        private void Move()
        {
            if (PositionX + (Texture.Width / 4) > screenMax.X) {
                movement.X = GameRandomHelper.GetRandomFloat(minSpeed, maxSpeed) * -1f;
                // RotateToFacePoint(Position + movement);
            }

            if (PositionX < screenMin.X) {
                movement.X = GameRandomHelper.GetRandomFloat(minSpeed, maxSpeed);
                // RotateToFacePoint(Position + movement);
            }

            if (PositionY + (Texture.Height / 4) > screenMax.Y) {
                movement.Y = GameRandomHelper.GetRandomFloat(minSpeed, maxSpeed) * -1;
                // RotateToFacePoint(Position + movement);
            }

            if (PositionY < screenMin.Y) {
                movement.Y = GameRandomHelper.GetRandomFloat(minSpeed, maxSpeed);
                // RotateToFacePoint(Position + movement);
            }
            
            Position = Position + movement;
        }

        /// <summary>
        /// Returns the next move frame of this <see cref="DisturbingElement"/>.
        /// </summary>
        /// <returns>The next move frame.</returns>
        private Rectangle GetNextMoveFrame()
        {
            Rectangle frame = new Rectangle();
            frame.X = moveFrameX * FrameWidth;
            frame.Y = moveFrameY * FrameHeight;
            frame.Width = FrameWidth;
            frame.Height = FrameHeight;

            if (moveFrameX > 2) {
                moveFrameX = 0;

                if (moveFrameY < 3) {
                    moveFrameY++;
                } else {
                    moveFrameY = 0;
                }
            } else {
                moveFrameX++;
            }

            return frame;
        }

        /// <summary>
        /// Returns the next terminate frame of this <see cref="DisturbingElement"/>.
        /// </summary>
        /// <returns>The next terminate frame.</returns>
        private Rectangle GetNextTerminateFrame()
        {
            Rectangle frame = new Rectangle();
            frame.X = terminateFrameX * FrameWidth;
            frame.Y = terminateFrameY * FrameHeight;
            frame.Width = FrameWidth;
            frame.Height = FrameHeight;

            if ((GameHost as WizzleGameHost).IsSoundEnabled) {
                if (terminateFrameX == 1 && terminateFrameY == 1) {
                    TerminationSound.Play();
                }
            }

            if (terminateFrameX > 2) {
                terminateFrameX = 0;

                if (terminateFrameY < 3) {
                    terminateFrameY++;
                } else {
                    terminateFrameY = 0;
                    return Rectangle.Empty;
                }
            } else {
                terminateFrameX++;
            }

            return frame;
        }

        /// <summary>
        /// Calculates the rotation of this <see cref="DisturbingElement"/>
        /// towards facing point.
        /// </summary>
        /// <param name="point">The point of the rotation direction.</param>
        private void RotateToFacePoint(Vector2 point)
        {
            // Find the angle between the disturbing element and the specified point
            point -= Position;

            // If the point is exactly on the disturbing element, ignore the touch
            if (point == Vector2.Zero) {
                return;
            }

            // Ensure the current angle is between 0 and 2 PI
            while (Angle < 0) {
                Angle += MathHelper.TwoPi;
            }

            while (Angle > MathHelper.TwoPi) {
                Angle -= MathHelper.TwoPi;
            }

            // Get the current angle in degrees
            float angleDegrees;
            angleDegrees = MathHelper.ToDegrees(Angle);

            // Calculate the angle between the disturbing element and the point,
            // and convert to degrees
            float targetAngleDegrees;
            targetAngleDegrees = MathHelper.ToDegrees((float) Math.Atan2(point.Y, point.X));

            // XNA puts 0 degrees upwards, whereas Atan2 returns it facing left,
            // so add 90 degrees to rotate the Atan2 value into alignment with XNA
            targetAngleDegrees += 90;

            // Atan2 returns values between -180 and +180, so having added 90
            // degrees we now have a value in the range -90 to +270; in case we
            // are less than zero, add 360 to get an angle in the range 0 to 360
            if (targetAngleDegrees < 0) {
                targetAngleDegrees += 360;
            }

            // Is the target angle over 180 degrees less than the current angle?
            if (targetAngleDegrees < angleDegrees - 180) {
                // Instead of rotating the whole way around to the left,
                // rotate the smaller distance to the right instead
                targetAngleDegrees += 360;
            }

            // Is the target angle over 180 degrees more than the current angle?
            if (targetAngleDegrees > angleDegrees + 180) {
                // Instead of rotating the whole way around to the right,
                // rotate the smaller distance to the left instead
                targetAngleDegrees -= 360;
            }

            // Store the calculated angle and converted back to radians
            targetAngle = MathHelper.ToRadians(targetAngleDegrees);
        }

        /// <summary>
        /// Raises the <see cref="DisturbingElement.Terminated"/> event.
        /// </summary>
        /// <param name="args">Always <see langword="null"/>.</param>
        /// <remarks>
        /// The <see cref="DisturbingElement.OnTerminated"/> method also allows derived
        /// classes to handle the event without attaching a delegate.
        /// </remarks>
        protected virtual void OnTerminated(EventArgs args)
        {
            if (Terminated != null) {
                Terminated(this, args);
            }
        }
        #endregion
    }
}
