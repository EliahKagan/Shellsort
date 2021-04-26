<Query Kind="Program" />

// Shellsort.linq - comparing Shellsort by gap sequence and to other sorts
//
// Written in 2018 by Eliah Kagan <degeneracypressure@gmail.com>.
//
// To the extent possible under law, the author(s) have dedicated all copyright
// and related and neighboring rights to this software to the public domain
// worldwide. This software is distributed without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along
// with this software. If not, see
// <http://creativecommons.org/publicdomain/zero/1.0/>.

#LINQPad optimize+

private sealed class GapView<T> : IList<T> {
    public GapView(IList<T> items, int gap, int start, int? stop = null)
    {
        if (gap <= 0) throw new ArgumentOutOfRangeException(nameof(gap));
        if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if (stop < 0) throw new ArgumentOutOfRangeException(nameof(stop));

        _items = items;
        _gap = gap;
        _start = start;

        var realStop = Math.Min(_items.Count, stop ?? int.MaxValue);
        var span = Math.Max(0, realStop - _start);
        var quot = Math.DivRem(span, _gap, out var rem);
        Count = (rem == 0 ? quot : quot + 1);
    }

    public T this[int index] {
        get => _items[MapIndex(index)];
        set => _items[MapIndex(index)] = value;
    }

    public int Count { get; }

    public bool IsReadOnly => false;

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
    void ICollection<T>.Add(T item) => throw new NotSupportedException();
    void ICollection<T>.Clear() => throw new NotSupportedException();
    bool ICollection<T>.Contains(T item) => throw new NotSupportedException();
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException();
    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
    int IList<T>.IndexOf(T item) => throw new NotSupportedException();
    void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
    void IList<T>.RemoveAt(int index) => throw new NotSupportedException();

    private int MapIndex(int index)
    {
        if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
        return _start + _gap * index;
    }

    private readonly IList<T> _items;

    private readonly int _gap, _start;
}

private static void CheckNonnull(object arg, string name)
{
    if (arg == null) throw new ArgumentNullException(paramName: name);
}

private static void DoInsertionSort<T>(IList<T> items) where T : IComparable<T>
{
    var count = items.Count;

    for (var right = 1; right < count; ++right) {
        var item = items[right];

        var left = right;
        while (left != 0 && item.CompareTo(items[left - 1]) < 0) {
            items[left] = items[left - 1];
            --left;
        }

        items[left] = item;
    }
}

private static void InsertionSort<T>(IList<T> items) where T : IComparable<T>
{
    CheckNonnull(items, nameof(items));
    DoInsertionSort(items);
}

/// <summary>
/// Generates a gap sequence consisting of one less than powers of 2.
/// (Found by Hibbard 1963: https://dl.acm.org/citation.cfm?doid=366552.366557)
/// </summary>
private static List<int> GetHibbardGaps(int bound)
{
    var n = (uint)Math.Max(2, bound);
    var a = new List<int>();

    checked {
        for (var k = 1; ; ++k) {
            var x = (1u << k) - 1u;
            if (x >= n) break;
            a.Add((int)x);
        }
    }

    a.Reverse();
    return a;
}

/// <summary>
/// Generates a gap sequence consisting of the 3-smooth numbers.
/// (Found by Pratt 1972: http://www.dtic.mil/get-tr-doc/pdf?AD=AD0740110)
/// </summary>
private static List<int> Get3SmoothGaps(int bound)
{
    var n = Math.Max(2L, bound);
    var a = new List<int>();

    checked {
        for (var x = 1L; x < n; x *= 2L)
            for (var y = x; y < n; y *= 3L) a.Add((int)y);
    }

    InsertionSort(a);
    a.Reverse();
    return a;
}

/// <summary>
/// Generates a gap sequence of values that differ by a bit more than 9/4.
/// (Found by Tokuda 1992: https://dl.acm.org/citation.cfm?id=659879.)
/// </summary>
private static List<int> GetTokudaGaps(int bound)
{
    var n = Math.Max(2.0, bound);
    var a = new List<int>();

    checked {
        for (var x = 1.0; x < n; x = x * 2.25 + 1)
            a.Add((int)Math.Ceiling(x));
    }

    a.Reverse();
    return a;
}

private static void CheckShellsortArgs<T>(IList<T> items, IList<int> gaps)
{
    CheckNonnull(items, nameof(items));
    CheckNonnull(gaps, nameof(gaps));

    if (gaps.Count == 0 || gaps[gaps.Count - 1] != 1) {
        throw new ArgumentException(message: "gap sequence must end in 1",
                                    paramName: nameof(gaps));
    }
}

public static void Shellsort<T>(IList<T> items, IList<int> gaps)
        where T : IComparable<T>
{
    CheckShellsortArgs(items, gaps);

    foreach (var gap in gaps) {
        foreach (var start in Enumerable.Range(0, gap))
            DoInsertionSort(new GapView<T>(items, gap, start));
    }
}

public static void Shellsort<T>(IList<T> items) where T : IComparable<T>
{
    CheckNonnull(items, nameof(items));
    Shellsort(items, Get3SmoothGaps(items.Count));
}

private static void InsertionSortSubsequence<T>(IList<T> items,
                                                int gap, int start)
        where T : IComparable<T>
{
    var stop = items.Count;

    for (var right = start + gap; right < stop; right += gap) {
        var item = items[right];

        var left = right;
        while (left != start && item.CompareTo(items[left - gap]) < 0) {
            items[left] = items[left - gap];
            left -= gap;
        }

        items[left] = item;
    }
}

public static void ShellsortAlt<T>(IList<T> items, IList<int> gaps)
        where T : IComparable<T>
{
    CheckShellsortArgs(items, gaps);

    foreach (var gap in gaps) {
        foreach (var start in Enumerable.Range(0, gap))
            InsertionSortSubsequence(items, gap, start);
    }
}

public static void ShellsortAlt<T>(IList<T> items) where T : IComparable<T>
{
    CheckNonnull(items, nameof(items));
    ShellsortAlt(items, Get3SmoothGaps(items.Count));
}

public static void Quicksort<T>(IList<T> items) where T : IComparable<T>
{
    CheckNonnull(items, nameof(items));

    void Swap(int i, int j) {
        var tmp = items[i];
        items[i] = items[j];
        items[j] = tmp;
    }

    int Partition(int low, int high) { // returns the new pivot index
        Swap(low, low + (high - low) / 2);
        var pivot = items[low];
        var left = low;

        for (var right = left + 1; right <= high; ++right)
            if (items[right].CompareTo(pivot) < 0) Swap(++left, right);

        Swap(low, left);
        return left;
    }

    void QSort(int low, int high) {
        if (low >= high) return;

        var mid = Partition(low, high);
        QSort(low, mid - 1);
        QSort(mid + 1, high);
    }

    QSort(0, items.Count - 1);
    //items.Dump(); // FIXME: remove after debugging
}

public static void Heapsort<T>(IList<T> items) where T : IComparable<T>
{
    CheckNonnull(items, nameof(items));

    const int no_child = -1;
    var size = items.Count;

    int PickChild(int parent) {
        var left = parent * 2 + 1;
        if (left >= size) return no_child;

        var right = left + 1;
        return right == size || items[left].CompareTo(items[right]) >= 0
                ? left
                : right;
    }

    void SiftDown(int parent, T item) {
        for (; ; ) {
            var child = PickChild(parent);
            if (child == no_child || item.CompareTo(items[child]) >= 0) break;

            items[parent] = items[child];
            parent = child;
        }

        items[parent] = item;
    }

    // max-heapify the array
    for (var parent = size / 2; parent >= 0; --parent)
        SiftDown(parent, items[parent]);

    // extract each maximum to the running end position to sort the array
    while (--size > 0) {
        var item = items[size];
        items[size] = items[0];
        SiftDown(0, item);
    }
}

private static Random CreateReproducibleGenerator()
    => new Random(((int)DateTime.Now.Ticks).Dump("SEED"));

private static int[] RandomSequence(Random gen, int length)
{
    var items = new int[length];

    for (var i = 0; i != length; ++i)
        items[i] = gen.Next(int.MinValue, int.MaxValue);

    return items;
}

private static int[] DescendingSequence(int count)
{
    var a = new int[count];
    foreach (var i in Enumerable.Range(0, count)) a[i] = count - i;
    return a;
}

private static bool IsSorted<T>(IList<T> items) where T : IComparable<T>
{
    if (items.Count == 0) return true;

    var prev = items[0];
    foreach (var cur in items.Skip(1)) {
        if (cur.CompareTo(prev) < 0) return false;
        prev = cur;
    }

    return true;
}

private static void TestMethod<T>(Action<List<T>> method, string name, IList<T> items)
        where T : IComparable<T>
{
    var a = new List<T>(items);

    var ti = Util.ElapsedTime;
    method(a);
    var tf = Util.ElapsedTime;

    var result = IsSorted(a) ? "Correct" : "WRONG! :(";
    var caption = $"{name} of {items.Count} {typeof(T)}s took {tf - ti}.";
    result.Dump(caption);
}

private static void Test<T>(IList<T> items) where T : IComparable<T>
{
    TestMethod(a => a.Sort(Comparer<T>.Default), "System introsort", items);

    if (items.Count <= 100_000)
        TestMethod(InsertionSort, "Insertion sort", items);

    TestMethod(a => Shellsort(a, GetHibbardGaps(a.Count)),
               "Shellsort [Hibbard 1963]", items);

    TestMethod(a => ShellsortAlt(a, GetHibbardGaps(a.Count)),
               "Shellsort (alt.) [Hibbard 1963]", items);

    TestMethod(Shellsort, "Shellsort [Pratt 3-smooth]", items);

    TestMethod(ShellsortAlt, "Shellsort (alt.) [Pratt 3-smooth]", items);

    TestMethod(a => Shellsort(a, GetTokudaGaps(a.Count)),
               "Shellsort [Tokuda 1992]", items);

    TestMethod(a => ShellsortAlt(a, GetTokudaGaps(a.Count)),
               "Shellsort (alt.) [Tokuda 1992]", items);

    TestMethod(Quicksort, "Quicksort", items);

    TestMethod(Heapsort, "Heapsort", items);
}

private static void Test<T>(params T[] items) where T : IComparable<T>
    => Test((IList<T>)items);

private static void Main()
{
    var gen = CreateReproducibleGenerator();

    Test(1, 17, 4, 32, 6, -5, 9, 2, 5, 11, 10);

    var sizes = new[] {
        1000,
        10_000,
        100_000,
        1_000_000,
        10_000_000,
        100_000_000,
    };

    foreach (var n in sizes) {
        "Descending sequence...".Dump();
        Test(DescendingSequence(n));

        "Random sequence...".Dump();
        Test(RandomSequence(gen, n));
    }
}
