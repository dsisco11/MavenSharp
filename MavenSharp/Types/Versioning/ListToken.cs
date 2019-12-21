/**
 * Adapted from the Apache maven standards
 * Documentation: https://cwiki.apache.org/confluence/display/MAVENOLD/Versioning
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MavenArtifactDownloader.Types.Versioning
{
    public class ListToken : List<IVersionToken>, IVersionToken
    {
        public readonly List<IVersionToken> Value = new List<IVersionToken>();

        public VersionTokenType Type => VersionTokenType.List;


        /// <summary>
        /// Removes any trailing 'null' items from the list
        /// </summary>
        public void Normalize()
        {
            int idx;
            while (Count > 0 && (idx = FindLastIndex(t => t.isNull())) == (Count - 1))
            {
                RemoveAt(idx);
            }
        }

        public int CompareTo([AllowNull] IVersionToken other)
        {
            if (other == null)
            {
                if (Count == 0)
                {
                    return 0;
                }

                return this[0].CompareTo(null);
            }

            switch (other.Type)
            {
                case VersionTokenType.Integer:
                    {
                        return -1;
                    }
                case VersionTokenType.String:
                    {
                        return 1;
                    }
                case VersionTokenType.List:
                    {
                        var left = GetEnumerator();
                        var right = ((ListToken)other).GetEnumerator();
                        bool lmov = left.MoveNext();
                        bool rmov = right.MoveNext();

                        while (lmov || rmov)
                        {
                            var l = left.Current ?? null;
                            var r = right.Current ?? null;

                            lmov = left.MoveNext();
                            rmov = right.MoveNext();

                            // if this list is shorter then invert the comparison and multiply by -1
                            int result = l == null ? -1 * r.CompareTo(l) : l.CompareTo(r);

                            if (result != 0)
                            {
                                return result;
                            }
                        }

                        return 0;
                    }
                default:
                    {
                        throw new Exception($"Invalid item: {other}");
                    }
            }
        }

        public bool Equals([AllowNull] IVersionToken other)
        {
            if (other == null) return false;
            if (other.Type != VersionTokenType.List) return false;

            ListToken list = (ListToken)other;
            if (Count != list.Count) return false;

            var left = GetEnumerator();
            var right = ((ListToken)other).GetEnumerator();

            while (left.MoveNext() || right.MoveNext())
            {
                var l = left.Current ?? null;
                var r = right.Current ?? null;

                if (!l.Equals(r))
                {
                    return false;
                }
            }

            return true;
        }

        public bool isNull() => this.Count == 0;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            string[] items = (from o in this select o.ToString()).ToArray();
            sb.Append(string.Join(',', items));

            sb.Append("]");

            return sb.ToString();
        }
    }
}
