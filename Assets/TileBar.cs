using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

public class TileBar : MonoBehaviour
{
    public string text;
    public float animSpeed;
    public List<Tile> tiles;
    public bool shake, animate, invalid = false;
    private Vector3 target;

    //Prefabs
    [SerializeField] protected Tile tilePrefab;
    public ParticleSystem TileGib;

    private void Update()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null)
            {
                tiles[i].transform.position = Vector3.MoveTowards(tiles[i].transform.position, transform.position + (Vector3)GetTilePosition(i) + tiles[i].GetComponent<SpriteAnimator>().shakeOffset, animSpeed * Time.deltaTime);
                tiles[i].GetComponent<SpriteAnimator>().shake = shake;
                tiles[i].GetComponent<SpriteAnimator>().animate = animate;
                tiles[i].shakeTile = invalid;
            }
        }
    }

    public void SetText(string text)
    {
        this.text = text;
        UpdateTiles();
    }

    private Tile CreateTile(string value)
    {
        Tile tile = Instantiate(tilePrefab, transform.position + new Vector3(0f, 3f, 0f), Quaternion.identity, transform);
        tile.SetValue(value);
        return tile;
    }

    private Vector2 GetTilePosition(int index)
    {
        Vector2 wordPosition = Vector2.zero;

        int length = tiles.Count;
        wordPosition.x -= 0.5f * (length - 1);
        wordPosition.x += 1f * index;

        return wordPosition;
    }

    private void UpdateTiles()
    {
        int c = tiles.Count;
        for (int i = 0; i < c; i++)
        {
            DestroyTile(0);
        }
        tiles.Clear();
        for (int i = 0; i < text.Length; i++)
        {
            string value = "";
            value += text[i];
            if (value.Equals("Q"))
            {
                value += text[i + 1];
                i++;
            }
            tiles.Add(CreateTile(value));
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].GetComponent<SpriteAnimator>().shake = shake;
            tiles[i].GetComponent<SpriteAnimator>().animate = animate;
            tiles[i].shakeTile = invalid;
            tiles[i].transform.position = transform.position + new Vector3(0f, 3f + 0.5f * i, 0f) + (Vector3)GetTilePosition(i) + tiles[i].GetComponent<SpriteAnimator>().shakeOffset;
        }
    }

    public void DestroyTile(int i)
    {
        Transform tile = transform.GetChild(i);
        StartCoroutine(EmitTileGib(tile.position));
        Destroy(tile.gameObject);
    }
    private IEnumerator EmitTileGib(Vector3 position)
    {
        ParticleSystem gib = Instantiate(TileGib, position, Quaternion.identity, transform);
        gib.Emit(10);
        yield return new WaitForSeconds(1f);
        Destroy(gib.gameObject);
    }
}
