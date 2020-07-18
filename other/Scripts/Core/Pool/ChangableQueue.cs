using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangableQueue<T>// : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection
{
	private List<T> m_ObjectList;

	public int Count
	{
		get
		{
			return m_ObjectList.Count;
		}
	}

	public ChangableQueue()
	{
		m_ObjectList = new List<T>();
	}

	public T Dequeue()
	{
		T obj = m_ObjectList[0];
		m_ObjectList.RemoveAt(0);
		return obj;
	}

	public void Enqueue(T item)
	{
		m_ObjectList.Add(item);
	}

	public void Remove(T item)
	{
		m_ObjectList.Remove(item);
	}

	public void Remove(int index)
	{
		m_ObjectList.RemoveAt(index);
	}

	public bool Contains(T item)
	{
		return m_ObjectList.Remove(item);
	}

	public void Clear()
	{
		m_ObjectList.Clear();
	}
}
