using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public string value;
    public Vector2 gridIndex;
    public int wordIndex = -1;

    public Board board;

    [SerializeField] protected List<Sprite> tileSprites;
    private bool flipX;
    [SerializeField] protected List<Sprite> letterSprites;

    private SpriteAnimator spriteAnimator;

    private void Awake()
    {
        spriteAnimator = GetComponent<SpriteAnimator>();

        List<Sprite> sprites = new List<Sprite>();
        int r = Random.Range(0, tileSprites.Count);
        sprites.Add(tileSprites[r]);
        while (tileSprites[r].Equals(sprites[0]))
            r = Random.Range(0, tileSprites.Count);
        sprites.Add(tileSprites[r]);
        spriteAnimator.sprites = sprites;


        flipX = Random.Range(0, 2) == 0;
        GetComponent<SpriteRenderer>().flipX = flipX;

    }

    private void Update()
    {
        UpdateLetterSprite();

        gameObject.name = value + " Tile";

        if(board != null)
        {
            if (wordIndex == -1)
            {
                transform.position = Vector3.MoveTowards(transform.position, board.transform.position + (Vector3)gridIndex, 30f * Time.deltaTime);
                if (board.word.Contains(this))
                    board.word.Remove(this);
                if (gridIndex == board.selectorPosition)
                    spriteAnimator.animate = true;
                else
                    spriteAnimator.animate = false;
                if (transform.position.Equals(board.transform.position + (Vector3)gridIndex))
                {
                    GetComponent<SpriteRenderer>().sortingOrder = 0;
                    for (int i = 0; i < transform.childCount; i++)
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 2;
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, (Vector3)board.GetLetterPosition(wordIndex) + spriteAnimator.shakeOffset, 30f * Time.deltaTime);
                if (!board.word.Contains(this))
                    board.word.Insert(wordIndex, this);
                spriteAnimator.animate = true;
                GetComponent<SpriteRenderer>().sortingOrder = 4;
                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 5;
            }
        }
    }
    private void UpdateLetterSprite()
    {
        if (transform.childCount < value.Length)
        {
            //make new letters
            for (int i = 0; i < value.Length - transform.childCount; i++)
            {
                GameObject letter = new GameObject();
                letter.transform.parent = transform;
                letter.AddComponent<SpriteRenderer>();
                letter.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }
        }
        else if(transform.childCount > value.Length)
        {
            //delete excess letters
            Destroy(transform.GetChild(transform.childCount - 1).gameObject);
        }
        for(int i = 0; i < value.Length; i++)
        {
            try
            {
                transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = letterSprites[Board.GetIndex(value[i])];
                transform.GetChild(i).name = "Letter " + value[i];
                transform.GetChild(i).position = transform.position + (Vector3)GetLetterPosition(i);
            }
            catch
            {
                Debug.LogError(value[i] + " is not a valid character!");
            }
        }
    }
    private Vector2 GetLetterPosition(int index)
    {
        Vector2 position = Vector2.zero;

        int length = value.Length;
        position.x -= 0.2f * (length - 1);
        position.x += 0.4f * index;
        position += (GetTileOffset(GetTileIndex()) / 64f);

        return position;
    }
    private Vector2 GetTileOffset(int i)
    {
        switch (i)
        {
            case 0: return Vector2.zero;
            case 1: return Vector2.up;
            case 2: return Vector2.right;
            case 3: return new Vector2(-1, 1);
            default: return Vector2.zero;
        }
    }
    private int GetTileIndex()
    {
        for (int i = 0; i < tileSprites.Count; i++)
        {
            if (tileSprites[i].Equals(GetComponent<SpriteRenderer>().sprite))
                return i;
        }
        return -1;
    }
}
