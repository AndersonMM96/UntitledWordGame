using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{
    //Variables
    public int playerNumber;
    public int health;
    public bool boardEnabled = false;
    public bool movementEnabled;
    public bool shakeBoard = false;
    public bool canScramble = true;
    public Tile[,] grid = new Tile[4, 4];
    public List<Tile> word = new List<Tile>();
    public bool validWord;
    //Movement variables
    public Vector2Int selectorPosition = new Vector2Int(0, 0);
    private Vector2Int heldDirection;
    private bool[] directions;
    private float moveCooldown;
    private float moveInterval = 0.15f;

    //Prefabs
    public Tile tilePrefab;
    public ParticleSystem TileGib;

    //Child objects
    private SpriteAnimator selector;
    public Transform gridTransform;
    public Transform wordTransform;
    private TextBar HealthBar;

    private void Start()
    {
        /*
        InputManager.GetMovementEvent += GetMovement;
        InputManager.GetSelectEvent += SelectTile;
        InputManager.GetBackEvent += DeselectTile;
        InputManager.GetEnterEvent += SubmitWord;
        InputManager.GetScrambleEvent += Scramble;
        */
    }

    private void Awake()
    {
        //Get child objects
        selector = transform.GetChild(0).GetComponent<SpriteAnimator>();
        gridTransform = transform.GetChild(1);
        wordTransform = transform.GetChild(2);
        HealthBar = transform.GetChild(3).GetComponent<TextBar>();
    }

    public void InitializeBoard(int playerNumber, Color selectorColor, Vector3 wordPosition)
    {
        this.playerNumber = playerNumber;

        //Set some variables
        HealthBar.SetText(health.ToString("000"));

        directions = new bool[4];
        selectorPosition = new Vector2Int(0, 3);
        selector.transform.position = gridTransform.position + (Vector3)(Vector2)selectorPosition;

        validWord = false;
        wordTransform.position = wordPosition;

        //Set selector variables based on player number
        selector.GetComponent<SpriteRenderer>().color = selectorColor;
        selector.pulseTimer = (selector.pulseInterval * 0.5f) * playerNumber;
        selector.rotateDirection = playerNumber % 2 == 1 ? true : false;
    }
    public void InitializeBoard(int playerNumber, Color selectorColor) { InitializeBoard(playerNumber, selectorColor, wordTransform != null ? wordTransform.position : Vector3.up * 3f); }

    private void Update()
    {
        selector.transform.position = gridTransform.position + (Vector3)(Vector2)selectorPosition;
        moveCooldown -= Time.deltaTime;
        if (moveCooldown <= 0f && movementEnabled)
        {
            moveCooldown = moveInterval;
            MoveSelector(heldDirection);
        }
    }

    public void GetMovement(int playernum, Vector2 v)
    {
        if (playerNumber == playernum && movementEnabled)
        {
            bool[] newDirections = new bool[] { false, false, false, false };
            if(v.y > 0)
                newDirections[0] = true;
            if (v.x > 0)
                newDirections[1] = true;
            if (v.y < 0)
                newDirections[2] = true;
            if (v.x < 0)
                newDirections[3] = true;

            if (newDirections[0] && !directions[0])
                MoveSelector(Vector2Int.up);
            if (newDirections[1] && !directions[1])
                MoveSelector(Vector2Int.right);
            if (newDirections[2] && !directions[2])
                MoveSelector(Vector2Int.down);
            if (newDirections[3] && !directions[3])
                MoveSelector(Vector2Int.left);

            directions = newDirections;
            heldDirection = new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
            moveCooldown = moveInterval;
        }
    }
    private void MoveSelector(Vector2Int direction)
    {
        selectorPosition += direction;

        if (selectorPosition.x < 0)
            selectorPosition.x = 3;
        if (selectorPosition.x > 3)
            selectorPosition.x = 0;
        if (selectorPosition.y < 0)
            selectorPosition.y = 3;
        if (selectorPosition.y > 3)
            selectorPosition.y = 0;
    }
    public void SelectTile(int playernum, bool b)
    {
        if (b && playerNumber == playernum && boardEnabled && movementEnabled)
        {
            if (grid[selectorPosition.x, selectorPosition.y].wordIndex == -1)
            {
                word.Add(grid[selectorPosition.x, selectorPosition.y]);
                word[word.Count - 1].wordIndex = word.Count;
                CheckValidity();
            }
        }

    }
    public void DeselectTile(int playernum, bool b)
    {
        if (b && word.Count > 0 && playerNumber == playernum && boardEnabled && movementEnabled)
        {
            word[word.Count - 1].wordIndex = -1;
            word.RemoveAt(word.Count - 1);
            CheckValidity();
        }
    }
    private void CheckValidity()
    {
        string w = GetWord();
        validWord = w.Length >= 3 && Dictionary.instance.ContainsWord(w);
        Game.instance.UpdateReaction(ScoreWord(w), validWord);
    }
    public void SubmitWord(int playernum, bool b)
    {
        if (b && playerNumber == playernum && boardEnabled && movementEnabled)
        {
            if (validWord)
            {
                //code for displaying word scores in console--otherwise these 3 lines can be removed
                string w = GetWord();
                double points = ScoreWord(w);
                Debug.Log(w + " - " + points + " Points");

                //Send word to game
                StartCoroutine(Game.instance.SendScore());
            }
            else
                Debug.LogWarning(GetWord() + " - Not a valid word!");
        }
    }

    public void FillGrid(bool replace)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (replace || grid[i, j] == null)
                {
                    if (grid[i, j] != null)
                        DestroyTile(i, j);
                    grid[i, j] = MakeTile(new Vector2Int(i, j));
                }
            }
        }
    }
    public void Scramble(int playernum, bool b)
    {
        if (playernum == playerNumber && b && canScramble && boardEnabled && movementEnabled)
        {
            ResetWord();
            FillGrid(true);
            canScramble = false;
        }
    }
    public void DropTiles()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 1; j < 4; j++)
            {
                DropTile(i, j);
            }
        }
    }
    private void DropTile(int i, int j)
    {
        while (j > 0)
        {
            j--;
            if (grid[i, j] == null)
            {
                grid[i, j] = grid[i, j + 1];
                grid[i, j + 1] = null;
                if (grid[i, j] != null)
                    grid[i, j].gridIndex = new Vector2Int(i, j);
            }
            else
                break;
        }
    }

    public void DestroyTile(int i, int j)
    {
        //Debug.LogWarning("Destroying " + grid[i, j].name + " (" + i + ", " + j + ")");
        StartCoroutine(EmitTileGib(grid[i, j].transform.position));
        Destroy(grid[i, j].gameObject);
        grid[i, j] = null;
    }
    private IEnumerator EmitTileGib(Vector3 position)
    {
        ParticleSystem gib = Instantiate(TileGib, position, Quaternion.identity, transform);
        gib.Emit(10);
        yield return new WaitForSeconds(1f);
        Destroy(gib.gameObject);
    }

    public string GetWord()
    {
        string letters = "";
        foreach (Tile tile in word)
        {
            letters += tile.GetValue();
        }
        return letters;
    }
    public static double ScoreWord(string word)
    {
        double points = 0;
        for (int i = 0; i < word.Length; i++)
        {
            points += GetPoints(word[i]);
        }
        return points;
    }
    public Tile MakeTile(Vector2Int position)
    {
        string letter = GetRandomLetter();
        while (LetterCount(letter[0]) >= 4)
            letter = GetRandomLetter();

        Tile tile = Instantiate(tilePrefab, gridTransform.position + (Vector3)(Vector2)position + Vector3.up * 10f, Quaternion.identity, transform);
        tile.SetValue(letter);
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
                    foreach (char c in grid[i, j].GetValue())
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
        /*
        switch (value)
        {
            case 'A': return 1.0;
            case 'B': return 1.25;
            case 'C': return 1.25;
            case 'D': return 1.0;
            case 'E': return 1.0;
            case 'F': return 1.25;
            case 'G': return 1.0;
            case 'H': return 1.25;
            case 'I': return 1.0;
            case 'J': return 1.75;
            case 'K': return 1.75;
            case 'L': return 1.0;
            case 'M': return 1.25;
            case 'N': return 1.0;
            case 'O': return 1.0;
            case 'P': return 1.25;
            case 'Q': return 1.75;
            case 'R': return 1.0;
            case 'S': return 1.0;
            case 'T': return 1.0;
            case 'U': return 1.0;
            case 'V': return 1.5;
            case 'W': return 1.5;
            case 'X': return 2.0;
            case 'Y': return 1.5;
            case 'Z': return 2.0;
            default: return 0;
        }
        */
        /*
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
        */
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
            case ' ': return 65;
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
    public Vector2 GetTilePosition(int index)
    {
        Vector2 wordPosition = wordTransform.position;

        int length = word.Count;
        wordPosition.x -= 0.5f * (length + 1);
        wordPosition.x += 1f * index;

        return wordPosition;
    }
    public void SetBoardState(bool enabled)
    {
        boardEnabled = enabled;
    }
    public void ResetWord()
    {
        foreach (Tile t in grid)
            if (t != null)
                t.wordIndex = -1;
        word.Clear();
    }

    public void SetHealth(int n)
    {
        health = n;
        HealthBar.SetText(health.ToString("000"));
    }
    public void AddHealth(int n)
    {
        health += n;
        HealthBar.SetText(health.ToString("000"));
    }
    public IEnumerator TickHealth(int n)
    {
        if (n < 0)
            n = 0;
        while (health != n)
        {
            if(health > n)
                SetHealth(health - 1);
            else
                SetHealth(health + 1);
            yield return new WaitForSeconds(0.004f);
        }
    }
}
