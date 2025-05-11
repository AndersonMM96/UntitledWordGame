using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TilePile : Pile
{
    public new static TilePile Create(Transform parent)
    {
        GameObject pile = new();
        pile.name = "Tiles";
        pile.transform.parent = parent;
        pile.transform.localPosition = Vector3.zero;
        pile.AddComponent<TilePile>().Initialize();
        pile.GetComponent<TilePile>().AddColor(Color.white);
        pile.GetComponent<TilePile>().AddColor(Color.gray);
        return pile.GetComponent<TilePile>();
    }

    public void Add(Tile tile)
    {
        Add(tile.gameObject);
        //SetValid(word.Length >= 3 && Dictionary.instance.ContainsWord(GetWord()));
    }
    public void SetValid(bool valid)
    {
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<Tile>().ValidTile(valid);
    }

    public string GetWord()
    {
        string word = "";
        foreach(GameObject t in values)
        {
            word += t.GetComponent<Tile>().GetValue();
        }
        return word;
    }
    public void SetWord(string word)
    {
        for (int i = 0; i < word.Length; i++)
        {
            string value = word[i].ToString();
            if(value.Equals("Q"))
                value += word[i + 1];
            Add(Tile.Create(value, transform).gameObject);
            i += value.Length - 1;
        }
        for (int i = 0; i < values.Count; i++)
        {
            values[i].GetComponent<SpriteAnimator>().SetPosition(GetPosition(i), true);
        }
        SetValid(Dictionary.instance.ContainsWord(GetWord()));
    }
    public void DestroyTile()
    {
        for (int i = 0; i < values.Count; i++)
            if (values[i] != null)
                values[i].GetComponent<Tile>().DestroyTile();
        Clear();
    }
    public void DestroyTile(int index)
    {
        values[index].GetComponent<Tile>().DestroyTile();
        RemoveAt(index);
    }
    public new void SetSortingOrder(int sortingOrder)
    {
        this.sortingOrder = sortingOrder;
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<Tile>().SetSortingOrder(this.sortingOrder);
    }
    public new void AddColor(Color color)
    {
        colors.Add(color);
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<Tile>().AddColor(color);
    }
    public new void SetColor(int index)
    {
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<Tile>().SetColor(index);
    }
}
