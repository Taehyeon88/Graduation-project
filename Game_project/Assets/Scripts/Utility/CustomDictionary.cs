using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class SOGroup
{
    public string type;
    [SerializeField]
    public List<ScriptableObject> assets = new();
}

[System.Serializable]
public class CustomDictionary
{
    [SerializeField]
    private List<SOGroup> entries = new();
    public int Count => entries.Count;
    public List<string> Keys
    {
        get
        {
            List<string> list = new List<string>();
            foreach (var entry in entries)
            {
                list.Add(entry.type);
            }
            return list;
        }
    }
    public List<ScriptableObject> this[string type]
    {
        get
        {
            foreach (var entry in entries)
            {
                if(type == entry.type)
                    return entry.assets;
            }
            throw new KeyNotFoundException($"Key not found: {type}");
        }
        set
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (type == entries[i].type)
                {
                    entries[i].assets.Clear();
                    entries[i].assets.AddRange(value);
                    return;
                }
            }

            entries.Add(new SOGroup { type = type, assets = value });
        }
    }

    public bool TryGetValue(string key, out List<ScriptableObject> value)
    {
        foreach (var entry in entries)
        {
            if (entry.type == key)
            {
                value = entry.assets;
                return true;
            }
        }
        value = default;
        return false;
    }

    public List<ScriptableObject> Get(string key)
    {
        foreach (var entry in entries)
        {
            if (key == entry.type)
                return entry.assets;
        }

        throw new KeyNotFoundException($"Key not found: {key}");
    }

    public bool ContainsKey(string key)
    {
        foreach (var entry in entries)
        {
            if (key == entry.type)
                return true;
        }
        return false;
    }

    public void Add(string key, List<ScriptableObject> value)
    {
        if (ContainsKey(key))
            throw new ArgumentException($"Key already exists: {key}");

        var group = new SOGroup { type = key};
        group.assets.AddRange(value);
        entries.Add(group);
    }

    public bool Remove(string key)
    {
        int index = entries.FindIndex(e => 
                e.type == key);

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
