using Leyoutech.Utility;
using System;
using System.Collections;
using System.Collections.Generic;

public class BetterQueue<T>
	: IEnumerable<T>
	, ICollection
	, IReadOnlyCollection<T>
{
	private const string LOG_TAG = "BetterQueue";
	private const int MINIMUM_GROW = 4;
	private const int SHRINK_THRESHOLD = 32;
	private const int GROW_FACTOR = 2;
	private const int DEFAULT_CAPACITY = 4;
	private const double TRIM_EXCESS_FACTOR = 0.9;

	private readonly static T[] EMPTY_ARRAY = new T[0];

	private T[] m_Array;
	/// <summary>
	/// First valid element in the queue
	/// </summary>
	private int m_Head;
	/// <summary>
	/// Last valid element in the queue
	/// </summary>
	private int m_Tail;
	/// <summary>
	/// Number of elements.
	/// </summary>
	private int m_Size;
	private int m_Version;
	private object m_SyncRoot;

	public T this[int index]
	{
		get
		{
			DebugUtility.Assert(index >= 0 && index < m_Size, LOG_TAG, "index >= 0 && index < m_Size");
			return GetElement(index);
		}
		set
		{
			SetElement(index, value);
		}
	}

	public int Count
	{
		get { return m_Size; }
	}

	bool ICollection.IsSynchronized
	{
		get { return false; }
	}

	object ICollection.SyncRoot
	{
		get
		{
			if (m_SyncRoot == null)
			{
				System.Threading.Interlocked.CompareExchange<object>(ref m_SyncRoot, new object(), null);
			}
			return m_SyncRoot;
		}
	}

	public BetterQueue()
	{
		m_Array = EMPTY_ARRAY;
	}

	public BetterQueue(int capacity)
	{
		DebugUtility.Assert(capacity >= 0, LOG_TAG, "capacity >= 0");

		m_Array = new T[capacity];
		m_Head = 0;
		m_Tail = 0;
		m_Size = 0;
	}

	public BetterQueue(IEnumerable<T> collection)
	{
		DebugUtility.Assert(collection != null, LOG_TAG, "collection != null");

		m_Array = new T[DEFAULT_CAPACITY];
		m_Size = 0;
		m_Version = 0;

		using (IEnumerator<T> enumerator = collection.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Enqueue(enumerator.Current);
			}
		}
	}

	public void Clear()
	{
		if (m_Head < m_Tail)
		{ 
			Array.Clear(m_Array, m_Head, m_Size);
		}
		else
		{
			Array.Clear(m_Array, m_Head, m_Array.Length - m_Head);
			Array.Clear(m_Array, 0, m_Tail);
		}

		m_Head = 0;
		m_Tail = 0;
		m_Size = 0;
		m_Version++;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		DebugUtility.Assert(array != null, LOG_TAG, "array != null");
		int arrayLenth = array.Length;
		DebugUtility.Assert(arrayIndex >= 0 && arrayIndex <= arrayLenth, LOG_TAG, "arrayIndex >= 0 && arrayIndex <= arrayLenth");
		DebugUtility.Assert(arrayLenth - arrayIndex >= m_Size, LOG_TAG, "arrayLenth - arrayIndex >= m_Size");

		int numToCopy = (arrayLenth - arrayIndex < m_Size) 
			? (arrayLenth - arrayIndex) 
			: m_Size;

		if (numToCopy == 0)
		{
			return;
		}

		int firstPart = (m_Array.Length - m_Head < numToCopy) 
			? m_Array.Length - m_Head 
			: numToCopy;
		Array.Copy(m_Array, m_Head, array, arrayIndex, firstPart);

		numToCopy -= firstPart;
		if (numToCopy > 0)
		{
			Array.Copy(m_Array, 0, array, arrayIndex + m_Array.Length - m_Head, numToCopy);
		}
	}

	public void Enqueue(T item)
	{
		if (m_Size == m_Array.Length)
		{
			int newcapacity = m_Array.Length * GROW_FACTOR;
			if (newcapacity < m_Array.Length + MINIMUM_GROW)
			{
				newcapacity = m_Array.Length + MINIMUM_GROW;
			}
			SetCapacity(newcapacity);
		}

		m_Array[m_Tail] = item;
		m_Tail = (m_Tail + 1) % m_Array.Length;
		m_Size++;
		m_Version++;
	}

	public T Dequeue()
	{
		DebugUtility.Assert(m_Size > 0, LOG_TAG, "m_Size > 0");

		T removed = m_Array[m_Head];
		m_Array[m_Head] = default(T);
		m_Head = (m_Head + 1) % m_Array.Length;
		m_Size--;
		m_Version++;
		return removed;
	}

	public T PeekHead()
	{
		DebugUtility.Assert(m_Size > 0, LOG_TAG, "m_Size > 0");

		return m_Array[m_Head];
	}

	public T PeekTail()
	{
		DebugUtility.Assert(m_Size > 0, LOG_TAG, "m_Size > 0");

		return m_Array[(m_Tail - 1) % m_Array.Length];
	}

	public bool Contains(T item)
	{
		int index = m_Head;
		int count = m_Size;

		EqualityComparer<T> c = EqualityComparer<T>.Default;
		while (count-- > 0)
		{
			if (item == null)
			{
				if (m_Array[index] == null)
				{ 
					return true;
				}
			}
			else if (m_Array[index] != null 
				&& c.Equals(m_Array[index], item))
			{
				return true;
			}
			index = (index + 1) % m_Array.Length;
		}

		return false;
	}

	public T GetElement(int index)
	{
		DebugUtility.Assert(index >= 0 && index < m_Size, LOG_TAG, "index >= 0 && index < m_Size");
		return m_Array[(m_Head + index) % m_Array.Length];
	}

	public void SetElement(int index, T value)
	{
		DebugUtility.Assert(index >= 0 && index < m_Size, LOG_TAG, "index >= 0 && index < m_Size");
		m_Array[(m_Head + index) % m_Array.Length] = value;
		m_Version++;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public T[] ToArray()
	{
		if (m_Size == 0)
		{
			return EMPTY_ARRAY;
		}

		T[] arr = new T[m_Size];
		if (m_Head < m_Tail)
		{
			Array.Copy(m_Array, m_Head, arr, 0, m_Size);
		}
		else
		{
			Array.Copy(m_Array, m_Head, arr, 0, m_Array.Length - m_Head);
			Array.Copy(m_Array, 0, arr, m_Array.Length - m_Head, m_Tail);
		}

		return arr;
	}

	public void TrimExcess()
	{
		int threshold = (int)(m_Array.Length * TRIM_EXCESS_FACTOR);
		if (m_Size < threshold)
		{
			SetCapacity(m_Size);
		}
	}

	private void SetCapacity(int capacity)
	{
		T[] newarray = new T[capacity];
		if (m_Size > 0)
		{
			if (m_Head < m_Tail)
			{
				Array.Copy(m_Array, m_Head, newarray, 0, m_Size);
			}
			else
			{
				Array.Copy(m_Array, m_Head, newarray, 0, m_Array.Length - m_Head);
				Array.Copy(m_Array, 0, newarray, m_Array.Length - m_Head, m_Tail);
			}
		}

		m_Array = newarray;
		m_Head = 0;
		m_Tail = (m_Size == capacity) ? 0 : m_Size;
		m_Version++;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		DebugUtility.Assert(array != null, LOG_TAG, "array != null");
		DebugUtility.Assert(array.Rank == 1, LOG_TAG, "array.Rank == 1");
		DebugUtility.Assert(array.GetLowerBound(0) == 0, LOG_TAG, "array.GetLowerBound(0) == 0");
		int arrayLength = array.Length;
		DebugUtility.Assert(index >= 0 || index <= arrayLength, LOG_TAG, "index >= 0 || index <= arrayLength");
		DebugUtility.Assert(arrayLength - index >= m_Size, LOG_TAG, "arrayLength - index >= m_Size");

		int numToCopy = (arrayLength - index < m_Size) ? arrayLength - index : m_Size;
		if (numToCopy == 0)
		{
			return;
		}

		try
		{
			int firstPart = (m_Array.Length - m_Head < numToCopy)
				? m_Array.Length - m_Head
				: numToCopy;
			Array.Copy(m_Array, m_Head, array, index, firstPart);
			numToCopy -= firstPart;

			if (numToCopy > 0)
			{
				Array.Copy(m_Array, 0, array, index + m_Array.Length - m_Head, numToCopy);
			}
		}
		catch (ArrayTypeMismatchException)
		{
			DebugUtility.Assert(false, LOG_TAG, "ArrayTypeMismatchException");
		}
	}

	public struct Enumerator
		: IEnumerator<T>
		, IEnumerator
	{
		private BetterQueue<T> m_Queue;
		/// <summary>
		/// -1 = not started, -2 = ended/disposed
		/// </summary>
		private int m_Index;
		private int m_StartVersion;
		private T m_CurrentElement;

		internal Enumerator(BetterQueue<T> queue)
		{
			m_Queue = queue;
			m_StartVersion = m_Queue.m_Version;
			m_Index = -1;
			m_CurrentElement = default;
		}

		public void Dispose()
		{
			m_Index = -2;
			m_CurrentElement = default;
		}

		public bool MoveNext()
		{
			DebugUtility.Assert(m_StartVersion == m_Queue.m_Version, LOG_TAG, "m_StartVersion == m_Queue.m_Version");

			if (m_Index == -2)
			{ 
				return false;
			}

			m_Index++;

			if (m_Index == m_Queue.m_Size)
			{
				m_Index = -2;
				m_CurrentElement = default;
				return false;
			}

			m_CurrentElement = m_Queue.GetElement(m_Index);
			return true;
		}

		public T Current
		{
			get
			{
				DebugUtility.Assert(m_Index >= 0, LOG_TAG, "m_Index >= 0");
				return m_CurrentElement;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				DebugUtility.Assert(m_Index >= 0, LOG_TAG, "m_Index >= 0");
				return m_CurrentElement;
			}
		}

		void IEnumerator.Reset()
		{
			DebugUtility.Assert(m_StartVersion == m_Queue.m_Version, LOG_TAG, "m_StartVersion == m_Queue.m_Version");
			m_Index = -1;
			m_CurrentElement = default;
		}
	}
}