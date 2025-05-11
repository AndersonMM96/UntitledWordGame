using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    //Variables
    [SerializeField] private int playerNumber;
    public TilePile[,] tiles;
    public Vector2Int selectorPosition = new Vector2Int(0, 0);
    private List<Vector2Int> selectedTiles;
    public bool canScramble = true;
    private int health = 999;
    private bool boardEnabled = true;
    public bool movementEnabled = true;
    private bool shakeBoard = false;

    //Child objects
    private Transform gridTransform;
    private GameObject selector;
    public TilePile word;
    private TextPile healthText;

    //Events
    [HideInInspector] public static Action<int> SubmitWord;
    [HideInInspector] public static Action UpdateWord;

    private void Awake()
    {
        //Movement Events
        InputManager.Move += MoveSelector;
        InputManager.Select += SelectTile;
        InputManager.Back += DeselectTile;
        InputManager.Enter += Enter;
        InputManager.Scramble += Scramble;
    }

    public static Board Create(int playernum, Transform parent)
    {
        GameObject board = new();
        board.name = "Board " + playernum;
        board.transform.parent = parent;
        board.transform.localPosition = Vector3.zero;
        board.AddComponent<Board>();
        return board.GetComponent<Board>();
    }

    public void Initialize(int playerNumber)
    {
        this.playerNumber = playerNumber;

        gridTransform = new GameObject().transform;
        gridTransform.name = "Grid";
        gridTransform.parent = transform;
        gridTransform.localPosition = new Vector3(-1.5f, -1.5f, 0);

        tiles = new TilePile[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tiles[i, j] = TilePile.Create(gridTransform);
                tiles[i, j].max = 1;
                tiles[i, j].name = "Tile " + i + "-" + j;
                tiles[i, j].transform.localPosition = new Vector3(i, j, 0);
            }
        }

        //Selector
        selector = new GameObject();
        selector.name = "Selector";
        selector.transform.parent = transform;
        SpriteRenderer selectorRenderer = selector.AddComponent<SpriteRenderer>();
        SpriteAnimator selectorAnimator = selector.AddComponent<SpriteAnimator>();
        selectorRenderer.sprite = Resources.Load<Sprite>("Textures/Selector");
        selectorRenderer.color = playerNumber % 2 == 0 ? Color.red : Color.blue;
        selectorAnimator.sortingOrder = 15;
        selectorAnimator.SetPulse(true, 0.15f, 1.25f, 0.25f);
        selectorAnimator.SetRotate(true, playerNumber % 2 == 0, 15f);

        selectorPosition = new Vector2Int(0, 3);
        selectedTiles = new List<Vector2Int>();

        //Word
        if (word == null)
        {
            word = TilePile.Create(transform);
            word.name = "Word";
            word.transform.localPosition = new Vector3(0, 3f);
        }

        //Health
        healthText = TextPile.Create(transform);
        healthText.name = "Health";
        healthText.transform.localPosition = new Vector3(0, -2.5f);
        healthText.max = 4;
        healthText.AddColor(Color.black);
        SetHealth(health);
    }

    public void SetEnabled(bool boardEnabled)
    {
        this.boardEnabled = boardEnabled;
        if(boardEnabled)
        {
            foreach(TilePile pile in tiles)
            {
                pile.SetColor(0);
            }
        }
        else
        {
            foreach (TilePile pile in tiles)
            {
                pile.SetColor(1);
            }
        }
    }

    private void Start()
    {
        MoveSelector(playerNumber, Vector2Int.zero);
    }
    private void Update()
    {
        selector.GetComponent<SpriteAnimator>().SetPosition(gridTransform.localPosition + (Vector3)(Vector2)selectorPosition);
    }
    
    private void MoveSelector(int playernum, Vector2Int direction)
    {
        if(playernum == playerNumber && movementEnabled)
        {
            tiles[selectorPosition.x, selectorPosition.y].SetAnimate(false);

            selectorPosition += direction;

            if (selectorPosition.x < 0)
                selectorPosition.x = 3;
            if (selectorPosition.x > 3)
                selectorPosition.x = 0;
            if (selectorPosition.y < 0)
                selectorPosition.y = 3;
            if (selectorPosition.y > 3)
                selectorPosition.y = 0;

            tiles[selectorPosition.x, selectorPosition.y].SetAnimate(true);
        }
    }
    private void SelectTile(int playernum)
    {
        if(playernum == playerNumber && boardEnabled && movementEnabled)
        {
            if(!selectedTiles.Contains(selectorPosition))
            {
                if (tiles[selectorPosition.x, selectorPosition.y].length > 0)
                {
                    selectedTiles.Add(selectorPosition);
                    word.Add(tiles[selectorPosition.x, selectorPosition.y].Get(0));
                    tiles[selectorPosition.x, selectorPosition.y].RemoveAt(0);
                    word.Get(word.length - 1).transform.parent = word.transform;
                    word.SetAnimate(true);
                }
                string spelledWord = word.GetWord();
                word.SetValid(spelledWord.Length >= 3 && Dictionary.instance.ContainsWord(spelledWord));
                UpdateWord?.Invoke();
            }
        }
    }
    private void DeselectTile(int playernum)
    {
        if (playernum == playerNumber && boardEnabled && movementEnabled)
        {
            if(word.length > 0 && selectedTiles.Count > 0)
            {
                int index = word.length - 1;
                tiles[selectedTiles[index].x, selectedTiles[index].y].Add(word.Get(index));
                tiles[selectedTiles[index].x, selectedTiles[index].y].SetValid(true);
                tiles[selectedTiles[index].x, selectedTiles[index].y].Get(0).GetComponent<SpriteAnimator>().SetAnimate(false);
                word.Get(index).transform.parent = tiles[selectedTiles[index].x, selectedTiles[index].y].transform;
                word.RemoveAt(index);
                selectedTiles.RemoveAt(index);
            }
            string spelledWord = word.GetWord();
            word.SetValid(spelledWord.Length >= 3 && Dictionary.instance.ContainsWord(spelledWord));
            UpdateWord?.Invoke();
        }
    }
    private void Enter(int playernum)
    {
        if (playernum == playerNumber && boardEnabled && movementEnabled)
        {
            string spelledWord = word.GetWord();
            if(spelledWord.Length >= 3 && Dictionary.instance.ContainsWord(spelledWord))
            {
                Debug.Log(spelledWord + " - " + GetScore(word.GetWord()) + " points");
                SubmitWord?.Invoke(playernum);
            }
            else
            {
                Debug.LogWarning(spelledWord + " - Invalid Word");
            }
            
        }
    }
    private void Scramble(int playernum)
    {
        if (playernum == playerNumber && canScramble && boardEnabled && movementEnabled)
        {
            ResetWord();
            canScramble = false;
            FillBoard(true);
        }
    }

    public void FillBoard(bool replace)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (replace || tiles[i, j].length < tiles[i, j].max)
                {
                    if (tiles[i, j].length > 0)
                        DestroyTile(i, j);
                    tiles[i, j].Add(MakeTile(i, j));
                    tiles[i, j].Get(0).GetComponent<SpriteAnimator>().SetPosition(new Vector2(0f, 10f + (i * 0.25f) + (j * 0.25f)), false);
                    if(!boardEnabled)
                        tiles[i, j].SetColor(1);
                }
            }
        }
    }

    public void ResetWord()
    {
        while (word.length > 0)
        {
            int index = word.length - 1;
            tiles[selectedTiles[index].x, selectedTiles[index].y].Add(word.Get(index));
            tiles[selectedTiles[index].x, selectedTiles[index].y].SetValid(true);
            tiles[selectedTiles[index].x, selectedTiles[index].y].Get(0).GetComponent<SpriteAnimator>().SetAnimate(false);
            word.Get(index).transform.parent = tiles[selectedTiles[index].x, selectedTiles[index].y].transform;
            word.RemoveAt(index);
            selectedTiles.RemoveAt(index);
        }
    }

    public void ClearWord()
    {
        selectedTiles.Clear();
    }

    private Tile MakeTile(int i, int j)
    {
        string letter = GetRandomLetter();
        while (LetterCount(letter[0]) >= 4)
            letter = GetRandomLetter();
        return Tile.Create(letter, tiles[i, j].transform);
    }
    public void DestroyTile(int i, int j)
    {
        tiles[i, j].DestroyTile();
    }

    private static string GetRandomLetter()
    {
        int total = 0;
        for (int i = 0; i < 26; i++)
        {
            total += GetWeight(Letter.GetLetter(i));
        }
        int r = UnityEngine.Random.Range(0, total);
        string letter = "";
        for (int i = 0; i < 26; i++)
        {
            if (r < GetWeight(Letter.GetLetter(i)))
            {
                letter = Letter.GetLetter(i).ToString();
                break;
            }
            r -= GetWeight(Letter.GetLetter(i));
        }

        //add 'u' to 'Q'
        if (letter.Equals("Q"))
            letter += "u";

        return letter;
    }
    public int LetterCount(char letter)
    {
        //Make sure that no more than 4 of the same letter are on the board at a time
        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (tiles[i, j].length > 0)
                {
                    foreach (char c in tiles[i, j].GetWord())
                        if (c.Equals(letter))
                            count++;
                }
            }
        }
        return count;
    }

    public int GetHealth()
    {
        return health;
    }
    public void SetHealth(int health)
    {
        this.health = health;
        healthText.SetText(health.ToString());
    }
    public IEnumerator TickHealth(int targetHealth)
    {
        if(targetHealth < 0)
            targetHealth = 0;
        healthText.SetShake(true, 0.1f, 0.1f);
        while(health != targetHealth)
        {
            if (health > targetHealth)
                SetHealth(health - 1);
            else if (health < targetHealth)
                SetHealth(health + 1);
            yield return new WaitForSeconds(0.004f);
        }
        healthText.SetShake(false);
    }

    public void ShakeBoard(bool shake)
    {
        shakeBoard = shake;
        foreach (TilePile pile in tiles)
            pile.SetShake(shakeBoard, 0.04f, 0.025f);
    }

    public static double GetScore(string word)
    {
        double score = 0.0;
        foreach (char c in word)
            score += GetPoints(c);
        return score;
    }

    public static double GetPoints(char value)
    {
        value = char.ToUpper(value);
        switch (value)
        {
            case 'A': return 1.0;
            case 'B': return 1.5;
            case 'C': return 1.5;
            case 'D': return 1.0;
            case 'E': return 1.0;
            case 'F': return 1.5;
            case 'G': return 1.25;
            case 'H': return 1.5;
            case 'I': return 1.0;
            case 'J': return 2.5;
            case 'K': return 2.25;
            case 'L': return 1.25;
            case 'M': return 1.5;
            case 'N': return 1.0;
            case 'O': return 1.0;
            case 'P': return 1.25;
            case 'Q': return 2.5;
            case 'R': return 1.0;
            case 'S': return 1.0;
            case 'T': return 1.0;
            case 'U': return 1.0;
            case 'V': return 2.0;
            case 'W': return 2.0;
            case 'X': return 2.75;
            case 'Y': return 1.75;
            case 'Z': return 3.0;
            default: return 0;
        }
    }
    public static int GetWeight(char value)
    {
        value = char.ToUpper(value);
        switch (value)
        {
            case 'A': return 9;
            case 'B': return 2;
            case 'C': return 2;
            case 'D': return 4;
            case 'E': return 12;
            case 'F': return 2;
            case 'G': return 3;
            case 'H': return 2;
            case 'I': return 9;
            case 'J': return 1;
            case 'K': return 1;
            case 'L': return 4;
            case 'M': return 2;
            case 'N': return 6;
            case 'O': return 8;
            case 'P': return 2;
            case 'Q': return 1;
            case 'R': return 6;
            case 'S': return 4;
            case 'T': return 6;
            case 'U': return 4;
            case 'V': return 2;
            case 'W': return 2;
            case 'X': return 1;
            case 'Y': return 2;
            case 'Z': return 1;
            default: return 0;
        }
    }
}
