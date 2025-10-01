namespace W65C02S.Engine.Types;

/// <summary>
/// Generic ring buffer implementation.
/// </summary>
file class RingBuffer<T>
{
    private readonly int _capacity;
    private readonly List<T?> _buffer;
    private int _head;
    private int _tail;

    public RingBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = new List<T?>(capacity);
        for (int i = 0; i < capacity; i++)
            _buffer.Add(default);
        _head = 0;
        _tail = 0;
    }

    public void Put(T element)
    {
        _buffer[_tail] = element;
        _tail = (_tail + 1) % _buffer.Count;
    }

    public T? Take()
    {
        T? element = _buffer[_head];
        _head = (_head + 1) % _buffer.Count;
        return element;
    }

    public bool IsFull() => (_head + 1) % _capacity == _tail;
    public bool IsEmpty() => _head == _tail;
    public int Count() => (_tail - _head + _capacity) % _capacity;
}
