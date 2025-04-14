using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Tile tilePrefab;

    public Tile[,] grid = new Tile[4,4];
    public List<Tile> word = new List<Tile>();

    public Transform Selector;
    private Vector2Int selectorPosition = new Vector2Int(0, 0);
    private Vector2 Movement;
    private float moveCooldown;

    private void Awake()
    {
        Selector = transform.GetChild(0);
        FillGrid(true);
        selectorPosition = new Vector2Int(0, 3);
    }

    private void Start()
    {
        InputManager.GetMovementEvent += GetMovement;
        InputManager.GetSelectEvent += SelectTile;
        InputManager.GetBackEvent += DeselectTile;
    }

    private void Update()
    {
        Selector.position = transform.GetChild(1).position + (Vector3)(Vector2)selectorPosition;
        moveCooldown -= Time.deltaTime;
        if (moveCooldown <= 0f)
        {
            moveCooldown = 0.3f;
            MoveSelector();
        }
    }

    private void GetMovement(int playernum, Vector2 v)
    {
        moveCooldown = 0f;
        Movement = v;
    }
    private void MoveSelector()
    {
        if(Movement.x != 0)
            selectorPosition.x += (int)(Mathf.Sign(Movement.x) * 1);
        if (Movement.y != 0)
            selectorPosition.y += (int)(Mathf.Sign(Movement.y) * 1);

        if (selectorPosition.x < 0)
            selectorPosition.x = 3;
        if (selectorPosition.x > 3)
            selectorPosition.x = 0;
        if (selectorPosition.y < 0)
            selectorPosition.y = 3;
        if (selectorPosition.y > 3)
            selectorPosition.y = 0;
    }
    private void SelectTile(int playernum, bool b)
    {
        if (b)
            if(grid[selectorPosition.x, selectorPosition.y].wordIndex == -1)
                grid[selectorPosition.x, selectorPosition.y].wordIndex = word.Count;
    }
    private void DeselectTile(int playernum, bool b)
    {
        if (b && word.Count > 0)
            word[word.Count - 1].wordIndex = -1;
    }

    private void FillGrid(bool replace)
    {
        for(int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if(replace || grid[i, j] == null)
                {
                    if(grid[i, j] != null)
                        DestroyTile(i, j);
                    grid[i,j] = MakeTile(new Vector3(i, j, 0f));
                }
            }
        }
    }

    private void DestroyTile(int i, int j)
    {
        Debug.LogWarning("Destroying " + grid[i, j].name + " (" + i + ", " + j + ")");
        Destroy(grid[i, j].gameObject);
        grid[i, j] = null;
    }

    public string GetWord()
    {
        string letters = "";
        foreach (Tile tile in word)
        {
            letters += tile.value;
        }
        return letters;
    }
    public int ScoreWord(string word)
    {
        int points = 0;
        for (int i = 0; i < word.Length; i++)
        {
            points += GetPoints(word[i]);
        }
        return points;
    }
    public Tile MakeTile(Vector2 position)
    {
        int total = 0;
        for (int i = 0; i < 26; i++)
        {
            total += GetWeight(GetLetter(i));
        }
        int r = Random.Range(0, total);
        string letter = "";
        for (int i = 0; i < 26; i++)
        {
            if(r < GetWeight(GetLetter(i)))
            {
                letter = GetLetter(i).ToString();
                break;
            }
            r -= GetWeight(GetLetter(i));
        }

        //add 'u' to 'Q'
        if (letter.Equals("Q"))
            letter += "u";

        Tile tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
        tile.value = letter;
        tile.gridIndex = position;
        tile.board = this;
        return tile;
    }
    public static int GetPoints(char value)
    {
        value = char.ToUpper(value);
        switch (value)
        {
            case 'A': return 1;
            case 'B': return 3;
            case 'C': return 3;
            case 'D': return 2;
            case 'E': return 1;
            case 'F': return 4;
            case 'G': return 2;
            case 'H': return 4;
            case 'I': return 1;
            case 'J': return 8;
            case 'K': return 5;
            case 'L': return 1;
            case 'M': return 3;
            case 'N': return 1;
            case 'O': return 1;
            case 'P': return 3;
            case 'Q': return 10;
            case 'R': return 1;
            case 'S': return 1;
            case 'T': return 1;
            case 'U': return 1;
            case 'V': return 4;
            case 'W': return 4;
            case 'X': return 8;
            case 'Y': return 4;
            case 'Z': return 10;
            default: return 0;
        }
    }
    public static char GetLetter(int index)
    {
        switch (index)
        {
            case 0: return 'A';
            case 1: return 'B';
            case 2: return 'C';
            case 3: return 'D';
            case 4: return 'E';
            case 5: return 'F';
            case 6: return 'G';
            case 7: return 'H';
            case 8: return 'I';
            case 9: return 'J';
            case 10: return 'K';
            case 11: return 'L';
            case 12: return 'M';
            case 13: return 'N';
            case 14: return 'O';
            case 15: return 'P';
            case 16: return 'Q';
            case 17: return 'R';
            case 18: return 'S';
            case 19: return 'T';
            case 20: return 'U';
            case 21: return 'V';
            case 22: return 'W';
            case 23: return 'X';
            case 24: return 'Y';
            case 25: return 'Z';
            default: return ' ';
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
    public Vector2 GetLetterPosition(int index)
    {
        Vector2 wordPosition = (Vector2)transform.GetChild(2).position;

        int length = GetWord().Length;
        wordPosition.x -= 0.5f * (length - 1);
        wordPosition.x += 1f * index;

        return wordPosition;
    }
}
