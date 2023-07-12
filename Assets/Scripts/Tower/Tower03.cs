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
        timer.Duration = TimeBetweenAttacks; // thiết lập thời gian giữa 2 lần tấn công
        // nếu enemy gần nhất trong tầm tấn công và nó trong tầm tấn công của chúng ta, thiết lập enemy đó làm mục tiêu
        if (targetEnemy == null || targetEnemy.IsDead) // nếu enemy đang bị chết hoặc không có enemy nào trong tầm tấn công
        {
            Enemy03 closestEnemy = GetClosestEnemyInRange(); // lấy enemy gần nhất trong tầm tấn công
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

    /// <summary>
    /// Di chuyển projectile đến enemy (bắn ra từ tower)
    /// </summary>
    IEnumerator MoveProjectile(Projectile projectile)
    {
        // nêu enemy chưa chết và khoảng cách từ projectile đến enemy > 0.20f
        while (getTargetDistance(targetEnemy) > 0.20f && projectile != null && targetEnemy != null)
        {
            if (targetEnemy == null || targetEnemy.IsDead)
            {
                break;
            }
            var dir = targetEnemy.transform.localPosition - transform.localPosition;               // hướng của projectile
            var angleDirection = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;                         // Góc của projectile
            projectile.transform.rotation = Quaternion.AngleAxis(angleDirection, Vector3.forward);  // Xoay projectile
            projectile.transform.localPosition = Vector2.MoveTowards(projectile.transform.localPosition, targetEnemy.transform.localPosition, 5f * Time.deltaTime); //Move Projectile
            yield return null; // wait 1 frame
        }
        if (projectile != null || targetEnemy == null)
        {
            Destroy(projectile); // destroy projectile
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
    /// <summary>
    /// Lấy danh sách enemy trong tầm tấn công
    /// </summary>
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
    /// <summary>
    /// Tìm enemy gần nhất trong tầm tấn công
    /// </summary>
    protected Enemy03 GetClosestEnemyInRange()
    {
        Enemy03 closestEnemy = null; // enemy gần nhất
        float smallestDistance = float.PositiveInfinity; // khoảng cách nhỏ nhất
        foreach (Enemy03 enemy in GetEnemiesInRange())
        {
            if (Vector2.Distance(transform.localPosition, enemy.transform.localPosition) < smallestDistance)
            {
                // nếu khoảng cách từ enemy đến chúng ta nhỏ hơn khoảng cách nhỏ nhất
                smallestDistance = Vector2.Distance(transform.localPosition, enemy.transform.localPosition);
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }
}
