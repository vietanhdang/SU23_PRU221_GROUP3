using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower03 : MonoBehaviour
{
    public float TimeBetweenAttacks { get; set; }    //AKA - Attack Speed

    public float AttackRange { get; set; }      //AKA - Attack Radius
    [SerializeField]
    public Projectile projectile;      //Type of Projectile
    protected Enemy03 targetEnemy = null;
    protected bool isAttacking = false;
    public bool isSelected = false;
    public bool firstPlace = true;
    public int level = 1;


    Timer timer;
    protected void Init(float timeBetweenAttacks, float attackRange)
    {
        TimeBetweenAttacks = timeBetweenAttacks;
        AttackRange = attackRange;
    }
    // Use this for initialization
    public virtual void Start()
    {
        timer = gameObject.AddComponent<Timer>();
        timer.Duration = TimeBetweenAttacks;
        timer.Run();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        timer.Duration = TimeBetweenAttacks;
        //If our closest enemy in range and if its within our attackRange, set our target enemy to the closest enemy in range.
        if (targetEnemy == null || targetEnemy.IsDead)
        {
            Enemy03 closestEnemy = GetClosestEnemyInRange();
            if (closestEnemy != null && Vector2.Distance(transform.localPosition, closestEnemy.transform.position) <= AttackRange)
            {
                targetEnemy = closestEnemy;
            }
        }
        else
        {
            if (timer.Finished && isAttacking == true)
            {
                Attack();
                timer.Run();
            }
            else
            {
                isAttacking = true;
            }
            //If enemy gets out of attack range, then that enemy can no longer be targeted
            if (Vector2.Distance(transform.position, targetEnemy.transform.position) > AttackRange)
            {
                targetEnemy = null;
            }
        }

    }

    public virtual void Attack()
    {
        isAttacking = false;
        Projectile newProjectile = Instantiate(projectile) as Projectile;
        newProjectile.transform.localPosition = transform.position;

        if (newProjectile.ProjectileType == ProjectileType.arrow)
        {
            GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Arrow);
        }
        else if (newProjectile.ProjectileType == ProjectileType.fireball)
        {
            GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Fireball);
        }
        else if (newProjectile.ProjectileType == ProjectileType.rock)
        {
            GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Rock);
        }
        //If we have a target enemy, start a coroutine to shoot projectile to target enemy
        if (targetEnemy == null)
        {
            Destroy(newProjectile);
        }
        else
        {
            StartCoroutine(MoveProjectile(newProjectile));
        }
    }

    ///Move Projectile to Target Enemy
    IEnumerator MoveProjectile(Projectile projectile)
    {
        while (getTargetDistance(targetEnemy) > 0.20f && projectile != null && targetEnemy != null)
        {
            if (targetEnemy == null || targetEnemy.IsDead)
            {
                break;
            }
            var dir = targetEnemy.transform.localPosition - transform.localPosition;
            var angleDirection = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;                         //Angle of the projectile
            projectile.transform.rotation = Quaternion.AngleAxis(angleDirection, Vector3.forward);  //Rotation of projectile
            projectile.transform.localPosition = Vector2.MoveTowards(projectile.transform.localPosition, targetEnemy.transform.localPosition, 5f * Time.deltaTime); //Move Projectile
            yield return null;
        }
        if (projectile != null || targetEnemy == null)
        {
            Destroy(projectile);
        }
    }

    ///Get the current target's distance

    protected float getTargetDistance(Enemy03 enemy)

    {
        if (enemy == null)
        {
            enemy = GetClosestEnemyInRange();
            if (enemy == null)
            {
                return 0f;
            }
        }
        return Mathf.Abs(Vector2.Distance(transform.localPosition, enemy.transform.localPosition));
    }
    ///Get Enemies in Attack Range
    protected List<Enemy03> GetEnemiesInRange()
    {
        List<Enemy03> enemiesInRange = new List<Enemy03>();
        if (GameManager.Instance == null || GameManager.Instance.EnemyList == null)
        {
            return enemiesInRange;
        }
        //Check if enemies are in range
        foreach (Enemy03 enemy in GameManager.Instance.EnemyList)
        {
            if (enemy != null && Vector2.Distance(transform.localPosition, enemy.transform.localPosition) <= AttackRange && !enemy.IsDead)
            {
                enemiesInRange.Add(enemy);
            }
        }
        return enemiesInRange;
    }
    ///Get Closest Enemy - Foreach enemy in range, get the closest enemy
    protected Enemy03 GetClosestEnemyInRange()
    {
        Enemy03 closestEnemy = null;
        float smallestDistance = float.PositiveInfinity;
        foreach (Enemy03 enemy in GetEnemiesInRange())
        {
            if (Vector2.Distance(transform.localPosition, enemy.transform.localPosition) < smallestDistance)
            {
                smallestDistance = Vector2.Distance(transform.localPosition, enemy.transform.localPosition);
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }
}
