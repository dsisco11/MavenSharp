using Xunit;
using System;

namespace MavenArtifactDownloader.Tests
{
    public class MavenArtifactTests
    {
        [Fact()]
        public void DownloadTest()
        {
            MavenArtifact artifact = new MavenArtifact(new Uri("https://files.minecraftforge.net/maven"), "de.oceanlabs.mcp:mcp_config:1.15.+@zip");
            bool result = artifact.Download(System.IO.Path.GetTempPath(), true, null).Result;

            Assert.True(result, "Failed to download artifact!");
        }
    }
}