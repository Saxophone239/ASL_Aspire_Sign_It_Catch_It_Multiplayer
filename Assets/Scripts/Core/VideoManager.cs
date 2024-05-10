using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// This class simply takes in the vocab set passed in through the main menu and generates a dictionary connecting the vocab word to the video URL found in the StreamingAssets folder
/// </summary>
public class VideoManager : MonoBehaviour
{
    public static Dictionary<string, string> VocabWordToPathDict = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void GenerateVocabListFromSelectedVocabSet()
    {
        string path = Application.streamingAssetsPath + "/" + Globals.vocabList.ToString();
        GenerateVocabListFromVideos(path);
        Debug.Log($"Done processing vids; length of set = {VocabWordToPathDict.Count}");
    }

    private static void GenerateVocabListFromVideos(string folderPath)
    {
        DirectoryInfo directory = new DirectoryInfo(folderPath);
        FileInfo[] files = directory.GetFiles("*.mp4");
        foreach (FileInfo file in files)
        {
            string processedName = file.Name.Trim().Replace(".mp4","").Replace("_"," ");
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            textInfo.ToTitleCase(processedName);

            VocabWordToPathDict[processedName] = file.FullName;
        }
    }
}
