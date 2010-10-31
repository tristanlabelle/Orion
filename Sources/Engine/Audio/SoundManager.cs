using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace Orion.Engine.Audio
{
    /// <summary>
    /// Loads sounds and caches them. Provides support for sound groups.
    /// </summary>
    public sealed class SoundManager : IDisposable
    {
        #region Fields
        private static readonly Regex groupNameRegex = new Regex(@"\A(.*?)(\.\d+)?\Z", RegexOptions.Compiled);
        private readonly ISoundContext context;
        private readonly DirectoryInfo directory;
        private readonly CancellationTokenSource canceller = new CancellationTokenSource();
        private readonly Dictionary<string, SoundGroup> groups = new Dictionary<string, SoundGroup>();
        #endregion

        #region Constructors
        public SoundManager(ISoundContext soundContext, string directoryPath)
        {
            Argument.EnsureNotNull(soundContext, "soundContext");
            Argument.EnsureNotNull(directoryPath, "directoryPath");

            this.context = soundContext;
            this.directory = new DirectoryInfo(directoryPath);

            Debug.Assert(directory.Exists, "The sounds directory does not exist.");

            if (directory.Exists)
            {
                var groups = directory.GetFiles("*.ogg", SearchOption.TopDirectoryOnly)
                        .Select(fileInfo => fileInfo.FullName)
                        .GroupBy(filePath => groupNameRegex.Match(Path.GetFileNameWithoutExtension(filePath)).Groups[1].Value)
                        .Select(group => new SoundGroup(group.Key,
                            group.Select(filePath => new Task<ISound>(() => soundContext.LoadSoundFromFile(filePath), canceller.Token))));
                foreach (SoundGroup group in groups)
                {
                    this.groups.Add(group.Name, group);

                    if (soundContext.IsSoundLoadingThreadSafe) group.Preload();
                }
            }
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
            if (!groups.TryGetValue(name, out group)) return null;

            if (!context.IsSoundLoadingThreadSafe) group.Load();
            return group.GetRandomSoundOrNull(random);
        }

        public void Dispose()
        {
            canceller.Cancel();
            foreach (SoundGroup group in groups.Values)
                group.Dispose();
            groups.Clear();
        }
        #endregion
    }
}
