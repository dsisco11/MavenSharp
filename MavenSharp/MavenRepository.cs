using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MavenSharp
{
    /// <summary>
    /// Represents a remote maven repository and provides helpful functions for resolving information about it
    /// </summary>
    public class MavenRepository
    {
        #region Properties
        public readonly Uri Origin;
        #endregion

        #region Constructors
        public MavenRepository(string origin)
        {
            Origin = new Uri(origin);
        }
        public MavenRepository(Uri origin)
        {
            Origin = origin;
        }
        #endregion

        public Uri Get_Package_Url(ArtifactIdentifier Package)
        {
            UriBuilder uriBuilder = new UriBuilder(Origin);
            uriBuilder.Path = $"{uriBuilder.Path}/{Package.Get_Path()}";
            return uriBuilder.Uri;
        }

        public Uri Get_Metadata_Url(ArtifactIdentifier Package)
        {
            UriBuilder uriBuilder = new UriBuilder(Origin);
            uriBuilder.Path = $"{uriBuilder.Path}/{Package.Group.Replace('.', '/')}/{Package.Name}/maven-metadata.xml";
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Returns the XML metadata for the artifact on a given repository
        /// </summary>
        /// <exception cref="WebException"></exception>
        public async Task<XElement> Fetch_Metadata(ArtifactIdentifier Package, IProgress<double> progress = null)
        {
            using WebClient webClient = new WebClient();
            if (progress != null)
            {
                webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => { progress.Report((double)e.BytesReceived / (double)e.TotalBytesToReceive); };
            }
            Uri metadataUri = Get_Metadata_Url(Package);
            byte[] metaData = await webClient.DownloadDataTaskAsync(metadataUri);
            using MemoryStream metaStream = new MemoryStream(metaData);
            XElement meta = XElement.Load(metaStream);

            return meta;
        }

        /// <summary>
        /// Searches all repositorys in the list to find one which contains the artifact and then returns all known versions of said artifact
        /// </summary>
        public async Task<IEnumerable<ArtifactVersion>> Get_Versions_For_Package(ArtifactIdentifier Package, IProgress<double> progress = null)
        {
            XElement metadata = await Fetch_Metadata(Package, progress);

            var versioningNode = metadata.Descendants("versioning").First();
            var versionsNode = versioningNode.Descendants("versions").First();
            //IEnumerable<ArtifactVersion> versions = from item in versionsList select new ArtifactVersion(item.Attribute("version").Value) ?? null;
            var versionsList = from item in versionsNode.Elements() select item.Value ?? null;

            if (!versionsList?.Any() ?? false)
            {
                throw new IOException("Invalid maven-metadata.xml file, missing version list");
            }

            List<ArtifactVersion> ReturnList = new List<ArtifactVersion>();
            foreach (string vers in versionsList)
            {
                ReturnList.Add(new ArtifactVersion(vers));
            }

            return ReturnList;
        }
    }
}
