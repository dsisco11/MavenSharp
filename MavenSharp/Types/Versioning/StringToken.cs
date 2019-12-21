/**
 * Adapted from the Apache maven standards
 * Documentation: https://cwiki.apache.org/confluence/display/MAVENOLD/Versioning
 */
 
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MavenArtifactDownloader.Types.Versioning
{
    public class StringToken : IVersionToken
    {
        #region Statics
        private static readonly List<string> QUALIFIER = new List<string>(new string[] { "alpha", "beta", "milestone", "rc", "snapshot", string.Empty, "sp" });
        private static readonly Dictionary<string, string> ALIASES = new Dictionary<string, string>();
        private static readonly string RELEASE_VERSION_INDEX = QUALIFIER.IndexOf(string.Empty).ToString();
        static StringToken()
        {
            ALIASES.Add("ga", "");
            ALIASES.Add("final", "");
            ALIASES.Add("cr", "rc");
        }
        #endregion

        #region Properties
        public readonly string Value;
        #endregion

        #region Constructors
        public StringToken(string value, bool followedByDigit)
        {
             if ( followedByDigit && value.Length == 1 )
             {
                 // a1 = alpha-1, b1 = beta-1, m1 = milestone-1
                 switch ( value[0] )
                 {
                     case 'a':
                         value = "alpha";
                         break;
                     case 'b':
                         value = "beta";
                         break;
                     case 'm':
                         value = "milestone";
                         break;
                 }
             }

            this.Value = ALIASES.GetValueOrDefault(value, value);
        }
        #endregion

        public VersionTokenType Type => VersionTokenType.String;

        public int CompareTo([AllowNull] IVersionToken other)
        {
            if (other == null)
            {
                return Get_Qualifier(Value).CompareTo(RELEASE_VERSION_INDEX);
            }

            switch (other.Type)
            {
                case VersionTokenType.Integer:
                    {
                        return -1;
                    }
                case VersionTokenType.String:
                    {
                        return Get_Qualifier(Value).CompareTo(Get_Qualifier(((StringToken)other).Value));
                    }
                case VersionTokenType.List:
                    {
                        return -1;
                    }
                default:
                    {
                        throw new Exception($"Invalid item: {other}");
                    }
            }
        }

        public bool Equals([AllowNull] IVersionToken other)
        {
            if (other.Type != VersionTokenType.String) return false;

            return Value.Equals(((StringToken)other).Value);
        }

        public bool isNull() => Get_Qualifier(Value).CompareTo(RELEASE_VERSION_INDEX) == 0;

        private static string Get_Qualifier(string qualifier)
        {
            int i = QUALIFIER.IndexOf(qualifier);
            return i == -1 ? $"{QUALIFIER.Count}-{qualifier}" : i.ToString();
        }

        public override string ToString() => Value;
    }
}
