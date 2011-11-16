
using System.Collections;
using System.Collections.Generic;


public class MultiMap<Key, V>
{
    // Use a Dictionary of string keys.
    Dictionary<Key, List<V>> _dictionary = new Dictionary<Key, List<V>>();

    // New Add method using TryGetValue for better performance
    public void Add(Key key, V value)
    {

        List<V> list;
        // if there is already a list with key then just add to it
        if (this._dictionary.TryGetValue(key, out list))
        {
            // Add the value to the list
            list.Add(value);
        }
        else
        {
            // create a new list for the key value
            list = new List<V>();
            // and add the first value
            list.Add(value);
            // save off list into dictionary
            this._dictionary[key] = list;
        }
    }

    // Expose the keys as Enumerable
    public IEnumerable<Key> Keys
    {
        get
        {
            return this._dictionary.Keys;
        }
    }

    // provide an indexer based on key to get the list 
    public List<V> this[Key key]
    {
        get
        {
            List<V> list;
            if (this._dictionary.TryGetValue(key, out list))
            {
                return list;
            }
            else
            {
                return new List<V>();
            }
        }
    }
}
