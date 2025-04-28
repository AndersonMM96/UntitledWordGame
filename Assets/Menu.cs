using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Menu : MonoBehaviour
{
    //Variables
    private List<string> titleText;
    private List<string> holidayText;

    //Prefabs
    public Game gamePrefab;

    //Child objects
    private GameObject titleBar;
    private TileBar titleBarTop;
    private TileBar titleBarBottomLeft;
    private TileBar titleBarBottomRight;
    private TextBar startText;
    private TextBar quitText;

    private void Awake()
    {
        //Get child objects
        titleBar = transform.GetChild(1).gameObject;
        titleBarTop = titleBar.transform.GetChild(0).GetComponent<TileBar>();
        titleBarBottomLeft = titleBar.transform.GetChild(1).GetComponent<TileBar>();
        titleBarBottomRight = titleBar.transform.GetChild(2).GetComponent<TileBar>();
        startText = transform.GetChild(2).GetChild(0).GetComponent<TextBar>();
        quitText = transform.GetChild(2).GetChild(1).GetComponent<TextBar>();

        startText.SetText("Start");
        quitText.SetText("Quit");

        titleText = File.ReadLines(@"Assets/title.txt").Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()).ToList();

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Instantiate(gamePrefab, transform);
            titleBar.SetActive(false);
        }
        //Application.Quit();
    }
}
