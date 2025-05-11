using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Tile : MonoBehaviour
{
    //Assets
    private Sprite[] tileSprites;
    private Sprite[] pipSprites;

    //Variables
    private string value = "";
    private List<Sprite> sprites;
    private List<bool> flipX;
    private int sortingOrder = 0;

    //Child Objects
    private SpriteRenderer tile;
    private SpriteAnimator animator;
    private TextPile text;
    private SpriteRenderer pip;

    public static Tile Create(string value, Transform parent)
    {
        GameObject tile = new();
        tile.name = value + " Tile";
        tile.transform.parent = parent;
        tile.transform.localPosition = Vector3.zero;
        tile.AddComponent<Tile>().Initialize();
        tile.GetComponent<Tile>().SetValue(value);
        return tile.GetComponent<Tile>();
    }

    private void Initialize()
    {
        //Initialize Tile
        if (gameObject.GetComponent<SpriteRenderer>() == null)
            gameObject.AddComponent<SpriteRenderer>();
        tile = gameObject.GetComponent<SpriteRenderer>();

        if (gameObject.GetComponent<SpriteAnimator>() == null)
            gameObject.AddComponent<SpriteAnimator>();
        animator = gameObject.GetComponent<SpriteAnimator>();

        animator.Moving += MoveTile;

        tileSprites = Resources.LoadAll<Sprite>("Textures/tiles");
        pipSprites = Resources.LoadAll<Sprite>("Textures/pips");

        //Get sprites randomly
        sprites = new List<Sprite> { tileSprites[Random.Range(0, tileSprites.Length)] };
        flipX = new List<bool> { Random.Range(0, 2) == 0 ? false : true, Random.Range(0, 2) == 0 ? false : true };
        Sprite s;
        do
        {
            s = tileSprites[Random.Range(0, tileSprites.Length)];
        } while (s.Equals(sprites[0]));
        sprites.Add(s);

        //Create TextPile
        text = TextPile.Create(transform);
        text.transform.localPosition = Vector3.zero + (Vector3)(GetTextOffset(GetSpriteIndex(), tile.flipX) / 64f);
        text.AddColor(Color.black);

        //Create Pip
        GameObject pipObject = new();
        pipObject.name = "Pip";
        pipObject.transform.parent = transform;
        pip = pipObject.AddComponent<SpriteRenderer>();
        UpdatePipSprite();

        animator.AddColor(Color.white);
        animator.AddColor(Color.gray);

        SetValue(value);

        UpdateTile();
    }

    private void Update()
    {
        text.transform.localPosition = Vector3.zero + (Vector3)(GetTextOffset(GetSpriteIndex(), tile.flipX) /64f);
        UpdatePipSprite();
    }

    public void SetValue(string value)
    {
        this.value = value;
        text.SetText(value);
        text.SetSpacing(0f);
        UpdateTile();
    }
    public string GetValue()
    {
        return value;
    }
    private void MoveTile(bool moving)
    {
        if(moving)
            SetSortingOrder(sortingOrder + 2);
        else
            SetSortingOrder(sortingOrder - 2);
    }
    public void SetSortingOrder(int sortingOrder)
    {
        this.sortingOrder = sortingOrder;
        animator.sortingOrder = sortingOrder;
        text.SetSortingOrder(sortingOrder + 1);
        pip.sortingOrder = sortingOrder + 1;
    }
    private void UpdateTile()
    {
        SetSortingOrder(sortingOrder);
        animator.SetAnimate(false, sprites, flipX, 0.5f);
    }

    private int GetSpriteIndex()
    {
        for (int i = 0; i < tileSprites.Length; i++)
        {
            if (tileSprites[i].Equals(tile.sprite))
                return i;
        }
        return -1;
    }
    //Letter offsets based on tile sprite
    private Vector2 GetTextOffset(int i, bool flipX)
    {
        Vector2 negate = flipX ? new Vector2(-1, 1) : new Vector2(1, 1);
        switch (i)
        {
            case 0: return Vector2.zero * negate;
            case 1: return Vector2.up * negate;
            case 2: return Vector2.right * negate;
            case 3: return new Vector2(-1, -1) * negate;
            default: return Vector2.zero;
        }
    }
    //Pip offsets
    private Vector2 GetPipOffset(int i, bool flipX)
    {
        if (!flipX)
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
        pip.transform.localPosition = new Vector3(0.328125f, 0.328125f, 0f) + (Vector3)GetPipOffset(GetSpriteIndex(), tile.flipX)/64f;
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

    public void ValidTile(bool valid)
    {
        animator.SetShake(!valid, 0.04f, 0.05f);
        SetColor(valid ? 0 : 1);
    }

    public void DestroyTile()
    {
        //Debug.LogWarning("Destroying " + name);
        ParticleManager.instance.EmitParticle(0, transform.position);
        Destroy(gameObject);
    }

    public void AddColor(Color color)
    {
        animator.AddColor(color);
    }
    public void SetColor(int index)
    {
        animator.SetColor(index);
        pip.color = animator.GetColors()[index];
    }
}
