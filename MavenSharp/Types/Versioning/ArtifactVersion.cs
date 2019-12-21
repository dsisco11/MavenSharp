using MavenSharp.Types;
using MavenSharp.Types.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MavenSharp
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
        public ArtifactVersion(string Version) : this(new GenericVersion(Version))
        {
        }

        public ArtifactVersion(GenericVersion Version)
        {
            generic = Version;
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
             * After these 3 standard versioning numbers we must still allow an unlimited ammount of additional number tokens.
             * At the very end of the version string, after all the leading integer parts, 
             * there CAN be EITHER a build number (-xxx) or a qualifier which is a string token
             * So next we skip all remaining integer tokens and then check for build/qualifier
             */
            Stream.Consume_While(tok => tok?.Type == VersionTokenType.Integer);

            /**
             * 
             * Build number is always specified by the pattern: "-###", 
             * this pattern by nature will create a sublist item in the token stream. 
             * So look for it.
             */
            if (!Stream.atEnd && Stream.Last.Type == VersionTokenType.List)
            {
                // It's a build number ONLY IF there is just a single number after the hypen
                var list = ((ListToken)Stream.Last);
                if (list.Count == 1)
                {
                    if (list[0].Type == VersionTokenType.Integer)
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
            if (!build.HasValue && generic.Value.Contains('-'))
            {
                int idx = generic.Value.IndexOf('-');
                if (idx > -1)
                {
                    qualifier = generic.Value.Substring(idx+1);
                }
            }
            else if (!Stream.atEnd && Stream.Last.Type == VersionTokenType.String)
            {
                qualifier = Stream.Last.ToString();
            }
        }
        #endregion

        public override string ToString() => generic.ToString();


        public override int GetHashCode()
        {
            return generic.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ArtifactVersion other)
            {
                return this.Equals(other);
            }

            return base.Equals(obj);
        }

        public bool Equals([AllowNull] ArtifactVersion other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo([AllowNull] ArtifactVersion other)
        {
            return generic.CompareTo(other.generic);
        }

        /// <summary>
        /// Returns an instance of this version without its qualifier
        /// </summary>
        /// <returns></returns>
        public ArtifactVersion Get_Unqualified()
        {
            ListToken Tokens = new ListToken(generic.Items);
            if (Tokens.Last().Type == VersionTokenType.String )
            {
                Tokens.Remove(Tokens.Last());
            }
            else if (Tokens.Last().Type == VersionTokenType.List)
            {
                ListToken list = (ListToken)Tokens.Last();
                if (list.Count > 1 && list[0].Type != VersionTokenType.Integer)// This isnt a build number! So it's a qualifier, delete it...
                {
                    Tokens.Remove(Tokens.Last());
                }
            }

            return new ArtifactVersion(new GenericVersion(Tokens));
        }

    }
}
