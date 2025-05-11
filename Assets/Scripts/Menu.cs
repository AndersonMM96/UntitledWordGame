using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Menu : MonoBehaviour
{
    private enum MenuState { Main, PlayerSelect, Game, Quit };

    public static Menu instance;

    //Variables
    private bool enableInput;
    private MenuState menuState;
    [SerializeField] private int menuSelection = 0;
    private List<string> titleText;
    private List<string> holidayText;
    private Game game;

    //Child objects
    private Transform titleTilesTransform;
    private TilePile[] titleTiles;
    private Transform titleOptionsTransform;
    private TextPile[] titleOptions;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        InputManager.Move += MoveSelector;
        InputManager.Select += Select;
        InputManager.Back += Back;
        InputManager.Enter += Enter;
        //InputManager.Scramble += Scramble;
    }

    private void Start()
    {
        game = null;

        //Main Menu TilePiles
        titleTilesTransform = new GameObject().transform;
        titleTilesTransform.parent = transform;
        titleTilesTransform.localPosition = Vector3.zero;
        titleTilesTransform.name = "Tiles";
        titleTiles = new TilePile[3];
        for (int i = 0; i < titleTiles.Length; i++)
        {
            titleTiles[i] = TilePile.Create(titleTilesTransform);
        }
        titleTiles[0].name = "UNTITLED";
        titleTiles[1].name = "WORD";
        titleTiles[2].name = "GAME";
        titleTiles[0].transform.localPosition = new Vector2(0f, 3.5f);
        titleTiles[1].transform.localPosition = new Vector2(-2.25f, 2.5f);
        titleTiles[2].transform.localPosition = new Vector2(2.25f, 2.5f);

        //Main Menu Options
        titleOptionsTransform = new GameObject().transform;
        titleOptionsTransform.parent = transform;
        titleOptionsTransform.localPosition = Vector3.zero;
        titleOptionsTransform.name = "Options";
        titleOptions = new TextPile[3];
        for (int i = 0; i < titleOptions.Length; i++)
        {
            titleOptions[i] = TextPile.Create(titleOptionsTransform);
            titleOptions[i].transform.localPosition = new Vector2(0f, -1.5f * i);
            titleOptions[i].AddColor(Color.white);
            titleOptions[i].AddColor(Color.black);
        }
        titleOptions[0].name = "Start";
        titleOptions[1].name = "Options";
        titleOptions[2].name = "Quit";

        //Main Menu Text
        titleText = System.IO.File.ReadLines(@"Assets/Resources/Text/title.txt").Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()).ToList();

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
            holidayText = new List<string>() { "HAUNTING", "GHOULISH", "SPOOKYSCARY" }; //Halloween
        else if (DateTime.Today == thanksgivingDay)
            holidayText = new List<string>() { "THANKFUL", "STUFFED", "CORNUCOPIAN" }; //Thanksgiving
        else if (DateTime.Today == new DateTime(DateTime.Today.Year, 12, 25))
            holidayText = new List<string>() { "FESTIVE", "JOVIAL", "JOLLY" }; //Christmas Day
        if (holidayText != null && holidayText.Count > 0)
            Debug.Log("Holiday mode activated...!");

        //Debug for testing that the game works on lower framerates
        //Application.targetFrameRate = 10;

        StartCoroutine(TitleScreen());
        enableInput = true;
    }

    private IEnumerator TitleScreen()
    {
        menuState = MenuState.Main;

        titleOptions[0].SetText("Start");
        titleOptions[1].SetText("Options");
        titleOptions[2].SetText("Quit");
        menuSelection = 0;
        UpdateMenuSelection();

        titleTiles[0].SetWord("UNTITLED");
        titleTiles[1].SetWord("WORD");
        titleTiles[2].SetWord("GAME");
        titleTiles[0].SetAnimate(true);
        titleTiles[1].SetAnimate(true);
        titleTiles[2].SetAnimate(true);
        for (int i = 0; i < titleTiles[0].length; i++)
            titleTiles[0].Get(i).GetComponent<SpriteAnimator>().SetPosition((Vector2)titleTiles[0].Get(i).transform.localPosition + new Vector2(0f, 5f + (i * 0.5f)), false);
        for (int i = 0; i < titleTiles[1].length; i++)
            titleTiles[1].Get(i).GetComponent<SpriteAnimator>().SetPosition((Vector2)titleTiles[1].Get(i).transform.localPosition + new Vector2(0f, 5f + (i * 0.5f)), false);
        for (int i = 0; i < titleTiles[2].length; i++)
            titleTiles[2].Get(i).GetComponent<SpriteAnimator>().SetPosition((Vector2)titleTiles[2].Get(i).transform.localPosition + new Vector2(0f, 5f + (i * 0.5f)), false);

        yield return new WaitForSeconds(5f);
        while (true)
        {
            yield return new WaitForSeconds(7f);

            string oldWord = titleTiles[0].GetWord();
            titleTiles[0].SetShake(true, 0.05f, 0.025f);
            for (int i = 0; i < titleTiles[0].length; i++)
            {
                titleTiles[0].Get(i).GetComponent<Tile>().DestroyTile();
                yield return new WaitForSeconds(Game.scoreSpeed);
            }
            titleTiles[0].Clear();
            titleTiles[0].SetShake(false, 0.04f, 0.05f);

            string newWord;
            do
            {
                if (holidayText != null && holidayText.Count > 0 && UnityEngine.Random.Range(0, 3) == 0)
                    newWord = holidayText[UnityEngine.Random.Range(0, holidayText.Count)];
                else
                    newWord = titleText[UnityEngine.Random.Range(0, titleText.Count)];
            } while (newWord.Equals(oldWord));
            titleTiles[0].SetWord(newWord);
            titleTiles[0].SetAnimate(true);
            for (int i = 0; i < titleTiles[0].length; i++)
                titleTiles[0].Get(i).GetComponent<SpriteAnimator>().SetPosition((Vector2)titleTiles[0].Get(i).transform.localPosition + new Vector2(0f, 5f + (i * 0.5f)), false);
        }
    }

    private IEnumerator QuitGame()
    {
        yield return null;
        Debug.LogWarning("Quitting game...");
        Application.Quit();
    }

    private IEnumerator StartGame()
    {
        game = new GameObject().AddComponent<Game>();
        game.transform.parent = transform;
        game.transform.localPosition = Vector3.zero;
        game.name = "Game";

        //Set gamerules here
        //game.maxHealth = 50;
        //game.timeLimit = 20;
        if (game.timeLimit > 0)
            game.timerEnabled = true;
        else
            game.timerEnabled = false;

        game.Initialize(new int[] { 0, 1 });
        yield return new WaitForSeconds(1f);
        game.StartGame();
    }

    private IEnumerator ChangeMenu(MenuState next)
    {
        enableInput = false;

        //Leaving Menu Code
        if(menuState.Equals(MenuState.Main))
        {
            //Destroy tiles
            foreach (TilePile pile in titleTiles)
                pile.DestroyTile();

            //Hide unselected options
            for (int i = 0; i < titleOptions.Length; i++)
            {
                if (menuSelection == i)
                {
                    titleOptions[i].SetBob(true);
                }
                else
                {
                    titleOptions[i].SetText("");
                }
            }
            yield return new WaitForSeconds(1f);
            titleOptions[menuSelection].SetText("");
            titleOptions[menuSelection].SetBob(false);
        }
        else if (menuState.Equals(MenuState.Game))
        {
            yield return StartCoroutine(game.EndGame());
        }

            menuState = next;

        //Load next menu
        switch (menuState)
        {
            case MenuState.Main:
                StartCoroutine(TitleScreen());
                break;
            case MenuState.PlayerSelect:
                break;
            case MenuState.Game:
                StartCoroutine(StartGame());
                break;
            case MenuState.Quit:
                StartCoroutine(QuitGame());
                break;
            default:
                break;
        }
        enableInput = true;
        yield return null;
    }

    private void UpdateMenuSelection()
    {
        if (menuState.Equals(MenuState.Main))
        {
            for (int i = 0; i < titleOptions.Length; i++)
            {
                if (menuSelection == i)
                {
                    titleOptions[i].SetColor(0);
                }
                else
                {
                    titleOptions[i].SetColor(1);
                }
            }
        }
    }
    private void MoveSelector(int playernum, Vector2Int direction)
    {
        if(enableInput)
        {
            if (menuState.Equals(MenuState.Main))
            {
                if (direction.y > 0f)
                    menuSelection--;
                else if (direction.y < 0f)
                    menuSelection++;

                if (menuSelection < 0)
                    menuSelection = titleOptions.Length - 1;
                if (menuSelection >= titleOptions.Length)
                    menuSelection = 0;

                UpdateMenuSelection();
            }
        }
    }

    private void Select(int playernum)
    {
        if (enableInput)
        {
            if (menuState.Equals(MenuState.Main))
            {
                switch (menuSelection)
                {
                    case 0:
                        StopAllCoroutines();
                        StartCoroutine(ChangeMenu(MenuState.Game));
                        break;
                    case 1:
                        Debug.Log("Settings");
                        break;
                    case 2:
                        StopAllCoroutines();
                        StartCoroutine(ChangeMenu(MenuState.Quit));
                        break;
                    default:
                        break;
                }
            }
            else if (menuState.Equals(MenuState.Game))
            {
                if (game != null && game.gameEnded)
                {
                    StopAllCoroutines();
                    StartCoroutine(ChangeMenu(MenuState.Main));
                }
            }
        }
    }
    private void Back(int playernum)
    {
        if (enableInput)
        {
            if (menuState.Equals(MenuState.Main))
            {
                if (menuSelection == titleOptions.Length - 1)
                {
                    StopAllCoroutines();
                    StartCoroutine(ChangeMenu(MenuState.Quit));
                }
                menuSelection = titleOptions.Length - 1;
                UpdateMenuSelection();
            }
        }
    }

    private void Enter(int playernum)
    {
        Select(playernum);
    }

    /*

    private IEnumerator LeaveTitle(MenuState next)
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
    */



}
