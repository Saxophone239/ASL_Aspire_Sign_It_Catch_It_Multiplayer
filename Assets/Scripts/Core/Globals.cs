using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Globals : MonoBehaviour
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum Vocab
    {
        Chemistry,
        Biology,
        FoodWeb,
        PartsOfTheCell
    }
    public static TextAsset jsonFile; //might use later for webgl stuff
    public static string vocabSet = "PartsOfTheCell"; //to be deprecated
    public static float fallingSpeed = 0.2f; //to be deprecated
    public static float spawnRate = 0.9f; //to be deprecated
    public static int incrementValue = 100; //to be deprecated
    // public static List<PlayerLeaderboardEntry> leaderboardEntries = new List<PlayerLeaderboardEntry>(); //to use later
    public static bool tutorial = false;
    public static Difficulty difficulty = Difficulty.Medium;
    public static Vocab vocabList = Vocab.Chemistry;
    public static bool isPlayButtonClicked;
}
