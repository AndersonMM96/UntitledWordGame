using System.Collections;
using UnityEngine;

public class TextPile : Pile
{
    private string text = "";

    public new static TextPile Create(Transform parent)
    {
        GameObject pile = new();
        pile.name = "Text";
        pile.transform.parent = parent;
        pile.transform.localPosition = Vector3.zero;
        pile.AddComponent<TextPile>().Initialize();
        return pile.GetComponent<TextPile>();
    }

    private void Start()
    {
        SetSpacing(4f/64f);
    }

    public void SetText(string text)
    {
        Clear();
        foreach (char c in text)
        {
            Add(Letter.Create(c, transform).gameObject, GetKerning(c));
        }
        for(int i = 0; i < values.Count; i++)
        {
            values[i].GetComponent<SpriteAnimator>().SetPosition(GetPosition(i), true);
            values[i].GetComponent<SpriteAnimator>().SetColor(colorIndex);
        }
        this.text = text;
    }
    public void UpdateText() { SetText(text); }

    private static Vector2 GetKerning(char c)
    {
        switch (c)
        {
            case 'A': return new Vector2(32f/64f, 0f);
            case 'B': return new Vector2(24f/64f, 0f);
            case 'C': return new Vector2(28f/64f, 0f);
            case 'D': return new Vector2(28f/64f, 0f);
            case 'E': return new Vector2(26f/64f, 0f);
            case 'F': return new Vector2(22f/64f, 0f);
            case 'G': return new Vector2(28f/64f, 0f);
            case 'H': return new Vector2(28f/64f, 0f);
            case 'I': return new Vector2(8f/64f, 0f);
            case 'J': return new Vector2(22f/64f, 0f);
            case 'K': return new Vector2(26f/64f, 0f);
            case 'L': return new Vector2(24f/64f, 0f);
            case 'M': return new Vector2(34f/64f, 0f);
            case 'N': return new Vector2(28f/64f, 1f/64f);
            case 'O': return new Vector2(30f/64f, 0f);
            case 'P': return new Vector2(24f/64f, 0f);
            case 'Q': return new Vector2(28f/64f, 0f);
            case 'R': return new Vector2(26f/64f, 0f);
            case 'S': return new Vector2(24f/64f, 0f);
            case 'T': return new Vector2(32f/64f, 0f);
            case 'U': return new Vector2(28f/64f, 0f);
            case 'V': return new Vector2(30f/64f, 0f);
            case 'W': return new Vector2(40f/64f, 1f/64f);
            case 'X': return new Vector2(30f/64f, 0f);
            case 'Y': return new Vector2(30f/64f, 0f);
            case 'Z': return new Vector2(30f/64f, 0f);
            case 'a': return new Vector2(20f/64f, 0f);
            case 'b': return new Vector2(20f/64f, 0f);
            case 'c': return new Vector2(18f/64f, -1f/64f);
            case 'd': return new Vector2(24f/64f, 0f);
            case 'e': return new Vector2(18f/64f, -1f/64f);
            case 'f': return new Vector2(14f/64f, 1f/64f);
            case 'g': return new Vector2(22f/64f, -8f/64f);
            case 'h': return new Vector2(24f/64f, 0f);
            case 'i': return new Vector2(10f/64f, 0f);
            case 'j': return new Vector2(20f/64f, -1f/64f);
            case 'k': return new Vector2(22f/64f, 0f);
            case 'l': return new Vector2(8f/64f, 1f/64f);
            case 'm': return new Vector2(38f/64f, -1f/64f);
            case 'n': return new Vector2(26f/64f, -1f/64f);
            case 'o': return new Vector2(20f/64f, -1f/64f);
            case 'p': return new Vector2(26f/64f, -9f/64f);
            case 'q': return new Vector2(20f/64f, -8f/64f);
            case 'r': return new Vector2(20f/64f, -1f/64f);
            case 's': return new Vector2(16f/64f, -1f/64f);
            case 't': return new Vector2(20f/64f, -1f/64f);
            case 'u': return new Vector2(20f/64f, -1f/64f);
            case 'v': return new Vector2(22f/64f, -1f/64f);
            case 'w': return new Vector2(34f/64f, -1f/64f);
            case 'x': return new Vector2(24f/64f, -2f/64f);
            case 'y': return new Vector2(22f/64f, -7f / 64f);
            case 'z': return new Vector2(22f/64f, -2f/64f);
            case '1': return new Vector2(26f/64f, 0f);
            case '2': return new Vector2(28f/64f, 0f);
            case '3': return new Vector2(24f/64f, 0f);
            case '4': return new Vector2(28f/64f, 0f);
            case '5': return new Vector2(30f/64f, 0f);
            case '6': return new Vector2(26f/64f, 0f);
            case '7': return new Vector2(24f/64f, 0f);
            case '8': return new Vector2(20f/64f, 0f);
            case '9': return new Vector2(22f/64f, 0f);
            case '0': return new Vector2(30f/64f, 0f);
            case '?': return new Vector2(18f/64f, 0f);
            case '!': return new Vector2(8f/64f, 0f);
            case '-': return new Vector2(18f/64f, 0f);
            default: return Vector2.zero;
        }
    }
}
