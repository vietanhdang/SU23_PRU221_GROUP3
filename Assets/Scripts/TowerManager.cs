using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

public class TowerManager : Singleton<TowerManager>
{
	public TowerButton towerButtonPressed { get; set; }
	private SpriteRenderer spriteRenderer;  //Setting image to our tower
	private List<Tower> TowerList = new List<Tower>();
	private List<Collider2D> BuildList = new List<Collider2D>();
	private Collider2D buildTile;
	private Dictionary<Collider2D, int> buildTileAndIndexOfTower = new Dictionary<Collider2D, int>();
	private Dictionary<int, Tower> towerAndIndexOfTower = new Dictionary<int, Tower>();
	private int indexOfTower;
	private Collider2D hitObject;
	private GameObject buttonUpgrade;
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
		
	}

	// Update is called once per frame
	void Update()
	{
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
				buildTile.tag = "buildSiteFull";     //This prevents us from stacking towers ontop of each other.
				RegisterBuildSite(buildTile);
				PlaceTower(hit);
				indexOfTower++;
			}

			try
			{

				if (hit.collider.tag == "buildSiteFull")
				{
					hitObject = hit.collider;
					int index = buildTileAndIndexOfTower.FirstOrDefault(x => x.Key.Equals(hitObject)).Value;
					selectedIndex = index;
					Tower tower = towerAndIndexOfTower[index];
					if (!tower.isSelected)
					{
						tower.isSelected = true;
						if (!tower.firstPlace)
						{
							Vector3 selectedTower = tower.gameObject.transform.position;
							selectedTower.y -= 0.2f;
							towerAndIndexOfTower[index].gameObject.transform.position = selectedTower;
							buttonUpgrade.SetActive(false);
						}
					}
					else if (tower.isSelected)
					{
						Vector3 selectedTower = tower.gameObject.transform.position;
						selectedTower.y += 0.2f;
						towerAndIndexOfTower[index].gameObject.transform.position = selectedTower;
						tower.isSelected = false;
						tower.firstPlace = false;
						buttonUpgrade.SetActive(true);
					}
					levelDisplay.text = "LV" + tower.level;
				}
			}
			catch (Exception ex)
			{

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
		Tower tower = towerAndIndexOfTower[selectedIndex];
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

	public void RegisterBuildSite(Collider2D buildTag)
	{
		//BuildList.Add(buildTag);
		buildTileAndIndexOfTower.Add(buildTag, indexOfTower);
	}

	public void RegisterTower(Tower tower)
	{
		//TowerList.Add(tower);
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
		if (towerSelected.TowerPrice <= GameManager.Instance.TotalMoney)
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
