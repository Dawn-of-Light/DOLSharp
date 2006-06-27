/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections;
using System.Reflection;
using DOL.GS.Utils;
using log4net;
using Hashtable = DOL.GS.Collections.Hashtable;
using DictionaryEntry = DOL.GS.Collections.DictionaryEntry;

namespace DOL.GS.Collections {
	class DictionaryEntry {
		public object key = null;
		public object value = null;
	}
		
	public sealed class Hashtable {

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static int LIST_SIZE_THRESHOLD = 3;

		private float loadFactor = 0.8f;

		private DictionaryEntry[] internalArray = null;
		
		private int[] nextInsertIndex = null;
		
		/// <summary>
		/// Used for start and end indexes of sublists
		/// bucketListIndexes[0] = start of list 0
		/// bucketListIndexes[1] = end of list 0 = start of list 1
		/// ...
		/// In general: end of list i = start of list i+1
		/// This explains why we need an extra cell to store the end index of last list in the array
		/// </summary>
		private int[] bucketListIndexes = null;

		private int usedSlots = 0;
		
		private int BUCKET_NUMBER = 6;
		private int BUCKET_LIST_SIZE = 3;


		#region Constructors
		
		public Hashtable() {
			Init();
		}

		public Hashtable(float loadFactor) {
			if ((loadFactor <= 0.0f) || (loadFactor > 1.0f)) {
				throw new ArgumentOutOfRangeException("The load factor must be between 0.0f exclusive and 1.0f inclusive");
			}

			this.loadFactor = loadFactor;
			Init();
		}

		public Hashtable(int initialSize) {
			if (initialSize <= 0) {
				throw new ArgumentOutOfRangeException("The initial size of a " + this.GetType() + " may not be neither 0 nor a negative number");
			}

			if (initialSize < LIST_SIZE_THRESHOLD) {
				BUCKET_NUMBER = 1;
				BUCKET_LIST_SIZE = initialSize;
			} else {
				BUCKET_NUMBER = initialSize / LIST_SIZE_THRESHOLD;
				BUCKET_LIST_SIZE = LIST_SIZE_THRESHOLD;
			}

			Init();
		}

		public Hashtable(int initialSize, float loadFactor) {
			if (initialSize <= 0) {
				throw new ArgumentOutOfRangeException("The initial size of a " + this.GetType() + " may not be neither 0 nor a negative number");
			}

			if ((loadFactor <= 0.0f) || (loadFactor > 1.0f)) {
				throw new ArgumentOutOfRangeException("The load factor must be between 0.0f exclusive and 1.0f inclusive");
			}

			this.loadFactor = loadFactor;

			if (initialSize < LIST_SIZE_THRESHOLD) {
				BUCKET_NUMBER = 1;
				BUCKET_LIST_SIZE = initialSize;
			} else {
				BUCKET_NUMBER = initialSize / LIST_SIZE_THRESHOLD;
				BUCKET_LIST_SIZE = LIST_SIZE_THRESHOLD;
			}

			Init();
		}

		#endregion
		
		private void Init() {
			int totalSlots = BUCKET_NUMBER * BUCKET_LIST_SIZE;

			internalArray = new DictionaryEntry[totalSlots];
			nextInsertIndex = new int[BUCKET_NUMBER];
			
			bucketListIndexes = new int[BUCKET_NUMBER + 1];

			int index;
			for (int i = 1; i < BUCKET_NUMBER; i++) {
				index = i * BUCKET_LIST_SIZE;
				bucketListIndexes[i] = index;
				nextInsertIndex[i] = index;
			}

			bucketListIndexes[BUCKET_NUMBER] = totalSlots;

			usedSlots = 0;
		}

		
		/// <summary>
		/// Tries to add one free slot to startBucket bucket by getting one from upward buckets
		/// NOTE : the start bucket is supposed to not contain any free slot and will not be used
		/// </summary>
		/// <param name="startBucket">the start bucket index</param>
		/// <returns>the index of the bucket from which a free slot has been taken or -1 if none upward</returns>
		private int GetForwardFreeSlot(int startBucket) {
			int currentBucket = startBucket + 1;

			// search for first bucket with a free slot
			while ((currentBucket < BUCKET_NUMBER) && (nextInsertIndex[currentBucket] >= bucketListIndexes[currentBucket + 1])) {
				currentBucket++;
			}

			if (currentBucket < BUCKET_NUMBER) {
				int foundBucket = currentBucket;

				// swap first and after last elements in all buckets from foundBucket (inclusive) to startBucket (exclusive)
				while (currentBucket > startBucket) {
					// there is some space left in the current bucket

					// swap the first element with the first free position (after the last element)
					// and decrease the current list size by one (a bit cryptic but it is correct... ;-))
					internalArray[nextInsertIndex[currentBucket]++] = internalArray[bucketListIndexes[currentBucket--]++];
				}
				
				internalArray[nextInsertIndex[startBucket]] = null;

				return foundBucket;
			}
			
			return -1;
		}


		/// <summary>
		/// Frees up one slot in upward buckets
		/// NOTE : the start bucket is supposed to not contain any free slot and will not be used
		/// </summary>
		/// <param name="startBucket"></param>
		/// <returns></returns>
		private int GetBackwardFreeSlot(int startBucket) {
			int currentBucket = startBucket - 1;

			// search for first bucket with a free slot
			while ((currentBucket >= 0) && (nextInsertIndex[currentBucket] >= bucketListIndexes[currentBucket + 1])) {
				currentBucket--;
			}

			if (currentBucket >= 0) {
				int foundBucket = currentBucket;

				// swap first and after last elements in all buckets from foundBucket (inclusive) to startBucket (exclusive)
				while (++currentBucket <= startBucket) {
					// there is some space left at the end of the list before current bucket
					
					// take last slot of previous list
					// and put the last element in the first position (which has just been freed)
					internalArray[--bucketListIndexes[currentBucket]] = internalArray[--nextInsertIndex[currentBucket]];
				}

				internalArray[nextInsertIndex[startBucket]] = null;

				return foundBucket;
			}
			
			return -1;
		}


		public void ShowStructure() {
			for (int i = 0; i < BUCKET_NUMBER; i++) {
				Console.Out.Write("Bucket " + i + ":");

				for (int j = bucketListIndexes[i]; j < nextInsertIndex[i]; j++) {
					Console.Out.Write(" [" + internalArray[j].key + "," + internalArray[j].value + ")");
				}

				Console.Out.WriteLine(" (" + (bucketListIndexes[i+1] - nextInsertIndex[i]) + ")");
			}
		}

		
		private void Rehash() {
			int newSize = 2*internalArray.Length;
			BUCKET_NUMBER = (2*BUCKET_NUMBER) + 1;
			BUCKET_LIST_SIZE = (newSize / BUCKET_NUMBER) + 1;

			if (BUCKET_LIST_SIZE > LIST_SIZE_THRESHOLD) {
				// recompute if first attempt was not successful, performance wise
				BUCKET_NUMBER = (newSize / LIST_SIZE_THRESHOLD) + 1;
				BUCKET_LIST_SIZE = (newSize / BUCKET_NUMBER) + 1;
			}

			DictionaryEntry[] oldInternalArray = internalArray;

			Init();

			for (int i = 0; i < oldInternalArray.Length; i++) {
				if (oldInternalArray[i] != null) {
					ForceAdd(oldInternalArray[i]);
				}
			}
		}


		/// <summary>
		/// IMPORTANT : 
		/// 1) may ONLY called after a rehash has occured
		/// 2) internalArray MUST have enough space to store gameObject (ensured by rehash())
		/// 3) gameObject may NOT be in internalArray already
		/// 
		/// If for some reasons, the 3 statements above were not fulfilled, the structure may be corrupt !!!!!
		/// </summary>
		/// <param name="entry"></param>
		private void ForceAdd(DictionaryEntry entry) {
			int bucketNumber = (entry.key.GetHashCode() & Int32.MaxValue) % BUCKET_NUMBER;

			if (nextInsertIndex[bucketNumber] >= bucketListIndexes[bucketNumber + 1]) {
				// current bucket list is full

				// try to get a slot from another bucket
				int usedBucket = GetForwardFreeSlot(bucketNumber);

				if (usedBucket == -1) {
					usedBucket = GetBackwardFreeSlot(bucketNumber);
				}
			}

			// normal add (a free slot has been granted by another bucket or there was enough space left in this bucket)
			internalArray[nextInsertIndex[bucketNumber]++] = entry;

			usedSlots++;
		}


		public object this[object key] {
			get {
				int bucketNumber = (key.GetHashCode() & Int32.MaxValue) % BUCKET_NUMBER;

				int i=-1;
				try {
					for (i = bucketListIndexes[bucketNumber]; i < nextInsertIndex[bucketNumber]; i++) {
						if (internalArray[i].key.Equals(key)) {
							return internalArray[i].value;
						}
					}					
				} catch (Exception e) {
					log.Warn("bucket="+bucketNumber+" bucketMaxList="+bucketListIndexes.Length+" bucketMaxNext="+nextInsertIndex.Length);
					log.Warn("internalArraySize="+internalArray.Length);
					log.Warn("checking "+i);
					log.Error(e);
					throw e;
				}
				return null;
			}

			set {
				Add(key, value);
			}
		}


		public object Remove(object key) {
			int bucketNumber = (key.GetHashCode() & Int32.MaxValue) % BUCKET_NUMBER;

			for (int i = bucketListIndexes[bucketNumber]; i < nextInsertIndex[bucketNumber]; i++) {
				if (internalArray[i].key.Equals(key)) {
					object value = internalArray[i].value;

					// swap this element (to be removed) with the last one
					int lastIndex = --nextInsertIndex[bucketNumber];

					if (i != lastIndex) {
						// we do not swap if we are removing the last element of the list
						internalArray[i] = internalArray[lastIndex];
					}

					internalArray[lastIndex] = null;

					usedSlots--;

					return value;
				}
			}

			return null;
		}

		public void Add(object key, object value) {
				int bucketNumber = (key.GetHashCode() & Int32.MaxValue) % BUCKET_NUMBER;

			for (int i = bucketListIndexes[bucketNumber]; i < nextInsertIndex[bucketNumber]; i++) {
				if (internalArray[i].key.Equals(key)) {
					// object found => replace associated value
					internalArray[i].value = value;
					return;
				}
			}

			// object not found
			// => add it
			DictionaryEntry entry = new DictionaryEntry();
			entry.key = key;
			entry.value = value;
			if (nextInsertIndex[bucketNumber] >= bucketListIndexes[bucketNumber + 1]) {
				// current bucket list is full

				if (LoadFactor >= loadFactor) {
					//Console.WriteLine(this.GetType() + " LoadFactor exceeded - rehashing... (" + loadFactor + ", " + LoadFactor + ")");
					Rehash();
					ForceAdd(entry);

					return;
				}

				// try to get a slot from another bucket
				int usedBucket = GetForwardFreeSlot(bucketNumber);

				if (usedBucket == -1) {
					usedBucket = GetBackwardFreeSlot(bucketNumber);
				}

				if ((usedBucket == -1) || (FastMath.Abs(usedBucket - bucketNumber) >= 7)) {
					// no free slots left or getting one becomes too costly

					Rehash();
					ForceAdd(entry);
						
					return;
				}
			}

			// normal add (a free slot has been granted by another bucket or there was enough space left in this bucket)
			internalArray[nextInsertIndex[bucketNumber]++] = entry;

			usedSlots++;
		}

		public void Clear() {
			if (usedSlots > 0) {
				Init();
			}
		}

		public float LoadFactor {
			get {
				return ((float) usedSlots) / internalArray.Length;
			}
		}

		public int Count {
			get {
				return usedSlots;
			}
		}

		public object SyncRoot {
			get {
				return this;
			}
		}
		

		public int Capacity {
			get {
				return internalArray.Length;
			}
		}


		public IEnumerator GetValueEnumerator() {
			return new ValueEnumerator(this);
		}

		public IEnumerator GetKeyEnumerator() {
			return new KeyEnumerator(this);
		}

		public IEnumerator GetEntryEnumerator() {
			return new DictionaryEntryEnumerator(this);
		}

		#region Iterators
		private class DictionaryEntryEnumerator : IEnumerator {
			protected Hashtable table = null;
			protected int currentBucketNumber = 0;
			protected int currentIndex = 0;
			protected DictionaryEntry currentEntry = null;

			public DictionaryEntryEnumerator(Hashtable table) {
				this.table = table;
			}

			public virtual object Current {
				get {
					return currentEntry;
				}
			}

			public bool MoveNext() {
				while ((currentIndex < table.internalArray.Length) && (currentIndex >= table.nextInsertIndex[currentBucketNumber])) {
					currentIndex = table.bucketListIndexes[++currentBucketNumber];
				}

				if (currentIndex >= table.internalArray.Length) {
					return false;
				} else {
					currentEntry = table.internalArray[currentIndex++];
					return true;
				}
			}

			public void Reset() {
				currentBucketNumber = 0;
				currentIndex = 0;
			}
		}


		private class KeyEnumerator : DictionaryEntryEnumerator {

			public KeyEnumerator(Hashtable table) : base(table) { }

			public override object Current {
				get {
					return (currentEntry != null) ? currentEntry.key : null;
				}
			}
		}


		private class ValueEnumerator : DictionaryEntryEnumerator {

			public ValueEnumerator(Hashtable table) : base(table) { }

			public override object Current {
				get {
					return (currentEntry != null) ? currentEntry.value : null;
				}
			}
		}

		#endregion
	}
}
