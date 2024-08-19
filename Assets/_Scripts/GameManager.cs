using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class GameManager : MonoBehaviour
{
    [SerializeField] TMP_Text winLoseText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] InputActionManager inputActionManager;
    public static GameManager Instance { get; private set; }

    private Health[] healths;

    private int wins = 0;
    private int losses = 0;
    private bool gameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        healths = FindObjectsOfType<Health>();
        winLoseText.SetText("");
        UpdateScore();
    }

    private void Update()
    {
        if(!gameOver) CheckHealths();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        winLoseText = GameObject.Find("WinLoseText").GetComponent<TMP_Text>();
        scoreText = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
        healths = FindObjectsOfType<Health>();
        winLoseText.SetText("");
        UpdateScore();
        gameOver = false;
    }

    private void CheckHealths()
    {
        foreach(Health health in healths)
        {
            if(health.CurrentHealth <= 0)
            {
                if(health is BasicPlayerHealth)
                {
                    PlayerDeath();
                    gameOver = true;
                    break;
                }
                if(health is BasicEnemyHealth)
                {
                    EnemyDeath();
                    gameOver = true;
                    break;
                }
            }
        }
    }

    private void PlayerDeath()
    {
        losses++;
        winLoseText.SetText("..YOU LOSE..");
        UpdateScore();
        StartCoroutine(GameOverCoroutine());
    }

    private void EnemyDeath()
    {
        wins++;
        winLoseText.SetText("!!!.YOU WIN.!!!");
        UpdateScore();
        StartCoroutine(GameOverCoroutine());
    }

    private void UpdateScore()
    {
        scoreText.SetText("You: " + wins + " | Enemy: " + losses);
    }

    private IEnumerator GameOverCoroutine()
    {
        inputActionManager.DisableInput();
        yield return new WaitForSeconds(5f);
        inputActionManager.EnableInput();
        winLoseText.SetText("");

        ReloadScene();
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

