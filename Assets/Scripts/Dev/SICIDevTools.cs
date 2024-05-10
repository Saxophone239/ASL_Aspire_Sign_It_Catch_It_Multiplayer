using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SICIDevTools : MonoBehaviour
{
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
        }
    }
}
