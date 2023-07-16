using Assets.Scripts.CustomException;
using Assets.Scripts.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum gameStatus
{
    next, play, gameover, continueGame, pause, start, playAgain
}
public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private Text totalMoneyLabel;   // số tiền hiện tại
    [SerializeField]
    private Text currentWaveLabel; // wave hiện tại
    [SerializeField]
    private Text totalEscapedLabel; // số lượng enemy đã thoát
    [SerializeField]
    private GameObject spawnPoint; // điểm xuất hiện enemy
    [SerializeField]
    private Enemy03[] enemies; // danh sách enemy
    [SerializeField]
    private int totalEnemies = 3; // số lượng enemy trong wave
    [SerializeField]
    private int enemiesPerSpawn; // số lượng enemy spawn trong 1 lần
    [SerializeField]
    private Text playButtonLabel; // label của button play
    [SerializeField]
    private Button playButton; // button play

    [SerializeField]
    private Text continueButtonLabel; // label của button continue
    [SerializeField]
    private Button continueButton; // button continue

    public float[] enemySpawnRates = new float[] { 0.7f, 0.2f, 0.1f }; // tỉ lệ xuất hiện của các enemy

    private int waveNumber = 0; // số lượng wave
    private int totalMoney = 25; // số tiền ban đầu
    private int totalEscaped = 0; // số lượng enemy đã thoát trong tất cả các wave
    private int roundEscaped = 0; // số lượng enemy đã thoát trong wave hiện tại
    private int totalKilled = 0; // số lượng enemy đã bị giết trong wave hiện tại
    private int whichEnemiesToSpawn = 0; // số lượng enemy còn lại trong wave hiện tại
    private int enemiesToSpawn = 0; // số lượng enemy cần spawn trong wave hiện tại
    private gameStatus currentState = gameStatus.play; // trạng thái game hiện tại
    private AudioSource audioSource;

    public List<Enemy03> EnemyList = new List<Enemy03>(); // danh sách enemy trong wave hiện tại
    private float spawnDelay = 1.5f; // thời gian delay giữa 2 lần spawn enemy
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

    void Start()
    {
        try
        {
            playButton.gameObject.SetActive(false); // ẩn button play
            continueButton.gameObject.SetActive(false); // ẩn button continue
            fileIOManager = gameObject.AddComponent<FileIOManager>(); // thêm fileIOManager vào game
            TowerPanel = GameObject.FindWithTag("towerPanel"); // tìm towerPanel
            TowerPanel?.SetActive(false); // ẩn towerPanel
            audioSource = GetComponent<AudioSource>(); // lấy audioSource
            if (audioSource == null)
            {
                throw new ExceptionHandling("audioSource is not found", "", DateTime.Now, "107");
            }
            LoadDefaultGameData(); // load dữ liệu mặc định
            LoadGameData(); // load dữ liệu đã lưu
            ShowMenu(); // hiển thị menu
        }
        catch (ExceptionHandling ex)
        {
            ex.Handle();
        }
        catch(Exception ex)
        {
            Debug.LogError(ex.Message);
        }
       
    }

    /// <summary>
    /// Load default game data
    /// </summary>
    private void LoadDefaultGameData()
    {
        gameData = fileIOManager.LoadDefaultGameData();
        if (gameData == null)
        {
            return;
        }
        totalMoney = gameData.totalMoney;
    }

    /// <summary>
    /// Load last game data
    /// </summary>
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

    /// <summary>
    /// Lưu dữ liệu khi thoát game
    /// </summary>
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

    /// <summary>
    /// Spawn enemy dùng để tạo ra enemy trong game và thêm vào danh sách enemy
    /// </summary>
    IEnumerator Spawn()
    {
        // nếu số lượng enemy còn lại trong wave lớn hơn 0 và số lượng enemy trong danh sách enemy nhỏ hơn tổng số lượng enemy trong wave
        if (enemiesPerSpawn > 0 && EnemyList.Count < totalEnemies)
        {
            for (int i = 0; i < enemiesPerSpawn; i++) // vòng lặp spawn enemy
            {
                if (EnemyList.Count < totalEnemies) // nếu số lượng enemy trong danh sách enemy nhỏ hơn tổng số lượng enemy trong wave
                {
                    if (whichEnemiesToSpawn == 0) // nếu số lượng enemy còn lại trong wave bằng 0
                    {
                        // Tạo một queue mới từ danh sách enemy
                        Queue<Enemy03> enemyQueue = new Queue<Enemy03>(enemies);
                        whichEnemiesToSpawn = enemyQueue.Count;
                    }

                    // Lấy enemy từ đầu queue và spawn nó để tạo ra enemy trong game
                    Enemy03 enemyToSpawn = whichEnemiesToSpawn > 0 ? enemies[enemies.Length - whichEnemiesToSpawn] : null; // lấy enemy từ danh sách enemy
                    if (enemyToSpawn != null) // nếu enemy khác null
                    {
                        Enemy03 newEnemy = Instantiate(enemyToSpawn); // tạo ra enemy mới
                        newEnemy.transform.position = spawnPoint.transform.position; // set vị trí cho enemy
                        whichEnemiesToSpawn--; // giảm số lượng enemy còn lại trong wave
                    }
                }
            }
            yield return new WaitForSeconds(spawnDelay); // delay giữa 2 lần spawn enemy
            StartCoroutine(Spawn()); // gọi lại hàm spawn
        }
    }

    /// <summary>
    /// Đăng ký enemy vào danh sách enemy
    /// </summary>
    public void RegisterEnemy(Enemy03 enemy)
    {
        EnemyList.Add(enemy);
    }

    /// <summary>
    /// Xóa enemy khỏi danh sách enemy
    /// </summary>
    public void UnregisterEnemy(Enemy03 enemy)
    {
        EnemyList.Remove(enemy);
        Destroy(enemy.gameObject);
    }

    /// <summary>
    /// Xóa tất cả enemy trong danh sách enemy
    /// </summary>
    public void DestroyAllEnemies()
    {
        foreach (Enemy03 enemy in EnemyList)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        EnemyList.Clear();
    }

    /// <summary>
    /// Tăng số tiền
    /// </summary>
    public void AddMoney(int amount)
    {
        TotalMoney += amount;
    }

    /// <summary>
    /// Giảm số tiền
    /// </summary>
    public void SubtractMoney(int amount)
    {
        TotalMoney -= amount;
    }

    /// <summary>
    /// Kiểm tra xem wave đã kết thúc chưa
    /// </summary>
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

    /// <summary>
    /// Set trạng thái game
    /// </summary>
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

    /// <summary>
    /// Hiển thị menu
    /// </summary>
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
    /// <summary>
    /// Xử lý khi button play được nhấn
    /// </summary>
    public void playButtonPressed()
    {
        switch (currentState)
        {
            case gameStatus.next: // nếu wave kết thúc
                waveNumber += 1;
                totalEnemies += waveNumber;
                break;
            case gameStatus.start: // nếu game đang start
                currentState = gameStatus.pause; // set trạng thái game thành pause
                Time.timeScale = 0; // dừng game
                playButtonLabel.text = "Continue"; // đổi label của button play thành Continue
                continueButton.gameObject.SetActive(true); // hiển thị button continue
                continueButtonLabel.text = "New Game"; // đổi label của button continue thành New Game
                TowerManager.Instance.IsPreventCreateTower = true; // ngăn không cho tạo tower
                return;
            case gameStatus.pause: // nếu game đang pause
                currentState = gameStatus.start; // set trạng thái game thành start
                Time.timeScale = 1; // tiếp tục game
                playButtonLabel.text = "Pause"; // đổi label của button play thành Pause
                continueButton.gameObject.SetActive(false); // ẩn button continue
                TowerManager.Instance.IsPreventCreateTower = false; // cho phép tạo tower
                return;
            default:
                totalEnemies = 3; // set số lượng enemy trong wave thành 3
                totalEscaped = 0; // set số lượng enemy đã thoát thành 0
                TowerManager.Instance.RenameTagsBuildSites(); // đổi tên các tag của các build site
                totalMoneyLabel.text = TotalMoney.ToString(); // set số tiền hiện tại
                totalEscapedLabel.text = "Escaped " + totalEscaped + "/10"; // set số lượng enemy đã thoát
                break;
        }
        AudioSource.PlayOneShot(SoundManager.Instance.NewGame); // chạy âm thanh khi bắt đầu game
        if (currentState != gameStatus.playAgain) // nếu trạng thái game khác playAgain thì set lại giá trị mặc định
        {
            setDefaultGameValue(isDefault: false);
        }
        continueButton.gameObject.SetActive(false); // ẩn button continue
        TowerPanel?.SetActive(true); // hiển thị towerPanel
        StartCoroutine(Spawn()); // gọi hàm spawn
        Time.timeScale = 1; // tiếp tục game 
        playButtonLabel.text = "Pause"; // đổi label của button play thành Pause
        currentState = gameStatus.start; // set trạng thái game thành start
        TowerManager.Instance.IsPreventCreateTower = false; // cho phép tạo tower
    }

    /// <summary>
    /// Set giá trị mặc định cho game
    /// </summary>
    private void setDefaultGameValue(bool isDefault = true, int status = 0)
    {
        if (status == (int)gameStatus.continueGame) // nếu trạng thái game là continueGame
        {
            totalMoneyLabel.text = TotalMoney.ToString();
            totalEscapedLabel.text = "Escaped " + totalEscaped + "/10";
        }
        else
        {
            if (isDefault) // nếu là giá trị mặc định
            {
                TotalMoney = 25; // set số tiền ban đầu
                totalEnemies = 3; // set số lượng enemy trong wave thành 3
                totalEscaped = 0; // set số lượng enemy đã thoát thành 0
                TowerManager.Instance.RenameTagsBuildSites(); // đổi tên các tag của các build site
                TowerManager.Instance.DestroyAllTower(); // xóa tất cả tower
                TowerPanel?.SetActive(false); // ẩn towerPanel
                waveNumber = 0; // set số lượng wave thành 0
                totalMoneyLabel.text = TotalMoney.ToString();
                totalEscapedLabel.text = "Escaped " + totalEscaped + "/10";
            }
            DestroyAllEnemies(); // xóa tất cả enemy
            TotalKilled = 0; // set số lượng enemy đã bị giết thành 0
            RoundEscaped = 0; // set số lượng enemy đã thoát trong wave hiện tại thành 0
            LoadDefaultGameData(); // load dữ liệu mặc định
        }

        currentWaveLabel.text = "Wave " + (waveNumber + 1); // set wave hiện tại
        currentState = gameStatus.play; // set trạng thái game thành play
        ShowMenu(); // hiển thị menu
    }
    /// <summary>
    /// Xử lý khi button continue được nhấn
    /// </summary>
    public void continueButtonPressed()
    {
        if (currentState == gameStatus.pause)
        {
            // nếu trạng thái game là pause thì set lại giá trị mặc định
            setDefaultGameValue();
            continueButton.gameObject.SetActive(false);
            currentState = gameStatus.playAgain;
        }
        else
        {
            // nếu trạng thái game là continueGame thì load dữ liệu đã lưu
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
