using System;
using System.Diagnostics.CodeAnalysis;
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

        #endregion

        #region Accessors
        public int Major => major;
        public int Minor => minor ?? 0;
        public int Incremental => incremental ?? 0;
        public int Build => build ?? 0;
        public string Qualifier => qualifier;
        #endregion

        #region Constructors
        public ArtifactVersion(int major, int? minor = null, int? incremental = null, int? build = null, string qualifier = null)
        {
            this.major = major;
            this.minor = minor;
            this.incremental = incremental;
            this.build = build;
            this.qualifier = qualifier;
        }
        public ArtifactVersion(string Version)
        {
            string[] toks = Version.Split('.');
            if (toks.Length > 0)
            {
                if (int.TryParse(toks[0], out int outInteger))
                {
                    major = outInteger;
                }
                else
                {
                    qualifier = toks[0];
                }
            }
            
            if (toks.Length > 1)
            {
                if (int.TryParse(toks[1], out int outInteger))
                {
                    minor = outInteger;
                }
                else
                {
                    qualifier = toks[1];
                }
            }
            
            if (toks.Length > 2)
            {
                if (int.TryParse(toks[2], out int outInteger))
                {
                    incremental = outInteger;
                }
                else
                {
                    qualifier = toks[2];
                }
            }
            
            if (toks.Length > 3)
            {
                if (int.TryParse(toks[3], out int outInteger))
                {
                    build = outInteger;
                }
                else
                {
                    qualifier = toks[3];
                }
            }
            
            if (toks.Length > 4)
            {
                qualifier = toks[4];
            }
        }
        #endregion

        #region Parsing
        public static ArtifactVersion Parse(string version)
        {
            int major = 0;
            int? minor = null, incremental = null, build = null;
            string qualifier = null;

            DataConsumer<char> Stream = new DataConsumer<char>(version.AsMemory());
            int ItemNo = 0;

            while (!Stream.atEnd)
            {
                Stream.Consume_While( char.IsWhiteSpace );
                string digits = null;

                if (Stream.Next == '-')
                {
                    if (ItemNo == 0)
                    {// This hypen is at the start of the version string, the next characters MUST be digts or it is malformed.
                        if (!char.IsDigit(Stream.NextNext))
                        {
                            throw new FormatException($"Bad Version string ({version}): expected digits after '-' @ {Stream.Slice(0, 6)}");
                        }

                        Stream.Consume();
                        if (Stream.Consume_While(char.IsDigit, out ReadOnlyMemory<char> outDigits))
                        {
                            digits = outDigits.ToString();
                        }
                        else
                        {
                            throw new FormatException($"Bad Version string ({version}): expected digits @ {Stream.Slice(0, 6)}");
                        }
                    }
                    else
                    {// This is the qualifier
                        qualifier = Stream.Consume(Stream.Remaining).ToString();
                        continue;
                    }
                }
                else if (Stream.Next == '.')
                {
                    Stream.Consume();
                    if (Stream.Consume_While(char.IsDigit, out ReadOnlyMemory<char> outDigits))
                    {
                        digits = outDigits.ToString();
                    }
                    else
                    {
                        throw new FormatException($"Bad Version string ({version}): expected digits @ {Stream.Slice(0, 6)}");
                    }
                }
                else if (char.IsDigit(Stream.Next))
                {
                    if (Stream.Consume_While(char.IsDigit, out ReadOnlyMemory<char> outDigits))
                    {
                        digits = outDigits.ToString();
                    }
                    else
                    {
                        throw new FormatException($"Bad Version string ({version}): expected digits @ {Stream.Slice(0, 6)}");
                    }
                }
                else
                {// this should be the qualifier
                    if (ItemNo == 0) throw new FormatException($"Bad Version string ({version}): must specify atleast a major version");

                    qualifier = Stream.Consume(Stream.Remaining).ToString();
                    continue;
                }



                if (!int.TryParse(digits, out int outParsed))
                {
                    throw new FormatException($"Bad Version string ({version}): unable to parse integer ({digits})");
                }

                switch (ItemNo++)
                {
                    case 0:
                        {
                            major = outParsed;
                            break;
                        }
                    case 1:
                        {
                            minor = outParsed;
                            break;
                        }
                    case 2:
                        {
                            incremental = outParsed;
                            break;
                        }
                    case 3:
                        {
                            build = outParsed;
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException("This error should never occur, contact the developer. (version string")
                        }
                }
            } 
        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(major);
            sb.Append('.');

            if (minor.HasValue)
            {
                sb.Append(minor);
                sb.Append('.');

                if (incremental.HasValue)
                {
                    sb.Append(incremental);
                    sb.Append('.');

                    if (build.HasValue)
                    {
                        sb.Append(build);
                        sb.Append('.');
                    }
                }
            }

            if (!string.IsNullOrEmpty(qualifier))
            {
                sb.Append(qualifier);
            }

            return sb.ToString();
        }


        public bool Equals([AllowNull] ArtifactVersion other)
        {
            return Major == other.Major &&
                   Minor == other.Minor &&
                   Incremental == other.Incremental &&
                   Build == other.Build &&
                   Qualifier == other.Qualifier;
        }

        public int CompareTo([AllowNull] ArtifactVersion other)
        {
            if (this.Equals(other)) return 0;

            if (Major < other.Major) return 1;
            else if (Major > other.Major) return -1;
            else if (Minor < other.Minor) return 1;
            else if (Minor > other.Minor) return -1;
            else if (Incremental < other.Incremental) return 1;
            else if (Incremental > other.Incremental) return -1;
            else if (Build < other.Build) return 1;
            else if (Build > other.Build) return -1;

            /**
             * If one qualifier is '+' then it comes BEFORE the other.
             */
            if (Qualifier.Equals('+')) return 1;
            else if (other.Qualifier.Equals('+')) return -1;

            // If one version is qualified and the other is not then the qualified one comes BEFORE the unqualified one (To avoid the system always choosing 'snapshot' versions for instance).
            if (string.IsNullOrEmpty(Qualifier) ^ string.IsNullOrEmpty(other.Qualifier))
            {
                if (string.IsNullOrEmpty(Qualifier)) return 1;
                else if (string.IsNullOrEmpty(other.Qualifier)) return -1;
            }


            return 0;
        }

    }
}
