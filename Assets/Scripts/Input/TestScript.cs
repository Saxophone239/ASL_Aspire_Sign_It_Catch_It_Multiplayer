using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    // Start is called before the first frame update
    void Start()
    {
        inputReader.MoveEvent += HandleMove;
        inputReader.JumpEvent += HandleJump;
    }

    private void HandleJump(bool obj)
    {
        Debug.Log("Jumped");
    }

    private void OnDestroy()
    {
        inputReader.MoveEvent -= HandleMove;
        inputReader.JumpEvent -= HandleJump;
    }

    private void HandleMove(float obj)
    {
        Debug.Log(obj);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
