using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game instance;

    //Settings
    [SerializeField] public bool timerEnabled = true;
    [SerializeField] public int maxHealth = 999;
    [SerializeField] public int timeLimit = 15;

    public const float scoreSpeed = 0.25f;
    private readonly string[] reactions = new string[] { "", "", "", "", "", "", "", "", "Good", "Great", "Awesome!", "Fantastic!", "INSANE!", "UNREAL!", "EXTRAORDINARY!", "UNFATHOMABLE", "GODLY" };

    private int[] players;
    private Board[] boards;
    private int turn = 0;
    private bool timerPaused = false;
    private float timerCurrent;
    public bool gameEnded = false;

    //Child objects
    private Camera mainCamera;
    private TilePile word;
    private SpriteAnimator bomb;
    private TextPile timer;
    private TextPile damageText;
    private TextPile reactionText;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        Board.SubmitWord += Submit;
        Board.UpdateWord += UpdateReaction;
    }

    public void Initialize(int[] players)
    {
        this.players = players;

        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        //Create Word
        word = TilePile.Create(transform);
        word.transform.localPosition = new Vector2(0, 2.5f);
        word.name = "Word";

        //Create Boards
        boards = new Board[players.Length];
        for(int i = 0; i < boards.Length; i++)
        {
            boards[i] = Board.Create(players[i], transform);
            boards[i].transform.localPosition = new Vector2(-3.75f * Mathf.Pow(-1, i), -1f);
            boards[i].word = word;
            boards[i].Initialize(players[i]);
            boards[i].FillBoard(true);
            boards[i].SetEnabled(false);
            boards[i].SetHealth(maxHealth);
        }

        //Create bomb
        Sprite[] bombSprites = Resources.LoadAll<Sprite>("Textures/bomb");
        bomb = new GameObject().AddComponent<SpriteAnimator>();
        bomb.name = "Bomb";
        bomb.gameObject.AddComponent<SpriteRenderer>();
        bomb.SetAnimate(false, new List<Sprite>() { bombSprites[0], bombSprites[2] });
        bomb.transform.parent = transform;
        bomb.SetPosition(new Vector2(0.25f, 1f), true);
        bomb.sortingOrder = -4;

        timer = TextPile.Create(bomb.transform);
        timer.name = "Timer";
        timer.transform.localPosition = new Vector2(-0.25f, -0.35f);
        timer.max = 2;
        timer.SetText(timeLimit.ToString());
        timer.SetSortingOrder(-3);

        timerPaused = true;

        //Damage Text
        damageText = TextPile.Create(transform);
        damageText.name = "Damage Text";
        damageText.SetText("");
        damageText.AddColor(Color.black);
        damageText.gameObject.AddComponent<SpriteAnimator>();
        damageText.GetComponent<SpriteAnimator>().SetPosition(new Vector3(0, -1f));
        damageText.GetComponent<SpriteAnimator>().SetPulse(false, 0.75f, 0.2f, 5f);

        //Reaction Text
        reactionText = TextPile.Create(transform);
        reactionText.name = "Reaction Text";
        reactionText.gameObject.AddComponent<SpriteAnimator>();
        reactionText.GetComponent<SpriteAnimator>().SetPosition(new Vector3(0, 3.8f));
        reactionText.GetComponent<SpriteAnimator>().SetMoveSpeed(15f);
        reactionText.SetText("");
        reactionText.AddColor(Color.black);
        reactionText.SetBob(true, 0.25f, 0.5f);
    }

    private void Update()
    {
        bomb.gameObject.SetActive(timerEnabled);

        //Update Bomb
        bomb.SetShake(timerEnabled && !timerPaused);
        bomb.SetSprite(timerEnabled && !timerPaused ? 1 : 0);

        if (timerEnabled && !timerPaused)
        {
            //Update Timer
            timerCurrent -= Time.deltaTime;
            timer.SetText(Mathf.FloorToInt(timerCurrent).ToString());
        }
        if (timerCurrent <= 0 && !timerPaused)
        {
            //Time's up
            timerCurrent = 0f;
            StartCoroutine(TimesUp());
        }
    }

    public void StartGame()
    {
        SetTurn(0);
        timerPaused = false;
        timerCurrent = timeLimit;
        gameEnded = false;
    }

    private void Submit(int playernum)
    {
        if (GetBoard(playernum) == turn)
        {
            StartCoroutine(ScoreWord());
        }
    }

    private IEnumerator ScoreWord()
    {
        Board current = boards[turn];
        Board other = boards[NotTurn()];

        //Disable board and pause timer
        current.SetEnabled(false);
        timerPaused = true;

        //Destroy current board's word tiles
        damageText.SetText("");
        double points = 0.0;
        int damage = 0;
        string spelledWord = word.GetWord();
        word.SetShake(true, 0.05f, 0.025f);
        for (int i = 0; i < word.length; i++)
        {
            points += Board.GetPoints(spelledWord[i]);
            damage = Mathf.FloorToInt(2f * Mathf.Pow((float)points, 2)) + 1;
            word.Get(i).GetComponent<Tile>().DestroyTile();
            damageText.SetText(damage.ToString());
            damageText.GetComponent<SpriteAnimator>().Pulse();
            yield return new WaitForSeconds(scoreSpeed);
        }
        word.Clear();
        word.SetShake(false, 0.04f, 0.05f);
        current.ClearWord();

        //"attack animation"
        if (points >= 12.0)
        {
            //Unreal+
            //possibly select random damage animation here, or just have one animation per line of text
            yield return StartCoroutine(DamageAnimLightning(damage));
        }
        else
            yield return StartCoroutine(DamageAnimDefault(damage));

        //current.DropTiles();
        current.FillBoard(false);
        yield return new WaitForSeconds(0.5f);

        //Check for loss
        if(other.GetHealth() <= 0)
        {
            //lose
            gameEnded = true;
            boards[0].movementEnabled = false;
            boards[1].movementEnabled = false;
            int winner = turn;
            int loser = NotTurn();
            SetReaction("Player " + (winner + 1) + " Wins!");

            boards[loser].ShakeBoard(true);
            List<TilePile> tiles = new List<TilePile>();
            foreach (TilePile pile in boards[loser].tiles)
                tiles.Add(pile);
            int c = tiles.Count;
            for(int i = 0; i < c; i++)
            {
                int r = Random.Range(0, tiles.Count);
                tiles[r].DestroyTile();
                tiles.RemoveAt(r);
                yield return new WaitForSeconds(scoreSpeed);
            }
        }
        else
        {
            timerCurrent = timeLimit;
            timerPaused = false;
            SetTurn(NotTurn());
            SetReaction("");
        }
    }

    public IEnumerator EndGame()
    {
        foreach (TilePile pile in boards[0].tiles)
            pile.DestroyTile();
        foreach (TilePile pile in boards[1].tiles)
            pile.DestroyTile();

        //Normally, this shouldn't need to be done. However, I am kind of stupid. And this works. So it's fine.
        Board.SubmitWord = null;
        Board.UpdateWord = null;

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private IEnumerator TimesUp()
    {
        Board current = boards[turn];
        Board other = boards[NotTurn()];

        current.ResetWord();
        current.SetEnabled(false);

        timerPaused = true;
        SpriteAnimator explosion = new GameObject().AddComponent<SpriteAnimator>();
        explosion.AddComponent<SpriteRenderer>();
        explosion.transform.parent = bomb.transform;
        explosion.name = "Explosion";
        explosion.transform.localPosition = Vector2.zero;
        List<Sprite> explosionSprites = Resources.LoadAll<Sprite>("Textures/explosion").ToList();
        explosion.SetAnimate(true, explosionSprites, 0.075f);
        timer.SetText("");
        bomb.GetComponent<SpriteRenderer>().enabled = false;
        AudioManager.instance.PlayAudio(Resources.Load<AudioClip>("Audio/explosion"), bomb.transform.position, 0.5f);
        yield return new WaitForSeconds(1.2f);
        Destroy(explosion.gameObject);
        bomb.GetComponent<SpriteRenderer>().enabled = true;
        timerCurrent = timeLimit;
        timerPaused = false;
        SetTurn(NotTurn());
    }

    private IEnumerator DamageAnimDefault(int damage)
    {
        Board current = boards[turn];
        Board other = boards[NotTurn()];
        SpriteAnimator damageAnimator = damageText.GetComponent<SpriteAnimator>();

        //Move damage text
        damageAnimator.SetTarget(damageText.transform.localPosition + Vector3.left * 1f * (turn == 0 ? 1 : -1));
        damageAnimator.SetMoveSpeed(30f);
        yield return new WaitForSeconds(0.075f);
        damageAnimator.SetTarget(other.transform.localPosition);
        damageAnimator.SetMoveSpeed(50f);
        yield return new WaitForSeconds(0.05f);

        //Deal damage to other board
        other.ShakeBoard(true);
        damageText.SetText(""); 
        yield return StartCoroutine(other.TickHealth(other.GetHealth() - damage));
        if (other.GetHealth() <= 0)
            other.SetHealth(0);
        damageAnimator.SetPosition(new Vector3(0, -1f));
        other.ShakeBoard(false);
    }

    private IEnumerator DamageAnimLightning(int damage)
    {
        Board current = boards[turn];
        Board other = boards[NotTurn()];
        SpriteAnimator damageAnimator = damageText.GetComponent<SpriteAnimator>();
        SpriteRenderer darken = mainCamera.transform.GetChild(1).GetComponent<SpriteRenderer>();

        //Create lightning
        SpriteRenderer lightning = new GameObject().AddComponent<SpriteRenderer>();
        lightning.transform.parent = transform;
        lightning.name = "Lightning";
        lightning.sprite = Resources.Load<Sprite>("Textures/lightning");
        lightning.color = new Color(lightning.color.r, lightning.color.b, lightning.color.g, 0f);
        lightning.sortingOrder = 21;
        //Random flip sprite chance
        lightning.flipX = false;
        if (Random.Range(0, 2) == 1)
            lightning.flipX = true;
        lightning.transform.position = other.transform.position + new Vector3(lightning.flipX ? 0.45f : -0.2f, 1.85f);
        AudioClip thunder = Resources.Load<AudioClip>("Audio/thunder");

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
        AudioManager.instance.PlayAudio(thunder, lightning.gameObject.transform.position, 0.25f);
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

        //Deal damage to other board
        other.ShakeBoard(true);
        damageText.SetText("");
        StartCoroutine(other.TickHealth(other.GetHealth() - damage)); 
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
        if (other.GetHealth() <= 0)
            other.SetHealth(0);
        damageAnimator.SetPosition(new Vector3(0, -1f));
        other.ShakeBoard(false);
        Destroy(lightning.gameObject);
    }

    private void UpdateReaction()
    {
        string spelledWord = word.GetWord();
        double points = Board.GetScore(spelledWord);
        bool valid = spelledWord.Length >= 3 && Dictionary.instance.ContainsWord(spelledWord);
        string text = "";
        if (valid)
        {
            if (points >= reactions.Length)
            {
                text = reactions[reactions.Length];
            }
            else
            {
                for (int i = 1; i < reactions.Length; i++)
                {
                    if (points < i)
                    {
                        text = reactions[i - 1];
                        break;
                    }
                }
            }
        }
        SetReaction(text);
    }

    public void SetReaction(string text)
    {
        reactionText.SetText(text);
        reactionText.GetComponent<SpriteAnimator>().SetPosition(new Vector2(0f, 5f), false);
    }

    public int GetBoard(int playernum)
    {
        for(int i = 0; i < players.Length; i++)
            if (players[i] == playernum)
                return i;
        return -1;
    }

    public void SetTurn(int turn)
    {
        this.turn = turn;
        boards[turn].ResetWord();
        boards[turn].SetEnabled(true);
        boards[turn].canScramble = true;
        boards[NotTurn()].ResetWord();
        boards[NotTurn()].SetEnabled(false);
        boards[NotTurn()].canScramble = true;
    }

    public int NotTurn()
    {
        return (turn + 1) % 2;
    }
}
