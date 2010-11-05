using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Orion.Engine.Audio
{
    internal sealed class SoundGroup : IDisposable
    {
        #region Fields
        private readonly string name;
        private readonly ReadOnlyCollection<Task<ISound>> sounds;
        #endregion

        #region Constructors
        internal SoundGroup(string name, IEnumerable<Task<ISound>> sounds)
        {
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureNotNull(sounds, "sounds");

            this.name = name;
            this.sounds = sounds.ToList().AsReadOnly();
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }
        #endregion

        #region Methods
        public void Preload()
        {
            foreach (Task<ISound> soundTask in sounds)
                soundTask.Start();
        }

        public void Load()
        {
            foreach (Task<ISound> soundTask in sounds)
                if (soundTask.Status == TaskStatus.Created)
                    soundTask.RunSynchronously();
        }

        public ISound GetRandomSoundOrNull(Random random)
        {
            Argument.EnsureNotNull(random, "random");

            if (sounds.Count == 0) return null;

            int completedCount = sounds.Count(sound => sound.Status == TaskStatus.RanToCompletion);
            if (completedCount == 0) return null;

            int completedIndex = random.Next(completedCount);
            return sounds.Where(sound => sound.Status == TaskStatus.RanToCompletion)
                .ElementAt(completedIndex)
                .Result;
        }

        public void Dispose()
        {
            foreach (Task<ISound> soundTask in sounds)
            {
                soundTask.Wait();
                if (soundTask.Status == TaskStatus.RanToCompletion)
                    soundTask.Result.Dispose();
            }
        }
        #endregion
    }
}
