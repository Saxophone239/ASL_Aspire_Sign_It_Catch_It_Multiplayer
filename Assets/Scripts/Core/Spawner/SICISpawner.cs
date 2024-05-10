using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SICISpawner : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private FallingWord word;
    [SerializeField] private GameObject[] powerUps;

    [Header("World Bounds and Parameters")]
    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;
    [SerializeField] private LayerMask layerMask;

    private int wordValue = 10;
    private Collider2D[] wordBuffer = new Collider2D[1];
    private float wordRadius;

    public float fallingSpeed = 0.2f;
    public float spawnRate = 1.0f;

    // Videoplayer
    [SerializeField] private VideoPlayer videoPlayer;
    private RawImage rawImage;

    private List<string> currentWordsToSpawn = new List<string>();
    private int currentWordsToSpawnSize = 6;

    // Specific correct word/link chosen at period
    //public string CorrectWord { get; private set; } = "";
    public NetworkVariable<FixedString32Bytes> CorrectWord = new NetworkVariable<FixedString32Bytes>();
    private bool isSpawnerActive;

    //private List<string> vidVocabList;

    Coroutine spawnerCoroutine;

    public override void OnNetworkSpawn()
    {
        // TODO: Delete below code when successfully implemented
        VideoManager.GenerateVocabListFromSelectedVocabSet();

        wordRadius = word.GetComponent<CircleCollider2D>().radius;

        BasketPlayer.OnPlayerSpawned += HandlePlayerSpawned;

        if (IsClient)
        {
            CorrectWord.OnValueChanged += HandleCorrectWordChanged;
            

            // Make videoplayer transparent
            rawImage = videoPlayer.gameObject.GetComponent<RawImage>();
            //ChangeVideoplayerVisibility(10);
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            StartSpawningWords();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            CorrectWord.OnValueChanged -= HandleCorrectWordChanged;
            BasketPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        }
    }

    private void HandlePlayerSpawned(BasketPlayer player)
    {
        //HandleCorrectWordChanged(string.Empty, CorrectWord.Value);
        Debug.Log($"{player.PlayerName.Value} has spawned, showing videoboard");
        ChangeVideoplayerVisibility(150);
    }

    private void OnClientConnected(ulong clientId)
    {
        HandleCorrectWordChanged(string.Empty, CorrectWord.Value);
        Debug.Log($"new player has joined with clientId: {clientId}, play vid for them");
        StartVideoplayerForConnectedClientClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new List<ulong>()
                {
                    clientId
                }
            }
        });
    }

    [ClientRpc]
    private void StartVideoplayerForConnectedClientClientRpc(ClientRpcParams clientRpcParams = new ClientRpcParams())
    {
        // Debug.Log("i should be a client, starting vid now");
        // ChangeVideoplayerVisibility(150);

        // Debug.Log("about to play video on client side, but in a clientRpc");
        // videoPlayer.url = VideoManager.VocabWordToPathDict[CorrectWord.Value.ToString()];
        // videoPlayer.Play();
        HandleCorrectWordChanged(string.Empty, CorrectWord.Value);
    }

    private void HandleCorrectWordChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        // Make videoplayer visible
        ChangeVideoplayerVisibility(150);

        // Play correct video
        Debug.Log("about to play video on client side");
        videoPlayer.url = VideoManager.VocabWordToPathDict[newValue.ToString()];
        videoPlayer.Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        // // TODO: Delete below code when successfully implemented
        // VideoManager.GenerateVocabListFromSelectedVocabSet();

        // // Make videoplayer transparent
        // rawImage = videoPlayer.gameObject.GetComponent<RawImage>();
        // rawImage.color = new Color32(255, 255, 255, 0);

        // // Handle difficulty setting
        // switch (Globals.difficulty)
		// {
		// 	case Globals.Difficulty.Easy:
        //         fallingSpeed = 0.2f;
        //         spawnRate = 1.2f;
		// 		break;
		// 	case Globals.Difficulty.Medium:
		// 		fallingSpeed = 0.3f;
        //         spawnRate = 1.0f;
		// 		break;
		// 	case Globals.Difficulty.Hard:
		// 		fallingSpeed = 0.4f;
        //         spawnRate = 0.8f;
		// 		break;
		// }

        //StartSpawningWords();
    }

    private void ChangeVideoplayerVisibility(int visibility)
    {
        // Make videoplayer visible
        Debug.Log("rawImage is now visible");
        rawImage.color = new Color32(255, 255, 255, (byte) visibility);
    }

    public void StartSpawningWords()
    {
        if (isSpawnerActive) return;
        isSpawnerActive = true;

        ChangeCorrectWord();
        spawnerCoroutine = StartCoroutine(SpawnRandomGameObject());
    }

    public void StopSpawningWords()
    {
        if (!isSpawnerActive) return;
        isSpawnerActive = false;

        ChangeVideoplayerVisibility(10);
        StopCoroutine(spawnerCoroutine);
    }

    public IEnumerator SpawnRandomGameObject()
    {
        yield return new WaitForSeconds(spawnRate);

        SpawnOneWord();

        // if (Random.Range(0, 10) == 0)
        // {
        //     StartCoroutine(SpawnRandomPowerUp());
        // }

        spawnerCoroutine = StartCoroutine(SpawnRandomGameObject());
    }

    public void SpawnOneWord()
    {
        int randomVocabWordIndex = Random.Range(0, currentWordsToSpawn.Count);

        FallingWord wordInstance = SpawnWord(currentWordsToSpawn[randomVocabWordIndex]);

        //Debug.Log($"spawning word with wordText: {wordInstance.wordText.Value}");
    }

    private FallingWord SpawnWord(string wordText)
    {
        FallingWord wordInstance = Instantiate(word, GetSpawnPoint(), Quaternion.identity);
        wordInstance.GetComponent<NetworkObject>().Spawn();

        wordInstance.SetValue(wordValue);
        wordInstance.SetText(wordText);
        wordInstance.SetGravityScale(fallingSpeed);
        // Debug.Log($"this time setting word internally as {wordInstance.wordText.Value}");

        wordInstance.OnCollected += HandleCorrectWordCollected;
        wordInstance.OnIncorrectCollected += HandleIncorrectWordCollected;

        return wordInstance;
    }

    private Vector2 GetSpawnPoint()
    {
        float x = 0;
        float y = 0;

        int maxTries = 50;
        int currentTry = 0;

        while(currentTry < maxTries)
        {
            x = Random.Range(xSpawnRange.x, xSpawnRange.y);
            y = Random.Range(ySpawnRange.x, ySpawnRange.y);
            Vector2 spawnPoint = new Vector2(x, y);
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, wordRadius, wordBuffer, layerMask);
            if (numColliders == 0)
            {
                return spawnPoint;
            }
            currentTry++;
        }

        return new Vector2(x, y);
    }

    private void HandleCorrectWordCollected(FallingWord word)
    {
        Debug.Log($"got correct word, textmeshpro: {word.GetComponentInChildren<TextMeshProUGUI>().text}");
        word.GetComponent<NetworkObject>().Despawn();

        ChangeCorrectWord();
    }

    private void HandleIncorrectWordCollected(FallingWord word)
    {
        Debug.Log($"got incorrect word, textmeshpro: {word.GetComponentInChildren<TextMeshProUGUI>().text}");
        word.GetComponent<NetworkObject>().Despawn();
    }

    public bool CheckIfCollectedWordIsCorrect(FallingWord word)
    {
        return word.wordText.Value == CorrectWord.Value;
    }

    // IEnumerator SpawnRandomPowerUp()
    // {
    //     yield return new WaitForSeconds(Random.Range(1, 2));

    //     int randomPowerUp = Random.Range(0, powerUps.Length);
    //     GameObject tmp = Instantiate(powerUps[randomPowerUp], new Vector2(Random.Range(xBoundLeft, xBoundRight), yBound), Quaternion.identity);
    //     tmp.transform.SetParent(transform, false);
    // }

    // public void ReadFromFileJSON()
    // {
    //     //Debug.Log("about to read file");
    //     // feed in textasset.text, add json file as text asset to a game object (forces load)
    //     Questions questionsjson = JsonUtility.FromJson<Questions>(jsonFile.text);
    //     //Debug.Log("file read");
    //     foreach (Question q in questionsjson.questions)
    //     {
    //         links.Add(q.Link);
    //         words.Add(q.Word);
    //     }
    // }

    public void ChangeCorrectWord()
    {
        //List<string> levelVocabList = LevelOperator.CurrentLevelVocabList;
        List<string> levelVocabList = VideoManager.VocabWordToPathDict.Keys.ToList();
        
        int randomWordIndex = Random.Range(0, levelVocabList.Count);
        string randomWord = levelVocabList[randomWordIndex];
        CorrectWord.Value = randomWord;
        currentWordsToSpawn.Clear();
    
        if (levelVocabList.Count <= currentWordsToSpawnSize)
        {
            currentWordsToSpawn.AddRange(levelVocabList);
        }
        else
        {
            currentWordsToSpawn.Add(CorrectWord.Value.ToString());
            
            for (int i = 0; i < currentWordsToSpawnSize; i++)
            {
                int randomVocabWordIndex = Random.Range(0, levelVocabList.Count);
                currentWordsToSpawn.Add(levelVocabList[randomVocabWordIndex]);
                
            }
            
        }
        
        // Debug.Log("about to play video");
        // //videoPlayerController.PlayVideo(correctWord);
        // videoPlayer.url = VideoManager.VocabWordToPathDict[CorrectWord.Value.ToString()];
        // videoPlayer.Play();
    }
}

// ----------------------------------------------- JSON READING CLASSES ---------------------------------------------

[System.Serializable]
public class Question
{
    //these variables are case sensitive and must match the strings "Word" and "Link" in the JSON.
    public string Word;
    public string Link;
}

[System.Serializable]
public class Questions
{
    //Questions is case sensitive and must match the string "questions" in the JSON.
    public Question[] questions;
}