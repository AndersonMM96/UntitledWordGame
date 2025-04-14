using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    public string value;
    public Vector2 gridIndex;
    public int wordIndex = -1;

    public Board board;

    private void Update()
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = value;
        gameObject.name = value + " Tile";

        if(wordIndex == -1)
        {
            transform.position = gridIndex; 
            if (board.word.Contains(this))
                board.word.Remove(this);
        }
        else
        {
            transform.position = board.GetLetterPosition(wordIndex);
            if(!board.word.Contains(this))
                board.word.Insert(wordIndex, this);
        }
    }
}
