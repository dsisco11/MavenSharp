using MavenArtifactDownloader.Types;
using MavenArtifactDownloader.Types.Versioning;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MavenArtifactDownloader
{
    public class ArtifactVersion : IEquatable<ArtifactVersion>, IComparable<ArtifactVersion>
    {
        #region Properties
        private readonly int major = 0;

        private readonly int? minor = null;

        private readonly int? incremental = null;

        private readonly int? build = null;

        private readonly string qualifier = null;

        private readonly GenericVersion generic;

        #endregion

        #region Accessors
        public int Major => major;
        public int Minor => minor ?? 0;
        public int Incremental => incremental ?? 0;
        public int Build => build ?? 0;
        public string Qualifier => qualifier;
        #endregion

        #region Constructors
        public ArtifactVersion(string Version)
        {
            generic = new GenericVersion(Version);
            DataConsumer<IVersionToken> Stream = new DataConsumer<IVersionToken>(new ReadOnlyMemory<IVersionToken>( generic.Items.ToArray() ));

            if (!Stream.atEnd && Stream.Next.Type == VersionTokenType.Integer)
            {
                major = ((IntegerToken)Stream.Consume()).Value;
            }
            if (!Stream.atEnd && Stream.Next.Type == VersionTokenType.Integer)
            {
                minor = ((IntegerToken)Stream.Consume()).Value;
            }
            if (!Stream.atEnd && Stream.Next.Type == VersionTokenType.Integer)
            {
                incremental = ((IntegerToken)Stream.Consume()).Value;
            }

            /**
             * Build number is always specified by the pattern: "-###", 
             * this pattern by nature will create a sublist item in the token stream. 
             * So look for it.
             */
            if (!Stream.atEnd && Stream.Next.Type == VersionTokenType.List)
            {
                // It's a build number ONLY IF there is just a single number after the hypen
                var list = ((ListToken)Stream.Next);
                if (list.Count == 1)
                {
                    list = (ListToken)Stream.Consume();
                    Stream = new DataConsumer<IVersionToken>(new ReadOnlyMemory<IVersionToken>(list.ToArray()));

                    if (!Stream.atEnd && Stream.Next.Type == VersionTokenType.Integer)
                    {
                        build = ((IntegerToken)Stream.Consume()).Value;
                    }
                }
            }

            /**
             * Ok so we only have a qualifier under the following conditions:
             * 1) The token stream has a string token at the end
             * 2) Version string contains a hypen(-) and we have no build number, the qualifier is EVERYTHING after the first hypen
             */
            if (!build.HasValue && Version.Contains('-'))
            {
                int idx = Version.IndexOf('-');
                if (idx > -1)
                {
                    qualifier = Version.Substring(idx+1);
                }
            }
            else if (!Stream.atEnd && Stream.Next.Type == VersionTokenType.String)
            {
                qualifier = Stream.Consume().ToString();
            }
        }
        #endregion

        public override string ToString() => generic.ToString();

        public bool Equals([AllowNull] ArtifactVersion other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo([AllowNull] ArtifactVersion other)
        {
            return generic.CompareTo(other.generic);
        }

    }
}
