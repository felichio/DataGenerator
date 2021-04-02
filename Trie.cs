using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class Trie
{

    private TrieNode root {get; set;}
    private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    public Trie()
    {
        root = new TrieNode();
    }

    public void insert(P pair)
    {
        cacheLock.EnterWriteLock();
        root = TrieNode.insert(pair.key, pair, root);
        cacheLock.ExitWriteLock();
    }

    public void delete(string key)
    {
        cacheLock.EnterWriteLock();
        if (search(key))
            root = TrieNode.delete(key, root);
        cacheLock.ExitWriteLock();
    }

    public bool search(string key)
    {
        cacheLock.EnterReadLock();
        bool result = TrieNode.search(key, root);
        cacheLock.ExitReadLock();
        return result;
    }

    public P get(string key)
    {
        
        if (!search(key)) 
        {
            return null;
        }
        cacheLock.EnterReadLock();
        P p = TrieNode.get(key, root);
        cacheLock.ExitReadLock();
        return p;
    }

    public V getV(string key)
    {
        P p = get(key);
        return p == null ? null : p.value;
    }

    public P multiget(string keydots)
    {
        string[] keys = keydots.Split(".");
        cacheLock.EnterReadLock();
        P p = TrieNode.multiget(keys, root);
        cacheLock.ExitReadLock();
        return p;
    }

    public V multigetV(string keydots)
    {
        P p = multiget(keydots);
        return p == null ? null : p.value;
    }

    public Trie getTrie(string key)
    {
        cacheLock.EnterReadLock();
        Trie t = TrieNode.getTrie(key, root);
        cacheLock.ExitReadLock();
        return t;
    }

    public int size()
    {
        cacheLock.EnterReadLock();
        int s = TrieNode.sizeI(root);
        cacheLock.ExitReadLock();
        return s;
    }

    class TrieNode
    {
        public bool _value = false;
        public Dictionary<char, TrieNode> links = new Dictionary<char, TrieNode>();
        public P _pair;
        public Trie _next;

        public TrieNode(bool v)
        {
            this._value = v;
        }

        public TrieNode() {}

        public static TrieNode insert(string key, P pair, TrieNode t)
        {
            
            if (key == "")
            {
                t._value = true;
                t._pair = pair;
                if (pair.value is CollectionV)
                {
                    t._next = new Trie();
                    foreach (P innerpair in (CollectionV) pair.value)
                    {
                        // t._next = insert(innerpair.key, innerpair, t._next);
                        t._next.insert(innerpair);
                    }
                }
                return t;
            }
            char c = key[0];
            string subkey = key[1..];

            if (t.links.ContainsKey(c))
            {
                insert(subkey, pair, t.links[c]);
                return t;
            }

            t.links.Add(c, insert(subkey, pair, new TrieNode()));
            return t;
        }

        public static TrieNode delete(string key, TrieNode t)
        {

            // if (!search(key, t)) return t;
            
            if (key == "")
            {
                t._value = false;
                t._next = null;
                return t;
            }

            char c = key[0];
            string subkey = key[1..];

            
            delete(subkey, t.links[c]);
            
            if (t.links[c]._value == false && t.links[c].links.Count == 0)
            {
                t.links.Remove(c);
            }
            return t;

        }

        public static P get(string key, TrieNode t)
        {
            if (key == "") return t._value ? t._pair : null;
            char c = key[0];
            string subkey = key[1..];
            if (t.links.ContainsKey(c))
            {
                return get(subkey, t.links[c]);
            }
            return null;
        }

        public static P multiget(string[] keys, TrieNode t)
        {
            if (keys.Length == 1) return get(keys[0], t);
            else
            {
                Trie tr = getTrie(keys[0], t);
                if (tr != null) return tr.multiget(String.Join(".", keys[1..]));
            }
            return null;
        }

        public static Trie getTrie(string key, TrieNode t)
        {
            if (key == "") return t._next;
            char c = key[0];
            string subkey = key[1..];
            if (t.links.ContainsKey(c))
            {
                return getTrie(subkey, t.links[c]);
            }
            return null;
        }

        public static bool search(string key, TrieNode t)
        {
            
            if (key == "") return t._value;
            char c = key[0];
            string subkey = key[1..];
            if (t.links.ContainsKey(c))
            {
                return search(subkey, t.links[c]);
            }

            return false;
        }

        public static int size(TrieNode t)
        {
            if (t == null) return 0;
            int s = 0;
            foreach (KeyValuePair<char, TrieNode> pair in t.links)
            {
                s += size(pair.Value);
            }
            return t._value ? s + 1 : s;
        }

        public static int sizeI(TrieNode t)
        {
            
            int s = 0;
            Queue<TrieNode> q = new Queue<TrieNode>();
            q.Enqueue(t);
            while (q.Count != 0)
            {
                t = q.Dequeue();
                s += (t._value ? 1 : 0);
                foreach (KeyValuePair<char, TrieNode> pair in t.links)
                {
                    q.Enqueue(pair.Value);
                }
            }
            return s;

        }

    }
}