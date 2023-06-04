using Assets.Scripts.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum gameStatus
{
    next, play, gameover, continueGame, pause, start, playAgain
}
public class GameManager : Singleton<GameManager>
{
    //SerializeField - Allows Inspector to get access to private fields.
    //If we want to get access to this from another class, we'll just need to make public getters
    [SerializeField]
    private Text totalMoneyLabel;   //Refers to money label at upper left corner
    [SerializeField]
    private Text currentWaveLabel;
    [SerializeField]
    private Text totalEscapedLabel;
    [SerializeField]
    private GameObject spawnPoint;
    [SerializeField]
    private Enemy[] enemies;
    [SerializeField]
    private int totalEnemies = 3;
    [SerializeField]
    private int enemiesPerSpawn;
    [SerializeField]
    private Text playButtonLabel;
    [SerializeField]
    private Button playButton;

    [SerializeField]
    private Text continueButtonLabel;
    [SerializeField]
    private Button continueButton;

    public float[] enemySpawnRates = new float[] { 0.7f, 0.2f, 0.1f };

    private int waveNumber = 0; // số lượng wave
    private int totalMoney = 25; // số tiền ban đầu
    private int totalEscaped = 0; // số lượng enemy đã thoát trong tất cả các wave
    private int roundEscaped = 0; // số lượng enemy đã thoát trong wave hiện tại
    private int totalKilled = 0; // số lượng enemy đã bị giết trong wave hiện tại
    private int whichEnemiesToSpawn = 0; // số lượng enemy còn lại trong wave hiện tại
    private int enemiesToSpawn = 0; // số lượng enemy cần spawn trong wave hiện tại
    private gameStatus currentState = gameStatus.play;
    private AudioSource audioSource;

    public List<Enemy> EnemyList = new List<Enemy>();
    private float spawnDelay = 1.5f; //Spawn Delay in seconds
    public int TotalMoney
    {
        get { return totalMoney; }
        set
        {
            totalMoney = value;
            totalMoneyLabel.text = totalMoney.ToString();
        }
    }
    public int TotalEscape
    {
        get { return totalEscaped; }
        set { totalEscaped = value; }
    }
    public int RoundEscaped
    {
        get { return roundEscaped; }
        set { roundEscaped = value; }
    }
    public int TotalKilled
    {
        get { return totalKilled; }
        set { totalKilled = value; }
    }

    public AudioSource AudioSource
    {
        get { return audioSource; }
    }

    private FileIOManager fileIOManager;
    private GameObject towerPanel;
    public GameObject TowerPanel
    {
        get { return towerPanel; }
        set { towerPanel = value; }
    }

    private GameData gameData;

    public GameData GameData
    {
        get { return gameData; }
    }

    // Use this for initialization
    void Start()
    {
        playButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        fileIOManager = gameObject.AddComponent<FileIOManager>();
        TowerPanel = GameObject.FindWithTag("towerPanel");
        TowerPanel?.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        LoadGameData();
        ShowMenu();
    }

    private void LoadGameData()
    {
        gameData = fileIOManager.LoadGameData();
        if (gameData == null)
        {
            return;
        }
        continueButton.gameObject.SetActive(true);
        continueButtonLabel.text = "Continue Last Game";
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }
    private void SaveGameData()
    {

        GameData gameData = new GameData
        {
            waveNumber = waveNumber,
            totalMoney = TotalMoney,
            totalEscaped = TotalEscape,
            roundEscaped = RoundEscaped,
            totalKilled = TotalKilled,
            whichEnemiesToSpawn = whichEnemiesToSpawn,
            enemiesToSpawn = enemiesToSpawn,
            currentState = currentState,
            totalEnemies = totalEnemies,
            enemiesPerSpawn = enemiesPerSpawn,
            spawnDelay = spawnDelay,
            enemySpawnRates = enemySpawnRates,
        };
        fileIOManager.SaveGame(gameData);
    }

    IEnumerator Spawn()
    {
        if (enemiesPerSpawn > 0 && EnemyList.Count < totalEnemies)
        {
            for (int i = 0; i < enemiesPerSpawn; i++)
            {
                if (EnemyList.Count < totalEnemies)
                {
                    if (whichEnemiesToSpawn == 0)
                    {
                        // Create a new queue with all enemy types
                        Queue<Enemy> enemyQueue = new Queue<Enemy>(enemies);
                        whichEnemiesToSpawn = enemyQueue.Count;
                    }

                    // Dequeue an enemy from the front of the queue and spawn it
                    Enemy enemyToSpawn = whichEnemiesToSpawn > 0 ? enemies[enemies.Length - whichEnemiesToSpawn] : null;
                    if (enemyToSpawn != null)
                    {
                        Enemy newEnemy = Instantiate(enemyToSpawn);
                        newEnemy.transform.position = spawnPoint.transform.position;
                        whichEnemiesToSpawn--;
                    }
                }
            }
            yield return new WaitForSeconds(spawnDelay);
            StartCoroutine(Spawn());
        }
    }

    ///Register - when enemy spawns
    public void RegisterEnemy(Enemy enemy)
    {
        EnemyList.Add(enemy);
    }
    ///Unregister - When they escape the screen
    public void UnregisterEnemy(Enemy enemy)
    {
        EnemyList.Remove(enemy);
        Destroy(enemy.gameObject);
    }
    ///Destroy - At the end of the wave
    public void DestroyAllEnemies()
    {
        foreach (Enemy enemy in EnemyList)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        EnemyList.Clear();
    }

    public void AddMoney(int amount)
    {
        TotalMoney += amount;
    }

    public void SubtractMoney(int amount)
    {
        TotalMoney -= amount;
    }

    public void IsWaveOver()
    {
        totalEscapedLabel.text = "Escaped " + TotalEscape + "/10";
        if (RoundEscaped + TotalKilled == totalEnemies)
        {
            if (waveNumber <= enemies.Length)
            {
                enemiesToSpawn = waveNumber;
            }
            SetCurrentGameState();
            ShowMenu();
        }
    }

    public void SetCurrentGameState()
    {
        if (totalEscaped >= 10)
        {
            currentState = gameStatus.gameover;
        }
        else if (waveNumber == 0 && (TotalKilled + RoundEscaped) == 0)
        {
            currentState = gameStatus.play;
        }
        else
        {
            currentState = gameStatus.next;
        }
    }

    public void ShowMenu()
    {
        switch (currentState)
        {
            case gameStatus.gameover:
                playButtonLabel.text = "Play Again!";
                AudioSource.PlayOneShot(SoundManager.Instance.Gameover);
                break;
            case gameStatus.next:
                playButtonLabel.text = "Next Wave";
                break;
            case gameStatus.play:
                playButtonLabel.text = "Play";
                break;
        }
        playButton.gameObject.SetActive(true);
    }
    public void playButtonPressed()
    {
        switch (currentState)
        {
            case gameStatus.next:
                waveNumber += 1;
                totalEnemies += waveNumber;
                break;
            case gameStatus.start:
                currentState = gameStatus.pause;
                Time.timeScale = 0;
                playButtonLabel.text = "Continue";
                continueButton.gameObject.SetActive(true);
                continueButtonLabel.text = "New Game";
                TowerManager.Instance.IsPreventCreateTower = true;
                return;
            case gameStatus.pause:
                currentState = gameStatus.start;
                Time.timeScale = 1;
                playButtonLabel.text = "Pause";
                continueButton.gameObject.SetActive(false);
                TowerManager.Instance.IsPreventCreateTower = false;
                return;
            default:
                totalEnemies = 3;
                totalEscaped = 0;
                TowerManager.Instance.RenameTagsBuildSites();
                totalMoneyLabel.text = TotalMoney.ToString();
                totalEscapedLabel.text = "Escaped " + totalEscaped + "/10";
                break;
        }
        AudioSource.PlayOneShot(SoundManager.Instance.NewGame);
        // if current state is not playAgain, set default value for game
        if (currentState != gameStatus.playAgain)
        {
            setDefaultGameValue(isDefault: false);
        }
        continueButton.gameObject.SetActive(false);
        TowerPanel?.SetActive(true);
        StartCoroutine(Spawn());
        Time.timeScale = 1;
        playButtonLabel.text = "Pause";
        currentState = gameStatus.start;
        TowerManager.Instance.IsPreventCreateTower = false;
    }
    // set default value for game
    private void setDefaultGameValue(bool isDefault = true, int status = 0)
    {
        if (status == (int)gameStatus.continueGame)
        {
            totalMoneyLabel.text = TotalMoney.ToString();
            totalEscapedLabel.text = "Escaped " + totalEscaped + "/10";
        }
        else
        {
            if (isDefault)
            {
                TotalMoney = 25;
                totalEnemies = 3;
                totalEscaped = 0;
                TowerManager.Instance.RenameTagsBuildSites();
                TowerManager.Instance.DestroyAllTower();
                TowerPanel?.SetActive(false);
                waveNumber = 0;
                totalMoneyLabel.text = TotalMoney.ToString();
                totalEscapedLabel.text = "Escaped " + totalEscaped + "/10";
            }
            DestroyAllEnemies();
            TotalKilled = 0;
            RoundEscaped = 0;
        }

        currentWaveLabel.text = "Wave " + (waveNumber + 1);
        currentState = gameStatus.play;
        ShowMenu();
    }
    public void continueButtonPressed()
    {
        if (currentState == gameStatus.pause)
        {
            setDefaultGameValue();
            continueButton.gameObject.SetActive(false);
            currentState = gameStatus.playAgain;
        }
        else
        {
            waveNumber = gameData.waveNumber;
            totalMoney = gameData.totalMoney;
            totalEscaped = gameData.totalEscaped;
            roundEscaped = gameData.roundEscaped;
            totalKilled = gameData.totalKilled;
            whichEnemiesToSpawn = gameData.whichEnemiesToSpawn;
            enemiesToSpawn = gameData.enemiesToSpawn;
            currentState = gameData.currentState;
            totalEnemies = gameData.totalEnemies;
            enemiesPerSpawn = gameData.enemiesPerSpawn;
            spawnDelay = gameData.spawnDelay;
            enemySpawnRates = gameData.enemySpawnRates;
            setDefaultGameValue(isDefault: false, status: (int)gameStatus.continueGame);
            StartCoroutine(Spawn());
            continueButton.gameObject.SetActive(false);
            currentState = gameStatus.play;
        }
    }
}
