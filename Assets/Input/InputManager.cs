using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    //Variables
    public bool logInputs = false;
    public List<bool> inputEnabled;
    public List<PlayerInput> players;
    private List<Vector2Int> heldDirection;
    private List<bool[]> directions;
    private List<float> moveCooldown;
    private static float moveInterval = 0.15f;

    //Events
    [HideInInspector] public static Action<int, Vector2Int> Move;
    [HideInInspector] public static Action<int> Select;
    [HideInInspector] public static Action<int> Back;
    [HideInInspector] public static Action<int> Enter;
    [HideInInspector] public static Action<int> Scramble;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        PlayerInputManager.GetMovementEvent += GetMovement;
        PlayerInputManager.GetSelectEvent += GetSelect;
        PlayerInputManager.GetBackEvent += GetBack;
        PlayerInputManager.GetEnterEvent += GetEnter;
        PlayerInputManager.GetScrambleEvent += GetScramble; 
        
        inputEnabled = new List<bool>();
        players = new List<PlayerInput>();
        heldDirection = new List<Vector2Int>();
        directions = new List<bool[]>();
        moveCooldown = new List<float>();
    }
    
    public void JoinPlayer(PlayerInput player)
    {
        if (!players.Contains(player))
        {
            string device = "";
            if (player.devices.Count > 0)
                device = player.devices[0].name;
            Debug.Log("Joining player " + player.playerIndex + " - " + device);
            player.name = "Player " + player.playerIndex + " - " + device;
            player.transform.SetParent(transform);
            players.Add(player);
            inputEnabled.Add(true);
            heldDirection.Add(Vector2Int.zero);
            directions.Add(new bool[4]);
            moveCooldown.Add(moveInterval);
        }
    }
    public void LeavePlayer(PlayerInput player)
    {

    }

    private void Update()
    {
        for(int i = 0; i < players.Count; i++)
        {
            if (!heldDirection[i].Equals(Vector2Int.zero))
            {
                moveCooldown[i] -= Time.deltaTime;
                if (moveCooldown[i] <= 0f && inputEnabled[i])
                {
                    moveCooldown[i] = moveInterval;
                    MoveSelector(i, heldDirection[i]);
                }
            }
            else
                moveCooldown[i] = moveInterval;
        }
    }

    private void GetMovement(int playernum, Vector2 v)
    {
        bool[] newDirections = new bool[] { false, false, false, false };
        if (v.y > 0)
            newDirections[0] = true;
        if (v.x > 0)
            newDirections[1] = true;
        if (v.y < 0)
            newDirections[2] = true;
        if (v.x < 0)
            newDirections[3] = true;

        if (newDirections[0] && !directions[playernum][0])
            MoveSelector(playernum, Vector2Int.up);
        if (newDirections[1] && !directions[playernum][1])
            MoveSelector(playernum, Vector2Int.right);
        if (newDirections[2] && !directions[playernum][2])
            MoveSelector(playernum, Vector2Int.down);
        if (newDirections[3] && !directions[playernum][3])
            MoveSelector(playernum, Vector2Int.left);

        directions[playernum] = newDirections;
        heldDirection[playernum] = new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        moveCooldown[playernum] = moveInterval;
    }
    private void MoveSelector(int playernum, Vector2Int direction)
    {
        if(logInputs)
            Debug.Log(players[playernum].name + " - " + direction);
        Move?.Invoke(playernum, direction);
    }
    private void GetSelect(int playernum, bool b)
    {
        if (b)
        {
            if (logInputs)
                Debug.Log(players[playernum].name + " - Select");
            Select?.Invoke(playernum);
        }
    }
    private void GetBack(int playernum, bool b)
    {
        if (b)
        {
            if (logInputs)
                Debug.Log(players[playernum].name + " - Back");
            Back?.Invoke(playernum);
        }
    }
    private void GetEnter(int playernum, bool b)
    {
        if (b)
        {
            if (logInputs)
                Debug.Log(players[playernum].name + " - Enter");
            Enter?.Invoke(playernum);
        }
    }
    private void GetScramble(int playernum, bool b)
    {
        if (b)
        {
            if(logInputs)
                Debug.Log(players[playernum].name + " - Scramble");
            Scramble?.Invoke(playernum);
        }
    }
}
