#region Header
//+ <source name="Program.cs" language="C#" begin="16-Apr-2012">
//+ <author href="mailto:giuseppe.greco@agamura.com">Giuseppe Greco</author>
//+ <copyright year="2012">
//+ //+ <holder web="http://www.agamura.com">Agamura, Inc.</holder>
//+ </copyright>
//+ <legalnotice>All rights reserved.</legalnotice>
//+ </source>
#endregion

#region References
using System;
#endregion

namespace TilePlayer
{
#if WINDOWS || XBOX
    static class Program
    {
    #region Methods
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (WizzleGameHost gameHost = new WizzleGameHost()) {
                gameHost.Run();
            }
        }
        #endregion
    }
#endif
}
