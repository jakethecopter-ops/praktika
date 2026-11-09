using System;
using System.Collections;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("Тест SmartStack:");
        var stack = new SmartStack<int>();
        stack.Push(10);
        stack.Push(20);
        stack.Push(30);
        Console.WriteLine($"Count: {stack.Count}, Capacity: {stack.Capacity}");
        Console.WriteLine($"Peek: {stack.Peek()}");
        var popped = stack.Pop();
        Console.WriteLine($"Popped: {popped}");
        Console.WriteLine($"Contains 20: {stack.Contains(20)}");
        Console.WriteLine($"Element at depth 0: {stack[0]}");
        Console.WriteLine($"Element at depth 1: {stack[1]}");
        Console.Write("Iterating: ");
        foreach (var item in stack)
        {
            Console.Write($"{item} ");
        }
        Console.WriteLine();
        var numbers = new List<int> { 100, 200, 300 };
        stack.PushRange(numbers);
        Console.WriteLine($"After PushRange Count: {stack.Count}");
        Console.WriteLine($"New Peek: {stack.Peek()}");

        try
        {
            var emptyStack = new SmartStack<string>();
            emptyStack.Pop();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Exception caught: {ex.Message}");
        }
    }
}

public class SmartStack<T> : IEnumerable<T>
{
    private T[] _items;
    private int _count;
    public SmartStack()
    {
        _items = new T[4];
        _count = 0;
    }
    public SmartStack(int capacity)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Ёмкость не может быть отрицательной.");
        }
        _items = new T[capacity];
        _count = 0;
    }

    public SmartStack(IEnumerable<T> collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }
        var count = 0;
        foreach (var _ in collection)
        {
            count++;
        }

        _items = new T[count];
        _count = count;

        var index = 0;
        foreach (var item in collection)
        {
            _items[index++] = item;
        }
    }
    public int Count => _count;
    public int Capacity => _items.Length;
    public void Push(T item)
    {
        if (_count == _items.Length)
        {
            var newCapacity = _items.Length == 0 ? 4 : _items.Length * 2;
            var newItems = new T[newCapacity];
            Array.Copy(_items, newItems, _count);
            _items = newItems;
        }
        _items[_count++] = item;
    }
    public void PushRange(IEnumerable<T> collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        var itemsToAdd = 0;
        foreach (var _ in collection)
        {
            itemsToAdd++;
        }
        if (itemsToAdd == 0)
        {
            return;
        }
        while (_count + itemsToAdd > _items.Length)
        {
            var newCapacity = _items.Length == 0 ? 4 : _items.Length * 2;
            
            if (newCapacity < _count + itemsToAdd)
            {
                newCapacity = _count + itemsToAdd;
            }
            var newItems = new T[newCapacity];
            Array.Copy(_items, newItems, _count);
            _items = newItems;
        }
        foreach (var item in collection)
        {
            _items[_count++] = item;
        }
    }
    public T Pop()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Стек пуст.");
        }
        _count--;
        var item = _items[_count];
        _items[_count] = default; 
        return item;
    }
    public T Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Стек пуст.");
        }

        return _items[_count - 1];
    }
    public bool Contains(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < _count; i++)
        {
            if (comparer.Equals(_items[i], item))
            {
                return true;
            }
        }
        return false;
    }
    public T this[int depth]
    {
        get
        {
            if (depth < 0 || depth >= _count)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "Глубина вне диапазона допустимых значений.");
            }

            return _items[_count - 1 - depth];
        }
    }
    public IEnumerator<T> GetEnumerator()
    {
        for (var i = _count - 1; i >= 0; i--)
        {
            yield return _items[i];
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}