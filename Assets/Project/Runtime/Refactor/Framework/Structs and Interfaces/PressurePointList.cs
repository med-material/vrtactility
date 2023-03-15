using System.Collections;
using System.Collections.Generic;

public class PressurePointList : IList<PressurePoint>
{
    public IEnumerator<PressurePoint> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(PressurePoint item)
    {
        throw new System.NotImplementedException();
    }

    public void Clear()
    {
        throw new System.NotImplementedException();
    }

    public bool Contains(PressurePoint item)
    {
        throw new System.NotImplementedException();
    }

    public void CopyTo(PressurePoint[] array, int arrayIndex)
    {
        throw new System.NotImplementedException();
    }

    public bool Remove(PressurePoint item)
    {
        throw new System.NotImplementedException();
    }

    public int Count { get; }
    public bool IsReadOnly { get; }
    public int IndexOf(PressurePoint item)
    {
        throw new System.NotImplementedException();
    }

    public void Insert(int index, PressurePoint item)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new System.NotImplementedException();
    }

    public PressurePoint this[int index]
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }
}
