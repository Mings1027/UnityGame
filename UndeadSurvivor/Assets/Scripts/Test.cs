using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;

public class Test : MonoBehaviour
{
    private CustomQueue<int> _customQueue;

    private void Awake()
    {
        _customQueue = new CustomQueue<int>(10);
    }

    private void Start()
    {
        // _customQueue.CustomEnQueue(1);
        // print(_customQueue.CustomDeQueue());
    }
}

public class CustomQueue<T>
{
    private readonly T[] _array;
    private int _head, _tail;

    public CustomQueue(int capacity)
    {
        _array = new T[capacity];
        _head = _tail = 0;
    }

    public void CustomEnQueue(T item)
    {
        _array[_tail++ % _array.Length] = item;
    }

    public T CustomDeQueue()
    {
        return _array[_head++ % _array.Length];
    }
}