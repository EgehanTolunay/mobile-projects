using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;

public class GameController : Singleton<GameController>
{
    public UnityEvent OnGameEnded;
    public UnityEvent OnGameInitialized;
    public UnityEvent OnGameStarted;

    private bool isStarted;
    public bool IsStarted
    {
        get { return isStarted; }
    }

    public void InitializeGame()
    {
        OnGameInitialized.Invoke();
    }

    public void StartGame()
    {
        isStarted = true;
        OnGameStarted.Invoke();
        Debug.Log("Game Started");
    }

    public void EndGame()
    {
        isStarted = false;
        OnGameEnded.Invoke();

        StartCoroutine(RestartInSeconds(5));
    }

    IEnumerator RestartInSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
