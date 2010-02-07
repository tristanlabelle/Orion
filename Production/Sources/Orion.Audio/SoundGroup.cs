using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Orion.Audio
{
    internal sealed class SoundGroup
    {
        #region Fields
        private readonly string name;
        private readonly ReadOnlyCollection<string> filePaths;
        #endregion

        #region Constructors
        public SoundGroup(string name)
        {
            Argument.EnsureNotNull(name, "name");

            this.name = name;
            this.filePaths = Directory.GetFiles("../../../Assets/Sounds/", name + ".*")
                .Where(path => AudioContext.SupportedFormats.Contains(Path.GetExtension(path))
                    && Regex.IsMatch(Path.GetFileNameWithoutExtension(path).Substring(name.Length), @"\A(\.\d+)?\Z"))
                .ToList()
                .AsReadOnly();
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }

        public ReadOnlyCollection<string> FilePaths
        {
            get { return filePaths; }
        }

        public int FileCount
        {
            get { return filePaths.Count; }
        }

        public bool IsEmpty
        {
            get { return filePaths.Count == 0; }
        }
        #endregion

        #region Methods
        public string GetRandomFilePath(Random random)
        {
            return filePaths[random.Next(filePaths.Count)];
        }

        public override string ToString()
        {
            return "{0} ({1} file(s))".FormatInvariant(name, filePaths.Count);
        }
        #endregion
    }
}
