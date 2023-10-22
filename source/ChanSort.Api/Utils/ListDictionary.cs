using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ChanSort.Api.Utils;

public class ListDictionary<K,V> : IDictionary<K,V>
{
  private readonly ListDictionary dict = new();

  public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => (IEnumerator<KeyValuePair<K, V>>)dict.GetEnumerator();
    
  IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();
  public void Add(KeyValuePair<K, V> item) => dict.Add(item.Key, item.Value);
  public void Clear() => dict.Clear();

  public bool Contains(KeyValuePair<K, V> item)
  {
    var val = dict[item.Key];
    return val != null && Equals(val, item.Value);
  }

  public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
  {
    var i = arrayIndex;
    foreach (DictionaryEntry entry in dict)
      array[i++] = new KeyValuePair<K, V>((K)entry.Key, (V)entry.Value);
  }

  public bool Remove(KeyValuePair<K, V> item)
  {
    var val = dict[item.Key];
    if (val != null && Equals(val, item.Value))
    {
      dict.Remove(item.Key);
      return true;
    }

    return false;
  }

  public int Count => dict.Count;
  public bool IsReadOnly => dict.IsReadOnly;
  public bool ContainsKey(K key) => dict.Contains(key);

  public void Add(K key, V value) => dict.Add(key, value);

  public bool Remove(K key)
  {
    if (!dict.Contains(key)) 
      return false;

    dict.Remove(key);
    return true;
  }

  public bool TryGetValue(K key, out V value)
  {
    var obj = dict[key];
    if (obj == null)
    {
      value = default;
      return false;
    }

    value = (V)obj;
    return true;
  }

  public V this[K key]
  {
    get => (V)dict[key];
    set => dict[key] = value;
  }

  public ICollection<K> Keys => (ICollection<K>)dict.Keys;

  public ICollection<V> Values
  {
    get
    {
      var list = new List<V>();
      foreach(DictionaryEntry e in dict)
        list.Add((V)e.Value);
      return list;
    }
  }
}