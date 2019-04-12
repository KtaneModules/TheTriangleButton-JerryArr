using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using KModkit;


public class TriangleButton : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    public MeshRenderer dispDigit;
    public MeshRenderer dispLabel;
    public MeshRenderer buttonColorZone;
    public MeshRenderer bgColor;
    public MeshRenderer colorblindLabel;

    public MeshRenderer fullButton;
        
    public KMSelectable button;

    private bool colorblindModeEnabled;
    public KMColorblindMode colorblindMode;

    //public KMRuleSeedable RuleSeedable;
    string[] words = new string[75]
    { "RED", "GREEN", "PURPLE", "BROWN", "ORANGE", "BLUE", "GREY", "PINK", "WHITE", "UP",
      "DOWN", "LEFT", "RIGHT", "UPWARD", "DOWNWARD", "LEFTWARD", "LEFTY", "RIGHTY", "BLANK", "MAUVE",
      "BLACK", "CYAN", "BOOM", "KABOOM", "BAM", "BLAM", "BOMB", "BOM", "HOLD", "PRESS",
      "TAP", "DETONATE", "RELEASE", "POINTING", "TRIANGLE", "SQUARE", "HEXAGON", "BUTTON", "CIRCLE", "REGULAR",
      "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN",
      "ZERO", "MINUS", "PLUS", "MODULO", "DIGITAL", "LABEL", "CAPTION", "TEXT", "ABORT", "SUBMIT",
      "YES", "NO", "SUBMIT", "ENTER", "CLEAR", "RUN", "WALK", "DUCK", "QUACK", "DIGIT",
      "DISPLAY", "PUNCH", "SLAP", "DEFUSE", "EXPERT"
    };

    string[] colorNames = new string[9]
    {
        "Red", "Green", "Purple",
        "Brown", "Orange", "Blue",
        "Grey", "Pink", "White"
    };
    
    Color[] colory = {
        new Color(0.85f, 0.15f, 0.15f), new Color(0.15f, 0.85f, 0.15f), new Color(0.35f, 0.0f, 0.45f),
        new Color(0.4f, 0.25f, 0.1f), new Color(0.9f, 0.45f, 0.1f), new Color(0.15f, 0.15f, 0.75f),
        new Color(0.55f, 0.55f, 0.55f), new Color(0.9f, 0.65f, 0.65f), new Color(0.95f, 0.95f, 0.95f)};

    int[,] actions = new int[,]
    {
        {1, 2, 0 },
        {2, 0, 1 },
        {0, 1, 2 }
    };

    string[] actionNames = new string[3]
    {
        "tap", "hold", "release"
    };

    string[] rotationNames = new string[8]
    {
        "up", "up-right", "right", "down-right", "down", "down-left", "left", "up-left"
    };

    string[] alphabet = new string[26]
    { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
      "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};


    int[] values = new int[26] 
    { 1, 5, 5, 5, 1, 5, 5, 6, 1, 3, 6, 4, 6,
      4, 1, 6, 3, 4, 4, 4, 1, 6, 2, 3, 2, 3 };


    bool pressedAllowed = false;

    // TWITCH PLAYS SUPPORT
    //int tpStages; This one is not needed for this module
    int holdWait = -1;
    int releaseWait = -1;
    // TWITCH PLAYS SUPPORT

    int labelNumber;
    int shownDigit;

    int rotations;
    int colorNumber;

    int neededNumber;
    int actionNeeded;

    //int holds = 0;

    string heldString;
    string releasedString;

    double rLevel;
    double gLevel;
    double bLevel;
    bool rUp;
    bool gUp;
    bool bUp;
    bool altColorFalls;
    bool foundGreen;

    int heldDigit;
    double heldTime;
    int releasedDigit;
    double releasedTime;

    bool isHeld = false;
    bool isSolved = false;
    bool tpActive = false;
    
    void Start()
    {
        _moduleId = _moduleIdCounter++;
        colorblindModeEnabled = colorblindMode.ColorblindModeActive;
        Init();
        pressedAllowed = true;
    }

    void Init()
    {
        delegationZone();
        //Module.OnActivate += delegate { inputResult.GetComponentInChildren<TextMesh>().text = ""; };

        button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y, 2f);
        rLevel = 0.8f;
        gLevel = 0.8f;
        bLevel = 1.0f;
        rUp = true;
        gUp = false;
        bUp = false;
        altColorFalls = false;
        foundGreen = false;
        shownDigit = UnityEngine.Random.Range(1, 9);
        labelNumber = UnityEngine.Random.Range(0, words.Length);
        rotations = UnityEngine.Random.Range(0, 8);
        colorNumber = UnityEngine.Random.Range(0, 9);
        //colorNumber = 6; //debug

        buttonColorZone.material.color = colory[colorNumber];
        if (colorNumber == 0 || colorNumber == 2 || colorNumber == 3 || colorNumber == 5)
        {
            dispDigit.GetComponentInChildren<TextMesh>().color = colory[8];
            dispLabel.GetComponentInChildren<TextMesh>().color = colory[8];
        }
        fullButton.transform.Rotate(Vector3.forward, 45 * rotations);
        //Debug.Log(actions[1, 1] + " then " + actions[0, 2]);
        dispDigit.GetComponentInChildren<TextMesh>().text = shownDigit.ToString();
        dispLabel.GetComponentInChildren<TextMesh>().text = words[labelNumber];

        Debug.LogFormat("[The Triangle Button #{0}] The button is pointing {1} and is colored {2}. Its label is '{3}' and the digit is {4}.", _moduleId, rotationNames[rotations], colorNames[colorNumber],
            words[labelNumber], shownDigit);
        if (colorblindModeEnabled)
        {
            colorblindLabel.GetComponentInChildren<TextMesh>().text = colorNames[colorNumber];
        }
        figureAnswer();
        pressedAllowed = true;
    }


    void OnHold()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
        isHeld = true;
        button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y, -.1f);
        //button.transform.Translate(0f, 0f, -.0073f);
        //holds++;
        if (pressedAllowed)
        {
            heldString = Bomb.GetFormattedTime();
            heldTime = Math.Floor(Bomb.GetTime());
            heldDigit = (int)Math.Floor(Bomb.GetTime()) % 10;
            Debug.LogFormat("[The Triangle Button #{0}] Button down at {1}, final digit is {2}.", _moduleId, Bomb.GetFormattedTime(), heldDigit);
        }
    }

    void OnRelease()
    {
        isHeld = false;
        button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y, 2f);
            /*
        for (int hN = 0; hN < holds; hN++)
        {
            button.transform.Translate(0f, 0f, .0073f);
        }
        holds = 0; Nothing doing until I figure translation out for good */
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
        if (pressedAllowed)
        {
            releasedString = Bomb.GetFormattedTime();
            releasedTime = Math.Floor(Bomb.GetTime());
            releasedDigit = (int)Math.Floor(Bomb.GetTime()) % 10;
            Debug.LogFormat("[The Triangle Button #{0}] Button up at {1}, final digit is {2}.", _moduleId, Bomb.GetFormattedTime(), releasedDigit);
            switch (actionNeeded)
            {
                case 0:
                    if (heldTime != releasedTime)
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You held the button (from {1} to {2}) when you should have tapped on a countdown time ending in {3}. Strike!", 
                            _moduleId, heldString, releasedString, neededNumber);
                        Module.HandleStrike();
                    }
                    else if (heldDigit != neededNumber)
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You correctly chose to tap the button, but you did it at {1} when you should have tapped on a countdown time ending in {2}. Strike!", 
                            _moduleId, heldString, neededNumber);
                        Module.HandleStrike();
                    }
                    else
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You correctly chose to tap the button, and correctly did so at {1}! Module defused!", _moduleId, heldString);
                        Module.HandlePass();
                        pressedAllowed = false;
                        isSolved = true;
                    }
                    break;
                case 1:
                    if (heldTime == releasedTime)
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You tapped the button at {1}, but needed to hold it from a time ending in {2} to a time ending in 0! Strike!", _moduleId, heldString, neededNumber);
                        Module.HandleStrike();
                    }
                    else if (heldDigit != neededNumber)
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You correctly chose to hold the button, but did so at {1} instead of on a time ending in {2}! Strike!", _moduleId, heldString, neededNumber);
                        Module.HandleStrike();
                    }
                    else if (releasedDigit != 0)
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You correctly chose to hold the button, and correctly did so at {1}, but you released at {2} instead of a time ending in 0! Strike!", 
                            _moduleId, heldString, releasedString);
                        Module.HandleStrike();
                    }
                    else
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You correctly chose to hold the button, and correctly did so at {1}, and correctly released it at {2}! Module defused!", 
                            _moduleId, heldString, releasedString);
                        Module.HandlePass();
                        pressedAllowed = false;
                        isSolved = true;
                    }
                    break;
                case 2:
                    if (heldTime == releasedTime)
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You tapped the button at {1}, but needed to hold it from a time ending in 0 to a time ending in {2}! Strike!", _moduleId, heldString, neededNumber);
                        Module.HandleStrike();
                    }
                    else if (releasedDigit != neededNumber)
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You correctly chose to hold the button, but released it at {1} instead of on a time ending in {2}! Strike!", _moduleId, releasedString, neededNumber);
                        Module.HandleStrike();
                    }
                    else if (heldDigit != 0)
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You correctly chose to hold the button, and correctly released it at {1}, but you starting holding it at {2} instead of a time ending in 0! Strike!",
                            _moduleId, releasedString, heldString);
                        Module.HandleStrike();
                    }
                    else
                    {
                        Debug.LogFormat("[The Triangle Button #{0}] You correctly chose to hold the button, and correctly did so at {1}, and correctly released it at {2}! Module defused!",
                            _moduleId, heldString, releasedString);
                        Module.HandlePass();
                        pressedAllowed = false;
                        isSolved = true;
                    }
                    break;
                default: //uh oh
                    break;
            }
            return;
        }

    }

    void figureAnswer()
    {
        var letterTally = shownDigit;
        var directionsMessage = "The digit is " + shownDigit + ". ";
        for (int cN = 0; cN < words[labelNumber].Length; cN++)
        {
            for (int aP = 0; aP < 26; aP++)
            {
                if (alphabet[aP] == words[labelNumber].Substring(cN, 1))
                {
                    letterTally+= values[aP];
                    directionsMessage = directionsMessage + "Add " + alphabet[aP] + "'s value of " + values[aP] + " to get " + letterTally + ". ";
                    aP = 26;
                }
            }
            
        }
        neededNumber = (letterTally % 9) + 1;
        directionsMessage = directionsMessage + "This modulo 9 is " + (letterTally % 9) + ", and add one to get " + neededNumber + ".";
        Debug.LogFormat("[The Triangle Button #{0}] {1}", _moduleId, directionsMessage);
        int rowNum = colorNumber / 3;
        int colNum = colorNumber % 3;
        if (rotations == 7 || rotations == 0 || rotations == 1)
        {
            rowNum--;
            if (rowNum == -1)
            {
                rowNum = 2;
            }
        }
        else if (rotations >= 3 && rotations <= 5)
        {
            rowNum++;
            if (rowNum == 3)
            {
                rowNum = 0;
            }

        }
        if (rotations >= 5 && rotations <= 7)
        {
            colNum--;
            if (colNum == -1)
            {
                colNum = 2;
            }
        }
        else if (rotations >= 1 && rotations <= 3)
        {
            colNum++;
            if (colNum == 3)
            {
                colNum = 0;
            }

        }
        actionNeeded = actions[rowNum, colNum];
        switch (actionNeeded)
        {
            case 0:
                directionsMessage = "You need to tap on " + neededNumber + ".";
                break;
            case 1:
                directionsMessage = "You need to hold on " + neededNumber + " and release on 0.";
                break;
            case 2:
                directionsMessage = "You need to hold on 0 and release on " + neededNumber;
                break;
            default: //uh oh
                break;
        }
        Debug.LogFormat("[The Triangle Button #{0}] {1}", _moduleId, directionsMessage);
    }

    private void FixedUpdate()
    {
        if (isSolved)
        {
            colorCycle();
        }
        else if (holdWait == -1 && releaseWait == -1 && tpActive)
        {

        }
        else if (isHeld)
        {
            colorCycle();
            if (Math.Floor(Bomb.GetTime()) % 10 == releaseWait)
            {
                button.OnInteractEnded();
                holdWait = -1;
                releaseWait = -1;
            }
        }
        else
        {
            if (Math.Floor(Bomb.GetTime()) % 10 == holdWait)
            {
                button.OnInteract();
                holdWait = -1;
            }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Tap the button when integer number of seconds ends in (1-9) with !{0} (tap/t) (1-9). Hold when the number of seconds ends in (1-9) and release on 0 with !{0} (hold/h) (1-9). Hold when the number of seconds ends in 0 and release on (1-9) with !{0} (release/r) (1-9).";
    private readonly bool TwitchShouldCancelCommand = false;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        tpActive = true;
        var pieces = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string theError;
        theError = "";
        yield return null;
        if (pieces.Count() == 0)
        {
            theError = "sendtochaterror Not enough arguments! You need to use 'tap/t/hold/h/release/r', then (1-9).";
            yield return theError;
        }
        else if (pieces.Count() >= 1 && pieces[0] == "colorblind")
        {
            colorblindModeEnabled = true;
            colorblindLabel.GetComponentInChildren<TextMesh>().text = colorNames[colorNumber];
            yield return null;
        }
        else if (pieces.Count() == 1 && (pieces[0] == "tap" || pieces[0] == "t" || pieces[0] == "hold" || pieces[0] == "h" || pieces[0] == "release" || pieces[0] == "r"))
        {
            theError = "sendtochaterror Not enough arguments! You need to specify a digit the number of seconds remaining ends in, using !{0} tap/t/hold/h/release/r (1-9).";
            yield return theError;

        }
        else if (pieces.Count() == 1 && !(pieces[0] == "tap" || pieces[0] == "t" || pieces[0] == "hold" || pieces[0] == "h" || pieces[0] == "release" || pieces[0] == "r"))
        {
            theError = "sendtochaterror Invalid argument: " + pieces[0] + "! You need to specify a digit the number of seconds remaining ends in, using !{0} tap/t/hold/h/release/r (1-9).";
            yield return theError;

        }
        else if (pieces.Count() > 1 && !(pieces[0] == "tap" || pieces[0] == "t" || pieces[0] == "hold" || pieces[0] == "h" || pieces[0] == "release" || pieces[0] == "r"))
        {
            theError = "sendtochaterror Invalid argument: " + pieces[0] + "! You need to specify a digit the number of seconds remaining ends in, using !{0} tap/t/hold/h/release/r (1-9).";
            yield return theError;

        }
        else if (pieces.Count() > 1 && !(pieces[1] == "1" || pieces[1] == "2" || pieces[1] == "3" ||
                                        pieces[1] == "6" || pieces[1] == "5" || pieces[1] == "6" ||
                                        pieces[1] == "7" || pieces[1] == "8" || pieces[1] == "9"))
        {
            theError = "sendtochaterror Invalid argument: " + pieces[1] + " is not a digit from 1 to 9! You need to specify a digit the number of seconds remaining ends in, using !{0} tap/t/hold/h/release/r (1-9).";
            yield return theError;
        }
        else if (pieces[0] == "tap" || pieces[0] == "t")
        {
            yield return null;
            holdWait = Int16.Parse(pieces[1]);
            releaseWait = Int16.Parse(pieces[1]);
            if (actionNeeded != 0 || holdWait != neededNumber)
            {
                yield return "strike";
            }
            else
            {
                yield return "solve";
            }
        }
        else if (pieces[0] == "hold" || pieces[0] == "h")
        {
            yield return null;
            holdWait = Int16.Parse(pieces[1]);
            releaseWait = 0;
            if (actionNeeded != 1 || holdWait != neededNumber)
            {
                yield return "strike";
            }
            else
            {
                yield return "solve";
            }
            //           yield return theError;
        }
        else if (pieces[0] == "release" || pieces[0] == "r")
        {
            yield return null;
            holdWait = 0;
            releaseWait = Int16.Parse(pieces[1]);
            if (actionNeeded != 2 || releaseWait != neededNumber)
            {
                yield return "strike";
            }
            else
            {
                yield return "solve";
            }
            //            yield return theError;
        }
     }



    void colorCycle()
    {
        if (!foundGreen)
        {
            if (rUp)
            {
                if (altColorFalls)
                {
                    bLevel = bLevel - .03f;
                    if (bLevel <= .4f)
                    {
                        bLevel = .4f;
                        altColorFalls = false;
                        rUp = false;
                        gUp = true;

                    }
                }
                else
                {
                    rLevel = rLevel + .03f;
                    if (rLevel >= 1f)
                    {
                        altColorFalls = true;
                        rLevel = 1f;
                    }
                }
            }
            else if (gUp)
            {
                if (altColorFalls)
                {
                    rLevel = rLevel - .03f;
                    if (rLevel <= .4f)
                    {
                        rLevel = .4f;
                        altColorFalls = false;
                        gUp = false;
                        bUp = true;
                        if (isSolved)
                        {
                            foundGreen = true;
                        }
                    }
                }
                else
                {
                    gLevel = gLevel + .03f;
                    if (gLevel >= 1f)
                    {
                        altColorFalls = true;
                        gLevel = 1f;
                    }
                }
            }
            else if (bUp)
            {
                if (altColorFalls)
                {
                    gLevel = gLevel - .03f;
                    if (gLevel <= .4f)
                    {
                        gLevel = .4f;
                        altColorFalls = false;
                        bUp = false;
                        rUp = true;

                    }
                }
                else
                {
                    bLevel = bLevel + .03f;
                    if (bLevel >= 1f)
                    {
                        altColorFalls = true;
                        bLevel = 1f;
                    }
                }

            }
            bgColor.material.color = new Color((float)rLevel, (float)gLevel, (float)bLevel);
        }

        
    }

    void delegationZone()
    {
        button.OnInteract += delegate () { OnHold(); return false; };
        button.OnInteractEnded += delegate () { OnRelease(); };

    }
    /*
    public static class Extensions
    {
        // Fisher-Yates Shuffle
        public static IList<T> shuffle<T>(this IList<T> list, MonoRandom rnd)
        {
            var i = list.Count;
            while (i > 1)
            {
                var index = rnd.Next(i);
                i--;
                var value = list[index];
                list[index] = list[i];
                list[i] = value;
            }

            return list;
        }
    } */
    /*
    void doShuffle()
    {
        var rnd = RuleSeedable.GetRNG();
        if (rnd.Seed == 1)
        {

        }
        else
        {
            
            var numberCount = theNumbers.Length;
            while (numberCount > 1)
            {
                var xyz = rnd.Next(numberCount);
                numberCount--;
                var value = theNumbers[xyz];
                theNumbers[xyz] = theNumbers[numberCount];
                theNumbers[numberCount] = value;
            }
            var theThingy = "";

            for (var i = 0; i < 42; i++)
            {
                //list[i].innerText = theFunctions[theNumbers[i]];
            }
        }
        
    } */

    

}
