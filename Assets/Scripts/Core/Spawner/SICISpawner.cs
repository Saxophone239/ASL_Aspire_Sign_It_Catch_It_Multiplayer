using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// This script manages spawning in words and powerups.
/// </summary>
public class SICISpawner : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject fallingWordPrefab;
    [SerializeField] private GameObject[] powerUps;
    // [SerializeField] private GameObject serverFallingWord;
    // [SerializeField] private GameObject clientFallingWord;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("World Bounds and Parameters")]
    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;
    [SerializeField] private LayerMask layerMask;

    private int wordValue = 10;
    private Collider2D[] wordBuffer = new Collider2D[1];
    private float wordRadius;

    public NetworkVariable<FixedString32Bytes> CorrectWord = new NetworkVariable<FixedString32Bytes>();
    public float fallingSpeed = 0.2f;
    public float spawnRate = 1.0f;

    // Videoplayer
    private RawImage rawImage;

    private List<string> currentWordsToSpawn = new List<string>();
    private int currentWordsToSpawnSize = 6;
    
    private bool isSpawnerActive;

    //private List<string> vidVocabList;

    Coroutine spawnerCoroutine;

    public override void OnNetworkSpawn()
    {
        // TODO: Delete below code when successfully implemented
        VideoManager.GenerateVocabListFromSelectedVocabSet();

        wordRadius = fallingWordPrefab.GetComponent<CircleCollider2D>().radius;

        //BasketPlayer.OnPlayerPrefabSpawned += HandlePlayerSpawned;
        rawImage = videoPlayer.gameObject.GetComponent<RawImage>();

        if (IsClient)
        {
            CorrectWord.OnValueChanged += HandleCorrectWordChanged;
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
            //BasketPlayer.OnPlayerPrefabSpawned -= HandlePlayerSpawned;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            StopSpawningWords();
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
        //HandleCorrectWordChanged(string.Empty, CorrectWord.Value);
        Debug.Log($"Player with id {clientId} has joined, playing videoplayer");
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

    private void ChangeVideoplayerVisibility(int visibility)
    {
        // Make videoplayer visible
        if (rawImage != null) rawImage.color = new Color32(255, 255, 255, (byte) visibility);
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

        // TODO: implement spawning powerups
        // if (Random.Range(0, 10) == 0)
        // {
        //     StartCoroutine(SpawnRandomPowerUp());
        // }

        spawnerCoroutine = StartCoroutine(SpawnRandomGameObject());
    }

    public void SpawnOneWord(bool spawnCenter = false)
    {
        if (!IsServer) return;

        int randomVocabWordIndex = Random.Range(0, currentWordsToSpawn.Count);
        //Debug.Log($"Spawning {currentWordsToSpawn[randomVocabWordIndex]}");

        Vector2 spawnPos = GetSpawnPoint();
        if (spawnCenter) spawnPos = Vector2.zero;

        GameObject wordInstance = Instantiate(this.fallingWordPrefab, spawnPos, Quaternion.identity);
        wordInstance.GetComponent<NetworkObject>().Spawn();

        if (wordInstance.TryGetComponent<FallingWord>(out FallingWord fallingWord))
        {
            fallingWord.SetValue(wordValue);
            if (spawnCenter)
            {
                fallingWord.SetText("testing");
            }
            else
            {
                fallingWord.SetText(currentWordsToSpawn[randomVocabWordIndex]);
            }
            fallingWord.SetGravityScale(fallingSpeed);

            fallingWord.OnCollected += HandleCorrectWordCollected;
            fallingWord.OnIncorrectCollected += HandleIncorrectWordCollected;
        }
    }

    // NOTE: below is tested code for spawning in client-specific and server-specific prefabs, below is here for reference.

    // private GameObject SpawnWordServer(int wordValue, string wordText, float fallingSpeed, Vector2 spawnPosition)
    // {
    //     GameObject wordInstance = Instantiate(serverFallingWord, spawnPosition, Quaternion.identity);
    //     wordInstance.GetComponent<NetworkObject>().Spawn();

    //     if (wordInstance.TryGetComponent<FallingWord>(out FallingWord fallingWord))
    //     {
    //         fallingWord.SetValue(wordValue);
    //         fallingWord.SetText(wordText);
    //         fallingWord.SetGravityScale(fallingSpeed);

    //         fallingWord.OnCollected += HandleCorrectWordCollected;
    //         fallingWord.OnIncorrectCollected += HandleIncorrectWordCollected;
    //     }

    //     return wordInstance;
    // }

    // private GameObject SpawnWordClient(int wordValue, string wordText, float fallingSpeed, Vector2 spawnPosition)
    // {
    //     GameObject wordInstance = Instantiate(clientFallingWord, spawnPosition, Quaternion.identity);
    //     wordInstance.GetComponent<NetworkObject>().Spawn();

    //     if (wordInstance.TryGetComponent<FallingWord>(out FallingWord fallingWord))
    //     {
    //         fallingWord.SetValue(wordValue);
    //         fallingWord.SetText(wordText);
    //         fallingWord.SetGravityScale(fallingSpeed);

    //         fallingWord.OnCollected += HandleCorrectWordCollected;
    //         fallingWord.OnIncorrectCollected += HandleIncorrectWordCollected;
    //     }

    //     return wordInstance;
    // }

    // [ClientRpc]
    // private void SpawnDummyWordClientRpc(int wordValue, string wordText, float fallingSpeed, Vector2 spawnPosition)
    // {
    //     SpawnWordClient(wordValue,
    //             wordText,
    //             fallingSpeed,
    //             spawnPosition);
    // }

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
        //word.GetComponent<NetworkObject>().Despawn();

        ChangeCorrectWord();
    }

    private void HandleIncorrectWordCollected(FallingWord word)
    {
        Debug.Log($"got incorrect word, textmeshpro: {word.GetComponentInChildren<TextMeshProUGUI>().text}");
        //word.GetComponent<NetworkObject>().Despawn();
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