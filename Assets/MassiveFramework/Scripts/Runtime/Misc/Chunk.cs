﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MassiveCore.Framework.Runtime
{
    // https://learn.microsoft.com/en-us/dotnet/csharp/linq/group-results-by-contiguous-keys
    // A Chunk is a contiguous group of one or more source elements that have the same key. A Chunk
    // has a key and a list of ChunkItem objects, which are copies of the elements in the source sequence.
    public class Chunk<TKey, TSource> : IGrouping<TKey, TSource>
    {
        // INVARIANT: DoneCopyingChunk == true ||
        //   (predicate != null && predicate(enumerator.Current) && current.Value == enumerator.Current)

        // A Chunk has a linked list of ChunkItems, which represent the elements in the current chunk. Each ChunkItem
        // has a reference to the next ChunkItem in the list.
        public class ChunkItem
        {
            public ChunkItem(TSource value)
            {
                Value = value;
            }

            public readonly TSource Value;
            public ChunkItem? Next;
        }

        // Stores a reference to the enumerator for the source sequence
        private IEnumerator<TSource> enumerator;

        // A reference to the predicate that is used to compare keys.
        private Func<TSource, bool> predicate;

        // Stores the contents of the first source element that
        // belongs with this chunk.
        private readonly ChunkItem head;

        // End of the list. It is repositioned each time a new
        // ChunkItem is added.
        private ChunkItem tail;

        // Flag to indicate the source iterator has reached the end of the source sequence.
        internal bool isLastSourceElement;

        // Private object for thread syncronization
        private readonly object m_Lock;

        // REQUIRES: enumerator != null && predicate != null
        public Chunk(TKey key, IEnumerator<TSource> enumerator, Func<TSource, bool> predicate)
        {
            Key = key;
            this.enumerator = enumerator;
            this.predicate = predicate;

            // A Chunk always contains at least one element.
            head = new ChunkItem(enumerator.Current);

            // The end and beginning are the same until the list contains > 1 elements.
            tail = head;

            m_Lock = new object();
        }

        // Indicates that all chunk elements have been copied to the list of ChunkItems,
        // and the source enumerator is either at the end, or else on an element with a new key.
        // the tail of the linked list is set to null in the CopyNextChunkElement method if the
        // key of the next element does not match the current chunk's key, or there are no more elements in the source.
        private bool DoneCopyingChunk => tail == null;

        // Adds one ChunkItem to the current group
        // REQUIRES: !DoneCopyingChunk && lock(this)
        private void CopyNextChunkElement()
        {
            // Try to advance the iterator on the source sequence.
            // If MoveNext returns false we are at the end, and isLastSourceElement is set to true
            isLastSourceElement = !enumerator.MoveNext();

            // If we are (a) at the end of the source, or (b) at the end of the current chunk
            // then null out the enumerator and predicate for reuse with the next chunk.
            if (isLastSourceElement || !predicate(enumerator.Current))
            {
                enumerator = null;
                predicate = null;
            }
            else
            {
                tail.Next = new ChunkItem(enumerator.Current);
            }

            // tail will be null if we are at the end of the chunk elements
            // This check is made in DoneCopyingChunk.
            tail = tail.Next!;
        }

        // Called after the end of the last chunk was reached. It first checks whether
        // there are more elements in the source sequence. If there are, it
        // Returns true if enumerator for this chunk was exhausted.
        internal bool CopyAllChunkElements()
        {
            while (true)
            {
                lock (m_Lock)
                {
                    if (DoneCopyingChunk)
                    {
                        // If isLastSourceElement is false,
                        // it signals to the outer iterator
                        // to continue iterating.
                        return isLastSourceElement;
                    }
                    else
                    {
                        CopyNextChunkElement();
                    }
                }
            }
        }

        public TKey Key { get; }

        // Invoked by the inner foreach loop. This method stays just one step ahead
        // of the client requests. It adds the next element of the chunk only after
        // the clients requests the last element in the list so far.
        public IEnumerator<TSource> GetEnumerator()
        {
            //Specify the initial element to enumerate.
            ChunkItem current = head;

            // There should always be at least one ChunkItem in a Chunk.
            while (current != null)
            {
                // Yield the current item in the list.
                yield return current.Value;

                // Copy the next item from the source sequence,
                // if we are at the end of our local list.
                lock (m_Lock)
                {
                    if (current == tail)
                    {
                        CopyNextChunkElement();
                    }
                }

                // Move to the next ChunkItem in the list.
                current = current.Next;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
