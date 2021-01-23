#region Header
//+ <source name="Dashboard.cs" language="C#" begin="10-Aug-2012">
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Phone.Tasks;
using PlaXore.GameFramework;
using PlaXore.GameFramework.Controls;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// 
    /// </summary>
    class Dashboard : Control
    {
        #region Fields
        private AdsManager adsManager;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Dashboard"/> class
        /// with the specified game host.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        public Dashboard(GameHost gameHost)
            : base(gameHost, Vector2.Zero, gameHost.Textures["DashboardBackground"])
        {
            LayerDepth = 0.9f;
            Click += OnClick;

            if (GameHost.License.IsTrial) {
                adsManager = new AdsManager(GameHost, new Vector2((float) Texture.Width, (float) Texture.Height));
                GameHost.License.Bought += OnBought;
            }
        }
        #endregion

        #region Finalizer
        /// <summary>
        /// Frees the resources used by this <see cref="Dashboard"/>.
        /// </summary>
        ~Dashboard()
        {
            Dispose(false);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Releases the unmanaged resources used by this <see cref="Dashboard"/>
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
                    if (adsManager != null) {
                        adsManager.Dispose();
                    }
                }

                // Release unmanaged resources
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Updates this <see cref="Dashboard"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (adsManager != null) {
                adsManager.Update(gameTime);
                Texture = adsManager.CurrentAd != null ? adsManager.CurrentAd : GameHost.Textures["DashboardBackground"];
            }
        }

        /// <summary>
        /// Handles the <see cref="License.Bought"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="License"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnBought(object sender, EventArgs args)
        {
            adsManager.Dispose();
            adsManager = null;

            Texture = GameHost.Textures["DashboardBackground"];
            GameHost.License.Bought -= OnBought;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Dashboard"/> that generated the event.</param>
        /// <param name="args">The event data.</param>
        private void OnClick(object sender, EventArgs args)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();

            if (adsManager != null && adsManager.Uri != null) {
                webBrowserTask.Uri = adsManager.Uri;
                webBrowserTask.Show();
            } else {
                webBrowserTask.Uri = new Uri("http://www.agamura.com");
                webBrowserTask.Show();
            }
        }
        #endregion
    }
}
