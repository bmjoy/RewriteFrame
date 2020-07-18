using Leyoutech.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 每个entry会分配一个自增长的Id
/// 移除entry时，尽量从Id小的entry移除，否则会有内存浪费
/// </summary>
/// <remarks>
/// 命名为Queue是因为它的内部实现和Queue比较接近
/// 
/// 例如：
///		capacity为4时，添加了3个entry
///			[0123] capacity
///			[abc-] entries(字母表示entry的id，-表示null)
///		移除第一个entry
///			[0123] capacity
///			[-bc-] entries
///		添加2个entry
///			[0123] capacity
///			[ebcd] entries
///		添加1个entry，此时已满，capacity扩容*2
///			[01234567] capacity
///			[bcdef---] entries
///		移除（c、e）entry并添加3个entry
///			[01234567] capacity
///			[b-d-fghi] entries
///		添加1个entry，此时虽然有空位，但空位的下一个entry的Id值不是最小的，这个空位会被浪费
///			[0123456789012345] capacity
///			[b-d-fghij-------] entries
///			
/// 这个类部分代码没有遵守项目的代码规范，是因为想让这个类和<see cref="System.Collections"/>尽量接近
/// </remarks>
public sealed class AutoIncrementQueue<T>
{
	private const int DEFAULT_CAPACITY = 4;
	/// <summary>
	/// <see cref="Array.MaxArrayLength"/>
	/// </summary>
	private const int MAX_ARRAY_LENGTH = 0X7FEFFFFF;
	private const string ALGORITHM_WRONG_ASSERT = "\n应该是代码逻辑写错了，请找黄文淼";

	private static readonly Entry[] ms_Emptyntries = new Entry[0];

	private Entry[] m_Entries;

	/// <summary>
	/// 每次分配一个id的时候会++
	/// 初始值为-1时，第一次分配的id为0
	/// </summary>
	private int m_LastAutoIncrementId = -1;
	/// <summary>
	/// 第一个Entry（Id值最小的Entry）在<see cref="m_Entries"/>中的Index
	/// </summary>
	private int m_FirstEntryIndex = 0;
	/// <summary>
	/// 第一个Entry的Id
	/// </summary>
	private int m_FirstEntryId = 0;

	/// <summary>
	/// 伪单元测试
	/// </summary>
	internal static void _UnitTest()
	{
		AutoIncrementQueue<int> queue = new AutoIncrementQueue<int>(4);
		queue.Add(5); // 0
		queue.Add(8); // 1
		queue.Add(9); // 2
		queue.Add(6); // 3
		Debug.Log(queue.ToString());
		queue.Remove(0);
		queue.Add(3); // 4
		Debug.Log(queue.ToString());
		queue.Add(7); // 5
		queue.Add(2); // 6
		Debug.Log(queue.ToString());
		queue.Remove(1);
		queue.Remove(3);
		Debug.Log(queue.ToString());
		queue.Add(17); // 7
		queue.Add(12); // 8
		queue.Add(15); // 9
		Debug.Log(queue.ToString());
		queue.Add(14); // 10
		queue.Add(13); // 11
		Debug.Log(queue.ToString());
		queue.Remove(2);
		Debug.Log(queue.ToString());
		queue.Capacity = 8;
		Debug.Log(queue.ToString());
		Debug.Log(queue[4]); // log(3)
		queue[4] = 9;
		Debug.Log(queue[4]); // log(9)
	}

	public T this[int id]
	{
		get
		{
			if (AssetVerifyIdInQueue(id))
			{
				int index = IdToIndex(id);
				DebugUtility.Assert(index < m_Entries.Length, "index < m_Entries.Length" + ALGORITHM_WRONG_ASSERT);
				DebugUtility.Assert(m_Entries[index].Used, "m_Entries[index].Used" + ALGORITHM_WRONG_ASSERT);

				return m_Entries[index].Value;
			}
			else
			{
				return default;
			}
		}
		set
		{
			if (AssetVerifyIdInQueue(id))
			{
				int index = IdToIndex(id);
				DebugUtility.Assert(index < m_Entries.Length, "index < m_Entries.Length" + ALGORITHM_WRONG_ASSERT);
				DebugUtility.Assert(m_Entries[index].Used, "m_Entries[index].Used" + ALGORITHM_WRONG_ASSERT);

				m_Entries[index].Value = value;
			}
		}
	}

	/// <summary>
	/// Count是从当前Entry中Id最小的Entry到曾经Id最大的Entry的个数
	/// 即空的Entry也会被计数
	/// 例如：
	///		[-ef-a-c]从a开始到f结束，Count为6
	///		移除f
	///		[-e--a-c]仍然是从a开始到f结束，Count为6
	///		移除a
	///		[-e----c]从c开始到f结束，Count为4
	/// </summary>
	public int Count { get; private set; }

	/// <summary>
	/// Gets and sets the capacity of this queue.
	/// The capacity is the size of the internal array used to hold items.  
	/// When set, the internal array of the queue is reallocated to the given capacity.
	/// </summary>
	public int Capacity
	{
		get
		{
			return m_Entries.Length;
		}
		set
		{
			if (value < Count)
			{
				throw new ArgumentOutOfRangeException("Capacity");
			}

			if (value != m_Entries.Length)
			{
				if (value > 0)
				{
					Entry[] newEntries = new Entry[value];
					for (int iEntry = 0; iEntry < newEntries.Length; iEntry++)
					{
						newEntries[iEntry].Used = false;
					}

					if (Count > 0)
					{
						int lastEntryIndex = IdToIndex(m_LastAutoIncrementId);
						if (lastEntryIndex < m_FirstEntryIndex)
						{
							int firstEntryIndexToEntriesLength = m_Entries.Length - m_FirstEntryIndex;
							Array.Copy(m_Entries, m_FirstEntryIndex
								, newEntries, 0
								, firstEntryIndexToEntriesLength);
							Array.Copy(m_Entries, 0
								, newEntries, firstEntryIndexToEntriesLength
								, lastEntryIndex + 1);
						}
						else
						{
							Array.Copy(m_Entries, m_FirstEntryIndex
								, newEntries, 0
								, Count);
						}
					}

					m_FirstEntryIndex = 0;
					m_Entries = newEntries;
				}
				else
				{
					m_Entries = ms_Emptyntries;
				}
			}
		}
	}

	public AutoIncrementQueue() : this(DEFAULT_CAPACITY) { }

	public AutoIncrementQueue(int capacity)
	{
		DebugUtility.Assert(capacity >= 0, "capacity >= 0");

		m_Entries = capacity == 0
			? ms_Emptyntries
			: new Entry[capacity];
		Count = 0;

		for (int iEntry = 0; iEntry < capacity; iEntry++)
		{
			m_Entries[iEntry].Used = false;
		}
	}

	/// <summary>
	/// 添加一个Entry
	/// </summary>
	/// <returns>这个Entry的Id</returns>
	public int Add(T item)
	{
		int id = m_LastAutoIncrementId + 1;
		int index = IdToIndex(id);
		if (index >= m_Entries.Length
			|| m_Entries[index].Used)
		{
			EnsureCapacity(m_Entries.Length + 1);
			index = IdToIndex(id);
		}

		// 这行要放在EnsureCapacity后面，因为EnsureCapacity里面会用到这个变量
		m_LastAutoIncrementId = id;

		m_Entries[index].Used = true;
		m_Entries[index].Value = item;
		Count++;

		return id;
	}

	/// <summary>
	/// 移除一个Entry
	/// </summary>
	/// <returns>移除是否成功	</returns>
	public bool Remove(int id)
	{
		if (!AssetVerifyIdInQueue(id))
		{
			return false;
		}

		int index = IdToIndex(id);
		DebugUtility.Assert(index < m_Entries.Length, "index < m_Entries.Length" + ALGORITHM_WRONG_ASSERT);
		if (m_Entries[index].Used)
		{
			m_Entries[index].Used = false;
			m_Entries[index].Value = default;
			if (index == m_FirstEntryIndex) // 第一个Entry被移除
			{
				while (true)
				{
					Count--;
					m_FirstEntryIndex = NextIndex(m_FirstEntryIndex);
					m_FirstEntryId++;

					if (m_Entries[m_FirstEntryIndex].Used)
					{
						break;
					}
					else
					{
						DebugUtility.Assert(m_FirstEntryId <= m_LastAutoIncrementId, "m_FirstEntryId <= m_LastAutoIncrementId" + ALGORITHM_WRONG_ASSERT);

						m_Entries[m_FirstEntryIndex].Used = false;
						m_Entries[m_FirstEntryIndex].Value = default;
					}
				}
			}
			return true;
		}
		else
		{
			return false;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = StringUtility.AllocStringBuilderCache();
		stringBuilder.Append("Values:(");
		for (int iEntry = 0; iEntry < m_Entries.Length; iEntry++)
		{
			Entry iterEntry = m_Entries[iEntry];
			stringBuilder.Append(iterEntry.Used ? iterEntry.Value.ToString() : "Unused");
			if (iEntry + 1 < m_Entries.Length)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder.Append(")\n");

		stringBuilder.Append("Ids:(");
		for (int iEntry = 0; iEntry < m_Entries.Length; iEntry++)
		{
			stringBuilder.Append(m_Entries[iEntry].Used
				? IndexToId(iEntry).ToString()
				: "Unused");

			if (iEntry + 1 < m_Entries.Length)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder.Append(")\n");

		stringBuilder.Append("VerifyConvertId&Index:(");
		for (int iEntry = 0; iEntry < m_Entries.Length; iEntry++)
		{
			int id = IndexToId(iEntry);
			stringBuilder.Append(iEntry == IdToIndex(id));

			if (iEntry + 1 < m_Entries.Length)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder.Append(")\n");
		return StringUtility.ReleaseStringBuilderCacheAndReturnString();
	}

	/// <summary>
	/// 验证id是否合法
	/// </summary>
	private bool AssetVerifyIdInQueue(int id)
	{
		DebugUtility.Assert(id <= m_LastAutoIncrementId, "这个Entry还未分配，Id:" + id);
		DebugUtility.Assert(id >= m_FirstEntryId, "这个Entry已经被Remove，Id：" + id);

		return id <= m_LastAutoIncrementId && id >= m_FirstEntryId;
	}

	/// <summary>
	/// Ensures that the capacity of this queue is at least the given minimum value. 
	/// If the currect capacity of the queue is less than min, the capacity is increased to twice the current capacity or to min, whichever is larger.
	/// </summary>
	private void EnsureCapacity(int min)
	{
		if (m_Entries.Length < min)
		{
			int newCapacity = m_Entries.Length == 0
				? DEFAULT_CAPACITY
				: m_Entries.Length * 2;
			// Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
			// Note that this check works even when _items.Length overflowed thanks to the (uint) cast
			if ((uint)newCapacity > MAX_ARRAY_LENGTH)
			{
				newCapacity = MAX_ARRAY_LENGTH;
			}
			if (newCapacity < min)
			{
				newCapacity = min;
			}
			Capacity = newCapacity;
		}
	}

	/// <summary>
	/// 计算entry在<see cref="m_Entries"/>中的Index
	/// </summary>
	private int IdToIndex(int id)
	{
		int index = id - m_FirstEntryId + m_FirstEntryIndex;
		if (index >= m_Entries.Length)
		{
			index -= m_Entries.Length;
		}
		return index;
	}

	/// <summary>
	/// 计算<see cref="m_Entries"/>中某个entry的Id
	/// </summary>
	private int IndexToId(int index)
	{
		int id = index < m_FirstEntryIndex ? index + m_Entries.Length : index;
		id = id - m_FirstEntryIndex + m_FirstEntryId;
		return id;
	}

	private int NextIndex(int index)
	{
		index++;
		return index < m_Entries.Length ? index : 0;
	}

	public struct Entry
	{
		/// <summary>
		/// true if used, false if unused
		/// </summary>
		public bool Used;
		/// <summary>
		/// Value of entry
		/// </summary>
		public T Value;
	}
}