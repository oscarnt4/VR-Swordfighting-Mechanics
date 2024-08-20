using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class GameManager : MonoBehaviour
{
    [SerializeField] TMP_Text winLoseText;
    [SerializeField] TMP_Text scoreText;
    public static GameManager Instance { get; private set; }

    private InputActionManager inputActionManager;
    private Health[] healths;
    private ComplexEnemyController complexEnemyController; 
    private XRSimpleInteractable _swordInteractable;

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
        inputActionManager = FindObjectOfType<InputActionManager>();
        healths = FindObjectsOfType<Health>();
        complexEnemyController = FindObjectOfType<ComplexEnemyController>();
        _swordInteractable = FindObjectOfType<XRSimpleInteractable>();

        complexEnemyController.enabled = false;

        winLoseText.SetText("");
        UpdateScore();
    }

    private void Update()
    {
        if(!gameOver) CheckHealths(); 
        if (_swordInteractable.isSelected) complexEnemyController.enabled = true;
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
        inputActionManager = FindObjectOfType<InputActionManager>();
        healths = FindObjectsOfType<Health>();
        complexEnemyController = FindObjectOfType<ComplexEnemyController>();
        _swordInteractable = FindObjectOfType<XRSimpleInteractable>();

        complexEnemyController.enabled = false;

        winLoseText = GameObject.Find("WinLoseText").GetComponent<TMP_Text>();
        scoreText = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
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
                gameOver = true;
                if (health is BasicPlayerHealth)
                {
                    Debug.Log("..::PLAYER LOSES::..");
                    PlayerDeath();
                    break;
                }
                if(health is BasicEnemyHealth)
                {
                    Debug.Log("!!!.PLAYER WINS.!!!");
                    EnemyDeath();
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
        inputActionManager.DisableInput();
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
        Debug.Log("Current Score => " + scoreText.text);

        complexEnemyController.enabled = false;

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

