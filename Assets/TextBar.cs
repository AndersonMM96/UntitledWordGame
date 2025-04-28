using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public class TextBar : MonoBehaviour
{
    private string text;
    public int sortingOrder;
    public float animSpeed;
    public List<GameObject> letters;
    [SerializeField] protected List<Sprite> letterSprites;

    public bool shake, bob = false;

    private Vector3 target;

    private void Awake()
    {
        letters = new List<GameObject>();
    }

    private void Update()
    {
        for(int i = 0; i < letters.Count; i++)
        {
            letters[i].transform.position = Vector3.MoveTowards(letters[i].transform.position, transform.position + (Vector3)GetLetterPosition(i) + letters[i].GetComponent<SpriteAnimator>().shakeOffset + letters[i].GetComponent<SpriteAnimator>().bobOffset + target, animSpeed * Time.deltaTime);
        }
    }

    private GameObject CreateLetter()
    {
        GameObject letter = new GameObject();
        letter.name = "Letter";
        letter.transform.parent = transform;
        letter.transform.position = transform.position;
        letter.AddComponent<SpriteRenderer>();
        letter.AddComponent<SpriteAnimator>();
        return letter;
    }
    private Vector2 GetLetterPosition(int index)
    {
        Vector2 position = Vector2.zero;

        int length = letters.Count;
        position.x -= 0.25f * (length - 1);
        position.x += 0.5f * index;

        return position;
    }

    public void UpdateLetters()
    {
        letters.Clear();
        for(int i = 0; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);
        for(int i = 0; i < text.Length; i++)
            letters.Add(CreateLetter());

        for (int i = 0; i < letters.Count; i++)
        {
            letters[i].GetComponent<SpriteAnimator>().shake = shake;
            letters[i].GetComponent<SpriteAnimator>().bob = bob;
            letters[i].GetComponent<SpriteAnimator>().bobIntensity = 0.25f;
            letters[i].GetComponent<SpriteAnimator>().bobSpeed = 0.5f;
            letters[i].GetComponent<SpriteRenderer>().sprite = letterSprites[Board.GetIndex(text[i])];
            letters[i].GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
            letters[i].GetComponent<SpriteAnimator>().bobTimer = i * 0.1f; 
            letters[i].transform.position = transform.position + (Vector3)GetLetterPosition(i) + letters[i].GetComponent<SpriteAnimator>().shakeOffset + letters[i].GetComponent<SpriteAnimator>().bobOffset;
        }
    }

    public void OffsetLetters(Vector3 offset, bool inOut)
    {
        if (inOut)
            for (int i = 0; i < letters.Count; i++)
                letters[i].transform.position += offset;
        else
            target = offset;
    }

    public void SetText(string text)
    {
        this.text = text;
        UpdateLetters();
    }

    public string GetText() { return text; }

}
