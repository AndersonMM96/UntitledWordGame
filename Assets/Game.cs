using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Board[] boards;
    bool turn = true;

    private int maxHP = 999;

    private bool enableTimer = false;
    private float timer;
    private TextBar TimerBar;
    public float maxTime = 15f;
    private bool updateTimer;

    private string[] reactions = new string[] { "", "", "", "", "", "", "", "", "Good", "Great", "Awesome!", "Fantastic!", "INSANE!", "UNREAL!", "EXTRAORDINARY!", "UNFATHOMABLE", "GODLY" };

    // Start is called before the first frame update
    void Start()
    {
        TimerBar = transform.GetChild(0).GetComponent<TextBar>();
        timer = maxTime;
        updateTimer = true;

        boards[0].game = this;
        boards[1].game = this;
        boards[0].SetHealth(maxHP);
        boards[1].SetHealth(maxHP);
        boards[0].FillGrid(true);
        boards[1].FillGrid(true);
        UpdateReaction(0, false);
        SwitchTurn(turn);
    }

    private void Update()
    {
        if(updateTimer && enableTimer)
        {
            timer -= Time.deltaTime;
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
        TimerBar.text = (Mathf.FloorToInt(timer)).ToString();
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
        transform.GetChild(5).GetComponent<TextBar>().text = text;
        transform.GetChild(5).GetComponent<TextBar>().bob = true;
        transform.GetChild(5).GetComponent<TextBar>().UpdateLetters();
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
        TextBar scoreBar = transform.GetChild(4).GetComponent<TextBar>();
        scoreBar.text = "";
        double points = 0.0;
        int damage = 0;
        for (int i = 0; i < current.word.Count; i++)
        {
            points += Board.GetPoints(current.GetWord()[i]);
            damage = Mathf.FloorToInt(2f * Mathf.Pow((float)points, 2)) + 1;
            current.DestroyTile((int)current.word[i].gridIndex.x, (int)current.word[i].gridIndex.y);
            scoreBar.text = damage.ToString();
            scoreBar.GetComponent<SpriteAnimator>().pulse = true;
            yield return new WaitForSeconds(0.2f);
            scoreBar.GetComponent<SpriteAnimator>().pulse = false;
        }
        current.ResetWord();
        //deal damage to other board
        other.AddHealth(-damage);
        if(other.health <= 0)
        {
            //lose
            other.SetHealth(0);
        }
        else
        {
            //refill current board's tiles
            current.DropTiles();
            current.FillGrid(false);
            yield return new WaitForSeconds(0.5f);

            //switch turn and set other board to be active
            scoreBar.text = "";
            timer = maxTime;
            updateTimer = true;
            turn = !turn;
            UpdateReaction(0, false);
            other.SetBoardState(true);
            other.ResetWord();
            other.canScramble = true;
        }
    }
    private IEnumerator TimesUp()
    {
        updateTimer = false;
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
