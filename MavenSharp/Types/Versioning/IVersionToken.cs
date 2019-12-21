using System.Diagnostics.CodeAnalysis;

namespace MavenSharp.Types
{
    /// <summary>
    /// Represents an item within a version string section (a number or string)
    /// </summary>
    public interface IVersionToken
    {
        VersionTokenType Type { get; }

        int CompareTo([AllowNull] IVersionToken other);
        bool Equals([AllowNull] IVersionToken other);
        bool isNull();
        string ToString();
    }

}
