using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace MavenSharp
{
    /// <summary>
    /// Provides access to a genericized, consumable stream of data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataConsumer<T>
    {
        #region Properties
        /// <summary>
        /// Our stream of tokens
        /// </summary>
        private readonly ReadOnlyMemory<T> Data;

        public int Position { get; private set; } = 0;
        /// <summary>
        /// The current position at which data will be read from the stream
        /// </summary>
        public ulong LongPosition { get => (ulong)Position; set => Position = (int)value; }

        public readonly T EOF_ITEM = default;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new stream from a memory pointer
        /// </summary>
        /// <param name="Memory"></param>
        /// <param name="EOF_ITEM"></param>
        public DataConsumer(ReadOnlyMemory<T> Memory)
        {
            Data = Memory;
        }

        /// <summary>
        /// Creates a new stream from a memory pointer
        /// </summary>
        /// <param name="Memory"></param>
        /// <param name="EOF_ITEM"></param>
        public DataConsumer(ReadOnlyMemory<T> Memory, T EOF_ITEM)
        {
            Data = Memory;
            this.EOF_ITEM = EOF_ITEM;
        }

        /// <summary>
        /// Creates a new stream from an array
        /// </summary>
        /// <param name="Items"></param>
        public DataConsumer(T[] Items)
        {
            if (Items is null) throw new ArgumentNullException(nameof(Items));
            Contract.EndContractBlock();

            Data = new ReadOnlyMemory<T>(Items);
        }

        /// <summary>
        /// Creates a new stream from an array
        /// </summary>
        /// <param name="Items"></param>
        /// <param name="EOF_ITEM"></param>
        public DataConsumer(T[] Items, T EOF_ITEM)
        {
            if (Items is null) throw new ArgumentNullException(nameof(Items));
            Contract.EndContractBlock();

            Data = new ReadOnlyMemory<T>(Items);
            this.EOF_ITEM = EOF_ITEM;
        }
        #endregion

        #region Accessors
        public int Length => Data.Length;
        public ulong LongLength => (ulong)Data.Length;
        public int Remaining => (Data.Length - Position);
        /// <summary>
        /// Returns the next item to be consumed, equivalent to calling Peek(0)
        /// </summary>
        public T Next => Peek(0);
        /// <summary>
        /// Returns the next item to be consumed, equivalent to calling Peek(1)
        /// </summary>
        public T NextNext => Peek(1);
        /// <summary>
        /// Returns the next item to be consumed, equivalent to calling Peek(2)
        /// </summary>
        public T NextNextNext => Peek(2);

        /// <summary>
        /// Returns whether the stream position is currently at the end of the stream
        /// </summary>
        //public bool atEnd => Position >= Length;
        public bool atEnd => (Remaining <= 0);

        /// <summary>
        /// Returns whether the next character in the stream is the EOF character
        /// </summary>
        public bool atEOF => Peek(0).Equals(EOF_ITEM);
        #endregion

        #region Data
        /// <summary>
        /// Direct accessor to the Data <see cref="Memory{T}"/> instance
        /// </summary>
        public ReadOnlyMemory<T> AsMemory() => Data;
        /// <summary>
        /// Direct accessor to the Data <see cref="Memory{T}"/> instances' span
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<T> AsSpan() => Data.Span;
        #endregion

        #region Seeking
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _seek(int position, bool from_end)
        {
            if (from_end)
            {
                Position = (Length - position);
            }
            else
            {
                Position = position;
            }
        }

        /// <summary>
        /// Seeks to a specific position in the stream
        /// </summary>
        /// <param name="Position"></param>
        public void Seek(int Position, bool FromEnd = false)
        {
            _seek((int)Position, FromEnd);
        }
        /// <summary>
        /// Seeks to a specific position in the stream
        /// </summary>
        /// <param name="Position"></param>
        public void Seek(uint Position, bool FromEnd = false)
        {
            _seek((int)Position, FromEnd);
        }
        /// <summary>
        /// Seeks to a specific position in the stream
        /// </summary>
        /// <param name="Position"></param>
        public void Seek(long Position, bool FromEnd = false)
        {
            _seek((int)Position, FromEnd);
        }
        /// <summary>
        /// Seeks to a specific position in the stream
        /// </summary>
        /// <param name="Position"></param>
        public void Seek(ulong Position, bool FromEnd = false)
        {
            _seek((int)Position, FromEnd);
        }
        #endregion

        #region Peeking
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T _get(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException($"{nameof(index)}({index}) cannot be negative");
            }

            if (index >= Data.Length)
            {
                return EOF_ITEM;
            }

            return Data.Span[index];
        }

        /// <summary>
        /// Returns the item at <paramref name="Index"/>
        /// </summary>
        public T Get(int Index = 0)
        {
            return _get(Index);
        }
        /// <summary>
        /// Returns the item at <paramref name="Index"/>
        /// </summary>
        public T Get(uint Index)
        {
            return _get((int)Index);
        }
        /// <summary>
        /// Returns the item at <paramref name="Index"/>
        /// </summary>
        public T Get(long Index)
        {
            return _get((int)Index);
        }
        /// <summary>
        /// Returns the item at <paramref name="Index"/>
        /// </summary>
        public T Get(ulong Index)
        {
            return _get((int)Index);
        }
        #endregion

        #region Peeking
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T _peek(int Offset)
        {
            if (Offset < 0)
            {
                throw new IndexOutOfRangeException($"{nameof(Offset)}({Offset}) cannot be negative");
            }

            var index = Position + Offset;

            if (index >= Data.Length)
            {
                return EOF_ITEM;
            }

            return Data.Span[index];
        }

        /// <summary>
        /// Returns the item at +<paramref name="Offset"/> from the current read position
        /// </summary>
        /// <param name="Offset">Distance from the current read position at which to peek</param>
        /// <returns></returns>
        public T Peek(int Offset = 0)
        {
            return _peek(Offset);
        }
        /// <summary>
        /// Returns the item at +<paramref name="Offset"/> from the current read position
        /// </summary>
        /// <param name="Offset">Distance from the current read position at which to peek</param>
        /// <returns></returns>
        public T Peek(uint Offset)
        {
            return _peek((int)Offset);
        }
        /// <summary>
        /// Returns the item at +<paramref name="Offset"/> from the current read position
        /// </summary>
        /// <param name="Offset">Distance from the current read position at which to peek</param>
        /// <returns></returns>
        public T Peek(long Offset)
        {
            return _peek((int)Offset);
        }
        /// <summary>
        /// Returns the item at +<paramref name="Offset"/> from the current read position
        /// </summary>
        /// <param name="Offset">Distance from the current read position at which to peek</param>
        /// <returns></returns>
        public T Peek(ulong Offset)
        {
            return _peek((int)Offset);
        }
        #endregion

        #region Scanning
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _scan(T subject, out uint outOffset, int startOffset, IEqualityComparer<T> comparer)
        {
            var Comparator = comparer ?? EqualityComparer<T>.Default;
            var offset = startOffset;

            while ((offset + Position) < Length)
            {
                var current = Peek(offset);
                if (Comparator.Equals(current, subject))
                {
                    outOffset = (uint)offset;
                    return true;
                }

                offset++;
            }

            outOffset = 0;
            return false;
        }

        /// <summary>
        /// Returns the index of the first item matching the given <paramref name="Subject"/>  or -1 if none was found
        /// </summary>
        /// <returns>Index of first item matching the given one or -1 if none was found</returns>
        public bool Scan(T Subject, out int OutOffset, int StartOffset = 0, IEqualityComparer<T> Comparer = null)
        {
            bool RetVal = _scan(Subject, out var outOffset, (int)StartOffset, Comparer);
            OutOffset = (int)outOffset;
            return RetVal;
        }
        /// <summary>
        /// Returns the index of the first item matching the given <paramref name="Subject"/>  or -1 if none was found
        /// </summary>
        /// <returns>Index of first item matching the given one or -1 if none was found</returns>
        public bool Scan(T Subject, out uint OutOffset, uint StartOffset = 0, IEqualityComparer<T> Comparer = null)
        {
            bool RetVal = _scan(Subject, out var outOffset, (int)StartOffset, Comparer);
            OutOffset = outOffset;
            return RetVal;
        }
        /// <summary>
        /// Returns the index of the first item matching the given <paramref name="Subject"/>  or -1 if none was found
        /// </summary>
        /// <returns>Index of first item matching the given one or -1 if none was found</returns>
        public bool Scan(T Subject, out long OutOffset, long StartOffset = 0, IEqualityComparer<T> Comparer = null)
        {
            bool RetVal = _scan(Subject, out var outOffset, (int)StartOffset, Comparer);
            OutOffset = outOffset;
            return RetVal;
        }
        /// <summary>
        /// Returns the index of the first item matching the given <paramref name="Subject"/>  or -1 if none was found
        /// </summary>
        /// <returns>Index of first item matching the given one or -1 if none was found</returns>
        public bool Scan(T Subject, out ulong OutOffset, ulong StartOffset = 0, IEqualityComparer<T> Comparer = null)
        {
            bool RetVal = _scan(Subject, out var outOffset, (int)StartOffset, Comparer);
            OutOffset = outOffset;
            return RetVal;
        }


        // ===================================

        private bool _scan(Predicate<T> Predicate, out uint outOffset, int startOffset)
        {
            var offset = startOffset;

            while ((offset + Position) < Length)
            {
                var current = Peek(offset);
                if (Predicate(current))
                {
                    outOffset = (uint)offset;
                    return true;
                }

                offset++;
            }

            outOffset = 0;
            return false;
        }

        /// <summary>
        /// Returns the index of the first item matching the given predicate or -1 if none was found
        /// </summary>
        /// <returns>Index of first item matching the given predicate or -1 if none was found</returns>
        public bool Scan(Predicate<T> Predicate, out int OutOffset, int StartOffset = 0)
        {
            bool RetVal = _scan(Predicate, out var outOffset, (int)StartOffset);
            OutOffset = (int)outOffset;
            return RetVal;
        }
        /// <summary>
        /// Returns the index of the first item matching the given predicate or -1 if none was found
        /// </summary>
        /// <returns>Index of first item matching the given predicate or -1 if none was found</returns>
        public bool Scan(Predicate<T> Predicate, out uint OutOffset, uint StartOffset)
        {
            bool RetVal = _scan(Predicate, out var outOffset, (int)StartOffset);
            OutOffset = outOffset;
            return RetVal;
        }
        /// <summary>
        /// Returns the index of the first item matching the given predicate or -1 if none was found
        /// </summary>
        /// <returns>Index of first item matching the given predicate or -1 if none was found</returns>
        public bool Scan(Predicate<T> Predicate, out long OutOffset, long StartOffset)
        {
            bool RetVal = _scan(Predicate, out var outOffset, (int)StartOffset);
            OutOffset = outOffset;
            return RetVal;
        }
        /// <summary>
        /// Returns the index of the first item matching the given predicate or -1 if none was found
        /// </summary>
        /// <returns>Index of first item matching the given predicate or -1 if none was found</returns>
        public bool Scan(Predicate<T> Predicate, out ulong OutOffset, ulong StartOffset)
        {
            bool RetVal = _scan(Predicate, out var outOffset, (int)StartOffset);
            OutOffset = outOffset;
            return RetVal;
        }
        #endregion

        #region Consume
        /// <summary>
        /// Returns the first unconsumed item from the stream and progresses the current reading position
        /// </summary>
        public T Consume()
        {
            if (LongPosition >= (ulong)Data.Span.Length) return EOF_ITEM;

            T retVal = Data.Span[(int)LongPosition];
            Position += 1;

            return retVal;
        }
        /// <summary>
        /// Returns the first unconsumed item from the stream and progresses the current reading position
        /// </summary>
        public CastType Consume<CastType>() where CastType : T
        {
            if (LongPosition >= (ulong)Data.Span.Length) return default(CastType);

            T retVal = Data.Span[(int)LongPosition];
            Position += 1;

            return (CastType)retVal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<T> _consume(int Count)
        {
            var startIndex = Position;
            var endIndex = (Position + Count);

            /*if (endIndex >= Data.Span.Length)
            {
                endIndex = ((Data.Span.Length) - 1);
            }*/

            Position = endIndex;
            return Data.Span.Slice(startIndex, Count);
        }

        /// <summary>
        /// Returns the specified number of items from the stream and progresses the current reading position by that number
        /// </summary>
        /// <param name="Count">Number of characters to consume</param>
        public ReadOnlySpan<T> Consume(int Count = 1)
        {
            return _consume(Count);
        }
        /// <summary>
        /// Returns the specified number of items from the stream and progresses the current reading position by that number
        /// </summary>
        /// <param name="Count">Number of characters to consume</param>
        public ReadOnlySpan<T> Consume(uint Count)
        {
            return _consume((int)Count);
        }
        /// <summary>
        /// Returns the specified number of items from the stream and progresses the current reading position by that number
        /// </summary>
        /// <param name="Count">Number of characters to consume</param>
        public ReadOnlySpan<T> Consume(long Count)
        {
            return _consume((int)Count);
        }
        /// <summary>
        /// Returns the specified number of items from the stream and progresses the current reading position by that number
        /// </summary>
        /// <param name="Count">Number of characters to consume</param>
        public ReadOnlySpan<T> Consume(ulong Count)
        {
            return _consume((int)Count);
        }
        #endregion

        #region Consume While
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _consume_while(Predicate<T> Predicate)
        {
            bool consumed = Predicate(Next);
            while (Predicate(Next) && !atEnd) { Consume(); }

            return consumed;
        }

        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate)
        {
            return _consume_while(Predicate);
        }

        // ================================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _consume_while(Predicate<T> Predicate, int Limit)
        {
            bool consumed = Predicate(Next);
            var limit = Limit;
            while (Predicate(Next) && !atEnd && limit-- >= 0) { Consume(); }

            return consumed;
        }

        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, int Limit)
        {
            return _consume_while(Predicate, (int)Limit);
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, uint Limit)
        {
            return _consume_while(Predicate, (int)Limit);
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, long Limit)
        {
            return _consume_while(Predicate, (int)Limit);
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, ulong Limit)
        {
            return _consume_while(Predicate, (int)Limit);
        }

        // ================================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _consume_while(Predicate<T> Predicate, out int outStart, out int outEnd)
        {
            outStart = Position;
            bool consumed = Predicate(Next);
            while (Predicate(Next) && !atEnd) { Consume(); }

            outEnd = Position;
            return consumed;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _consume_while(Predicate<T> Predicate, int Limit, out int outStart, out int outEnd)
        {
            outStart = Position;
            bool consumed = Predicate(Next);
            var limit = Limit;
            while (Predicate(Next) && !atEnd && limit-- >= 0) { Consume(); }

            outEnd = Position;
            return consumed;
        }

        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlyMemory<T> outConsumed)
        {
            bool RetVal = _consume_while(Predicate, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Slice(outStart, Count);
            return RetVal;
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlySpan<T> outConsumed)
        {
            bool RetVal = _consume_while(Predicate, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Span.Slice(outStart, Count);
            return RetVal;
        }


        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlyMemory<T> outConsumed, int Limit)
        {
            bool RetVal = _consume_while(Predicate, (int)Limit, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Slice(outStart, Count);
            return RetVal;
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlyMemory<T> outConsumed, uint Limit)
        {
            bool RetVal = _consume_while(Predicate, (int)Limit, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Slice(outStart, Count);
            return RetVal;
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlyMemory<T> outConsumed, long Limit)
        {
            bool RetVal = _consume_while(Predicate, (int)Limit, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Slice(outStart, Count);
            return RetVal;
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlyMemory<T> outConsumed, ulong Limit)
        {
            bool RetVal = _consume_while(Predicate, (int)Limit, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Slice(outStart, Count);
            return RetVal;
        }


        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlySpan<T> outConsumed, int Limit)
        {
            bool RetVal = _consume_while(Predicate, (int)Limit, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Span.Slice(outStart, Count);
            return RetVal;
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlySpan<T> outConsumed, uint Limit)
        {
            bool RetVal = _consume_while(Predicate, (int)Limit, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Span.Slice(outStart, Count);
            return RetVal;
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlySpan<T> outConsumed, long Limit)
        {
            bool RetVal = _consume_while(Predicate, (int)Limit, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Span.Slice(outStart, Count);
            return RetVal;
        }
        /// <summary>
        /// Consumes items until reaching the first one that does not match the given predicate, then returns all matched items and progresses the current reading position by that number
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns>True if atleast a single item was consumed</returns>
        public bool Consume_While(Predicate<T> Predicate, out ReadOnlySpan<T> outConsumed, ulong Limit)
        {
            bool RetVal = _consume_while(Predicate, (int)Limit, out var outStart, out var outEnd);
            var Count = outEnd - outStart;
            outConsumed = Data.Span.Slice(outStart, Count);
            return RetVal;
        }
        #endregion

        #region Reconsume
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _reconsume(int Count)
        {
            if (Count > Position) throw new ArgumentOutOfRangeException($"{nameof(Count)} exceeds the number of items consumed.");
            Position -= Count;
        }

        /// <summary>
        /// Pushes the given number of items back onto the front of the stream
        /// </summary>
        /// <param name="Count"></param>
        public void Reconsume(int Count = 1)
        {
            _reconsume((int)Count);
        }
        /// <summary>
        /// Pushes the given number of items back onto the front of the stream
        /// </summary>
        /// <param name="Count"></param>
        public void Reconsume(uint Count)
        {
            _reconsume((int)Count);
        }
        /// <summary>
        /// Pushes the given number of items back onto the front of the stream
        /// </summary>
        /// <param name="Count"></param>
        public void Reconsume(long Count)
        {
            _reconsume((int)Count);
        }
        /// <summary>
        /// Pushes the given number of items back onto the front of the stream
        /// </summary>
        /// <param name="Count"></param>
        public void Reconsume(ulong Count)
        {
            _reconsume((int)Count);
        }
        #endregion

        #region SubStream
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DataConsumer<T> _substream(int Count)
        {
            var Remain = (Data.Length - Position);
            if (Count > Remain) throw new ArgumentOutOfRangeException($"{nameof(Count)} exceeds the number of remaining items.");
            var consumed = Data.Slice(Position, Count);
            Position += Count;
            return new DataConsumer<T>(consumed, EOF_ITEM);
        }

        /// <summary>
        /// Consumes the number of items specified by <paramref name="Count"/> and then returns them as a new stream, progressing this streams reading position to the end of the consumed items
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public DataConsumer<T> Substream(int Count)
        {
            return _substream((int)Count);
        }
        /// <summary>
        /// Consumes the number of items specified by <paramref name="Count"/> and then returns them as a new stream, progressing this streams reading position to the end of the consumed items
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public DataConsumer<T> Substream(uint Count)
        {
            return _substream((int)Count);
        }
        /// <summary>
        /// Consumes the number of items specified by <paramref name="Count"/> and then returns them as a new stream, progressing this streams reading position to the end of the consumed items
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public DataConsumer<T> Substream(long Count)
        {
            return _substream((int)Count);
        }
        /// <summary>
        /// Consumes the number of items specified by <paramref name="Count"/> and then returns them as a new stream, progressing this streams reading position to the end of the consumed items
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public DataConsumer<T> Substream(ulong Count)
        {
            return _substream((int)Count);
        }

        // ====================================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DataConsumer<T> _substream(int Offset, int? count)
        {
            if (Offset < 0) throw new ArgumentOutOfRangeException(nameof(Offset));
            Contract.EndContractBlock();

            var Count = count;
            if (!Count.HasValue) Count = Length - (Position + Offset);

            var Remain = (Data.Length - Position);
            if (Count > Remain) throw new ArgumentOutOfRangeException($"{nameof(count)} exceeds the number of remaining items.");
            Position += Offset;
            var consumed = Data.Slice(Position, Count.Value);
            Position += Count.Value;
            return new DataConsumer<T>(consumed, EOF_ITEM);
        }

        /// <summary>
        /// Consumes the number of items specified by <paramref name="Count"/> and then returns them as a new stream, progressing this streams reading position to the end of the consumed items
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public DataConsumer<T> Substream(int offset, int? Count)
        {
            return _substream((int)offset, (int?)Count);
        }
        /// <summary>
        /// Consumes the number of items specified by <paramref name="Count"/> and then returns them as a new stream, progressing this streams reading position to the end of the consumed items
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public DataConsumer<T> Substream(uint offset, uint? Count)
        {
            return _substream((int)offset, (int?)Count);
        }
        /// <summary>
        /// Consumes the number of items specified by <paramref name="Count"/> and then returns them as a new stream, progressing this streams reading position to the end of the consumed items
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public DataConsumer<T> Substream(long offset, long? Count)
        {
            return _substream((int)offset, (int?)Count);
        }
        /// <summary>
        /// Consumes the number of items specified by <paramref name="Count"/> and then returns them as a new stream, progressing this streams reading position to the end of the consumed items
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public DataConsumer<T> Substream(ulong offset, ulong? Count)
        {
            return _substream((int)offset, (int?)Count);
        }

        /// <summary>
        /// Consumes items until reaching the first one that does not match the given <paramref name="Predicate"/>, progressing this streams reading position by that number and then returning all matched items as new stream
        /// </summary>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public DataConsumer<T> Substream(Predicate<T> Predicate)
        {
            var startIndex = Position;

            while (Predicate(Next)) { Consume(); }

            var count = Position - startIndex;
            var consumed = Data.Slice(startIndex, count);

            return new DataConsumer<T>(consumed, EOF_ITEM);
        }
        #endregion

        #region Slicing

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlyMemory<T> _slice(int offset)
        {
            int n = Position + offset;
            if (n < 0) throw new IndexOutOfRangeException();

            var index = (Data.Length < n ? Data.Length : n);
            var count = (Data.Length - index);
            return Data.Slice(index, count);
        }

        /// <summary>
        /// Returns a slice of this streams memory containing all of the data after current stream position + <paramref name="offset"/>
        /// </summary>
        /// <param name="offset">Offset from the current stream position where the memory slice to begin</param>
        /// <returns></returns>
        public ReadOnlyMemory<T> Slice(int offset = 0)
        {
            return _slice((int)offset);
        }
        /// <summary>
        /// Returns a slice of this streams memory containing all of the data after current stream position + <paramref name="offset"/>
        /// </summary>
        /// <param name="offset">Offset from the current stream position where the memory slice to begin</param>
        /// <returns></returns>
        public ReadOnlyMemory<T> Slice(uint offset)
        {
            return _slice((int)offset);
        }
        /// <summary>
        /// Returns a slice of this streams memory containing all of the data after current stream position + <paramref name="offset"/>
        /// </summary>
        /// <param name="offset">Offset from the current stream position where the memory slice to begin</param>
        /// <returns></returns>
        public ReadOnlyMemory<T> Slice(long offset)
        {
            return _slice((int)offset);
        }
        /// <summary>
        /// Returns a slice of this streams memory containing all of the data after current stream position + <paramref name="offset"/>
        /// </summary>
        /// <param name="offset">Offset from the current stream position where the memory slice to begin</param>
        /// <returns></returns>
        public ReadOnlyMemory<T> Slice(ulong offset)
        {
            return _slice((int)offset);
        }

        // =======================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlyMemory<T> _slice(int offset, int count)
        {
            int n = Position + offset;
            if (n < 0) throw new IndexOutOfRangeException();
            if (count > Remaining) throw new ArgumentOutOfRangeException(nameof(count));

            var index = (Data.Length < n ? Data.Length : n);
            return Data.Slice(index, count);
        }

        /// <summary>
        /// Returns a slice of this streams memory containing all of the data after current stream position + <paramref name="offset"/>
        /// </summary>
        /// <param name="offset">Offset from the current stream position where the memory slice to begin</param>
        /// <param name="count">The number of items to include in the slice</param>
        /// <returns></returns>
        public ReadOnlyMemory<T> Slice(int offset, int count)
        {
            return _slice((int)offset, (int)count);
        }
        /// <summary>
        /// Returns a slice of this streams memory containing all of the data after current stream position + <paramref name="offset"/>
        /// </summary>
        /// <param name="offset">Offset from the current stream position where the memory slice to begin</param>
        /// <param name="count">The number of items to include in the slice</param>
        /// <returns></returns>
        public ReadOnlyMemory<T> Slice(uint offset, uint count)
        {
            return _slice((int)offset, (int)count);
        }
        /// <summary>
        /// Returns a slice of this streams memory containing all of the data after current stream position + <paramref name="offset"/>
        /// </summary>
        /// <param name="offset">Offset from the current stream position where the memory slice to begin</param>
        /// <param name="count">The number of items to include in the slice</param>
        /// <returns></returns>
        public ReadOnlyMemory<T> Slice(long offset, long count)
        {
            return _slice((int)offset, (int)count);
        }
        /// <summary>
        /// Returns a slice of this streams memory containing all of the data after current stream position + <paramref name="offset"/>
        /// </summary>
        /// <param name="offset">Offset from the current stream position where the memory slice to begin</param>
        /// <param name="count">The number of items to include in the slice</param>
        /// <returns></returns>
        public ReadOnlyMemory<T> Slice(ulong offset, ulong count)
        {
            return _slice((int)offset, (int)count);
        }
        #endregion

        #region Cloning
        /// <summary>
        /// Creates and returns a copy of this stream
        /// </summary>
        /// <returns></returns>
        public DataConsumer<T> Clone()
        {
            return new DataConsumer<T>(Data, EOF_ITEM) { LongPosition = this.LongPosition };
        }
        #endregion

        #region Overrides
        public override string ToString() => Slice(0).ToString();
        #endregion
    }

    public static class DataConsumerExtensions
    {
        public static bool IsNullOrEmpty<T>(this DataConsumer<T> Stream)
        {
            return (Stream is null || Stream.Length <= 0);
        }
    }
}
