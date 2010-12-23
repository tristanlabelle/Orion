using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Orion.Engine
{
    public class AssetsDirectory
    {
        #region Fields
        private readonly string assetsPath;
        #endregion

        #region Constructors
        public AssetsDirectory(string assetsPath)
        {
            Argument.EnsureNotNull(assetsPath, "assetsPath");
            this.assetsPath = assetsPath;
        }
        #endregion

        #region Properties
        public string AssetsPath
        {
            get { return assetsPath; }
        }
        #endregion

        #region Methods
        public IEnumerable<string> EnumerateFiles(string subdirectory, string searchPattern)
        {
            return Directory.GetFiles(GetDirectoryPath(subdirectory), searchPattern);
        }

        public IEnumerable<string> EnumerateFiles(string subdirectory, string searchPattern, SearchOption options)
        {
            return Directory.GetFiles(GetDirectoryPath(subdirectory), searchPattern, options);
        }

        public string GetDirectoryPath(string subdirectory)
        {
            return assetsPath + Path.DirectorySeparatorChar + subdirectory;
        }
        #endregion
    }
}
