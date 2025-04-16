using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Board[] boards;
    bool turn = true;

    // Start is called before the first frame update
    void Start()
    {
        boards[0].game = this;
        boards[1].game = this;
        SwitchTurn(turn);
    }

    public void SendScore(double score)
    {
        if (turn)
            boards[1].AddHealth(-Mathf.RoundToInt(Mathf.Pow((float)score, 2)));
        else
            boards[0].AddHealth(-Mathf.RoundToInt(Mathf.Pow((float)score, 2)));
        turn = !turn;
        SwitchTurn(turn);
    }
    private void SwitchTurn(bool turn)
    {
        boards[0].SetBoardState(turn);
        boards[1].SetBoardState(!turn);
    }
}
