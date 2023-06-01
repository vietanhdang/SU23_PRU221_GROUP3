using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TowerManager : Singleton<TowerManager>
{
    public TowerButton towerButtonPressed { get; set; }
    private SpriteRenderer spriteRenderer;  //Setting image to our tower
    private List<Tower> TowerList = new List<Tower>();
    private List<Collider2D> BuildList = new List<Collider2D>();
    private Collider2D buildTile;

    // Use this for initialization
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        buildTile = GetComponent<Collider2D>();
        spriteRenderer.enabled = false;
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
            }
        }

        //When we have a sprite enabled, have it follow the mouse (I.E - Placing a Tower)
        if (spriteRenderer.enabled)
        {
            FollowMouse();
        }
    }


    public void RegisterBuildSite(Collider2D buildTag)
    {
        BuildList.Add(buildTag);
    }

    public void RegisterTower(Tower tower)
    {
        TowerList.Add(tower);
    }

    public void RenameTagsBuildSites()
    {
        foreach (Collider2D buildTag in BuildList)
        {
            buildTag.tag = "buildSite";
        }
        BuildList.Clear();
    }

    public void DestroyAllTower()
    {
        foreach (Tower tower in TowerList)
        {
            Destroy(tower.gameObject);
        }
        TowerList.Clear();
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
