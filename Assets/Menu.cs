using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;


public class Menu : MonoBehaviour
{
    private enum MenuState { Main, PlayerSelect, Game, Quit };

    public static Menu instance;

    //Variables
    private MenuState menuState;
    private List<string> titleText;
    private List<string> holidayText;
    private Game game;
    public List<PlayerInput> players;
    //Movement variables
    private int menuSelection;
    private Vector2Int heldDirection;
    private bool[] directions;
    private float moveCooldown;
    private float moveInterval = 0.15f;

    //Prefabs
    public Game gamePrefab;

    //Child objects
    private GameObject titleBar;
    private TileBar titleBarTop;
    private TileBar titleBarBottomLeft;
    private TileBar titleBarBottomRight;
    private Transform options;
    private List<TextBar> optionsText;
    private Transform playersTransform;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        menuSelection = 0;
        directions = new bool[4];

        //Input Events
        InputManager.GetMovementEvent += GetMovement;
        InputManager.GetSelectEvent += Select;
        InputManager.GetBackEvent += Back;
        InputManager.GetEnterEvent += Enter;
        InputManager.GetScrambleEvent += Scramble;

        //Get child objects
        titleBar = transform.GetChild(1).gameObject;
        titleBarTop = titleBar.transform.GetChild(0).GetComponent<TileBar>();
        titleBarBottomLeft = titleBar.transform.GetChild(1).GetComponent<TileBar>();
        titleBarBottomRight = titleBar.transform.GetChild(2).GetComponent<TileBar>();
        options = transform.GetChild(2);
        optionsText = new List<TextBar>();
        for (int i = 0; i < options.childCount; i++)
            optionsText.Add(options.GetChild(i).GetComponent<TextBar>());
        playersTransform = transform.GetChild(3);

        optionsText[0].SetText("Start");
        optionsText[1].SetText("Settings");
        optionsText[2].SetText("Quit");

        titleText = System.IO.File.ReadLines(@"Assets/title.txt").Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()).ToList();

        //Holiday Words
        DateTime thanksgivingDay = new DateTime(DateTime.Today.Year, 11, 1);
        thanksgivingDay = thanksgivingDay.AddDays((((int)DayOfWeek.Thursday - (int)thanksgivingDay.DayOfWeek + 7) % 7) + 21);
        if (DateTime.Today == new DateTime(DateTime.Today.Year, 1, 1)) 
            holidayText = new List<string>() { "RENEWED", "REFLECTIVE", "CYCLICAL" }; //New Year's Day
        else if (DateTime.Today == new DateTime(DateTime.Today.Year, 2, 14))
            holidayText = new List<string>() { "LOVELY", "BEAUTIFUL", "PASSIONATE" }; //Valentine's Day
        else if (DateTime.Today == new DateTime(DateTime.Today.Year, 3, 17))
            holidayText = new List<string>() { "LUCKY", "VERDANT", "FORTUNATE" }; //St. Patrick's Day
        else if (DateTime.Today == new DateTime(DateTime.Today.Year, 4, 1))
            holidayText = new List<string>() { "FOOLISH", "MISCHIEVOUS", "COMEDIC" }; //April Fool's Day
        else if (DateTime.Today == new DateTime(DateTime.Today.Year, 9, 19))
            holidayText = new List<string>() { "SCURVY", "SEAWORTHY", "PIRATICAL" }; //Talk Like a Pirate Day
        else if (DateTime.Today == new DateTime(DateTime.Today.Year, 10, 31))
            holidayText = new List<string>() { "HAUNTING", "TERRIFYING", "SPOOKYSCARY" }; //Halloween
        else if (DateTime.Today == thanksgivingDay)
            holidayText = new List<string>() { "THANKFUL", "STUFFED", "CORNUCOPIAN" }; //Thanksgiving
        else if (DateTime.Today == new DateTime(DateTime.Today.Year, 12, 25))
            holidayText = new List<string>() { "FESTIVE", "JOVIAL", "JOLLY" }; //Christmas Day
        if (holidayText != null && holidayText.Count > 0)
            Debug.Log("Holiday mode activated...!");

        //Debug for testing that the game works on lower framerates
        //Application.targetFrameRate = 10;

        StartCoroutine(RunTitle());
    }

    private IEnumerator RunTitle()
    {
        menuState = MenuState.Main;
        UpdateOptions();
        titleBarBottomLeft.SetText("WORD");
        titleBarBottomRight.SetText("GAME");
        titleBarTop.SetText("UNTITLED");
        yield return new WaitForSeconds(5f);
        while (titleBar.activeSelf)
        {
            yield return new WaitForSeconds(7f);
            titleBarTop.shake = true;
            for(int i = 0; i < titleBarTop.tiles.Count; i++)
            {
                titleBarTop.DestroyTile(0);
                yield return new WaitForSeconds(0.25f);
            }
            titleBarTop.tiles.Clear();
            titleBarTop.shake = false;
            titleBarTop.invalid = false;
            string newWord;
            do
            {
                if(holidayText != null && holidayText.Count > 0 && UnityEngine.Random.Range(0, 3) == 0)
                    newWord = holidayText[UnityEngine.Random.Range(0, holidayText.Count)];
                else
                    newWord = titleText[UnityEngine.Random.Range(0, titleText.Count)];
            } while (newWord.Equals(titleBarTop.text));
            if (!Dictionary.instance.ContainsWord(newWord))
                titleBarTop.invalid = true;
            titleBarTop.SetText(newWord);
        }
    }

    private IEnumerator LeaveTitle()
    {
        //break main menu animation
        //hide all options except for selected one, and make it bob like crazy
        Vector3 oldPosition = Vector3.zero;
        for (int i = 0; i < optionsText.Count; i++)
        {
            if (menuSelection == i)
            {

                optionsText[i].bob = true;
                optionsText[i].bobIntensity = 0.5f;
                optionsText[i].bobSpeed = 2f;
                oldPosition = optionsText[i].transform.position;
                optionsText[i].OffsetLetters(transform.position - oldPosition, false);
            }
            else
            {
                optionsText[i].gameObject.SetActive(false);
            }
        }
        //break all tiles
        for (int i = 0; i < titleBarTop.tiles.Count; i++)
            titleBarTop.DestroyTile(i);
        for (int i = 0; i < titleBarBottomLeft.tiles.Count; i++)
            titleBarBottomLeft.DestroyTile(i);
        for (int i = 0; i < titleBarBottomRight.tiles.Count; i++)
            titleBarBottomRight.DestroyTile(i);
        titleBarTop.tiles.Clear();
        titleBarBottomLeft.tiles.Clear();
        titleBarBottomRight.tiles.Clear();
        yield return new WaitForSeconds(1.5f);
        //Reset
        optionsText[menuSelection].bobIntensity = 0.25f;
        optionsText[menuSelection].bobSpeed = 0.5f;
        optionsText[menuSelection].bob = false;
        optionsText[menuSelection].gameObject.SetActive(false);
        optionsText[menuSelection].OffsetLetters(oldPosition, false);
    }

    private IEnumerator MoveToPlayerSelect()
    {
        menuState = MenuState.PlayerSelect;
        
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator StartGame()
    {
        yield return StartCoroutine(LeaveTitle());
        game = Instantiate(gamePrefab, transform);
        StartCoroutine(game.BeginGame(new int[] { 0, 1 }));
        menuState = MenuState.Game;
        yield return new WaitForSeconds(1f);
    }
    private IEnumerator QuitGame()
    {
        yield return StartCoroutine(LeaveTitle());
        menuState = MenuState.Quit;
        Debug.LogWarning("Closing game...");
        Application.Quit();
    }

    private void Update()
    {
        //Main Menu
        if(menuState.Equals(MenuState.Main))
        {
            //Update movement
            moveCooldown -= Time.deltaTime;
            if (moveCooldown <= 0f)
            {
                moveCooldown = moveInterval;
                MoveSelector(heldDirection);
            }
        }
        else
        {
            titleBar.SetActive(false);
            options.gameObject.SetActive(false);
            menuSelection = 0;
        }

    }
    private void UpdateOptions()
    {
        //Update selection
        for (int i = 0; i < optionsText.Count; i++)
        {
            if (i == menuSelection)
            {
                optionsText[i].color = Color.white;
                optionsText[i].bob = true;
            }
            else
            {
                optionsText[i].color = Color.black;
                optionsText[i].bob = false;
            }
            optionsText[i].UpdateLetters();
        }
    }


    //Input functions
    public void JoinPlayer(PlayerInput player)
    {
        if(!players.Contains(player))
        {
            string device = "";
            if (player.devices.Count > 0)
                device = player.devices[0].name;
            Debug.Log("Joining player " + player.playerIndex + " - " + device);
            player.name = "Player " + player.playerIndex + " - " + device;
            player.transform.SetParent(playersTransform);
            players.Add(player);
        }
    }
    public void LeavePlayer(PlayerInput player)
    {
        
    }
    private void GetMovement(int playernum, Vector2 v)
    {
        if (menuState.Equals(MenuState.Main))
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
        else if (menuState.Equals(MenuState.Game))
        {
            //Gameplay
            game.boards[game.PlayerIndexToNumber(playernum)].GetMovement(playernum, v);
        }
    }
    private void MoveSelector(Vector2Int direction)
    {
        if(menuState.Equals(MenuState.Main))
        {
            //Main Menu
            if(!direction.Equals(Vector2Int.zero) && direction.y != 0) 
            {
                menuSelection -= direction.y;
                if (menuSelection < 0)
                    menuSelection = optionsText.Count - 1;
                if (menuSelection >= optionsText.Count)
                    menuSelection = 0;
                UpdateOptions();
            }
        }
    }
    private void Select(int playernum, bool b)
    {
        if(b)
        {
            if (menuState.Equals(MenuState.Main))
            {
                switch (menuSelection)
                {
                    case 0:
                        StartCoroutine(StartGame());
                        break;
                    case 1:
                        Debug.LogWarning("Settings");
                        break;
                    case 2:
                        StartCoroutine(QuitGame());
                        break;
                    default: break;
                }
                UpdateOptions();
            }
            else if (menuState.Equals(MenuState.Game))
            {
                //Gameplay
                game.boards[game.PlayerIndexToNumber(playernum)].SelectTile(playernum, b);
            }
        }
    }
    private void Back(int playernum, bool b)
    {
        if (b)
        {
            if (menuState.Equals(MenuState.Main))
            {
                if (menuSelection == optionsText.Count - 1)
                {
                    StartCoroutine(QuitGame());
                }
                menuSelection = optionsText.Count - 1;
                UpdateOptions();
            }
            else if (menuState.Equals(MenuState.Game))
            {
                //Gameplay
                game.boards[game.PlayerIndexToNumber(playernum)].DeselectTile(playernum, b);
            }
        }
    }
    private void Enter(int playernum, bool b)
    {
        if(b)
        {
            if (menuState.Equals(MenuState.Game))
            {
                //Gameplay
                game.boards[game.PlayerIndexToNumber(playernum)].SubmitWord(playernum, b);
            }
        }
    }
    private void Scramble(int playernum, bool b)
    {
        if (b)
        {
            if (menuState.Equals(MenuState.Game))
            {
                //Gameplay
                game.boards[game.PlayerIndexToNumber(playernum)].Scramble(playernum, b);
            }
        }
    }
}
