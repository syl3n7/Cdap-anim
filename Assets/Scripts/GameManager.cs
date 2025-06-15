using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.LiveCapture.ARKitFaceCapture;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Time.timeScale = 10.0f;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Time.timeScale = 1.0f;
        }
    }
}
