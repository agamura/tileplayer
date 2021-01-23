#region Header
//+ <source name=HighScoresBox.cs" language="C#" begin="30-Apr-2012">
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlaXore.GameFramework;
using PlaXore.GameFramework.Controls;
using Wizzle.Engine;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Shows a dialog box with high scores.
    /// </summary>
    public class HighScoresBox : Window
    {
        #region Fields
        private TextObject caption;
        private TextObject[] scoreEntries;
        private Button okButton;
        private GameSession gameSession;
        private HighScoreTable highScoreTable;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="HighScoresBox"/> class
        /// with the specified game host.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        public HighScoresBox(GameHost gameHost)
            : this(gameHost, (gameHost as WizzleGameHost).GameSession)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HighScoresBox"/> class
        /// with the specified game host and <see cref="GameSession"/>.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="gameSession">The <see cref="GameSession"/> to show the high scores for.</param>
        public HighScoresBox(GameHost gameHost, GameSession gameSession)
            : base(gameHost, gameHost.Textures["HighScoresBoxBackground"])
        {
            Texture2D texture = gameHost.Textures["OkButton"];
            Vector2 position = new Vector2();
            position.X = BoundingBox.Left + (BoundingBox.Width / 2) - (texture.Width / 2);
            position.Y = BoundingBox.Bottom - Margin.Bottom - texture.Height;

            okButton = new Button(gameHost, position, texture);
            okButton.Tag = "OkButton";
            okButton.Click += OnClick;

            this.gameSession = gameSession;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the <see cref="GameSession"/> to show the high scores for.
        /// </summary>
        /// <value>The <see cref="GameSession"/> to show the high scores for.</value>
        public GameSession GameSession
        {
            get { return gameSession; }
            set {
                gameSession = value;
                caption = null;
                scoreEntries = null;
            }
        }

        /// <summary>
        /// Gets the caption of this <see cref="HighScoresBox"/>.
        /// </summary>
        /// <value>The caption of this <see cref="HighScoresBox"/>.</value>
        private TextObject Caption
        {
            get {
                if (caption == null) {
                    string text = String.Format(Resources.HighScoresMatrixSize, gameSession.Puzzle.Width, gameSession.Puzzle.Height);
                    SpriteFont font = GameHost.Fonts["DefaultRegular22"];

                    caption = ArrangeText(text,
                        new Rectangle(0, BoundingBox.Top, 0, (int) font.MeasureString(text).Y),
                        font, Color.Black, TextAlignment.Center, TextAlignment.Center)[0];
                }

                return caption;
            }
        }

        /// <summary>
        /// Gets the score entries for this <see cref="HighScoresBox"/>.
        /// </summary>
        /// <value>The score entries for this <see cref="HighScoresBox"/>.</value>
        private TextObject[] ScoreEntries
        {
            get {
                if (scoreEntries == null) {
                    WizzleGameHost gameHost = GameHost as WizzleGameHost;
                    SpriteFont font = gameHost.Fonts["DefaultRegular18"];

                    int areaHeight = (int) (WizzleGameHost.MaxScoreEntries * ContentControl.GetMaxTextHeight(font));
                    int separatorHeight = (okButton.BoundingBox.Top - Caption.BoundingBox.Bottom - areaHeight) / 2;

                    Rectangle area = new Rectangle(
                        BoundingBox.Left + Margin.Left, Caption.BoundingBox.Bottom + separatorHeight,
                        BoundingBox.Width - Margin.Left - Margin.Right, areaHeight);

                    scoreEntries = CreateScoreEntries(highScoreTable, font, area, gameHost.LastScore);
                }

                return scoreEntries;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Releases the unmanaged resources used by this <see cref="HighScoresBox"/>
        /// and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release unmanaged resources only.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed) {
                if (disposing) {
                    // Release managed resources
                    caption.Dispose();
                    caption = null;

                    foreach (TextObject textObject in scoreEntries) { textObject.Dispose(); }
                    scoreEntries = null;

                    okButton.Dispose();
                    okButton = null;
                }

                // Release unmanaged resources
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Draws this <see cref="HighScoresBox"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        /// <param name="spriteBatch">The <b>SpriteBatch</b> that groups the sprites to be drawn.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Caption.Draw(gameTime, spriteBatch);
            foreach (TextObject scoreEntry in ScoreEntries) { scoreEntry.Draw(gameTime, spriteBatch); }

            base.Draw(gameTime, spriteBatch);
        }

        /// <summary>
        /// Creates the score entries for this <see cref="HighScoresBox"/>.
        /// </summary>
        /// <param name="highScoreTable">The score entries table.</param>
        /// <param name="font">The score entries font.</param>
        /// <param name="area">The score entries area.</param>
        /// <param name="highlightEntry">The score entry to highlight.</param>
        /// <returns></returns>
        private TextObject[] CreateScoreEntries(HighScoreTable highScoreTable, SpriteFont font, Rectangle area, HighScoreEntry highlightEntry)
        {
            int entryCount = highScoreTable.Entries.Count;
            float y;
            float textHeight = ContentControl.GetMaxTextHeight(font);
            float positionLength = font.MeasureString(highScoreTable.Entries.Count.ToString() + ". ").X;
            List<TextObject> tableEntries = new List<TextObject>(highScoreTable.Entries.Count);
            TextObject textObject;
            Color entryColor;
            string date = null, score = null;

            for (int i = 0; i < entryCount; i++) {
                y = area.Top + (textHeight * i);

                if (highScoreTable.Entries[i] == highlightEntry) {
                    entryColor = Color.Red;
                } else {
                    entryColor = new Color(Vector3.Lerp(Color.MediumBlue.ToVector3(), Color.DeepSkyBlue.ToVector3(), (float) i / entryCount));
                }

                if (!String.IsNullOrEmpty(highScoreTable.Entries[i].Name)) {
                    date = highScoreTable.Entries[i].DateTime.ToString("d");
                    score = highScoreTable.Entries[i].Score.ToString();
                } else {
                    date = Resources.NA;
                    score = "0";
                }

                textObject = new TextObject(GameHost, font, new Vector2(area.Left, y), (i + 1).ToString() + ".");
                textObject.Color = entryColor;
                tableEntries.Add(textObject);

                textObject = new TextObject(GameHost, font, new Vector2(area.Left + positionLength, y), date);
                textObject.Color = entryColor;
                tableEntries.Add(textObject);

                textObject = new TextObject(GameHost, font, new Vector2(GameHost.Window.ClientBounds.Right - area.Left, y), score, TextAlignment.Far, TextAlignment.Near);
                textObject.Color = entryColor;
                tableEntries.Add(textObject);
            }

            return tableEntries.ToArray();
        }

        /// <summary>
        /// Handles the <see cref="Window.Shown"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnShown(EventArgs args)
        {
            string tableName = String.Format(Resources.MatrixSize, gameSession.Puzzle.Width, gameSession.Puzzle.Height);
            highScoreTable = GameHost.HighScores.GetTable(tableName);

            okButton.Show();
            base.OnShown(args);
        }

        /// <summary>
        /// Handles the <see cref="Window.Hidden"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnHidden(EventArgs args)
        {
            okButton.Hide();
            base.OnHidden(args);
        }

        /// <summary>
        /// Handles the <see cref="Window.Closed"/> event.
        /// </summary>
        /// <param name="args">The event data.</param>
        protected override void OnClosed(EventArgs args)
        {
            okButton.Dispose();
            base.OnClosed(args);
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Button"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick(object sender, EventArgs args)
        {
            Close();
        }
        #endregion
    }
}
