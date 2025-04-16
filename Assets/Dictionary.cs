using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Dictionary : MonoBehaviour
{
    HashSet<string> dictionary;

    private void Awake()
    {
        dictionary = new HashSet<string>(File
           .ReadLines(@"Assets/dictionary.txt")
           .Where(line => !string.IsNullOrWhiteSpace(line))
           .Select(line => line.Trim()), StringComparer.OrdinalIgnoreCase);
    }

    public bool ContainsWord(string word)
    {
        return dictionary.Contains(word.ToUpper());
    }
}
