using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableEntry<TKey, TValue>
{
    public TKey Key;
    public TValue Value;
}

[System.Serializable]
public class CustomDictionary<TKey, TValue>
{
    [SerializeField]
    private List<SerializableEntry<TKey, TValue>> entries = new();
    public int Count => entries.Count;
    public List<TKey> Keys
    {
        get
        {
            List<TKey> list = new List<TKey>();
            foreach (var entry in entries)
            {
                list.Add(entry.Key);
            }
            return list;
        }
    }
    public TValue this[TKey key]
    {
        get
        {
            foreach (var entry in entries)
            {
                if(EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                    return entry.Value;
            }
            throw new KeyNotFoundException($"Key not found: {key}");
        }
        set
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (EqualityComparer<TKey>.Default.Equals(entries[i].Key, key))
                    entries[i].Value = value;
                return;
            }

            entries.Add(new SerializableEntry<TKey, TValue> { Key = key, Value = value });
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        foreach (var entry in entries)
        {
            if (EqualityComparer<TKey>.Default.Equals(entry.Key, key))
            {
                value = entry.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    public TValue Get(TKey key)
    {
        foreach (var entry in entries)
        {
            if (EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                return entry.Value;
        }

        throw new KeyNotFoundException($"Key not found: {key}");
    }

    public bool ContainsKey(TKey key)
    {
        foreach (var entry in entries)
        {
            if (EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                return true;
        }
        return false;
    }

    public void Add(TKey key, TValue value)
    {
        if (ContainsKey(key))
            throw new ArgumentException($"Key already exists: {key}");

        entries.Add(new SerializableEntry<TKey, TValue>
        {
            Key = key,
            Value = value
        });
    }

    public bool Remove(TKey key)
    {
        int index = entries.FindIndex(e => 
             EqualityComparer<TKey>.Default.Equals(e.Key, key));

        if (index < 0)
            return false;

        entries.RemoveAt(index);
        return true;
    }

    public void Clear()
    {
        entries.Clear();
    }
}
