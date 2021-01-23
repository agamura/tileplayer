#region Header
//+ <source name="GameInfo.cs" language="C#" begin="20-Dec-2011">
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
using PlaXore.GameFramework;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Provides functionality for showing <see cref="LocalGameBoard"/> information.
    /// </summary>
    class GameInfo : SpriteObject
    {
        #region Fields
        private const int MarginWidth = 1;
        private const double BlinkInterval = 500.0;
        private const long ScoreThreshold = 1;

        private LocalGameBoard gameBoard;
        private TextObject score;
        private TextObject moveCount;
        private TextObject elapsed;
        private TextObject status;
        private TextObject hint;
        private TimeSpan elapsedTimeHint;
        private TimeSpan elapsedTimeScore;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GameInfo"/> class with
        /// the specified game host, <see cref="LocalGameBoard"/>, and position.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="gameBorad">The <see cref="LocalGameBoard"/> associated with this <see cref="GameInfo"/>.</param>
        /// <param name="position">The position of this <see cref="GameInfo"/>.</param>
        public GameInfo(GameHost gameHost, LocalGameBoard gameBorad, Vector2 position)
            : base(gameHost, position, gameHost.Textures["GameInfoBackground"])
        {
            LayerDepth = 1f;

            SpriteFont spriteFont = gameHost.Fonts["VerdanaRegular14"];
            float lineHeight = (BoundingBox.Height / 3) * .9f;
            float top = BoundingBox.Top * .9f;

            score = new TextObject(gameHost, spriteFont);
            score.HorizontalAlignment = TextAlignment.Near;
            score.VerticalAlignment = TextAlignment.Center;
            score.Position = new Vector2(MarginWidth, top + lineHeight);
            score.LayerDepth = 0.9f;

            moveCount = new TextObject(gameHost, spriteFont);
            moveCount.HorizontalAlignment = TextAlignment.Near;
            moveCount.VerticalAlignment = TextAlignment.Center;
            moveCount.Position = new Vector2(BoundingBox.Width * .45f, top + lineHeight);
            moveCount.LayerDepth = 0.9f;

            elapsed = new TextObject(gameHost, spriteFont);
            elapsed.HorizontalAlignment = TextAlignment.Near;
            elapsed.VerticalAlignment = TextAlignment.Center;
            elapsed.Position = new Vector2((BoundingBox.Right - MarginWidth) - spriteFont.MeasureString("99:99:99.9").X, top + lineHeight);
            elapsed.LayerDepth = 0.9f;

            status = new TextObject(gameHost, spriteFont);
            status.HorizontalAlignment = TextAlignment.Near;
            status.VerticalAlignment = TextAlignment.Center;
            status.Position = new Vector2(MarginWidth, top + (lineHeight * 2));
            status.LayerDepth = 0.9f;

            hint = new TextObject(gameHost, spriteFont);
            hint.HorizontalAlignment = TextAlignment.Near;
            hint.VerticalAlignment = TextAlignment.Center;
            hint.Position = new Vector2(MarginWidth, top + (lineHeight * 3));
            hint.LayerDepth = 0.9f;

            gameBoard = gameBorad;
            Reset();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Draws this <see cref="GameInfo"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            score.Draw(gameTime, spriteBatch);
            moveCount.Draw(gameTime, spriteBatch);
            elapsed.Draw(gameTime, spriteBatch);
            status.Draw(gameTime, spriteBatch);
            hint.Draw(gameTime, spriteBatch);

            base.Draw(gameTime, spriteBatch);
        }

        /// <summary>
        /// Updates this <see cref="GameInfo"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameHost.IsActive) {
                UpdateScore(gameTime);
                UpdateMoveCount(gameTime);
                UpdateElapsed(gameTime);
                UpdateState(gameTime);
                UpdateHint(gameTime);
            }
        }

        /// <summary>
        /// Resets this <see cref="GameInfo"/>.
        /// </summary>
        public void Reset()
        {
            score.Color = Color.White;
            moveCount.Color = Color.White;
            elapsed.Color = Color.White;
            status.Color = Color.White;
            hint.Color = Color.White;
        }

        /// <summary>
        /// Updates the score of this <see cref="GameInfo"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>UpdateScore</b>.</param>
        private void UpdateScore(GameTime gameTime)
        {
            long score = (long) gameBoard.Game.Score;

            if (score <= ScoreThreshold && gameBoard.Game.Counter.Current > 0) {
                elapsedTimeScore += gameTime.ElapsedGameTime;

                if (elapsedTimeScore.TotalMilliseconds > BlinkInterval) {
                    elapsedTimeScore = TimeSpan.Zero;
                    this.score.Color = this.score.Color != Color.Red ? Color.Red : Color.Transparent;
                }
            }

            this.score.Text = String.Format(Resources.Score, score);
        }

        /// <summary>
        /// Updates the move count of ths <see cref="GameInfo"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>UpdateMoveCount</b>.</param>
        private void UpdateMoveCount(GameTime gameTime)
        {
            moveCount.Text = String.Format(Resources.Moves, gameBoard.Game.Counter.Current);
        }

        /// <summary>
        /// Updates the elapsed elapsed of this <see cref="GameInfo"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>UpdateElapsed</b>.</param>
        private void UpdateElapsed(GameTime gameTime)
        {
            elapsed.Text = gameBoard.Game.Stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.f");
        }

        /// <summary>
        /// Updates the state of this <see cref="GameInfo"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>UpdateState</b>.</param>
        private void UpdateState(GameTime gameTime)
        {
            WizzleGameHost gameHost = GameHost as WizzleGameHost;

            switch (gameHost.GameState.Current) {
                case GameStateId.Running:
                    status.Text = Resources.StatusRunning;
                    break;
                case GameStateId.Paused:
                    status.Text = Resources.StatusPaused;
                    break;
                case GameStateId.Ready:
                    status.Text = Resources.StatusReady;
                    break;
                case GameStateId.GameOver:
                    status.Text = gameBoard.Game.Score < 1.0 ? Resources.StatusGameOver : Resources.StatusSolved;
                    break;
                default:
                    status.Text = String.Format(Resources.Welcome, gameHost.LocalGamer.Gamertag);
                    break;
            }
        }

        /// <summary>
        /// Updates the hint of this <see cref="GameInfo"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>UpdateHint</b>.</param>
        private void UpdateHint(GameTime gameTime)
        {
            string text = "";

            if ((GameHost as WizzleGameHost).GameState.Current > GameStateId.Loaded && gameBoard.DisturbingElementsEnabled) {
                if (gameBoard.DisturbingElementsLeft <= GameBoard.DisturbingElementsThreshold) {
                    elapsedTimeHint += gameTime.ElapsedGameTime;

                    if (elapsedTimeHint.TotalMilliseconds > BlinkInterval) {
                        elapsedTimeHint = TimeSpan.Zero;
                        hint.Color = hint.Color != Color.Red ? Color.Red : Color.Transparent;
                    }

                    text = String.Format("{0} ", Resources.Warning);
                } else {
                    hint.Color = Color.White;
                }

                text += String.Format(Resources.DisturbingElementsLeft,
                    gameBoard.DisturbingElementSample.PluralName,
                    gameBoard.DisturbingElementsLeft);
            }

            hint.Text = text;
        }
        #endregion
    }
}
