using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MavenArtifactDownloader
{
    public class MavenArtifact
    {
        #region Properties
        /// <summary>
        /// The Url of the repository which contains this artifact
        /// </summary>
        public List<Uri> Repositorys { get; protected set; } = new List<Uri>();
        /// <summary>
        /// The artifact name
        /// </summary>
        public ArtifactIdentifier Name { get; protected set; }

        protected Dictionary<string, string> Versions = new Dictionary<string, string>();
        #endregion

        #region Constructors
        public MavenArtifact(Uri repositoryUrl, string name)
        {
            Repositorys = new List<Uri>(new Uri[] { repositoryUrl });
            Name = new ArtifactIdentifier(name);
        }
        public MavenArtifact(IEnumerable<Uri> repositoryUrls, string name)
        {
            Repositorys = new List<Uri>(repositoryUrls);
            Name = new ArtifactIdentifier(name);
        }
        #endregion


        public async Task<bool> Download(string DestinationDirectory, bool Force, IProgress<double> progress)
        {
            Uri MavenRepo = await Locate_Maven_Repository_For_Package(Repositorys, Name);
            ArtifactIdentifier artifact = Name;
            /**
             * It seems like (standards compliant) artifact versions only allow the '+' wildcard as the qualifier, 
             * but there is very little documentation I can find.
             */
            if (Name.Version.Qualifier.Equals("+"))
            {
                // Find the highest version above the one specified in the ArtifactName
                IEnumerable<ArtifactVersion> VersionsList = await Fetch_Versions(MavenRepo);
                var TargetVersion = await Find_Highest_Matching_Version(VersionsList, Name.Version);
                artifact = Name.WithVersion(TargetVersion);
            }
            else if (Name.Version.Qualifier.Contains("-SNAPSHOT", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotImplementedException();
            }

            UriBuilder uriBuilder = new UriBuilder(MavenRepo);
            uriBuilder.Path = $"{uriBuilder.Path}/{artifact.Get_Path()}";

            string filePath = Path.Combine(DestinationDirectory, artifact.Get_Path());
            if (Force)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            return await Download_File(uriBuilder.Uri, filePath, null, progress);
        }

        public static async Task<ArtifactVersion> Find_Highest_Matching_Version(IEnumerable<ArtifactVersion> VersionList, ArtifactVersion TargetVersion)
        {
            ArtifactVersion Highest = null;
            foreach (ArtifactVersion vers in VersionList)
            {
                if (TargetVersion.CompareTo(vers) <= 0)
                {
                    if (Highest == null || Highest.CompareTo(vers) <= 0)
                    {
                        Highest = vers;
                    }
                }
            }

            return Highest;
        }


        public static async Task<Uri> Locate_Maven_Repository_For_Package(List<Uri> RepositoryList, ArtifactIdentifier Package)
        {
            foreach (Uri RepoUrl in RepositoryList)
            {
                try
                {
                    UriBuilder uriBuilder = new UriBuilder(RepoUrl);
                    uriBuilder.Path = $"{uriBuilder.Path}/{Package.Group.Replace('.', '/')}/{Package.Name}/maven-metadata.xml";
                    Uri metadataUri = uriBuilder.Uri;

                    WebRequest req = WebRequest.Create(metadataUri);
                    req.Method = "HEAD";
                    WebResponse response = await req.GetResponseAsync();

                    return RepoUrl;
                }
                catch (WebException ex)
                {
                    System.Diagnostics.Debug.Fail(ex.ToString());
                }
            }

            throw new Exception($"Unable to find package({Package}) in any of the known repositorys!");
        }

        /// <summary>
        /// Searches all repositorys in the list to find one which contains the artifact and then returns all known versions of said artifact
        /// </summary>
        public async Task<IEnumerable<ArtifactVersion>> Fetch_Versions(Uri MavenRepo, IProgress<double> progress = null)
        {
            XElement metadata = await Fetch_Metadata(MavenRepo);

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

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="WebException"></exception>
        protected async Task<XElement> Fetch_Metadata(Uri MavenRepository, IProgress<double> progress = null)
        {
            UriBuilder uriBuilder = new UriBuilder(MavenRepository);
            uriBuilder.Path = $"{uriBuilder.Path}/{Name.Group.Replace('.', '/')}/{Name.Name}/maven-metadata.xml";
            Uri metadataUri = uriBuilder.Uri;

            using WebClient webClient = new WebClient();
            if (progress != null)
            {
                webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => { progress.Report((double)e.BytesReceived / (double)e.TotalBytesToReceive); };
            }
            byte[] metaData = await webClient.DownloadDataTaskAsync(metadataUri);
            using MemoryStream metaStream = new MemoryStream(metaData);
            XElement meta = XElement.Load(metaStream);

            return meta;
        }

        /// <summary>
        /// Transforms a hexadecimal formatted string into the corrasponding sequence of bytes
        /// </summary>
        /// <param name="hexString">String representing data in hexadecimal form</param>
        /// <returns>Sequence of bytes the given hexadecimal string represents</returns>
        protected static ReadOnlyMemory<byte> Transform_Hex_String(ReadOnlySpan<char> hexString)
        {
            if (hexString.StartsWith("0x"))
            {
                hexString = hexString.Slice(2);
            }


            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var hexPair = hexString.Slice(i * 2, 2);
                byte b = Convert.ToByte(hexPair.ToString(), 16);
                bytes[i] = b;
            }

            return bytes;
        }

        protected static async Task<bool> Download_File(Uri URL, string Destination, string Hash, IProgress<double> progress)
        {
            if (URL == null)
            {
                throw new ArgumentNullException(nameof(URL));
            }
            Contract.EndContractBlock();

            string DestDir = Path.GetDirectoryName(Destination);
            if (!Directory.Exists(DestDir))
            {
                Directory.CreateDirectory(DestDir);
            }

            // If the file already exists, we have to decide if it should be deleted and redownloaded or not
            if (File.Exists(Destination))
            {
                // If we were provided a hash, check the files hash against the one given
                if (!string.IsNullOrEmpty(Hash))
                {
                    using SHA1 Sha = SHA1.Create();
                    using FileStream fs = File.OpenRead(Destination);
                    ReadOnlyMemory<byte> rawHash = Transform_Hex_String(Hash);
                    ReadOnlyMemory<byte> newHash = Sha.ComputeHash(fs);

                    // If the hashes match then this file has already been downloaded
                    if (rawHash.Equals(newHash))
                    {
                        return true;
                    }
                }
                // If no hash was provided OR they didn't match, then delete the file from disk and continue with the download
                File.Delete(Destination);
            }

            try
            {
                using WebClient webClient = new WebClient();
                webClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.Revalidate);
                if (progress != null)
                {
                    webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => { progress.Report((double)e.BytesReceived / (double)e.TotalBytesToReceive); };
                }

                await webClient.DownloadFileTaskAsync(URL, Destination).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.Fail(ex.ToString());
                return false;
            }

            return true;
        }
    }
}
