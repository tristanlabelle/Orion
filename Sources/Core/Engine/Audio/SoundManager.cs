using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Orion.Engine.Audio
{
    /// <summary>
    /// Loads sounds and caches them. Provides support for sound groups.
    /// </summary>
    public sealed class SoundManager : IDisposable
    {
        #region Fields
        private readonly ISoundContext context;
        private readonly DirectoryInfo directory;
        private readonly Dictionary<string, SoundGroup> groups = new Dictionary<string, SoundGroup>();
        #endregion

        #region Constructors
        public SoundManager(ISoundContext soundContext, string directoryPath)
        {
            Argument.EnsureNotNull(soundContext, "soundContext");
            Argument.EnsureNotNull(directoryPath, "directoryPath");

            this.context = soundContext;
            this.directory = new DirectoryInfo(directoryPath);

            Debug.Assert(directory.Exists);
        }
        #endregion

        #region Methods
        private ISound TryLoadFromFile(string filePath)
        {
            try { return context.LoadSoundFromFile(filePath); }
            catch (IOException) { return null; }
        }

        public ISound GetRandom(string name, Random random)
        {
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureNotNull(random, "random");

            SoundGroup group;
            if (groups.TryGetValue(name, out group))
                return group.GetRandomSoundOrNull(random);

            try
            {
                var sounds = Directory.GetFiles(directory.FullName, name + "*.*")
                    .Where(filePath => Regex.IsMatch(Path.GetFileNameWithoutExtension(filePath), name + @"(\.\d+)?$"))
                    .Select(filePath => TryLoadFromFile(filePath))
                    .Where(sound => sound != null);
                group = new SoundGroup(name, sounds);
            }
            catch (IOException)
            {
                group = new SoundGroup(name, Enumerable.Empty<ISound>());
            }

            groups.Add(name, group);

            return group.GetRandomSoundOrNull(random);
        }

        public void Dispose()
        {
            foreach (SoundGroup group in groups.Values)
                group.Dispose();
            groups.Clear();
        }
        #endregion
    }
}
