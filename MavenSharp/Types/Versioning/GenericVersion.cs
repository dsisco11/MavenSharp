using MavenSharp.Types;
using MavenSharp.Types.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MavenSharp
{
    public class GenericVersion : IEquatable<GenericVersion>, IComparable<GenericVersion>
    {
        #region Properties
        public ListToken Items { get; } = new ListToken();
        public string Value { get; } = string.Empty;
        public string Canonical { get; } = string.Empty;
        #endregion

        #region Accessors
        #endregion

        #region Constructors
        public GenericVersion(string version)
        {
            Value = version;
            version = version.ToLowerInvariant();

            Items = new ListToken();
            ListToken list = Items;
            Stack<IVersionToken> stack = new Stack<IVersionToken>();
            stack.Push(list);

            DataConsumer<char> Stream = new DataConsumer<char>(version.AsMemory());

            while (!Stream.atEnd)
            {
                switch (Stream.Next)
                {
                    case '-':// Starts a new (sub)list
                        {
                            Stream.Consume();// Eat the hypen

                            // If the last appended item was an integer, then normalize the list (remove all zeroes) before creating the new one
                            if (list.Count > 0 && list[list.Count-1].Type == VersionTokenType.Integer)
                            {
                                list.Normalize();
                            }

                            /**
                             * We should only create a new sublist IF the current one is not empty after normalization,
                             * This is because versions with multiple trailing zeroes specified should be considered equal to ones with fewer trailing zeroes as the zeroes are ignored.
                             */
                            if (list.Count > 0)
                            {
                                var newList = new ListToken();
                                list.Add(newList);
                                list = newList;
                                stack.Push(list);
                            }
                            break;
                        }
                    case '.':// Starts a new item
                        {
                            Stream.Consume();// Eat the fullstop
                            break;
                        }
                    case char c when char.IsDigit(c):
                        {// Consume an integer
                            if (!Stream.Consume_While(char.IsDigit, out ReadOnlyMemory<char> outConsumed))
                            {
                                throw new FormatException($"Unable to consume digits in version string @ \"{Stream.Slice(0, 6)}\"");
                            }

                            list.Add(new IntegerToken(outConsumed.ToString()));
                            break;
                        }
                    default:
                        {// Consume a string
                            if (!Stream.Consume_While(c => !char.IsDigit(c) && !isSeperator(c), out ReadOnlyMemory<char> outConsumed))
                            {
                                throw new FormatException($"Unable to consume non-digits in version string @ \"{Stream.Slice(0, 6)}\"");
                            }

                            list.Add(new StringToken(outConsumed.ToString(), char.IsDigit(Stream.Next)));
                            break;
                        }
                }
            }

            while (stack.Count > 0)
            {
                list = (ListToken)stack.Pop();
                list.Normalize();
            }

            Canonical = Items.ToString();
        }

        public GenericVersion(ListToken items)
        {
            Items = items;

            ListToken list = items;
            list.Normalize();
            var iter = list.GetEnumerator();
            while (iter.MoveNext())
            {
                if (iter.Current.Type == VersionTokenType.List)
                {
                    list = (ListToken)iter.Current;
                    list.Normalize();
                    iter = list.GetEnumerator();
                }
            }

            Canonical = Items.ToString();
        }
        #endregion

        #region Parsing
        /// <summary>
        /// Returns if the given <paramref name="codePoint"/> is one of the version-token seperator characters
        /// </summary>
        /// <param name="codePoint"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool isSeperator(char codePoint)
        {
            switch (codePoint)
            {
                case '.':
                case '-':
                {
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }

        #endregion

        public override string ToString() => this.Value;


        public override int GetHashCode()
        {
            const int factor = -1521134295;
            const int seed = 0x51ed270b;
            int hash = seed;
            for (int i = 0; i<this.Items.Count; i++)
            {
                hash = (hash * factor) + this.Items[i].GetHashCode();
            }
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is GenericVersion other)
            {
                return this.Equals(other);
            }

            return base.Equals(obj);
        }

        public bool Equals([AllowNull] GenericVersion other)
        {
            return (Canonical.Equals(other.Canonical, StringComparison.Ordinal));
        }

        public int CompareTo([AllowNull] GenericVersion other)
        {
            return Items.CompareTo(other.Items);
        }


        public static bool operator ==(GenericVersion left, GenericVersion right)
        {
            return left.CompareTo(right) == 0;
        }
        public static bool operator !=(GenericVersion left, GenericVersion right)
        {
            return left.CompareTo(right) != 0;
        }


        public static bool operator <(GenericVersion left, GenericVersion right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator >(GenericVersion left, GenericVersion right)
        {
            return left.CompareTo(right) > 0;
        }
    }
}
