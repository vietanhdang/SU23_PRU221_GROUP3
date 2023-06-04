using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Reflection;

public class TowerManager : Singleton<TowerManager>
{
    public TowerButton towerButtonPressed { get; set; }
    private SpriteRenderer spriteRenderer;  //Setting image to our tower
    public bool IsPreventCreateTower { get; set; } = false;
    private List<Tower> TowerList = new List<Tower>();
    private List<Collider2D> BuildList = new List<Collider2D>();
    private Collider2D buildTile;
    private Dictionary<Collider2D, int> buildTileAndIndexOfTower = new Dictionary<Collider2D, int>();
    private Dictionary<int, Tower> towerAndIndexOfTower = new Dictionary<int, Tower>();
    private int indexOfTower;
    private Collider2D hitObject;
    private GameObject buttonUpgrade;
    private GameObject buttonRemoveTower;
    private int selectedIndex;
    Text levelDisplay;
    // Use this for initialization
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        buildTile = GetComponent<Collider2D>();
        hitObject = GetComponent<Collider2D>();
        spriteRenderer.enabled = false;
        indexOfTower = 1;
        buttonUpgrade = GameObject.FindGameObjectWithTag("buttonUpgrade");
        buttonUpgrade.SetActive(false);
        buttonRemoveTower = GameObject.FindGameObjectWithTag("removeTower");
        buttonRemoveTower.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (IsPreventCreateTower) return;
        if (Input.GetMouseButtonDown(0))
        {
            //worldPoint is the position of the mouse click.
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            /* Ray Cast involves intersecting a ray with the object in an environment.
             * The ray cast tells you what objects in the environment the ray runs into.
             * and may return additional information as well, such as intersection point
             */
            //Finding the worldPoint of where we click, from Vector2.zero (which is buttom left corner)
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            //Check to see if mouse press location is on buildSites
            if (hit.collider.tag == "buildSite")
            {
                buildTile = hit.collider;
                buildTile.tag = "buildSiteFull";
                if (buildTileAndIndexOfTower.ContainsKey(buildTile))
                {
                    buildTileAndIndexOfTower[buildTile] = indexOfTower;
                }
                else
                {
                    RegisterBuildSite(buildTile);
                }
                PlaceTower(hit);
                indexOfTower++;
            }

            if (hit.collider.tag == "buildSiteFull")
            {
                hitObject = hit.collider;
                int index = buildTileAndIndexOfTower.FirstOrDefault(x => x.Key.Equals(hitObject)).Value;
                Tower tower;
                towerAndIndexOfTower.TryGetValue(index, out tower);
                if (tower == null) return;

                if (!tower.isSelected)
                {
                    foreach (Tower otherTower in towerAndIndexOfTower.Values)
                    {
                        if (otherTower.isSelected)
                        {
                            Vector3 deselectedTowerPosition = otherTower.gameObject.transform.position;
                            deselectedTowerPosition.y -= 0.2f;
                            otherTower.gameObject.transform.position = deselectedTowerPosition;
                            otherTower.isSelected = false;
                            otherTower.firstPlace = false;
                        }
                    }

                    Vector3 selectedTowerPosition = tower.gameObject.transform.position;
                    selectedTowerPosition.y += 0.2f;
                    tower.gameObject.transform.position = selectedTowerPosition;
                    tower.isSelected = true;
                    tower.firstPlace = false;

                    buttonUpgrade.SetActive(true);
                    buttonRemoveTower.SetActive(true);
                }
                else
                {
                    Vector3 selectedTowerPosition = tower.gameObject.transform.position;
                    selectedTowerPosition.y -= 0.2f;
                    tower.gameObject.transform.position = selectedTowerPosition;
                    tower.isSelected = false;
                    tower.firstPlace = false;

                    buttonUpgrade.SetActive(false);
                    buttonRemoveTower.SetActive(false);
                }

                if (levelDisplay)
                {
                    levelDisplay.text = "LV" + tower.level;
                }

                selectedIndex = index;
            }

            // Click outside of tower (to ground) then deselect all towers selected
            if (selectedIndex != 0 && hit.collider.tag == "ground")
            {
                foreach (Tower otherTower in towerAndIndexOfTower.Values)
                {
                    if (otherTower.isSelected)
                    {
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

        //When we have a sprite enabled, have it follow the mouse (I.E - Placing a Tower)
        if (spriteRenderer.enabled)
        {
            FollowMouse();
        }
    }

    public void ClickUpgrade()
    {
        if (IsPreventCreateTower) return;
        Tower tower;
        towerAndIndexOfTower.TryGetValue(selectedIndex, out tower);
        if (tower == null) return;
        levelDisplay = GameObject.FindWithTag("upgradePrice").GetComponent<Text>();
        if (tower.level < 3 && GameManager.Instance.TotalMoney >= 10)
        {
            GameManager.Instance.TotalMoney -= 10;
            tower.AttackRange *= 1.2f;
            tower.TimeBetweenAttacks -= tower.TimeBetweenAttacks * 0.2f;
            Debug.Log("index: " + selectedIndex + ", attack range: " + tower.AttackRange + ", time: " + tower.TimeBetweenAttacks);
            tower.level += 1;
            levelDisplay.text = "LV" + tower.level;
        }
        else if (tower.level >= 3)
        {
            levelDisplay.text = "Max";
        }
        else if (GameManager.Instance.TotalMoney < 10)
        {
            levelDisplay.text = "$";
        }
        towerAndIndexOfTower[selectedIndex] = tower;
    }

    public void ClickRemoveTower()
    {
        if (IsPreventCreateTower) return;
        Destroy(towerAndIndexOfTower[selectedIndex].gameObject);
        buildTileAndIndexOfTower.FirstOrDefault(x => x.Value == selectedIndex).Key.tag = "buildSite";
        towerAndIndexOfTower.Remove(selectedIndex);
        buttonUpgrade.SetActive(false);
        buttonRemoveTower.SetActive(false);
    }

    public void RegisterBuildSite(Collider2D buildTag)
    {
        buildTileAndIndexOfTower.Add(buildTag, indexOfTower);
    }

    public void RegisterTower(Tower tower)
    {
        towerAndIndexOfTower.Add(indexOfTower, tower);
    }

    public void RenameTagsBuildSites()
    {
        foreach (Collider2D buildTag in buildTileAndIndexOfTower.Keys)
        {
            buildTag.tag = "buildSite";
        }
        buildTileAndIndexOfTower.Clear();
    }

    public void DestroyAllTower()
    {
        foreach (Tower tower in towerAndIndexOfTower.Values)
        {
            Destroy(tower.gameObject);
        }
        towerAndIndexOfTower.Clear();
    }

    //Place new tower on the mouse click location
    public void PlaceTower(RaycastHit2D hit)
    {
        //If the pointer is not over the Tower Button GameObject && the tower button has been pressed
        //Created new tower at the click location
        if (towerButtonPressed != null && towerButtonPressed.TowerPrice <= GameManager.Instance.TotalMoney)
        {
            Tower newTower = Instantiate(towerButtonPressed.TowerObject);
            newTower.transform.position = hit.transform.position;
            BuyTower(towerButtonPressed.TowerPrice);
            GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.TowerBuilt);
            RegisterTower(newTower);
            DisableDragSprite();
        }
    }

    public void BuyTower(int price)
    {
        GameManager.Instance.SubtractMoney(price);

    }

    public void selectedTower(TowerButton towerSelected)
    {
        if (towerSelected.TowerPrice <= GameManager.Instance.TotalMoney && !IsPreventCreateTower)
        {
            towerButtonPressed = towerSelected;
            EnableDragSprite(towerSelected.DragSprite);
        }
    }

    public void FollowMouse()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector2(transform.position.x, transform.position.y);
    }

    public void EnableDragSprite(Sprite sprite)
    {
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprite; //Set sprite to the one we passed in the parameter
        spriteRenderer.sortingOrder = 10;
    }

    public void DisableDragSprite()
    {
        spriteRenderer.enabled = false;
    }
}
