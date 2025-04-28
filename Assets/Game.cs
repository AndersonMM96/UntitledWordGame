using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour
{
    //Instance
    public static Game instance;

    //Constants
    private const int maxHP = 999;
    private const float scoreSpeed = 0.25f;
    public const float maxTime = 15f;
    private readonly string[] reactions = new string[] { "", "", "", "", "", "", "", "", "Good", "Great", "Awesome!", "Fantastic!", "INSANE!", "UNREAL!", "EXTRAORDINARY!", "UNFATHOMABLE", "GODLY" };

    //Variables
    private int turn = 0;
    public bool enableTimer;
    private bool updateTimer;
    private float timer;
    public Board[] boards;

    //Prefabs
    public Board boardPrefab;

    //Child objects
    private Camera mainCamera;
    private SpriteAnimator bomb;
    private Vector3 bombPosition;
    private TextBar timerBar;
    private TextBar scoreBar;
    private TextBar reactionBar;
    private Transform animationTransform;
    private Transform wordTransform;
    private Transform[] boardTransforms;
    public List<AudioClip> audioClips;
    
    //Setup
    private void Awake()
    {
        if (instance == null)
            instance = this;

        //Get child objects
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        bomb = transform.GetChild(0).GetComponent<SpriteAnimator>();
        bombPosition = bomb.transform.position;
        timerBar = bomb.transform.GetChild(0).GetComponent<TextBar>();
        scoreBar = transform.GetChild(1).GetComponent<TextBar>();
        reactionBar = transform.GetChild(2).GetComponent<TextBar>();
        animationTransform = transform.GetChild(3);
        wordTransform = transform.GetChild(4);
        boardTransforms = new Transform[] { wordTransform.GetChild(0), wordTransform.GetChild(1) };

        //StartCoroutine(BeginGame(new int[]{ 0, 1 }));
    }

    public IEnumerator BeginGame(int[] players)
    {
        //Create and initialize boards
        boards = new Board[] { Instantiate(boardPrefab, boardTransforms[0]), Instantiate(boardPrefab, boardTransforms[1]) };
        boards[0].InitializeBoard(players[0], Color.red, wordTransform.position);
        boards[1].InitializeBoard(players[1], Color.blue, wordTransform.position);
        boards[0].SetHealth(maxHP);
        boards[1].SetHealth(maxHP);
        boards[0].FillGrid(true);
        boards[1].FillGrid(true);
        bomb.GetComponent<SpriteRenderer>().enabled = true;
        yield return new WaitForSeconds(1f);
        UpdateReaction("");
        timer = maxTime;
        updateTimer = true;
        boards[0].movementEnabled = true;
        boards[1].movementEnabled = true;
        SetTurn(turn);
    }

    //Game Logic
    private void Update()
    {
        //Update Bomb
        bomb.shake = (updateTimer && enableTimer);
        bomb.SetSprite(bomb.shake ? 1 : 0);
        bomb.transform.position = bombPosition + bomb.shakeOffset;

        if (updateTimer && enableTimer)
        {
            //Update Timer
            timer -= Time.deltaTime;
            timerBar.SetText(Mathf.FloorToInt(timer).ToString());
        }
        if(timer <= 0 && updateTimer)
        {
            //Time's up
            timer = 0f;
            StartCoroutine(TimesUp());
        }
    }

    public IEnumerator SendScore()
    {
        Board current, other;
        current = GetBoard(true);
        other = GetBoard(false);

        updateTimer = false;

        //Disable current board after word has been submitted
        current.SetBoardState(false);

        //Destroy current board's word tiles
        scoreBar.SetText("");
        double points = 0.0;
        int damage = 0;
        for (int i = 0; i < current.word.Count; i++)
        {
            points += Board.GetPoints(current.GetWord()[i]);
            damage = Mathf.FloorToInt(2f * Mathf.Pow((float)points, 2)) + 1;
            current.DestroyTile(current.word[i].gridIndex.x, current.word[i].gridIndex.y);
            scoreBar.SetText(damage.ToString());
            scoreBar.GetComponent<SpriteAnimator>().Pulse();
            yield return new WaitForSeconds(scoreSpeed);
        }
        current.ResetWord();

        //"attack animation"
        if(points >= 12.0)
        {
            //Unreal+
            //possibly select random damage animation here, or just have one animation per line of text
            yield return StartCoroutine(DamageAnimLightning(damage));
        }
        else
            yield return StartCoroutine(DamageAnimDefault(damage));

        //pre-reset
        current.DropTiles();
        current.FillGrid(false);
        yield return new WaitForSeconds(0.5f);

        //check for loss
        if (other.health <= 0)
        {
            //lose
            boards[0].movementEnabled = false;
            boards[1].movementEnabled = false;
            int winner = turn;
            int loser = GetOther(turn);
            UpdateReaction("Player " + (winner + 1) + " Wins!");

            //lose animation
            other.shakeBoard = true;
            List<Tile> tiles = new List<Tile>();
            foreach(Tile tile in boards[loser].grid)
                tiles.Add(tile);
            int c = tiles.Count;
            for (int i = 0; i < c; i++)
            {
                int r = Random.Range(0, tiles.Count);
                Tile tile = tiles[r];
                tiles.Remove(tile);
                boards[loser].DestroyTile(tile.gridIndex.x, tile.gridIndex.y);
                yield return new WaitForSeconds(scoreSpeed);
            }
        }
        else
        {
            //switch turn and set other board to be active
            timer = maxTime;
            updateTimer = true;
            turn = GetOther(turn);
            UpdateReaction("");
            other.SetBoardState(true);
            other.ResetWord();
            other.canScramble = true;
        }
    }

    private IEnumerator TimesUp()
    {
        Board current, other;
        current = GetBoard(true);
        other = GetBoard(false);

        current.SetBoardState(false);
        current.ResetWord();

        updateTimer = false;
        SpriteAnimator explosion = bomb.transform.GetChild(1).GetComponent<SpriteAnimator>();
        timerBar.SetText("");

        //Explode Bomb
        explosion.gameObject.SetActive(true);
        explosion.animate = true;
        bomb.enabled = false;
        bomb.GetComponent<SpriteRenderer>().enabled = false;
        AudioManager.instance.PlayAudio(audioClips[1], bomb.transform.position, 0.5f);
        yield return new WaitForSeconds(1.2f);
        explosion.gameObject.SetActive(false);
        explosion.animate = false;
        bomb.enabled = true;
        bomb.GetComponent<SpriteRenderer>().enabled = true;

        other.SetBoardState(true);
        other.ResetWord();
        timer = maxTime;
        updateTimer = true;
        turn = GetOther(turn);
    }

    private void SetTurn(int turn)
    {
        boards[turn].SetBoardState(true);
        boards[turn].ResetWord();
        boards[GetOther(turn)].SetBoardState(false);
        boards[GetOther(turn)].ResetWord();
    }
    private Board GetBoard(bool current)
    {
        if (current)
        {
            return boards[turn];
        }
        return boards[GetOther(turn)];
    }
    private int GetOther(int i)
    {
        return (i == 0 ? 1 : 0);
    }
    public int PlayerIndexToNumber(int index)
    {
        for (int i = 0; i < boards.Length; i++)
            if (boards[i].playerNumber == index)
                return i;
        return -1;
    }

    //Animations
    private IEnumerator DamageAnimDefault(int damage)
    { 
        Board current, other;
        current = GetBoard(true);
        other = GetBoard(false);

        scoreBar.OffsetLetters(Vector3.left * 1f * (turn == 0 ? 1 : -1), false);
        scoreBar.animSpeed = 30f;
        yield return new WaitForSeconds(0.075f);
        scoreBar.OffsetLetters(Vector3.right * 4f * (turn == 0 ? 1 : -1), false);
        scoreBar.animSpeed = 50f;
        yield return new WaitForSeconds(0.05f);
        //deal damage to other board
        other.shakeBoard = true;
        scoreBar.SetText("");
        yield return StartCoroutine(other.TickHealth(other.health - damage));
        if (other.health <= 0)
            other.SetHealth(0);
        scoreBar.OffsetLetters(Vector3.zero, false);
        scoreBar.OffsetLetters(Vector3.left * 4f * (turn == 0 ? 1 : -1), true);
        other.shakeBoard = false;
    }
    private IEnumerator DamageAnimLightning(int damage)
    {
        Board current, other;
        current = GetBoard(true);
        other = GetBoard(false);

        SpriteRenderer lightning = animationTransform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer darken = mainCamera.transform.GetChild(1).GetComponent<SpriteRenderer>();

        //random flip sprite chance
        lightning.flipX = false;
        if (Random.Range(0, 2) == 1)
            lightning.flipX = true;

        //set lightning position
        lightning.transform.position = other.transform.position + new Vector3(lightning.flipX ? 0.45f : -0.2f, 1.85f);

        //begin animation
        while (darken.color.a != 1f)
        {
            darken.color = new Color(darken.color.r, darken.color.b, darken.color.g, darken.color.a + (1f / 256f));
            yield return new WaitForSeconds(0.002f);
        }
        yield return new WaitForSeconds(0.5f);
        darken.color = new Color(darken.color.r, darken.color.b, darken.color.g, 200f / 256f);
        while (darken.color.a != 1f)
        {
            darken.color = new Color(darken.color.r, darken.color.b, darken.color.g, darken.color.a + (1f / 256f));
            yield return new WaitForSeconds(0.002f);
        }
        //play thunder audio
        AudioManager.instance.PlayAudio(audioClips[0], lightning.gameObject.transform.position, 0.25f);
        yield return new WaitForSeconds(0.1f);
        darken.color = new Color(darken.color.r, darken.color.b, darken.color.g, 200f / 256f);
        while (darken.color.a != 1f)
        {
            darken.color = new Color(darken.color.r, darken.color.b, darken.color.g, darken.color.a + (1f / 256f));
            yield return new WaitForSeconds(0.002f);
        }
        yield return new WaitForSeconds(0.5f);
        darken.color = new Color(darken.color.r, darken.color.b, darken.color.g, 200f / 256f);
        lightning.color = new Color(lightning.color.r, lightning.color.b, lightning.color.g, 1f);
        //deal damage to other board
        other.shakeBoard = true;
        scoreBar.SetText("");
        StartCoroutine(other.TickHealth(other.health - damage));
        while (darken.color.a != 0f || lightning.color.a != 0f)
        {
            darken.color = new Color(darken.color.r, darken.color.b, darken.color.g, darken.color.a - (0.5f / 256f));
            lightning.color = new Color(lightning.color.r, lightning.color.b, lightning.color.g, lightning.color.a - (1f / 256f));
            if (darken.color.a < 0f)
                darken.color = new Color(darken.color.r, darken.color.b, darken.color.g, 0f);
            if (lightning.color.a < 0f)
                lightning.color = new Color(lightning.color.r, lightning.color.b, lightning.color.g, 0f);
            yield return new WaitForSeconds(0.002f);
        }

        if (other.health <= 0)
            other.SetHealth(0);
        other.shakeBoard = false;
    }

    //Reaction Bar
    public void UpdateReaction(double score, bool valid)
    {
        //Debug.Log(score);
        string text = "";
        if (valid)
        {
            if (score >= reactions.Length)
            {
                text = reactions[reactions.Length];
            }
            else
            {
                for (int i = 1; i < reactions.Length; i++)
                {
                    if (score < i)
                    {
                        text = reactions[i - 1];
                        break;
                    }
                }
            }
        }
        UpdateReaction(text);
    }
    public void UpdateReaction(string text)
    {
        reactionBar.SetText(text);
        reactionBar.OffsetLetters(Vector3.up * 1.5f, true);
        reactionBar.bob = true;
    }

}
