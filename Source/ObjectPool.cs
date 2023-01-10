using System;
using System.Collections.Generic;

namespace BaseBuilding;

public class ObjectPool<T> where T : new()
{
	private readonly Action<T>? _resetAction;
	private readonly Stack<T> _items = new();

	public ObjectPool(Action<T> resetAction)
	{
		_resetAction = resetAction;
	}

	public T Acquire()
	{
		return _items.Count == 0 ? new T() : _items.Pop();
	}

	public void Release(T item)
	{
		_resetAction?.Invoke(item);
		_items.Push(item);
	}
}
