using System;
using System.Collections;
using System.Collections.Generic;

namespace ChanSort
{
  /// <summary>
  /// The DelegateComparer class is used as an adapter to use a simple delegate method in places
  /// where an IComparer interface is expected
  /// </summary>
  public class DelegateComparer : IComparer
  {
    private readonly Func<object,object,int> handler;
    private readonly bool reverse;


    /// <summary>
    /// Create a new DelegateComparer
    /// </summary>
    /// <param name="handler">The method used to compare two objects</param>
    public DelegateComparer(Func<object, object, int> handler)
    {
      this.handler = handler;
    }

    public DelegateComparer(Func<object, object, int> handler, bool reverse) : this(handler)
    {
      this.reverse=reverse;
    }

    /// <summary>
    /// Compares two objects by delegating the request to the handler-delegate
    /// </summary>
    public int Compare(object o1, object o2)
    {
      int r=this.handler(o1, o2);
      return this.reverse ? -r : +r;
    }
  }

  public class DelegateComparer<T> : IComparer<T>, IComparer
  {
    private readonly Func<T, T, int> handler;
    private readonly bool reverse;

    public DelegateComparer(Func<T,T,int> handler, bool reverse = false)
    {
      this.handler = handler;
      this.reverse = reverse;
    }

    public int Compare(T x, T y)
    {
      var r = handler(x, y);
      return reverse ? -r : r;
    }

    int IComparer.Compare(object x, object y) => this.Compare((T) x, (T) y);
  }
}
