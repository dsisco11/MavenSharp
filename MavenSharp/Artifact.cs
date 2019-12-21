using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MavenSharp
{
    public class Artifact
    {
        #region Properties
        /// <summary>
        /// The Url of the repository which contains this artifact
        /// </summary>
        public List<MavenRepository> Repositorys { get; protected set; } = new List<MavenRepository>();
        /// <summary>
        /// The artifact name
        /// </summary>
        public ArtifactIdentifier Name { get; protected set; }

        protected Dictionary<string, string> Versions = new Dictionary<string, string>();
        #endregion

        #region Constructors

        public Artifact(string repositoryUrl, string name)
        {
            Repositorys = new List<MavenRepository>(new MavenRepository[] { new MavenRepository(repositoryUrl) });
            Name = new ArtifactIdentifier(name);
        }
        public Artifact(IEnumerable<string> repositoryUrls, string name)
        {
            Repositorys = (from str in repositoryUrls select new MavenRepository(str)).ToList();
            Name = new ArtifactIdentifier(name);
        }

        public Artifact(Uri repositoryUrl, string name)
        {
            Repositorys = new List<MavenRepository>(new MavenRepository[] { new MavenRepository(repositoryUrl) });
            Name = new ArtifactIdentifier(name);
        }
        public Artifact(IEnumerable<Uri> repositoryUrls, string name)
        {
            Repositorys = (from url in repositoryUrls select new MavenRepository(url)).ToList();
            Name = new ArtifactIdentifier(name);
        }


        public Artifact(MavenRepository repository, string name)
        {
            Repositorys = new List<MavenRepository>(new MavenRepository[] { repository });
            Name = new ArtifactIdentifier(name);
        }
        public Artifact(IEnumerable<MavenRepository> repositoryUrls, string name)
        {
            Repositorys = new List<MavenRepository>(repositoryUrls);
            Name = new ArtifactIdentifier(name);
        }
        #endregion


        /// <summary>
        /// Sends a HEAD request for the Artifact resource and returns the result, verifying that the file exists on the server.
        /// </summary>
        public async Task<bool> Head()
        {
            MavenRepository MavenRepo = await Locate_Maven_Repository_For_Package(Repositorys, Name);
            ArtifactIdentifier Ident = await Resolve_Targeted_Package(MavenRepo);
            Uri packageUrl = MavenRepo.Get_Package_Url(Ident);

            try
            {
                WebRequest req = WebRequest.Create(packageUrl);
                req.Method = "HEAD";
                WebResponse response = await req.GetResponseAsync();

                return true;
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.Fail(ex.ToString());
            }

            return false;
        }

        /// <summary>
        /// Downloads the artifact to the given <paramref name="DestinationDirectory"/>
        /// </summary>
        /// <param name="DestinationDirectory">Base directory to place the downloaded artifact in</param>
        /// <param name="Force">If <c>True</c> then the artifact will be re-downloaded even if it already exists on disk.</param>
        /// <param name="progress">Progress reporter for the download</param>
        /// <returns></returns>
        public async Task<bool> Download(string DestinationDirectory, bool Force, IProgress<double> progress)
        {
            MavenRepository MavenRepo = await Locate_Maven_Repository_For_Package(Repositorys, Name);
            ArtifactIdentifier Ident = await Resolve_Targeted_Package(MavenRepo);
            Uri packageUrl = MavenRepo.Get_Package_Url(Name);

            string filePath = Path.Combine(DestinationDirectory, Ident.Get_Path());
            if (Force)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            return await Download_File(packageUrl, filePath, null, progress);
        }


        /// <summary>
        /// Resolves the provided artifact identifier, which can contain wildcards such as (eg: '1.5.+'), to a specific version
        /// </summary>
        protected async Task<ArtifactIdentifier> Resolve_Targeted_Package(MavenRepository Repository)
        {
            ArtifactIdentifier Ident = Name;
            if (Name.Version.Qualifier.Equals("+"))
            {
                // Find the highest version above the one specified in the ArtifactName
                IEnumerable<ArtifactVersion> VersionsList = await Repository.Get_Versions_For_Package(Name);
                var TargetVersion = Resolve_Version(VersionsList, Name.Version);
                Ident = Name.WithVersion(TargetVersion);
            }
            else if (Ident.isSnapshot)
            {// TODO: Add support for snapshot versions
                /**
                 * Snapshot support will entail doing some kind of hash checking against the file on the server vs' on disk.
                 */
                throw new NotImplementedException();
            }

            return Ident;
        }

        /// <summary>
        /// Searches a list of versions for the one that best matches the <paramref name="TargetVersion"/>.
        /// Usually this means finding the highest matching version for a version specifier like "1.5.+"
        /// </summary>
        /// <param name="VersionList">List of versions to search</param>
        /// <param name="TargetVersion">Version definition to match</param>
        /// <returns></returns>
        protected ArtifactVersion Resolve_Version(IEnumerable<ArtifactVersion> VersionList, ArtifactVersion TargetVersion)
        {
            ArtifactVersion MinVersion = TargetVersion.Get_Unqualified();
            ArtifactVersion Highest = null;
            foreach (ArtifactVersion vers in VersionList)
            {
                int minRes = vers.CompareTo(MinVersion);
                int vRes = vers.CompareTo(TargetVersion);
                if (minRes >= 0 && vRes <= 0)
                {
                    if (Highest == null || Highest.CompareTo(vers) <= 0)
                    {
                        Highest = vers;
                    }
                }
            }

            return Highest;
        }


        /// <summary>
        /// Searches all the known maven repository URI's provided for this artifact and returns the one that this package resides at.
        /// </summary>
        /// <param name="RepositoryList">List of repository URI's to check</param>
        /// <param name="Package">Artifact identifier</param>
        /// <returns></returns>
        public async Task<MavenRepository> Locate_Maven_Repository_For_Package(List<MavenRepository> RepositoryList, ArtifactIdentifier Package)
        {
            foreach (MavenRepository Repo in RepositoryList)
            {
                try
                {
                    Uri metadataUri = Repo.Get_Metadata_Url(this.Name);

                    WebRequest req = WebRequest.Create(metadataUri);
                    req.Method = "HEAD";
                    WebResponse response = await req.GetResponseAsync();

                    return Repo;
                }
                catch (WebException ex)
                {
                    System.Diagnostics.Debug.Fail(ex.ToString());
                }
            }

            throw new Exception($"Unable to find package({Package}) in any of the known repositorys!");
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
