using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine.Localization;
using Xunit;

namespace Orion.Tests
{
    /// <summary>
    /// Tests the localization classes.
    /// </summary>
    public sealed class LocalizationTests
    {
        [Fact]
        public void TestFailsWhenFileDoesNotExist()
        {
            Assert.Throws<FileNotFoundException>(() => new Localizer("fichierquiexistepas.xml"));
        }
    }
}
