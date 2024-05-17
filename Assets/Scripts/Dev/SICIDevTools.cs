using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SICIDevTools : MonoBehaviour
{
    [SerializeField] private GameObject testingWordPrefab;
    private SICISpawner spawner;

    // Start is called before the first frame update
    void Start()
    {
        spawner = FindObjectOfType<SICISpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            spawner.SpawnOneWord();
            // GameObject wordInstance = Instantiate(testingWordPrefab, testingWordPrefab.transform.position, Quaternion.identity);
            // wordInstance.GetComponent<NetworkObject>().Spawn();
        }
    }
}
