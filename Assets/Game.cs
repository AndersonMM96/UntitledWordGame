using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Board[] boards;
    bool turn = true;

    private int maxHP = 999;

    private TextBar TimerBar;
    private TextBar scoreBar;
    private Transform animations;
    private Camera mainCamera;
    public List<AudioClip> audioClips;

    public bool enableTimer;
    private float timer;
    public float maxTime = 15f;
    private bool updateTimer;

    private float scoreSpeed = 0.25f;

    private string[] reactions = new string[] { "", "", "", "", "", "", "", "", "Good", "Great", "Awesome!", "Fantastic!", "INSANE!", "UNREAL!", "EXTRAORDINARY!", "UNFATHOMABLE", "GODLY" };

    // Start is called before the first frame update
    void Start()
    {
        TimerBar = transform.GetChild(0).GetComponent<TextBar>();
        scoreBar = transform.GetChild(4).GetComponent<TextBar>();
        animations = transform.GetChild(6);
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        boards[0].game = this;
        boards[1].game = this;
        boards[0].movementEnabled = false;
        boards[1].movementEnabled = false;

        StartCoroutine(BeginGame());
    }

    private IEnumerator BeginGame()
    {
        boards[0].SetHealth(maxHP);
        boards[1].SetHealth(maxHP);
        boards[0].FillGrid(true);
        boards[1].FillGrid(true);
        yield return new WaitForSeconds(1f);
        UpdateReaction("");
        timer = maxTime;
        updateTimer = true;
        boards[0].movementEnabled = true;
        boards[1].movementEnabled = true;
        SwitchTurn(turn);
    }

    private void Update()
    {
        if(updateTimer && enableTimer)
        {
            timer -= Time.deltaTime;
            TimerBar.SetText(Mathf.FloorToInt(timer).ToString());
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(true);
            transform.GetChild(3).position = transform.GetChild(2).position + transform.GetChild(3).GetComponent<SpriteAnimator>().shakeOffset;
            TimerBar.transform.localPosition = new Vector3(4.5f, 5.75f, 0f) + transform.GetChild(3).GetComponent<SpriteAnimator>().shakeOffset;
        }
        else
        {
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(false);
            TimerBar.transform.localPosition = new Vector3(4.5f, 5.75f, 0f);
        }
        if(timer <= 0 && updateTimer)
        {
            timer = 0f;
            StartCoroutine(TimesUp());
        }
    }

    public void UpdateReaction(double score, bool valid)
    {
        //Debug.Log(score);
        string text = "";
        if(valid)
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
        transform.GetChild(5).GetComponent<TextBar>().SetText(text);
        transform.GetChild(5).GetComponent<TextBar>().OffsetLetters(Vector3.up * 1.5f, true);
        transform.GetChild(5).GetComponent<TextBar>().bob = true;
    }
    public void UpdateReaction(string text)
    {
        transform.GetChild(5).GetComponent<TextBar>().SetText(text);
        transform.GetChild(5).GetComponent<TextBar>().OffsetLetters(Vector3.up * 1.5f, true);
        transform.GetChild(5).GetComponent<TextBar>().bob = true;
    }
    public IEnumerator SendScore()
    {
        updateTimer = false;
        Board current, other;
        if(turn)
        {
            current = boards[0];
            other = boards[1];
        }
        else
        {
            current = boards[1];
            other = boards[0];
        }
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
            current.DestroyTile((int)current.word[i].gridIndex.x, (int)current.word[i].gridIndex.y);
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
            yield return StartCoroutine(DamageAnimLightning(current, other, damage));
        }
        else
            yield return StartCoroutine(DamageAnimDefault(current, other, damage));

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
            int winner = (turn ? 0 : 1);
            int loser = (!turn ? 0 : 1);
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
            turn = !turn;
            UpdateReaction("");
            other.SetBoardState(true);
            other.ResetWord();
            other.canScramble = true;
        }
    }
    private IEnumerator DamageAnimDefault(Board current, Board other, int damage)
    {
        scoreBar.OffsetLetters(Vector3.left * 1f * (turn ? 1 : -1), false);
        scoreBar.animSpeed = 30f;
        yield return new WaitForSeconds(0.075f);
        scoreBar.OffsetLetters(Vector3.right * 4f * (turn ? 1 : -1), false);
        scoreBar.animSpeed = 50f;
        yield return new WaitForSeconds(0.05f);
        //deal damage to other board
        other.shakeBoard = true;
        scoreBar.SetText("");
        yield return StartCoroutine(other.TickHealth(other.health - damage));
        if (other.health <= 0)
            other.SetHealth(0);
        scoreBar.OffsetLetters(Vector3.zero, false);
        scoreBar.OffsetLetters(Vector3.left * 4f * (turn ? 1 : -1), true);
        other.shakeBoard = false;
    }
    private IEnumerator DamageAnimLightning(Board current, Board other, int damage)
    {
        SpriteRenderer lightning = animations.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer darken = mainCamera.transform.GetChild(1).GetComponent<SpriteRenderer>();

        //random flip sprite chance
        lightning.flipX = false;
        if (Random.Range(0, 2) == 1)
            lightning.flipX = true;

        //set lightning position
        lightning.transform.position = other.transform.position + new Vector3(lightning.flipX ? 1.9f : 1.3f, 3.3f);

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
    private IEnumerator TimesUp()
    {
        updateTimer = false;
        TimerBar.SetText("");
        Board current, other;
        if (turn)
        {
            current = boards[0];
            other = boards[1];
        }
        else
        {
            current = boards[1];
            other = boards[0];
        }
        current.SetBoardState(false);
        current.ResetWord();

        transform.GetChild(1).gameObject.SetActive(true);
        transform.GetChild(1).GetComponent<SpriteAnimator>().animate = true;
        transform.GetChild(2).GetComponent<SpriteRenderer>().enabled = false; 
        AudioManager.instance.PlayAudio(audioClips[1], transform.GetChild(2).position, 0.5f);
        yield return new WaitForSeconds(1.2f);
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(1).GetComponent<SpriteAnimator>().animate = false;
        transform.GetChild(2).GetComponent<SpriteRenderer>().enabled = true;

        other.SetBoardState(true);
        other.ResetWord();
        timer = maxTime;
        updateTimer = true;
        turn = !turn;
    }
    private void SwitchTurn(bool turn)
    {
        boards[0].SetBoardState(turn);
        boards[0].ResetWord();
        boards[1].SetBoardState(!turn);
        boards[1].ResetWord();
    }
}
