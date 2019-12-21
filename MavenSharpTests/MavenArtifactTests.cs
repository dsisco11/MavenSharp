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
            bool result = artifact.Head().Result;

            Assert.True(result, "Failed to download artifact!");
        }
    }
}