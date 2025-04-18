using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
using UnityEngine.Windows;

public enum InputButtonState { ButtonUp, ButtonNotHeld, ButtonDown, ButtonHeld };   //Currently, only ButtonDown and ButtonUp are used.

public class InputManager : MonoBehaviour
{
    const int PLAYER_COUNT = 4; //Doesn't actually set the max number of players. For that, the PlayerInputManager component on the InputManager gameobject must be modified.

    private int playerIndex;

    [SerializeField] private bool logInputs;
    [SerializeField] private int overridePlayerNumber = -1;
    private Vector2 lastMovement;

    //[SerializeField] public Vector2 Movement;
    [SerializeField] public Vector2 Movement;
    [SerializeField] public InputButtonState Select;
    [SerializeField] public InputButtonState Back;
    [SerializeField] public InputButtonState Enter;
    [SerializeField] public InputButtonState Scramble;

    [HideInInspector] public static Action<int, Vector2> GetMovementEvent;
    [HideInInspector] public static Action<int, bool> GetSelectEvent;
    [HideInInspector] public static Action<int, bool> GetBackEvent;
    [HideInInspector] public static Action<int, bool> GetEnterEvent;
    [HideInInspector] public static Action<int, bool> GetScrambleEvent;


    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {

    }

    //Player Input Functions
    public void SelectInput(PlayerInput player)
    {
        UpdatePlayerIndex(player);
        Select = UpdateButtonState(Select, GetSelectEvent, player.actions.FindAction("Select"));
    }
    public void BackInput(PlayerInput player)
    {
        UpdatePlayerIndex(player);
        Back = UpdateButtonState(Back, GetBackEvent, player.actions.FindAction("Back"));
    }
    public void EnterInput(PlayerInput player)
    {
        UpdatePlayerIndex(player);
        Enter = UpdateButtonState(Enter, GetEnterEvent, player.actions.FindAction("Enter"));
    }
    public void ScrambleInput(PlayerInput player)
    {
        UpdatePlayerIndex(player);
        Scramble = UpdateButtonState(Scramble, GetScrambleEvent, player.actions.FindAction("Scramble"));
    }
    public void MovementInput(PlayerInput player)
    {
        UpdatePlayerIndex(player);
        Movement = player.actions.FindAction("Movement").ReadValue<Vector2>();
        if (Movement != lastMovement)
        {
            lastMovement = Movement;
            if (logInputs)
                Debug.Log("Player " + (playerIndex + 1) + " " + Movement.ToString());
            GetMovementEvent?.Invoke(playerIndex, Movement);
        }
    }

    private InputButtonState UpdateButtonState(InputButtonState buttonState, Action<int, bool> buttonEvent, InputAction input)
    {
        if (Mathf.Approximately(input.ReadValue<float>(), 1f))
        {
            if (!buttonState.Equals(InputButtonState.ButtonDown))
            {
                buttonState = InputButtonState.ButtonDown;
                buttonEvent?.Invoke(playerIndex, true);
                if (logInputs)
                    Debug.Log("Player " + (playerIndex + 1) + " " + input.name + " " + buttonState.ToString());
            }
        }
        else
        {
            if(!buttonState.Equals(InputButtonState.ButtonUp))
            {
                buttonState = InputButtonState.ButtonUp;
                buttonEvent?.Invoke(playerIndex, false);
                if (logInputs)
                    Debug.Log("Player " + (playerIndex + 1) + " " + input.name + " " + buttonState.ToString());
            }
        }
        return buttonState;
    }

    public void UpdatePlayerIndex(PlayerInput player)
    {
        if (overridePlayerNumber >= 0 && overridePlayerNumber < PLAYER_COUNT)
            playerIndex = overridePlayerNumber;
        else
            playerIndex = player.playerIndex;
    }
}
