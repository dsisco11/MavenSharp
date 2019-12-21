/**
 * Adapted from the Apache maven standards
 * Documentation: https://cwiki.apache.org/confluence/display/MAVENOLD/Versioning
 */

using System;
using System.Diagnostics.CodeAnalysis;

namespace MavenArtifactDownloader.Types.Versioning
{
    /// <summary>
    /// Represents a numeric item in a  version list
    /// </summary>
    public class IntegerToken : IVersionToken
    {
        #region Static
        public static readonly IntegerToken Zero = new IntegerToken(0);
        #endregion

        public readonly int Value = 0;
        #region Accessors
        public VersionTokenType Type => VersionTokenType.Integer;
        #endregion

        #region Constructors
        public IntegerToken()
        {
        }

        public IntegerToken(int value)
        {
            Value = value;
        }

        public IntegerToken(string value)
        {
            Value = int.Parse(value);
        }
        #endregion

        public int CompareTo([AllowNull] IVersionToken other)
        {
            if (other == null)
            {
                return (Value == 0) ? 0 : 1;
            }

            switch (other.Type)
            {
                case VersionTokenType.Integer:
                    {
                        return Value.CompareTo(((IntegerToken)other).Value);
                    }
                case VersionTokenType.String:
                    {
                        return 1;
                    }
                case VersionTokenType.List:
                    {
                        return 1;
                    }
                default:
                    {
                        throw new Exception($"Invalid item: {other}");
                    }
            }
        }

        public bool Equals([AllowNull] IVersionToken other)
        {
            if (other.Type != VersionTokenType.Integer) return false;

            return Value == ((IntegerToken)other).Value;
        }

        public bool isNull() => (Value == 0);

        public override string ToString() => Value.ToString();
    }
}
