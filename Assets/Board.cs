using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{
    public Game game; 
    public int health = 500;
    private bool boardEnabled = false;
    [SerializeField] protected int playerNumber = 0;

    public Tile tilePrefab;

    public Tile[,] grid = new Tile[4,4];
    public List<Tile> word = new List<Tile>();
    private bool validWord;

    public Transform Selector;
    public Vector2Int selectorPosition = new Vector2Int(0, 0);
    private Vector2 Movement;
    private float moveCooldown;

    private Dictionary dictionary;
    private TextBar HealthBar;

    private void Awake()
    {
        HealthBar = transform.GetChild(3).GetComponent<TextBar>();
        HealthBar.text = health.ToString("000");
        dictionary = GameObject.Find("Dictionary").GetComponent<Dictionary>();
        Selector = transform.GetChild(0);
        FillGrid(true);
        selectorPosition = new Vector2Int(0, 3);
        validWord = false;
    }

    private void Start()
    {
        //Application.targetFrameRate = 10;
        InputManager.GetMovementEvent += GetMovement;
        InputManager.GetSelectEvent += SelectTile;
        InputManager.GetBackEvent += DeselectTile;
        InputManager.GetEnterEvent += SubmitWord;
    }

    private void Update()
    {
        //TODO: fix movement the current system is very bad
        Selector.position = transform.GetChild(1).position + (Vector3)(Vector2)selectorPosition;
        moveCooldown -= Time.deltaTime;
        if (moveCooldown <= 0f)
        {
            moveCooldown = 0.3f;
            MoveSelector();
        }
        CheckValidity();
    }

    private void GetMovement(int playernum, Vector2 v)
    {
        if(playerNumber == playernum)
        {
            moveCooldown = 0f;
            Movement = v;
        }
    }
    private void MoveSelector()
    {
        if (Movement.x != 0)
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
        if (b && playerNumber == playernum && boardEnabled)
        {
            if (grid[selectorPosition.x, selectorPosition.y].wordIndex == -1)
            {
                word.Add(grid[selectorPosition.x, selectorPosition.y]);
                word[word.Count - 1].wordIndex = word.Count;
            }
            CheckValidity();
        }
            
    }
    private void DeselectTile(int playernum, bool b)
    {
        if (b && word.Count > 0 && playerNumber == playernum && boardEnabled)
        {
            word[word.Count - 1].GetComponent<SpriteRenderer>().color = Color.white;
            word[word.Count - 1].GetComponent<SpriteAnimator>().shake = false;
            word[word.Count - 1].wordIndex = -1;
            word.RemoveAt(word.Count - 1);
            CheckValidity();
        }
    }
    private void CheckValidity()
    {
        string w = GetWord();
        validWord = w.Length >= 3 && dictionary.ContainsWord(w);
        if(validWord)
            foreach(Tile t in word)
            {
                t.GetComponent<SpriteRenderer>().color = Color.white;
                t.GetComponent<SpriteAnimator>().shake = false;
            }
        else
            foreach (Tile t in word)
            {
                t.GetComponent<SpriteRenderer>().color = Color.gray;
                t.GetComponent<SpriteAnimator>().shake = true;
            }
    }
    private void SubmitWord(int playernum, bool b)
    {
        if(b && playerNumber == playernum && boardEnabled)
        {
            if (validWord)
            {
                string w = GetWord();
                double points = ScoreWord(w);
                Debug.Log(w + " - " + points + " Points");
                foreach (Tile t in word)
                    DestroyTile((int)t.gridIndex.x, (int)t.gridIndex.y);
                word.Clear();
                FillGrid(false);

                //Send word to game
                game.SendScore(points);
            }
            else
                Debug.LogWarning(GetWord() + " - Not a valid word!");
        }
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
                    grid[i,j] = MakeTile(new Vector2(i, j));
                }
            }
        }
    }

    private void DestroyTile(int i, int j)
    {
        //Debug.LogWarning("Destroying " + grid[i, j].name + " (" + i + ", " + j + ")");
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
    public double ScoreWord(string word)
    {
        double points = 0;
        for (int i = 0; i < word.Length; i++)
        {
            points += GetPoints(word[i]);
        }
        return points;
    }
    public Tile MakeTile(Vector2 position)
    {
        string letter = GetRandomLetter();
        while (LetterCount(letter[0]) >= 4)
            letter = GetRandomLetter();

        Tile tile = Instantiate(tilePrefab, transform.position + (Vector3)position + Vector3.up * 4f, Quaternion.identity, transform);
        tile.value = letter;
        tile.gridIndex = position;
        tile.board = this;
        return tile;
    }
    private string GetRandomLetter()
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
            if (r < GetWeight(GetLetter(i)))
            {
                letter = GetLetter(i).ToString();
                break;
            }
            r -= GetWeight(GetLetter(i));
        }

        //add 'u' to 'Q'
        if (letter.Equals("Q"))
            letter += "u";

        return letter;
    }
    public int LetterCount(char letter)
    {
        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (grid[i, j] != null)
                    foreach (char c in grid[i, j].value)
                        if (c.Equals(letter))
                            count++;
            }
        }
        return count;
    }
    public static double GetPoints(char value)
    {
        value = char.ToUpper(value);
        switch (value)
        {
            case 'A': return 1;
            case 'B': return 1.5;
            case 'C': return 1.5;
            case 'D': return 1.25;
            case 'E': return 1;
            case 'F': return 1.75;
            case 'G': return 1.25;
            case 'H': return 1.75;
            case 'I': return 1;
            case 'J': return 3.0;
            case 'K': return 2.5;
            case 'L': return 1;
            case 'M': return 1.5;
            case 'N': return 1;
            case 'O': return 1;
            case 'P': return 1.5;
            case 'Q': return 3.5;
            case 'R': return 1;
            case 'S': return 1;
            case 'T': return 1;
            case 'U': return 1;
            case 'V': return 2.0;
            case 'W': return 2.0;
            case 'X': return 3.5;
            case 'Y': return 1.75;
            case 'Z': return 4.0;
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
    public static int GetIndex(char letter)
    {
        switch (letter)
        {
            case 'A': return 0;
            case 'B': return 1;
            case 'C': return 2;
            case 'D': return 3;
            case 'E': return 4;
            case 'F': return 5;
            case 'G': return 6;
            case 'H': return 7;
            case 'I': return 8;
            case 'J': return 9;
            case 'K': return 10;
            case 'L': return 11;
            case 'M': return 12;
            case 'N': return 13;
            case 'O': return 14;
            case 'P': return 15;
            case 'Q': return 16;
            case 'R': return 17;
            case 'S': return 18;
            case 'T': return 19;
            case 'U': return 20;
            case 'V': return 21;
            case 'W': return 22;
            case 'X': return 23;
            case 'Y': return 24;
            case 'Z': return 25;
            case 'a': return 26;
            case 'b': return 27;
            case 'c': return 28;
            case 'd': return 29;
            case 'e': return 30;
            case 'f': return 31;
            case 'g': return 32;
            case 'h': return 33;
            case 'i': return 34;
            case 'j': return 35;
            case 'k': return 36;
            case 'l': return 37;
            case 'm': return 38;
            case 'n': return 39;
            case 'o': return 40;
            case 'p': return 41;
            case 'q': return 42;
            case 'r': return 43;
            case 's': return 44;
            case 't': return 45;
            case 'u': return 46;
            case 'v': return 47;
            case 'w': return 48;
            case 'x': return 49;
            case 'y': return 50;
            case 'z': return 51;
            case '1': return 52;
            case '2': return 53;
            case '3': return 54;
            case '4': return 55;
            case '5': return 56;
            case '6': return 57;
            case '7': return 58;
            case '8': return 59;
            case '9': return 60;
            case '0': return 61;
            case '?': return 62;
            case '!': return 63;
            case '-': return 64;
            default: return -1;
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

        int length = word.Count;
        wordPosition.x -= 0.5f * (length + 1);
        wordPosition.x += 1f * index;

        return wordPosition;
    }
    public void SetBoardState(bool enabled)
    {
        boardEnabled = enabled;
        if (boardEnabled)
        {
            foreach (Tile t in grid)
            {
                t.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        else
        {
            foreach (Tile t in grid)
            {
                t.GetComponent<SpriteRenderer>().color = Color.gray;
                t.wordIndex = -1;
            }
        }
    }

    public void AddHealth(int n)
    {
        health += n;
        HealthBar.text = health.ToString("000");
        HealthBar.UpdateLetters();
    }
}
