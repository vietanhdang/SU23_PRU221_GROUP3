using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum gameStatus
{
    next, play, gameover, win
}
public class GameManager : Singleton<GameManager>
{
    //SerializeField - Allows Inspector to get access to private fields.
    //If we want to get access to this from another class, we'll just need to make public getters
    [SerializeField]
    private int totalWaves = 10;
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

    public float[] enemySpawnRates = new float[] { 0.7f, 0.2f, 0.1f };

    private int waveNumber = 0;
    private int totalMoney = 25;
    private int totalEscaped = 0;
    private int roundEscaped = 0;
    private int totalKilled = 0;
    private int whichEnemiesToSpawn = 0;
    private int enemiesToSpawn = 0;
    private gameStatus currentState = gameStatus.play;
    private AudioSource audioSource;

    public List<Enemy> EnemyList = new List<Enemy>();
    const float spawnDelay = 1.5f; //Spawn Delay in seconds
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

    // Use this for initialization
    void Start()
    {
        playButton.gameObject.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        ShowMenu();
	}
	
	// Update is called once per frame
	void Update () {
        HandleEscape();
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
            if(enemy != null)
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
        Debug.Log("Play Button Pressed");
        switch (currentState)
        {
            case gameStatus.next:
                waveNumber += 1;
                totalEnemies += waveNumber;
                break;
            default:
                totalEnemies = 3;
                totalEscaped = 0;
                //TotalMoney = 20;
                //TowerManager.Instance.DestroyAllTower();
                TowerManager.Instance.RenameTagsBuildSites();
                totalMoneyLabel.text = TotalMoney.ToString();
                totalEscapedLabel.text = "Escaped " + totalEscaped + "/10";
                AudioSource.PlayOneShot(SoundManager.Instance.NewGame);
                break;
        }
        DestroyAllEnemies();
        //TowerManager.Instance.DestroyAllTower();
        TotalKilled = 0;
        RoundEscaped = 0;
        currentWaveLabel.text = "Wave " + (waveNumber + 1);
        StartCoroutine(Spawn());
        playButton.gameObject.SetActive(false);
    }
    private void HandleEscape()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TowerManager.Instance.DisableDragSprite();
            TowerManager.Instance.towerButtonPressed = null;
        }
    }

}
