using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class TowerManager : Singleton<TowerManager>
{
    public TowerButton towerButtonPressed { get; set; } // Button chọn tower
    private SpriteRenderer spriteRenderer;  // Sprite của tower
    public bool IsPreventCreateTower { get; set; } = false; // Kiểm tra xem có thể tạo tower hay không
    private List<Tower> TowerList = new List<Tower>(); // Danh sách các tower
    private List<Collider2D> BuildList = new List<Collider2D>(); // Danh sách các build site (nơi có thể xây tower)
    private Collider2D buildTile; // Collider của build site
    private Dictionary<Collider2D, int> buildTileAndIndexOfTower = new Dictionary<Collider2D, int>(); // Danh sách các build site và index của tower
    private Dictionary<int, Tower> towerAndIndexOfTower = new Dictionary<int, Tower>(); // Danh sách các tower và index của tower
    private int indexOfTower; // Index của tower
    private Collider2D hitObject; // Collider của object được click
    private GameObject buttonUpgrade; // Button upgrade tower
    private GameObject buttonRemoveTower; // Button remove tower
    private int selectedIndex; // Index của tower được chọn
    Text levelDisplay; // Hiển thị level của tower

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Lấy sprite renderer của tower
        buildTile = GetComponent<Collider2D>(); // Lấy collider của tower
        hitObject = GetComponent<Collider2D>(); // Lấy collider của object được click
        spriteRenderer.enabled = false; // Ẩn sprite của tower
        indexOfTower = 1; // Khởi tạo index của tower
        buttonUpgrade = GameObject.FindGameObjectWithTag("buttonUpgrade"); // Lấy button upgrade
        buttonUpgrade.SetActive(false); // Ẩn button upgrade
        buttonRemoveTower = GameObject.FindGameObjectWithTag("removeTower"); // Lấy button remove tower
        buttonRemoveTower.SetActive(false); // Ẩn button remove tower
    }

    void Update()
    {
        if (IsPreventCreateTower) return; // Nếu không thể tạo tower thì return
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Lấy vị trí của chuột
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero); // Lấy collider của object được click (nếu có)
            if (hit.collider.tag == "buildSite")
            {
                // Nếu click vào build site thì tạo tower
                buildTile = hit.collider; // Lấy collider của build site
                buildTile.tag = "buildSiteFull";  // Đổi tag của build site thành build site full để không thể tạo tower ở đây nữa
                if (buildTileAndIndexOfTower.ContainsKey(buildTile))
                {   // Nếu build site đã được tạo tower thì không tạo tower nữa
                    buildTileAndIndexOfTower[buildTile] = indexOfTower;
                }
                else
                {
                    // Nếu build site chưa được tạo tower thì tạo tower
                    RegisterBuildSite(buildTile);
                }
                PlaceTower(hit); // Tạo tower
                indexOfTower++; // Tăng index của tower
            }

            if (hit.collider.tag == "buildSiteFull")
            {
                // Nếu click vào build site full thì chọn tower
                hitObject = hit.collider; // Lấy collider của object được click
                int index = buildTileAndIndexOfTower.FirstOrDefault(x => x.Key.Equals(hitObject)).Value; // Lấy index của tower
                Tower tower; // Tower được chọn
                towerAndIndexOfTower.TryGetValue(index, out tower); // Lấy tower được chọn
                if (tower == null) return; // Nếu tower không tồn tại thì return
                if (!tower.isSelected)
                {
                    // Nếu tower chưa được chọn thì chọn tower
                    foreach (Tower otherTower in towerAndIndexOfTower.Values)
                    {
                        if (otherTower.isSelected)
                        {
                            // Nếu tower khác đã được chọn thì bỏ chọn tower đó
                            Vector3 deselectedTowerPosition = otherTower.gameObject.transform.position;
                            deselectedTowerPosition.y -= 0.2f;
                            otherTower.gameObject.transform.position = deselectedTowerPosition;
                            otherTower.isSelected = false;
                            otherTower.firstPlace = false;
                        }
                    }

                    Vector3 selectedTowerPosition = tower.gameObject.transform.position; // Lấy vị trí của tower
                    selectedTowerPosition.y += 0.2f; // Tăng vị trí của tower lên để hiển thị tower được chọn
                    tower.gameObject.transform.position = selectedTowerPosition; // Cập nhật vị trí của tower
                    tower.isSelected = true; // Đánh dấu tower đã được chọn
                    tower.firstPlace = false; // Đánh dấu tower không phải là tower đầu tiên được đặt

                    buttonUpgrade.SetActive(true); // Hiển thị button upgrade
                    buttonRemoveTower.SetActive(true); // Hiển thị button remove tower
                }
                else
                {
                    Vector3 selectedTowerPosition = tower.gameObject.transform.position; // Lấy vị trí của tower
                    selectedTowerPosition.y -= 0.2f; // Giảm vị trí của tower xuống để hiển thị tower không được chọn
                    tower.gameObject.transform.position = selectedTowerPosition; // Cập nhật vị trí của tower
                    tower.isSelected = false; // Đánh dấu tower không được chọn
                    tower.firstPlace = false; // Đánh dấu tower không phải là tower đầu tiên được đặt

                    buttonUpgrade.SetActive(false); // Ẩn button upgrade
                    buttonRemoveTower.SetActive(false); // Ẩn button remove tower
                }

                if (levelDisplay)
                {
                    // Hiển thị level của tower
                    levelDisplay.text = "LV" + tower.level;
                }

                selectedIndex = index; // Lấy index của tower được chọn
            }

            if (selectedIndex != 0 && hit.collider.tag == "ground")
            {
                // Nếu click vào ground thì bỏ chọn tất cả các tower
                foreach (Tower otherTower in towerAndIndexOfTower.Values)
                {
                    if (otherTower.isSelected)
                    {
                        // Nếu tower khác đã được chọn thì bỏ chọn tower đó
                        Vector3 deselectedTowerPosition = otherTower.gameObject.transform.position;
                        deselectedTowerPosition.y -= 0.2f;
                        otherTower.gameObject.transform.position = deselectedTowerPosition;
                        otherTower.isSelected = false;
                        otherTower.firstPlace = false;
                    }
                }
                buttonUpgrade.SetActive(false);
                buttonRemoveTower.SetActive(false);
            }

        }

        // Khi có sprite được hiển thị, di chuyển sprite theo chuột
        if (spriteRenderer.enabled)
        {
            FollowMouse();
        }
    }

    /// <summary>
    /// Nâng cấp tower
    /// </summary>
    public void ClickUpgrade()
    {
        if (IsPreventCreateTower) return; // Nếu không có quyền tạo tower thì return
        Tower tower; // Tower được chọn
        towerAndIndexOfTower.TryGetValue(selectedIndex, out tower); // Lấy tower được chọn
        if (tower == null) return; // Nếu tower không tồn tại thì return
        levelDisplay = GameObject.FindWithTag("upgradePrice").GetComponent<Text>(); // Lấy text hiển thị level của tower
        if (tower.level < 3 && GameManager.Instance.TotalMoney >= 10)
        {
            // Nếu level của tower nhỏ hơn 3 và đủ tiền để nâng cấp tower thì nâng cấp tower
            GameManager.Instance.TotalMoney -= 10;
            tower.AttackRange *= 1.2f; // Tăng tầm tấn công của tower
            tower.TimeBetweenAttacks -= tower.TimeBetweenAttacks * 0.2f; // Giảm thời gian giữa các lần tấn công của tower
            tower.level += 1; // Tăng level của tower
            levelDisplay.text = "LV" + tower.level; // Hiển thị level của tower
        }
        else if (tower.level >= 3)
        {
            // nếu level của tower lớn hơn hoặc bằng 3 thì không thể nâng cấp tower nữa
            levelDisplay.text = "Max";
            // bôi đỏ vùng hiển thị tiền trong khoảng 0.5s
            Invoke("RedMoney", 0.5f);
        }
        else if (GameManager.Instance.TotalMoney < 10)
        {
            // nếu không đủ tiền để nâng cấp tower thì không thể nâng cấp tower nữa
            levelDisplay.text = "Not enough money";
            // bôi đỏ vùng hiển thị tiền trong khoảng 0.5s
            Invoke("RedMoney", 0.5f);
        }
        towerAndIndexOfTower[selectedIndex] = tower;
    }

    public void RedMoney()
    {
        // Lấy tham chiếu tới vùng hiển thị tiền
        Text moneyDisplay = GameObject.FindWithTag("upgradePrice").GetComponent<Text>();

        // Tạo màu đỏ
        Color redColor = Color.red;

        // Thiết lập màu đỏ cho vùng hiển thị tiền trong một khoảng thời gian ngắn
        moneyDisplay.color = redColor;

        // Sử dụng Coroutine để đặt lại màu về màu ban đầu sau khoảng thời gian nhất định
        StartCoroutine(ResetMoneyColor(moneyDisplay));
    }

    // Coroutine để đặt lại màu vùng hiển thị tiền về màu ban đầu sau khoảng thời gian nhất định
    private IEnumerator ResetMoneyColor(Text moneyDisplay)
    {
        // Chờ 0.5 giây
        yield return new WaitForSeconds(0.5f);

        // Thiết lập màu về màu ban đầu và đặt lại levelDisplay về giá trị ban đầu
        moneyDisplay.color = Color.yellow;
        levelDisplay.text = "LV" + towerAndIndexOfTower[selectedIndex].level;
    }

    /// <summary>
    /// Xóa tower
    /// </summary>
    public void ClickRemoveTower()
    {
        if (IsPreventCreateTower) return; // Nếu không có quyền tạo tower thì return
        Destroy(towerAndIndexOfTower[selectedIndex].gameObject); // Xóa tower
        buildTileAndIndexOfTower.FirstOrDefault(x => x.Value == selectedIndex).Key.tag = "buildSite"; // Đổi tag của build site thành build site để có thể tạo tower
        towerAndIndexOfTower.Remove(selectedIndex); // Xóa tower khỏi danh sách tower
        buttonUpgrade.SetActive(false); // Ẩn button upgrade
        buttonRemoveTower.SetActive(false); // Ẩn button remove tower
    }

    /// <summary>
    /// Thêm build site vào danh sách build site
    /// </summary>
    public void RegisterBuildSite(Collider2D buildTag)
    {
        buildTileAndIndexOfTower.Add(buildTag, indexOfTower);
    }

    /// <summary>
    /// Thêm tower vào danh sách tower
    /// </summary>
    public void RegisterTower(Tower tower)
    {
        towerAndIndexOfTower.Add(indexOfTower, tower);
    }

    /// <summary>
    /// Đổi tag của tất cả các build site thành build site để có thể tạo tower
    /// </summary>
    public void RenameTagsBuildSites()
    {
        foreach (Collider2D buildTag in buildTileAndIndexOfTower.Keys)
        {
            buildTag.tag = "buildSite";
        }
        buildTileAndIndexOfTower.Clear();
    }

    /// <summary>
    /// Xóa tất cả các tower
    /// </summary>
    public void DestroyAllTower()
    {
        foreach (Tower tower in towerAndIndexOfTower.Values)
        {
            if (tower == null) continue; // Nếu tower không tồn tại thì bỏ qua
            Destroy(tower.gameObject); // Xóa tower
        }
        towerAndIndexOfTower.Clear(); // Xóa danh sách tower
    }

    /// <summary>
    /// Di chuyển tower mới tới vị trí click chuột
    /// </summary>
    public void PlaceTower(RaycastHit2D hit)
    {
        /// Nếu con trỏ không nằm trên Tower Button GameObject && button chọn tower đã được nhấn
        /// Tạo tower mới tại vị trí click chuột
        if (towerButtonPressed != null && towerButtonPressed.TowerPrice <= GameManager.Instance.TotalMoney)
        {
            Tower newTower = Instantiate(towerButtonPressed.TowerObject); // Tạo tower mới
            newTower.transform.position = hit.transform.position; // Đặt tower mới tại vị trí click chuột
            BuyTower(towerButtonPressed.TowerPrice); // Trừ tiền khi tạo tower
            GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.TowerBuilt); // Phát âm thanh khi tạo tower
            RegisterTower(newTower); // Thêm tower vào danh sách tower
            DisableDragSprite(); // Ẩn sprite của tower
        }
    }

    /// <summary>
    /// Trừ tiền khi tạo tower
    /// </summary>
    public void BuyTower(int price)
    {
        GameManager.Instance.SubtractMoney(price);

    }
    /// <summary>
    /// Chọn tower để tạo tower mới khi click vào button chọn tower
    /// </summary>
    public void selectedTower(TowerButton towerSelected)
    {
        if (towerSelected.TowerPrice <= GameManager.Instance.TotalMoney && !IsPreventCreateTower)
        {
            towerButtonPressed = towerSelected;
            EnableDragSprite(towerSelected.DragSprite);
        }
    }

    /// <summary>
    /// Hiển thị sprite của tower theo vị trí của chuột
    /// </summary>
    public void FollowMouse()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector2(transform.position.x, transform.position.y);
    }

    /// <summary>
    /// Cho phép hiển thị sprite của tower
    /// </summary>
    public void EnableDragSprite(Sprite sprite)
    {
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprite; //Set sprite to the one we passed in the parameter
        spriteRenderer.sortingOrder = 10;
    }

    /// <summary>
    /// Ẩn sprite của tower
    /// </summary>
    public void DisableDragSprite()
    {
        spriteRenderer.enabled = false;
    }
}
