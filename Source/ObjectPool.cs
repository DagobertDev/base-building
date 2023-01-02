using System.Collections.Generic;

namespace BaseBuilding;

public class ObjectPool<T> where T : new()
{
	private readonly Stack<T> _items = new();

	public T Acquire()
	{
		return _items.Count == 0 ? new T() : _items.Pop();
	}

	public void Release(T item)
	{
		_items.Push(item);
	}
}
