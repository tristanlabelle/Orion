using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Xunit;

namespace Orion.Tests
{
    /// <summary>
    /// Tests the <see cref="Sequence"/> class.
    /// </summary>
    public sealed class SequenceTests
    {
        [Fact]
        public static void TestNonDeferred()
        {
            List<int> values = Enumerable.Range(2, 16).ToList();
            List<int> originalValuesCopy = values.ToList();

            var nonDeferred = values.NonDeferred();

            values[0] = 1;
            values.Add(5);

            Assert.True(nonDeferred.SequenceEqual(originalValuesCopy));
        }
    }
}
