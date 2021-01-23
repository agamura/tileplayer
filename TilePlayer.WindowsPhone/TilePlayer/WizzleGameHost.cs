#region Header
//+ <source name="WizzleGameHost.cs" language="C#" begin="12-Nov-2011">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2011">
//+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Info;
using PlaXore;
using PlaXore.ShakeGestures;
using PlaXore.GameFramework;
using PlaXore.GameFramework.Controls;
using Wizzle.Engine;
using Puzzle = Wizzle.Engine.Matrix;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Implements game logic and provides rendering code. 
    /// </summary>
    public class WizzleGameHost : GameHost
    {
        #region Fields
        private const int SplashBoxMinShowTime = 3000;
        private const int MinimumRequiredMovesForShake = 5;

        internal const int TileNumbersCheatId = 1;
        internal const int DisturbingElementsHidingCheatId = 2;
        internal const int DisturbingElementsImmunityCheatId = 3;
        internal const int MaxScoreEntries = 6;

        private SpriteBatch spriteBatch;
        private List<Wizzle.Engine.Gamer> gamers;
        private LocalGamer localGamer;
        private Dashboard dashboard;
        private ScrollMenu menu;
        private WindowManager windowsManager;
        private bool? disturbingElementsEnabled;
        private bool? disturbingElementsTerminable;
        private bool? isMusicEnabled;
        private bool? isSoundEnabled;
        private bool? isVibrateEnabled;
        private bool? tileNumbersEnabled;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WizzleGameHost"/> class.
        /// </summary>
        public WizzleGameHost()
        {
            gamers = new List<Wizzle.Engine.Gamer>();

            // Create the GrapicsDeviceManager responsible for creating the
            // GraphicsDevice used used to render graphics to the screen
            GraphicsDeviceManager graphicsDeviceManager = new GraphicsDeviceManager(this);
            graphicsDeviceManager.PreferredBackBufferWidth = GraphicsDeviceManager.DefaultBackBufferHeight;
            graphicsDeviceManager.PreferredBackBufferHeight = GraphicsDeviceManager.DefaultBackBufferWidth;

            // Activate shake gestures
            ShakeGesturesHelper.Instance.ShakeGesture += new EventHandler<ShakeGestureEventArgs>(OnShakeGesture);
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = MinimumRequiredMovesForShake;
            ShakeGesturesHelper.Instance.IsActive = true;

            Content.RootDirectory = "Content";
            TargetElapsedTime = TimeSpan.FromSeconds(1f / 30);

            GameState.Update = UpdateUndefined;
            GameInput.AddGamePadInput(this, Buttons.Back, false);

            if (License.IsTrial) {
                License.Bought += OnBought;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether or not disturbing elements
        /// are enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if disturbing elements are enabled; otherwise,
        /// <see langword="false"/>.
        /// </value>
        internal bool DisturbingElementsEnabled
        {
            get {
                if (!disturbingElementsEnabled.HasValue) {
                    disturbingElementsEnabled = SettingsManager.GetValue("DisturbingElementsEnabled", true);
                }

                return disturbingElementsEnabled.Value;
            }
            set {
                disturbingElementsEnabled = value;
                SettingsManager.SetValue("DisturbingElementsEnabled", value);
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
        internal bool DisturbingElementsTerminable
        {
            get {
                if (!disturbingElementsTerminable.HasValue) {
                    disturbingElementsTerminable = SettingsManager.GetValue("DisturbingElementsTerminable", true);
                }

                return disturbingElementsTerminable.Value;
            }
            set {
                disturbingElementsTerminable = value;
                SettingsManager.SetValue("DisturbingElementsTerminable", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not music is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if music is enabled; otherwise, <see langword="false"/>.
        /// </value>
        internal bool IsMusicEnabled
        {
            get {
                if (!isMusicEnabled.HasValue) {
                    isMusicEnabled = SettingsManager.GetValue("IsMusicEnabled", true);
                }

                return isMusicEnabled.Value;
            }
            set {
                isMusicEnabled = value;
                SettingsManager.SetValue("IsMusicEnabled", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not sound is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if sound is enabled; otherwise, <see langword="false"/>.
        /// </value>
        internal bool IsSoundEnabled
        {
            get {
                if (!isSoundEnabled.HasValue) {
                    isSoundEnabled = SettingsManager.GetValue("IsSoundEnabled", true);
                }

                return isSoundEnabled.Value;
            }
            set {
                isSoundEnabled = value;
                SettingsManager.SetValue("IsSoundEnabled", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not vibrate is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if vibrate is enabled; otherwise, <see langword="false"/>.
        /// </value>
        internal bool IsVibrateEnabled
        {
            get {
                if (!isVibrateEnabled.HasValue) {
                    isVibrateEnabled = SettingsManager.GetValue("IsVibrateEnabled", true);
                }

                return isVibrateEnabled.Value;
            }
            set {
                isVibrateEnabled = value;
                SettingsManager.SetValue("IsVibrateEnabled", value);
            }
        }

        /// <summary>
        /// Gets the <see cref="GameSession"/> associated with this
        /// <see cref="WizzleGameHost"/>.
        /// </summary>
        /// <value>
        /// The <see cref="GameSession"/> associated with this <see cref="WizzleGameHost"/>.
        /// </value>
        internal GameSession GameSession
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the last score of the <see cref="LocalGamer"/>.
        /// </summary>
        /// <value>The last score of the <see cref="LocalGamer"/>.</value>
        internal HighScoreEntry LastScore
        {
            get {
                string tableName = String.Format(Resources.MatrixSize, GameSession.Puzzle.Width, GameSession.Puzzle.Height);

                if (HighScores.GetTable(tableName) != null) {
                    DateTime dateTime = SettingsManager.GetValue(tableName, DateTime.MinValue);

                    if (dateTime > DateTime.MinValue) {
                        foreach (HighScoreEntry highScoreEntry in HighScores.GetTable(tableName).Entries) {
                            if (highScoreEntry.DateTime == dateTime) {
                                return highScoreEntry;
                            }
                        }
                    }
                }

                return null;
            }
            set {
                string tableName = String.Format(Resources.MatrixSize, GameSession.Puzzle.Width, GameSession.Puzzle.Height);

                if (value != null) {
                    SettingsManager.SetValue(tableName, value.DateTime);
                } else {
                    SettingsManager.DeleteValue(tableName);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="LocalGameBoard"/> associated with this
        /// <see cref="WizzleGameHost"/>.
        /// </summary>
        /// <value>
        /// The <see cref="LocalGameBoard"/> associated with this <see cref="WizzleGameHost"/>.
        /// </value>
        internal LocalGameBoard LocalGameBoard
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="LocalGamer"/> that controls this <see cref="WizzleGameHost"/>.
        /// </summary>
        /// <value>
        /// The <see cref="LocalGamer"/> that controls this <see cref="WizzleGameHost"/>.
        /// </value>
        internal LocalGamer LocalGamer
        {
            get {
                if (localGamer == null) {
                    string localGamerId = SettingsManager.GetValue("LocalGamerId", null);
                    string gamertag = SettingsManager.GetValue("Gamertag", DeviceExtendedProperties.GetValue("DeviceName").ToString());

                    localGamer = new LocalGamer(localGamerId != null ? Guid.Parse(localGamerId) : Guid.NewGuid(), gamertag);
                    localGamer.IsSignedUp = localGamerId != null;
                }

                return localGamer;
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
        internal bool TileNumbersEnabled
        {
            get {
                if (!tileNumbersEnabled.HasValue) {
                    tileNumbersEnabled = SettingsManager.GetValue("TileNumbersEnabled", false);

                }

                return tileNumbersEnabled.Value;
            }
            set {
                tileNumbersEnabled = value;
                SettingsManager.SetValue("TileNumbersEnabled", value);
            }
        }

        /// <summary>
        /// Gets the <see cref="WindowManager"/> that manages the
        /// <see cref="Window"/> instances of this <see cref="WizzleGameHost"/>.
        /// </summary>
        /// <value>
        /// The <see cref="WindowManager"/> that manages the <see cref="Window"/>
        /// instances of this <see cref="WizzleGameHost"/>.
        /// </value>
        internal WindowManager WindowManager
        {
            get {
                if (windowsManager == null) {
                    windowsManager = new WindowManager(this, Rectangle.Empty);
                }

                return windowsManager;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Performs any initialization needed before starting to run. <b>Initialize</b>
        /// is called before <b>LoadContent</b>.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Called when graphics resources need to be loaded.
        /// </summary>
        protected override void LoadContent()
        {
            // Create the SpriteBatch used to draw textures
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Backgrounds
            Textures.Add("DashboardBackground", Content.Load<Texture2D>("Backgrounds/dashboard"));
            Textures.Add("GameInfoBackground", Content.Load<Texture2D>("Backgrounds/game_info"));
            Textures.Add("HorizontalMenuBackground", Content.Load<Texture2D>("Backgrounds/horizontal_menu"));
            Textures.Add("SplashScreenBackground", Content.Load<Texture2D>("Backgrounds/splash_screen"));
            if (License.IsTrial) {
                Textures.Add("AboutBoxTrialBackground", Content.Load<Texture2D>("Backgrounds/about_box_trial"));
            }
            Textures.Add("AboutBoxBackground", Content.Load<Texture2D>("Backgrounds/about_box"));
            Textures.Add("PublishScoreBoxBackground", Content.Load<Texture2D>("Backgrounds/publish_score_box"));
            Textures.Add("HighScoresBoxBackground", Content.Load<Texture2D>("Backgrounds/high_scores_box"));
            Textures.Add("BuyBoxBackground", Content.Load<Texture2D>("Backgrounds/buy_box"));
            Textures.Add("MessageBoxBackground", Content.Load<Texture2D>("Backgrounds/message_box"));

            // Buttons
            Textures.Add("OkButton", Content.Load<Texture2D>("Buttons/ok"));
            Textures.Add("CancelButton", Content.Load<Texture2D>("Buttons/cancel"));

            // Menu items
            Textures.Add("MatrixMenuItem", Content.Load<Texture2D>("MenuItems/matrix"));
            Textures.Add("MatrixWithNumbersMenuItem", Content.Load<Texture2D>("MenuItems/matrix_with_numbers"));
            Textures.Add("PlayMenuItem", Content.Load<Texture2D>("MenuItems/play"));
            Textures.Add("PauseMenuItem", Content.Load<Texture2D>("MenuItems/pause"));
            Textures.Add("StopMenuItem", Content.Load<Texture2D>("MenuItems/stop"));
            Textures.Add("DisturbingElementMenuItem", Content.Load<Texture2D>("MenuItems/scorpion"));
            Textures.Add("HighScoresMenuItem", Content.Load<Texture2D>("MenuItems/high_scores"));
            if (License.IsTrial) {
                Textures.Add("BuyMenuItem", Content.Load<Texture2D>("MenuItems/buy"));
            }
            Textures.Add("SettingsMenuItem", Content.Load<Texture2D>("MenuItems/settings"));
            Textures.Add("InfoMenuItem", Content.Load<Texture2D>("MenuItems/info"));

            // Matrix menu items
            Textures.Add("3x3MenuItem", Content.Load<Texture2D>("MenuItems/3x3"));
            Textures.Add("4x4MenuItem", Content.Load<Texture2D>("MenuItems/4x4"));
            Textures.Add("5x5MenuItem", Content.Load<Texture2D>("MenuItems/5x5"));

            // Settings menu items
            Textures.Add("SoundOnMenuItem", Content.Load<Texture2D>("MenuItems/sound_on"));
            Textures.Add("SoundOffMenuItem", Content.Load<Texture2D>("MenuItems/sound_off"));
            Textures.Add("MusicOnMenuItem", Content.Load<Texture2D>("MenuItems/music_on"));
            Textures.Add("MusicOffMenuItem", Content.Load<Texture2D>("MenuItems/music_off"));
            Textures.Add("VibrateOnMenuItem", Content.Load<Texture2D>("MenuItems/vibrate_on"));
            Textures.Add("VibrateOffMenuItem", Content.Load<Texture2D>("MenuItems/vibrate_off"));

            // Disturbing elements
            Textures.Add("WalkingScorpionGhost", Content.Load<Texture2D>("DisturbingElements/walking_scorpion_ghost"));
            Textures.Add("BurstingScorpionGhost", Content.Load<Texture2D>("DisturbingElements/bursting_scorpion_ghost"));

            // Fonts
            Fonts.Add("DefaultRegular10", Content.Load<SpriteFont>("Fonts/default_regular_10"));
            Fonts.Add("DefaultRegular12", Content.Load<SpriteFont>("Fonts/default_regular_12"));
            Fonts.Add("DefaultRegular14", Content.Load<SpriteFont>("Fonts/default_regular_14"));
            Fonts.Add("DefaultRegular18", Content.Load<SpriteFont>("Fonts/default_regular_18"));
            Fonts.Add("DefaultRegular22", Content.Load<SpriteFont>("Fonts/default_regular_22"));
            Fonts.Add("DefaultRegular28", Content.Load<SpriteFont>("Fonts/default_regular_28"));
            Fonts.Add("DefaultBold10", Content.Load<SpriteFont>("Fonts/default_bold_10"));
            Fonts.Add("DefaultBold14", Content.Load<SpriteFont>("Fonts/default_bold_14"));
            Fonts.Add("DefaultBold22", Content.Load<SpriteFont>("Fonts/default_bold_22"));
            Fonts.Add("DefaultSemibold12", Content.Load<SpriteFont>("Fonts/default_semibold_12"));
            Fonts.Add("VerdanaRegular14", Content.Load<SpriteFont>("Fonts/verdana_regular_14"));

            // Songs
            Songs.Add("MenuSong", Content.Load<Song>("Music/sahrawi_rock_001"));
            Songs.Add("GameSong", Content.Load<Song>("Music/sahrawi_rock_001"));

            // Sound effects
            SoundEffects.Add("MoveSound", Content.Load<SoundEffect>("Sounds/move"));
            SoundEffects.Add("ScrambleSound", Content.Load<SoundEffect>("Sounds/scramble"));
            SoundEffects.Add("SolvedSound", Content.Load<SoundEffect>("Sounds/solved"));
            SoundEffects.Add("GameOverSound", Content.Load<SoundEffect>("Sounds/game_over"));
            SoundEffects.Add("ScorpionBurstSound", Content.Load<SoundEffect>("Sounds/scorpion_burst"));
        }

        /// <summary>
        /// Called when graphics resources need to be unloaded.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Called when this <see cref="WizzleGameHost"/> is being activated.
        /// </summary>
        protected override void GameActivated()
        {
            if (GameState.Current == GameStateId.Running) {
                if (GameSession.IsActive) {
                    foreach (Wizzle.Engine.Gamer gamer in gamers) {
                        GameSession[gamer].Stopwatch.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Called when this <see cref="WizzleGameHost"/> is being deactivated.
        /// </summary>
        protected override void GameDeactivated()
        {
            foreach (Wizzle.Engine.Gamer gamer in gamers) {
                GameSession[gamer].Stopwatch.Stop();
            }
        }

        /// <summary>
        /// Called when the game determines it is time to draw a frame.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Draw</b>.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            DrawSprites(gameTime, spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Called when the game has determined that game logic needs to be processed.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        protected override void Update(GameTime gameTime)
        {
            BeginUpdate(gameTime);

            // Allow the game to exit
            if (GameInput.IsPressed(this, PlayerIndex.One)) {
                if (WindowManager.ActiveWindow != null) {
                    WindowManager.ActiveWindow.Close();
                } else if (GameState.Current == GameStateId.Running) {
                    GameState.Update = UpdatePaused;
                } else {
                    Exit();
                }
            }

            EndUpdate(gameTime);
        }

        private int UpdateUndefined(GameTime gameTime)
        {
            (WindowManager["TilePlayer.SplashScreen"] as SplashScreen).MinShowTime = SplashBoxMinShowTime;
            WindowManager["TilePlayer.SplashScreen"].Show();

            GameState.Update = UpdateLoading;
            return GameStateId.Undefined;
        }

        private int UpdateLoading(GameTime gameTime)
        {
            CreateDashboard();
            CreateMenu();
            CreateGameSession();

            HighScores.InitializeTable(String.Format(Resources.MatrixSize, 3, 3), MaxScoreEntries);
            HighScores.InitializeTable(String.Format(Resources.MatrixSize, 4, 4), MaxScoreEntries);
            HighScores.InitializeTable(String.Format(Resources.MatrixSize, 5, 5), MaxScoreEntries);
            HighScores.Load();

            LocalGameBoard.ResetDisturbingElements();

            GameState.Update = UpdateLoaded;
            return GameStateId.Loading;
        }

        private int UpdateLoaded(GameTime gameTime)
        {
            if ((WindowManager["TilePlayer.SplashScreen"] as SplashScreen).CanClose) {
                if (!LocalGamer.IsSignedUp) {
                    SignUp();
                } else {
                    if (MediaPlayer.GameHasControl) {
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.IsMuted = !IsMusicEnabled;
                        MediaPlayer.Play(Songs["MenuSong"]);
                    }

                    WindowManager["TilePlayer.SplashScreen"].Close();
                    GameState.Update = UpdateStarting;
                }
            }

            return GameStateId.Loaded;
        }

        private int UpdateStarting(GameTime gameTime)
        {
            if (License.IsTrial) {
                (WindowManager["TilePlayer.BuyBox"] as BuyBox).GameplayInterrupted = false;
                WindowManager["TilePlayer.BuyBox"].Show();
            }

            TouchIndicator.Enable();
            TouchIndicator.Show();

            menu.ScrollTest();

            GameState.Update = UpdateStarted;
            return GameStateId.Starting;
        }

        private int UpdateStarted(GameTime gameTime)
        {
            if (!WindowManager["TilePlayer.BuyBox"].IsVisible) {
                WindowManager.Clear();
                GameState.Update = UpdateReady;
            }

            return GameStateId.Started;
        }

        private int UpdateReady(GameTime gameTime)
        {
            switch (GameState.Current) {
                case GameStateId.Ready:
                    if (LocalGameBoard.IsActive) {
                        GameSession.Reset(null);
                        LocalGameBoard.Deactivate(false);

                        if (LocalGameBoard.DisturbingElementsEnabled) {
                            LocalGameBoard.ResetDisturbingElements();
                        }

                        // Switch on sensors to allow shaking
                        ShakeGesturesHelper.Instance.IsActive = true;
                    }
                    break;
                case GameStateId.Running:
                case GameStateId.Paused:
                    GameSession.Reset(null);
                    LocalGameBoard.Deactivate(false);

                    if (LocalGameBoard.DisturbingElementsEnabled) {
                        LocalGameBoard.ResetDisturbingElements();
                    }

                    // Switch on sensors to allow shaking
                    ShakeGesturesHelper.Instance.IsActive = true;
                    break;
                default:
                    break;
            }

            return GameStateId.Ready;
        }

        private int UpdateRunning(GameTime gameTime)
        {
            switch (GameState.Current) {
                case GameStateId.Ready:
                    GameSession.Reset(null);
                    GameSession.Puzzle.Scramble();
                    LocalGameBoard.Game.Stopwatch.Start();
                    // Switch off sensors to save battery
                    ShakeGesturesHelper.Instance.IsActive = false;
                    LocalGameBoard.Activate();
                    break;
                case GameStateId.Paused:
                    if (LocalGameBoard.Game.Puzzle.IsSolved) {
                        LocalGameBoard.Activate();
                        GameState.Update = UpdateReady;
                    } else {
                        LocalGameBoard.Game.Stopwatch.Start();
                        // Switch off sensors to save battery
                        ShakeGesturesHelper.Instance.IsActive = false;
                        LocalGameBoard.Activate();
                    }
                    break;
                default:
                    GameSession.Update();

                    if (LocalGameBoard.Game.Puzzle.IsSolved && LocalGameBoard.Game.Counter.Current > 0) {
                        if (IsSoundEnabled) { SoundEffects["SolvedSound"].Play(); }
                        (WindowManager["TilePlayer.PublishScoreBox"] as PublishScoreBox).Show();
                        GameState.Update = UpdateGameOver;
                    } else if (!LocalGameBoard.Game.Puzzle.IsSolved && LocalGameBoard.Game.Counter.Current > 0 && LocalGameBoard.Game.Score < 1.0) {
                        LocalGameBoard.Game.Stopwatch.Stop();
                        if (IsSoundEnabled) { SoundEffects["GameOverSound"].Play(); }
                        (WindowManager["TilePlayer.MessageBox"] as MessageBox).Caption = Resources.StatusGameOver;
                        (WindowManager["TilePlayer.MessageBox"] as MessageBox).Message = Resources.MessageGameOver;
                        (WindowManager["TilePlayer.MessageBox"] as MessageBox).Show();

                        GameSession.Reset(null);
                        LocalGameBoard.Deactivate(false);
                        GameState.Update = UpdateGameOver;
                    }
                    break;
            }

            return GameStateId.Running;
        }

        private int UpdatePaused(GameTime gameTime)
        {
            switch (GameState.Current) {
                case GameStateId.Ready:
                    if (LocalGameBoard.IsActive) {
                        LocalGameBoard.Deactivate(true);
                        // Switch on sensors to allow shaking
                        ShakeGesturesHelper.Instance.IsActive = true;
                    }
                    break;
                case GameStateId.Running:
                    LocalGameBoard.Game.Stopwatch.Stop();
                    LocalGameBoard.Deactivate(true);
                    // Switch on sensors to allow shaking
                    ShakeGesturesHelper.Instance.IsActive = true;
                    break;
                default:
                    break;
            }

            return GameStateId.Paused;
        }

        private int UpdateGameOver(GameTime gameTime)
        {
            /*
            if (WindowManager.ActiveWindow == null) {
                LocalGameBoard.ResetDisturbingElements();
                ShakeGesturesHelper.Instance.IsActive = true;
                GameState.Update = UpdateReady;
            }
            */

            return GameStateId.GameOver;
        }

        /// <summary>
        /// Creates the game dashboard.
        /// </summary>
        private void CreateDashboard()
        {
            dashboard = new Dashboard(this);
            dashboard.Show();
        }

        /// <summary>
        /// Creates the game menu.
        /// </summary>
        private void CreateMenu()
        {
            Texture2D texture = Textures["HorizontalMenuBackground"];
            Rectangle dockRectangle = new Rectangle(0, 0,
                GraphicsDevice.PresentationParameters.Bounds.Width,
                GraphicsDevice.PresentationParameters.Bounds.Height);

            menu = new ScrollMenu(this, dockRectangle, texture);
            menu.DockStyle = DockStyle.Bottom;
            menu.IsCircular = true;
            menu.SnapMenuItems = false;
            menu.LayerDepth = 1f;
            menu.MenuItems.ListChanged += OnMenuItemCollectionChanged;
            
            menu.MenuItems.Add(new MenuItem(this,
                TileNumbersEnabled ? Textures["MatrixMenuItem"] : Textures["MatrixWithNumbersMenuItem"], "MatrixMenuItem",
                License.IsTrial ? null : new MenuItem[] {
                    new MenuItem(this, Textures["3x3MenuItem"], "3x3MenuItem"),
                    new MenuItem(this, Textures["4x4MenuItem"], "4x4MenuItem"),
                    new MenuItem(this, Textures["5x5MenuItem"], "5x5MenuItem")
                }));
            menu.MenuItems["MatrixMenuItem"].Click += OnClick_MatrixMenuItem;
            if (!License.IsTrial) {
                menu.MenuItems["MatrixMenuItem"].MenuItems["3x3MenuItem"].Click += OnClick_3x3MenuItem;
                menu.MenuItems["MatrixMenuItem"].MenuItems["4x4MenuItem"].Click += OnClick_4x4MenuItem;
                menu.MenuItems["MatrixMenuItem"].MenuItems["5x5MenuItem"].Click += OnClick_5x5MenuItem;
            }

            menu.MenuItems.Add(new MenuItem(this, Textures["PlayMenuItem"], "PlayMenuItem"));
            menu.MenuItems["PlayMenuItem"].Click += OnClick_PlayMenuItem;

            menu.MenuItems.Add(new MenuItem(this, Textures["PauseMenuItem"], "PauseMenuItem"));
            menu.MenuItems["PauseMenuItem"].Click += OnClick_PauseMenuItem;

            menu.MenuItems.Add(new MenuItem(this, Textures["StopMenuItem"], "StopMenuItem"));
            menu.MenuItems["StopMenuItem"].Click += OnClick_StopMenuItem;

            menu.MenuItems.Add(new MenuItem(this, Textures["DisturbingElementMenuItem"], "DisturbingElementMenuItem"));
            menu.MenuItems["DisturbingElementMenuItem"].Click += OnClick_DisturbingElementMenuItem;
            menu.MenuItems["DisturbingElementMenuItem"].Hold += OnHold_DisturbingElementMenuItem;

            menu.MenuItems.Add(new MenuItem(this,
            License.IsTrial ? Textures["BuyMenuItem"] : Textures["HighScoresMenuItem"], "HighScoresMenuItem"));
            menu.MenuItems["HighScoresMenuItem"].Click += OnClick_HighScoresMenuItem;

            menu.MenuItems.Add(new MenuItem(this, Textures["SettingsMenuItem"], "SettingsMenuItem",
            new MenuItem[] {
            new MenuItem(this, IsSoundEnabled ? Textures["SoundOnMenuItem"] : Textures["SoundOffMenuItem"], "SoundMenuItem"),
            new MenuItem(this, IsMusicEnabled ? Textures["MusicOnMenuItem"] : Textures["MusicOffMenuItem"], "MusicMenuItem"),
            new MenuItem(this, IsVibrateEnabled ? Textures["VibrateOnMenuItem"] : Textures["VibrateOffMenuItem"], "VibrateMenuItem")
            }));

            menu.MenuItems["SettingsMenuItem"].MenuItems["SoundMenuItem"].Click += OnClick_SoundMenuItem;
            menu.MenuItems["SettingsMenuItem"].MenuItems["MusicMenuItem"].Click += OnClick_MusicMenuItem;
            menu.MenuItems["SettingsMenuItem"].MenuItems["VibrateMenuItem"].Click += OnClick_VibrateMenuItem;

            menu.MenuItems.Add(new MenuItem(this, Textures["InfoMenuItem"], "InfoMenuItem"));
            menu.MenuItems["InfoMenuItem"].Click += OnClick_InfoMenuItem;

            menu.Show();
        }

        /// <summary>
        /// Creates the game session and lets the local gamer join it.
        /// </summary>
        private void CreateGameSession()
        {
            GameSession = new GameSession(new Puzzle(4, 4));
            GameSession.GamerJoined += OnGamerJoined;
            GameSession.GamerLeft += OnGamerLeft;
            GameSession.Join(LocalGamer);
        }

        /// <summary>
        /// Handles the <see cref="Wizzle.Engine.GameSession.GamerJoined"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="GameSession"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnGamerJoined(object sender, GamerJoinedEventArgs args)
        {
            // Initialize cheats
            Cheat cheat = new Cheat(TileNumbersCheatId, Resources.TileNumbers, Resources.ShowTileNumbers);
            cheat.ElapseFactor = 5f;
            cheat.IncrementFactor = 4;
            GameSession[args.Gamer].Cheats.Add(cheat.Id, cheat);

            cheat = new Cheat(DisturbingElementsHidingCheatId, Resources.DisturbingElementsHiding, Resources.DisableDisturbingElements);
            cheat.ElapseFactor = 3f;
            cheat.IncrementFactor = 2;
            GameSession[args.Gamer].Cheats.Add(cheat.Id, cheat);

            cheat = new Cheat(DisturbingElementsImmunityCheatId, Resources.DisturbingElementsImmunity, Resources.DisableDisturbingElementsTemination);
            cheat.ElapseFactor = 1.5f;
            cheat.IncrementFactor = 2;
            GameSession[args.Gamer].Cheats.Add(cheat.Id, cheat);

            // Create game board
            GameBoard gameBoard = null;

            if (args.Gamer == LocalGamer) {
                Vector2 position = new Vector2(0,
                    (GraphicsDevice.PresentationParameters.Bounds.Height
                    - GraphicsDevice.PresentationParameters.Bounds.Width) / 2);

                gameBoard = LocalGameBoard = new LocalGameBoard(this,
                    position,
                    new Rectangle(0, 0,
                        GraphicsDevice.PresentationParameters.Bounds.Width,
                        GraphicsDevice.PresentationParameters.Bounds.Width),
                    GameSession[args.Gamer]);

                windowsManager.BackBufferRectangle = gameBoard.BoundingBox;

                // Disturbing elements loaded automatically at session startup
                gameBoard.DisturbingElementsEnabled = DisturbingElementsEnabled;
                gameBoard.DisturbingElementsTerminable = DisturbingElementsTerminable;
                gameBoard.TileNumbersEnabled = TileNumbersEnabled;

                // Show the game board
                gameBoard.Show();
            } else {
                gameBoard = new RemoteGameBoard(this,
                    dashboard.Position,
                    new Rectangle(0, 0,
                        dashboard.BoundingBox.Height,
                        dashboard.BoundingBox.Height),
                    GameSession[args.Gamer]);

                gameBoard.ResetDisturbingElements();
            }
            gameBoard.Deactivate(false);
            gameBoard.TileMoved += OnTileMoved;
            gameBoard.NoDisturbingElementsLeft += OnNoDisturbingElementsLeft;

            // Register gamer
            gamers.Add(args.Gamer);
        }

        /// <summary>
        /// Handles the <see cref="Wizzle.Engine.GameSession.GamerLeft"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="GameSession"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnGamerLeft(object sender, GamerLeftEventArgs args)
        {
            GameSession[args.Gamer].Cheats.Clear();
            gamers.Remove(args.Gamer);
        }

        /// <summary>
        /// Lets the <see cref="LocalGamer"/> sign up for this game.
        /// </summary>
        private void SignUp()
        {
            if (!Guide.IsVisible) {
                Guide.BeginShowKeyboardInput(PlayerIndex.One,
                    Resources.SignUp, Resources.Gamertag, LocalGamer.Gamertag,
                    new AsyncCallback(OnSignedUp), this);
            }
        }

        /// <summary>
        /// Called when the sign up operation completes.
        /// </summary>
        /// <param name="result">The result of the sign up operation.</param>
        private void OnSignedUp(IAsyncResult result)
        {
            if (result.IsCompleted) {
                string gamertag = Guide.EndShowKeyboardInput(result);

                if (gamertag == null) {
                    // Exit the game
                    // UnloadContent();
                    Exit();
                } else {
                    // Sign up and start playing
                    if (gamertag.Length > 0) {
                        LocalGamer.Gamertag = gamertag.Trim();
                    }

                    localGamer.IsSignedUp = true;
                    SettingsManager.SetValue("LocalGamerId", LocalGamer.Id.ToString());
                    SettingsManager.SetValue("Gamertag", LocalGamer.Gamertag);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="License.Bought"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="License"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnBought(object sender, EventArgs args)
        {
            menu.MenuItems["MatrixMenuItem"].MenuItems.Add(new MenuItem(this, Textures["3x3MenuItem"], "3x3MenuItem"));
            menu.MenuItems["MatrixMenuItem"].MenuItems["3x3MenuItem"].Click += OnClick_3x3MenuItem;

            menu.MenuItems["MatrixMenuItem"].MenuItems.Add(new MenuItem(this, Textures["4x4MenuItem"], "4x4MenuItem"));
            menu.MenuItems["MatrixMenuItem"].MenuItems["4x4MenuItem"].Click += OnClick_4x4MenuItem;

            menu.MenuItems["MatrixMenuItem"].MenuItems.Add(new MenuItem(this, Textures["5x5MenuItem"], "5x5MenuItem"));
            menu.MenuItems["MatrixMenuItem"].MenuItems["5x5MenuItem"].Click += OnClick_5x5MenuItem;

            menu.MenuItems["HighScoresMenuItem"].Texture = Textures["HighScoresMenuItem"];

            License.Bought -= OnBought;
        }

        /// <summary>
        /// Handles the <see cref="ObservableList&lt;T&gt;.ListChanged"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItemCollection"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnMenuItemCollectionChanged(object sender, ListChangedEventArgs<MenuItem> args)
        {
            switch (args.ListChangedType) {
                case ListChangedType.ItemAdded:
                case ListChangedType.ItemChanged:
                    args.Item.LayerDepth = 0.9f;
                    break;
            }
        }

        /// <summary>
        /// Handles the <c>ShakeGesture</c> event.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="PlaXore.ShakeGestures.ShakeGesturesHelper"/> that
        /// generated the event.
        /// </param>
        /// <param name="args">The event data.</param>
        private void OnShakeGesture(object sender, ShakeGestureEventArgs args)
        {
            GameState.Update = UpdateRunning;
        }

        /// <summary>
        /// Handles the <see cref="GameBoard.TileMoved"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="LocalGameBoard"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnTileMoved(object sender, TileMovedEventArgs args)
        {
            LocalGamer.AddMove(LocalGameBoard.Game.Puzzle.GetPosition(args.Tile.Order));
        }

        /// <summary>
        /// Handles the <see cref="GameBoard.OnNoDisturbingElementsLeft"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="LocalGameBoard"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnNoDisturbingElementsLeft(object sender, EventArgs args)
        {
            GameState.Update = UpdateRunning;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the info
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_InfoMenuItem(object sender, GameInputEventArgs args)
        {
            if (!WindowManager["TilePlayer.AboutBox"].IsVisible) {
                WindowManager["TilePlayer.AboutBox"].Show();
            } else {
                WindowManager["TilePlayer.AboutBox"].Close();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the high scores
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_HighScoresMenuItem(object sender, GameInputEventArgs args)
        {
            string windowName = License.IsTrial ? "TilePlayer.BuyBox" : "TilePlayer.HighScoresBox";

            if (!WindowManager[windowName].IsVisible) {
                WindowManager[windowName].Show();
            } else {
                WindowManager[windowName].Close();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the matrix
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_MatrixMenuItem(object sender, GameInputEventArgs args)
        {
            TileNumbersEnabled = LocalGameBoard.TileNumbersEnabled = !LocalGameBoard.TileNumbersEnabled;

            if (LocalGameBoard.TileNumbersEnabled) {
                menu.MenuItems["MatrixMenuItem"].Texture = Textures["MatrixMenuItem"];
            } else {
                menu.MenuItems["MatrixMenuItem"].Texture = Textures["MatrixWithNumbersMenuItem"];
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the play
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_PlayMenuItem(object sender, GameInputEventArgs args)
        {
            GameState.Update = UpdateRunning;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the pause
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_PauseMenuItem(object sender, GameInputEventArgs args)
        {
            GameState.Update = UpdatePaused;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the stop
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_StopMenuItem(object sender, GameInputEventArgs args)
        {
            GameState.Update = UpdateReady;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the disturbing
        /// elements <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_DisturbingElementMenuItem(object sender, GameInputEventArgs args)
        {
            if (GameState.Current != GameStateId.Paused) {
                DisturbingElementsEnabled = LocalGameBoard.DisturbingElementsEnabled = !LocalGameBoard.DisturbingElementsEnabled;
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Hold"/> event generated by the disturbing
        /// elements <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnHold_DisturbingElementMenuItem(object sender, GameInputEventArgs args)
        {
            if (GameState.Current != GameStateId.Paused) {
                if (args.GestureAge == 1) {
                    DisturbingElementsTerminable = LocalGameBoard.DisturbingElementsTerminable = !LocalGameBoard.DisturbingElementsTerminable;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the 3x3 matrix
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_3x3MenuItem(object sender, GameInputEventArgs args)
        {
            GameSession.Reset(new Puzzle(3, 3));
            GameState.Update = UpdateReady;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the 4x4 matrix
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_4x4MenuItem(object sender, GameInputEventArgs args)
        {
            GameSession.Reset(new Puzzle(4, 4));
            GameState.Update = UpdateReady;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the 5x5 matrix
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_5x5MenuItem(object sender, GameInputEventArgs args)
        {
            GameSession.Reset(new Puzzle(5, 5));
            GameState.Update = UpdateReady;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the sound
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_SoundMenuItem(object sender, GameInputEventArgs args)
        {
            IsSoundEnabled = !IsSoundEnabled;

            if (IsSoundEnabled) {
                menu.MenuItems["SettingsMenuItem"].MenuItems["SoundMenuItem"].Texture = Textures["SoundOnMenuItem"];
            } else {
                menu.MenuItems["SettingsMenuItem"].MenuItems["SoundMenuItem"].Texture = Textures["SoundOffMenuItem"];
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the music
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_MusicMenuItem(object sender, GameInputEventArgs args)
        {
            IsMusicEnabled = !IsMusicEnabled;
            if (MediaPlayer.GameHasControl) { MediaPlayer.IsMuted = !IsMusicEnabled; }

            if (IsMusicEnabled) {
                menu.MenuItems["SettingsMenuItem"].MenuItems["MusicMenuItem"].Texture = Textures["MusicOnMenuItem"];
            } else {
                menu.MenuItems["SettingsMenuItem"].MenuItems["MusicMenuItem"].Texture = Textures["MusicOffMenuItem"];
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event generated by the vibrate
        /// <see cref="MenuItem"/>.
        /// </summary>
        /// <param name="sender">The <see cref="MenuItem"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick_VibrateMenuItem(object sender, GameInputEventArgs args)
        {
            IsVibrateEnabled = !IsVibrateEnabled;

            if (IsVibrateEnabled) {
                menu.MenuItems["SettingsMenuItem"].MenuItems["VibrateMenuItem"].Texture = Textures["VibrateOnMenuItem"];
            } else {
                menu.MenuItems["SettingsMenuItem"].MenuItems["VibrateMenuItem"].Texture = Textures["VibrateOffMenuItem"];
            }
        }
        #endregion
    }
}
