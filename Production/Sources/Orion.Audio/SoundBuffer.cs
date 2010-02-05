using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK.Audio;
using System.Diagnostics;

namespace Orion.Audio
{
    internal sealed class SoundBuffer : IDisposable
    {
        #region Fields
        private readonly uint handle;
        private readonly string name;
        #endregion

        #region Constructors
        public SoundBuffer(string filePath)
        {
            Argument.EnsureNotNull(filePath, "filePath");

            handle = Alut.CreateBufferFromFile(filePath);
            Debug.Assert(handle != 0);
            name = Path.GetFileNameWithoutExtension(filePath);
        }
        #endregion

        #region Properties
        public uint Handle
        {
            get { return handle; }
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            uint handle = this.handle;
            AL.DeleteBuffer(ref handle);
            Debug.Assert(AL.GetError() == ALError.NoError);
        }
        #endregion
    }
}
