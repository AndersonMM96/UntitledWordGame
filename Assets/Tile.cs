using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private string value;
    public Vector2Int gridIndex;
    public int wordIndex = -1;

    public Board board;

    [SerializeField] protected List<Sprite> tileSprites;
    private bool flipX;
    [SerializeField] protected List<Sprite> letterSprites;
    [SerializeField] protected List<Sprite> pipSprites;

    private SpriteAnimator spriteAnimator;
    private SpriteRenderer pip;

    private void Awake()
    {
        spriteAnimator = GetComponent<SpriteAnimator>();
        pip = transform.GetChild(0).GetComponent<SpriteRenderer>();

        List<Sprite> sprites = new List<Sprite>();
        int r = Random.Range(0, tileSprites.Count);
        sprites.Add(tileSprites[r]);
        while (tileSprites[r].Equals(sprites[0]))
            r = Random.Range(0, tileSprites.Count);
        sprites.Add(tileSprites[r]);
        spriteAnimator.sprites = sprites;


        flipX = Random.Range(0, 2) == 0;
        GetComponent<SpriteRenderer>().flipX = flipX;

        GetComponent<SpriteRenderer>().sortingOrder = 4;
        pip.sortingOrder = 5;
        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 5;

    }

    private void Update()
    {
        UpdatePipSprite();

        gameObject.name = value + " Tile";

        if(board != null)
        {
            if(board.shakeBoard)
            {
                GetComponent<SpriteRenderer>().color = Color.gray;
                pip.color = Color.gray;
                GetComponent<SpriteAnimator>().shake = true;
            }
            else if(board.boardEnabled)
            {
                if (wordIndex == -1 || board.validWord)
                {
                    GetComponent<SpriteRenderer>().color = Color.white;
                    pip.color = Color.white;
                    GetComponent<SpriteAnimator>().shake = false;
                }
                else
                {
                    GetComponent<SpriteRenderer>().color = Color.gray;
                    pip.color = Color.gray;
                    GetComponent<SpriteAnimator>().shake = true;
                }
            }
            else
            {
                if (wordIndex != -1 && board.validWord)
                {
                    GetComponent<SpriteRenderer>().color = Color.white;
                    pip.color = Color.white;
                    GetComponent<SpriteAnimator>().shake = true;
                    GetComponent<SpriteAnimator>().shakeIntensity = 0.075f;
                }
                else
                {
                    GetComponent<SpriteRenderer>().color = Color.gray;
                    pip.color = Color.gray;
                    GetComponent<SpriteAnimator>().shake = false;
                }
            }

            if (wordIndex == -1)
            {
                transform.position = Vector3.MoveTowards(transform.position, board.transform.position + (Vector3)(Vector2)gridIndex + spriteAnimator.shakeOffset, 30f * Time.deltaTime);
                if (board.word.Contains(this))
                    board.word.Remove(this);
                if (gridIndex == board.selectorPosition)
                    spriteAnimator.animate = true;
                else
                    spriteAnimator.animate = false;
                if (transform.position.Equals(board.transform.position + (Vector3)(Vector2)gridIndex))
                {
                    GetComponent<SpriteRenderer>().sortingOrder = 0;
                    pip.sortingOrder = 2;
                    for (int i = 1; i < transform.childCount; i++)
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
                pip.sortingOrder = 5;
                for (int i = 1; i < transform.childCount; i++)
                    transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 5;
            }
        }
    }
    public void SetValue(string value)
    {
        this.value = value;
        UpdateLetterSprite();
    }
    public string GetValue() { return value; }
    private void UpdateLetterSprite()
    {
        for (int i = 1; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);
        for (int i = 0; i < value.Length; i++)
        {
            GameObject letter = new GameObject();
            letter.transform.parent = transform;
            letter.AddComponent<SpriteRenderer>();
            letter.GetComponent<SpriteRenderer>().sortingOrder = 5;
        }
        for (int i = 0; i < value.Length; i++)
        {
            try
            {
                transform.GetChild(i + 1).GetComponent<SpriteRenderer>().sprite = letterSprites[Board.GetIndex(value[i])];
                transform.GetChild(i + 1).name = "Letter " + value[i];
                transform.GetChild(i + 1).position = transform.position + (Vector3)GetLetterPosition(i);
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
            case 3: return new Vector2(-1, -1);
            default: return Vector2.zero;
        }
    }
    private Vector2 GetPipOffset(int i, bool flipX)
    {
        if(!flipX)
        {
            switch (i)
            {
                case 0: return Vector2.zero;
                case 1: return new Vector2(-3, 1);
                case 2: return new Vector2(-2, 1);
                case 3: return Vector2.down;
                default: return Vector2.zero;
            }
        }
        else
        {
            switch (i)
            {
                case 0: return Vector2.right;
                case 1: return Vector2.right * 2;
                case 2: return new Vector2(2, -3);
                case 3: return Vector2.left;
                default: return Vector2.zero;
            }
        }
    }
    private void UpdatePipSprite()
    {
        pip.transform.localPosition = new Vector3(0.328125f, 0.328125f, 0f) + (Vector3)GetPipOffset(GetTileIndex(), flipX) / 64f;
        double points = 0.0;
        for (int n = 0; n < value.Length; n++)
            points += Board.GetPoints(value[n]);
        if (points > 2)
            pip.sprite = pipSprites[2];
        else if (points > 1.25)
            pip.sprite = pipSprites[1];
        else
            pip.sprite = pipSprites[0];
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
