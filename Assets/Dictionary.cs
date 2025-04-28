using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Dictionary : MonoBehaviour
{
    public static Dictionary instance;

    HashSet<string> dictionary;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        dictionary = new HashSet<string>(File
           .ReadLines(@"Assets/dictionary.txt")
           .Where(line => !string.IsNullOrWhiteSpace(line))
           .Select(line => line.Trim()), StringComparer.OrdinalIgnoreCase);
        /*
        StreamReader sr = new StreamReader(@"Assets/dictionary.txt");
        double max = 0.0;
        int[] list = new int[200];
        while (sr.Peek() >= 0)
        {
            string t = sr.ReadLine(); 
            double c = Board.ScoreWord(t.ToUpper());
            int v = (int)(c * 4);
            list[v]++;
        }
        for(int i = 0; i < list.Length; i++)
        {
            Debug.Log((double)i/4.0 + " = " + list[i]);
        }
        */
    }

    public bool ContainsWord(string word)
    {
        return dictionary.Contains(word.ToUpper());
    }
}
