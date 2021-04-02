using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;


public class Generator
{
    private int l = 5;
    private int m = 5;
    private int d = 3;
    private int n = 100;
    private Random rnd;
    
    private (string, string)[] KeysValues;
    private string[] CollectionKeys;

    public Generator(int l, int m, int d, int n, (string, string)[] KeysValues, string[] CollectionKeys)
    {
        this.l = l;
        this.m = m;
        this.d = d;
        this.n = n;
        this.KeysValues = KeysValues;
        this.CollectionKeys = CollectionKeys;
        rnd = new Random();
    }

    public void printMe()
    {
        foreach ((string s, string v) in KeysValues)
        {
            Console.WriteLine(s + v);
        }
    }



    private int GenerateInt(int limit)
    {  
        return rnd.Next(limit);
    }



    public double GenerateDouble(int upper)
    {
        return Math.Round(rnd.NextDouble() * upper, 2);
    }

    public string GenerateString()
    {
        
        StringBuilder sb = new StringBuilder();
        int threshold = 1 + GenerateInt(l + 1);
        for (int i = 0; i < threshold; i++)
        {
            double d = GenerateDouble(1);
            if (d < 0.2)
            {
                sb.Append((char) rnd.Next(48, 57 + 1));
            }
            else if (d >= 0.2 && d < 0.6)
            {
                sb.Append((char) rnd.Next(65, 90 + 1));
            }
            else
            {
                sb.Append((char) rnd.Next(97, 122 + 1));
            }
        }
        return sb.ToString();
    }

    

    public P DiffLevelGenerator()
    {
        return DiffLevelGenerator(d + 1, KeysValues.ToList());
    }

    public IEnumerable<P> Gen()
    {
        for (int i = 0; i < n; i++)
        {
            P p = DiffLevelGenerator();
            p.key = "key" + (i + 1).ToString(); 
            yield return p;
        }
    }

    private P DiffLevelGenerator(int level, List<(string, string)> KV)
    {
        if (level == 0) return GenSingle(KV);

        CollectionV c = new CollectionV();
        int threshold = GenerateInt(m + 1);
        KV = KeysValues.ToList();
        
        for (int i = 0; i < threshold; i++)
        {
            c.Add(DiffLevelGenerator(level - (1 + GenerateInt(level)), KV));
        }
        
        // return new P(CollectionKeys[GenerateInt(CollectionKeys.Length)], c);
        return GenSingleCollection(c);
    }

    private P GenSingleCollection(CollectionV c)
    {
        // int num = 0;
        // foreach (P p in c)
        // {
        //     if (p.value is CollectionV)
        //     {
        //         num++;
        //     }
        // }

        List<string> l = CollectionKeys.ToList();
        foreach (P p in c)
        {
            if (p.value is CollectionV)
            {
                int index = GenerateInt(l.Count);
                string key = l[index];
                l.RemoveAt(index);
                p.key = key;
            }
        }

        // var keys = CollectionKeys.OrderBy(x => rnd.Next()).Take(num).ToList();
        // // Console.WriteLine(keys.Count + "    " + used.Count);
        // foreach (P p in c)
        // {
        //     if (p.value is CollectionV)
        //     {
                
        //     }
        // }
        
        return new P(CollectionKeys[GenerateInt(CollectionKeys.Length)], c);
    }

    private P GenSingle(List<(string, string)> KV)
    {
        int index = GenerateInt(KV.Count);
        var (key, type) = KV[index];
        KV.RemoveAt(index);
        switch (type)
        {
            case "int":
                return new P(key, new ScalarV<int>(GenerateInt(1000)));
            case "float":
                return new P(key, GenerateDouble(100));
            case "string":
                return new P(key, GenerateString());
            default:
                return new P(key, new ScalarV<int>(GenerateInt(1000)));
        }
    }

    



}