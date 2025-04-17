using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Board[] boards;
    bool turn = true;

    private float timer;
    private TextBar TimerBar;
    public float maxTime = 15f;
    private bool updateTimer;

    // Start is called before the first frame update
    void Start()
    {
        TimerBar = transform.GetChild(0).GetComponent<TextBar>();
        timer = maxTime;
        updateTimer = true;

        boards[0].game = this;
        boards[1].game = this;
        boards[0].FillGrid(true);
        boards[1].FillGrid(true);
        SwitchTurn(turn);
    }

    private void Update()
    {
        if(updateTimer)
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

    public IEnumerator SendScore(double score)
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
        for(int i = 0; i < current.word.Count; i++)
        {
            current.DestroyTile((int)current.word[i].gridIndex.x, (int)current.word[i].gridIndex.y);
            yield return new WaitForSeconds(0.2f);
        }
        current.ResetWord();

        //deal damage to other board
        other.AddHealth(-Mathf.RoundToInt(Mathf.Pow((float)score, 2)));
        if(other.health <= 0)
        {
            //lose
            other.health = 0;
        }
        else
        {
            //refill current board's tiles
            current.DropTiles();
            current.FillGrid(false);
            yield return new WaitForSeconds(0.5f);

            //switch turn and set other board to be active
            timer = maxTime;
            updateTimer = true;
            turn = !turn;
            other.SetBoardState(true);
            other.ResetWord();
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
