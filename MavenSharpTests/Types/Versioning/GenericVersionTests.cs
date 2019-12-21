using Xunit;
using MavenArtifactDownloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace MavenArtifactDownloader.Tests
{
    public class GenericVersionTests
    {
        /**
         * The following tests are pulled from the official Apache documentation.
         * Docs: https://cwiki.apache.org/confluence/display/MAVENOLD/Versioning
         */

        [Theory()]
        [InlineData("1", "1", 0)]
        [InlineData("1", "2", -1)]
        [InlineData("1.5", "2", -1)]
        [InlineData("1", "2.5", -1)]
        [InlineData("1", "1.0", 0)]
        [InlineData("1", "1.0.0", 0)]
        [InlineData("1.0", "1.1", -1)]
        [InlineData("1.1", "1.2", -1)]
        [InlineData("1.0.0", "1.1", -1)]
        [InlineData("1.1", "1.2.0", -1)]
        [InlineData("1.0-alpha-1", "1.0", -1)]
        [InlineData("1.0-alpha-1", "1.0-alpha-2", -1)]
        [InlineData("1.0-alpha-1", "1.0-beta-1", -1)]
        [InlineData("1.0", "1.0-1", -1)]
        [InlineData("1.0-1", "1.0-2", -1)]
        [InlineData("2.0-0", "2.0", 0)]
        [InlineData("2.0", "2.0-1", -1)]
        [InlineData("2.0.0", "2.0-1", -1)]
        [InlineData("2.0-1", "2.0.1", -1)]
        [InlineData("2.0.1-klm", "2.0.1-lmn", -1)]
        [InlineData("2.0.1-xyz", "2.0.1", 1)]// Maven3 specifications altered ordering for this from -1 to 1
        [InlineData("2.0.1", "2.0.1-123", -1)]
        [InlineData("2.0.1-xyz", "2.0.1-123", -1)]
        // SNAPSHOT TESTS
        [InlineData("1-SNAPSHOT", "1-SNAPSHOT", 0)]
        [InlineData("1-SNAPSHOT", "2-SNAPSHOT", -1)]
        [InlineData("1.5-SNAPSHOT", "2-SNAPSHOT", -1)]
        [InlineData("1-SNAPSHOT", "2.5-SNAPSHOT", -1)]
        [InlineData("1-SNAPSHOT", "1.0-SNAPSHOT", 0)]
        [InlineData("1-SNAPSHOT", "1.0.0-SNAPSHOT", 0)]
        [InlineData("1.0-SNAPSHOT", "1.1-SNAPSHOT", -1)]
        [InlineData("1.1-SNAPSHOT", "1.2-SNAPSHOT", -1)]
        [InlineData("1.0.0-SNAPSHOT", "1.1-SNAPSHOT", -1)]
        [InlineData("1.1-SNAPSHOT", "1.2.0-SNAPSHOT", -1)]
        [InlineData("1.0-alpha-1-SNAPSHOT", "1.0-SNAPSHOT", -1)]
        [InlineData("1.0-alpha-1-SNAPSHOT", "1.0-alpha-2-SNAPSHOT", -1)]
        [InlineData("1.0-alpha-1-SNAPSHOT", "1.0-beta-1-SNAPSHOT", -1)]
        [InlineData("1.0-SNAPSHOT", "1.0-1-SNAPSHOT", -1)]
        [InlineData("1.0-1-SNAPSHOT", "1.0-2-SNAPSHOT", -1)]
        [InlineData("2.0-0-SNAPSHOT", "2.0-SNAPSHOT", 0)]
        [InlineData("2.0-SNAPSHOT", "2.0-1-SNAPSHOT", -1)]
        [InlineData("2.0.0-SNAPSHOT", "2.0-1-SNAPSHOT", -1)]
        [InlineData("2.0-1-SNAPSHOT", "2.0.1-SNAPSHOT", -1)]
        [InlineData("2.0.1-klm-SNAPSHOT", "2.0.1-lmn-SNAPSHOT", -1)]
        [InlineData("2.0.1-xyz-SNAPSHOT", "2.0.1-SNAPSHOT", 1)]// Maven3 specifications altered ordering for this from -1 to 1
        [InlineData("2.0.1-SNAPSHOT", "2.0.1-123-SNAPSHOT", -1)]
        [InlineData("2.0.1-xyz-SNAPSHOT", "2.0.1-123-SNAPSHOT", -1)]
        public void CompareToTest(string Left, string Right, int ExpectedResult)
        {
            var left = new GenericVersion(Left);
            var right = new GenericVersion(Right);
            int Result = left.CompareTo(right);

            Assert.Equal(ExpectedResult, Result);
        }
    }
}