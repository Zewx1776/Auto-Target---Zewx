using System;
using System.Collections.Generic;

namespace WhereAreYouGoing;

public class BinaryHeap<TKey, TValue> where TKey : IComparable<TKey>
{
    private readonly List<KeyValuePair<TKey, TValue>> _items = new();
    private readonly Dictionary<TValue, int> _indices = new();

    public int Count => _items.Count;

    public void Add(TKey key, TValue value)
    {
        _items.Add(new KeyValuePair<TKey, TValue>(key, value));
        _indices[value] = _items.Count - 1;
        BubbleUp(_items.Count - 1);
    }

    public bool Contains(TValue value)
    {
        return _indices.ContainsKey(value);
    }

    public KeyValuePair<TKey, TValue> RemoveTop()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Heap is empty");

        var top = _items[0];
        _indices.Remove(top.Value);

        if (_items.Count > 1)
        {
            _items[0] = _items[^1];
            _indices[_items[0].Value] = 0;
            _items.RemoveAt(_items.Count - 1);
            BubbleDown(0);
        }
        else
        {
            _items.Clear();
        }

        return top;
    }

    private void BubbleUp(int index)
    {
        while (index > 0)
        {
            var parentIndex = (index - 1) / 2;
            if (_items[index].Key.CompareTo(_items[parentIndex].Key) >= 0)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void BubbleDown(int index)
    {
        while (true)
        {
            var smallest = index;
            var leftChild = 2 * index + 1;
            var rightChild = 2 * index + 2;

            if (leftChild < _items.Count && _items[leftChild].Key.CompareTo(_items[smallest].Key) < 0)
                smallest = leftChild;

            if (rightChild < _items.Count && _items[rightChild].Key.CompareTo(_items[smallest].Key) < 0)
                smallest = rightChild;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        var temp = _items[i];
        _items[i] = _items[j];
        _items[j] = temp;

        _indices[_items[i].Value] = i;
        _indices[_items[j].Value] = j;
    }
}
