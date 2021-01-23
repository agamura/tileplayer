#region Header
//+ <source name="AdsManager.cs" language="C#" begin="24-Jun-2012">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2012">
//+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlaXore.GameFramework;
using SOMAWP7;
#endregion

namespace TilePlayer
{
    /// <summary>
    /// Provides functionality for managing dynamic advertising.
    /// </summary>
    class AdsManager : GameObject
    {
        #region Fields
        private const int SomaAdSpace = 65751645;   // DO NOT CHANGE THIS VALUE!
        private const int SomaAdPub = 923854069;    // DO NOT CHANGE THIS VALUE!

        private SomaAd somaAd;
        private string adImageFileName;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AdsManager"/> class with
        /// the specified game host and advertising size.
        /// </summary>
        /// <param name="gameHost">The game host.</param>
        /// <param name="size">The advertising size.</param>
        public AdsManager(GameHost gameHost, Vector2 size)
            : base(gameHost)
        {
            somaAd = new SomaAd();
            somaAd.Adspace = SomaAdSpace;
            somaAd.Pub = SomaAdPub;
            somaAd.AdSpaceHeight = (int) size.Y;
            somaAd.AdSpaceWidth = (int) size.X;
            somaAd.LocationUseOK = true;
            somaAd.GetAd();
        }
        #endregion

        #region Finalizer
        /// <summary>
        /// Frees the resources used by this <see cref="AdsManager"/>.
        /// </summary>
        ~AdsManager()
        {
            Dispose(false);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current advertising.
        /// </summary>
        /// <value>the current advertising.</value>
        /// <remarks>
        /// Call <see cref="AdsManager.Update(GameTime)"/> to update <see cref="AdsManager.CurrentAd"/>.
        /// </remarks>
        public Texture2D CurrentAd
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the link associated with <see cref="CurrentAd"/>.
        /// </summary>
        /// <value>
        /// The link associated with <see cref="CurrentAd"/>.
        /// </value>
        public Uri Uri
        {
            get { return String.IsNullOrEmpty(somaAd.Uri) ? null : new Uri(somaAd.Uri); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Releases the unmanaged resources used by this <see cref="AdsManager"/>
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
                }

                // Release unmanaged resources
                somaAd.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Updates <see cref="AdsManager.CurrentAd"/>.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to <b>Update</b>.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (somaAd.Status != "error" && somaAd.AdImageFileName != null && somaAd.ImageOK) {
                if (adImageFileName != somaAd.AdImageFileName) {
                    adImageFileName = somaAd.AdImageFileName;

                    try {
                        using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication()) {
                            using (IsolatedStorageFileStream isolatedStorageFileStream = new IsolatedStorageFileStream(somaAd.AdImageFileName, FileMode.Open, isolatedStorageFile)) {
                                CurrentAd = Texture2D.FromStream(GameHost.GraphicsDevice, isolatedStorageFileStream);
                            }
                        }
                    } catch (IsolatedStorageException e) {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Always throws <b>NotImplementedException</b>.
        /// </summary>
        public override bool IsPointInObject(Vector2 point)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
