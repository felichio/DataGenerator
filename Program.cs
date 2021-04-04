using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using NDesk.Options;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;



class Program
{
    static void ShowHelp(OptionSet p)
    {
        Console.WriteLine ("Usage: createData [Options]");
        Console.WriteLine ("Example: createData -k keyFile.txt -n 1000 -d 3 -l 4 -m 5");
        Console.WriteLine ();
        Console.WriteLine ("Options:");
        p.WriteOptionDescriptions (Console.Out);
    }

    static void Main(string[] args)
    {

        bool showhelp = false;
        int n = 10;
        int d = 2;
        int l = 5;
        int m = 3;
        string KeyValueFile = "";

        (string, string)[] KeyValueStore;

        string[] CollectionKeys;
        
        

        OptionSet p = new OptionSet()
        {
            {"n=", "{number} of items to generate", (int v) => n = v},
            {"d=", "maximum {level} of nesting", (int v) => d = v},
            {"l=", "maximum {level} of charactes for string values", (int v) => l = v},
            {"m=", "maximum {number} of keys inside values", (int v) => m = v},
            {"k|template=", "the {file} template for key-value associations", v => KeyValueFile = v},
            {"h|help", "show this message", v => showhelp = v != null},
        };
        

        List<string> extra;
        try
        {
            extra = p.Parse(args);
        }
        catch (OptionException e)
        {
            Console.Write ("createData: ");
            Console.WriteLine (e.Message);
            Console.WriteLine ("Try `createData --help' for more information.");
            return ;
        }

        if (showhelp || extra.Count > 0)
        {
            ShowHelp(p);
            return ;
        }

        if (KeyValueFile == "")
        {
            ShowHelp(p);
            return ;
        }

        try
        {
            string[] lines = File.ReadAllLines(KeyValueFile);
            KeyValueStore = lines.Select(x => {
                var t = x.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                return (t[0], t[1]);
            }).Where(((string k, string v) s) => s.v == "int" || s.v == "float" || s.v == "string").ToArray();

            CollectionKeys = lines.Select(x => {
                var t = x.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                return (t[0], t[1]);
            }).Where(((string k, string v) s) => s.v == "dict").Select(((string k, string v) s) => s.k).ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine("Invalid file template");
            Console.WriteLine(e.Message);
            return ;
        }

        
        if (m > KeyValueStore.Length || m > CollectionKeys.Length)
        {
            Console.WriteLine("maximum number of keys m have to be at most the number of keys provided in template (scalar and dict values) ");
            return ;
        }
        
        Generator g = new Generator(l, m, d, n, KeyValueStore, CollectionKeys);
        foreach (P item in g.Gen())
        {
            Console.WriteLine(item);
        }

    }
}


