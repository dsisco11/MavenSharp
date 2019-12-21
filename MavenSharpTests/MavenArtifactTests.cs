using Xunit;
using System;

namespace MavenSharp.Tests
{
    public class MavenArtifactTests
    {
        [Fact()]
        public void DownloadTest()
        {
            Artifact artifact = new Artifact(new Uri("https://files.minecraftforge.net/maven"), "de.oceanlabs.mcp:mcp_config:1.15.+@zip");
            Assert.True(artifact.Head().Result, "Failed to download artifact!");

            artifact = new Artifact(new Uri("https://files.minecraftforge.net/maven"), "net.minecraftforge:forgeflower:1.5.380.33");
            Assert.True(artifact.Head().Result, "Failed to download artifact!");

            artifact = new Artifact(new Uri("https://files.minecraftforge.net/maven"), "net.minecraftforge:forgeflower:1.5.380.+");
            Assert.True(artifact.Head().Result, "Failed to download artifact!");
        }
    }
}