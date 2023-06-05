using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private Transform exitPoint; // Điểm cuối của enemy
    [SerializeField]
    private Transform[] wayPoints; // Mảng các điểm đến của enemy
    [SerializeField]
    private float navigationUpdate; // Tốc độ di chuyển của enemy
    [SerializeField]
    private int healthPoints; // Máu của enemy
    [SerializeField]
    private int rewardAmount; // Số tiền nhận được khi giết enemy

    private int target = 0; // Điểm đến của enemy
    private Transform enemy; // Transform của enemy
    private Collider2D enemyCollider; // Collider của enemy
    private Animator anim; // Animator của enemy
    private float navigationTime = 0; // Thời gian di chuyển của enemy
    private bool isDead = false; // Kiểm tra enemy đã chết hay chưa

    public bool IsDead
    {
        get { return isDead; }
    }

    void Start()
    {
        enemy = GetComponent<Transform>(); // lấy transform của enemy
        enemyCollider = GetComponent<Collider2D>(); // lấy collider của enemy
        anim = GetComponent<Animator>(); // lấy animator của enemy
        GameManager.Instance.RegisterEnemy(this); // thêm enemy vào danh sách enemy
    }

    void Update()
    {
        // nếu enemy chưa chết và mảng wayPoints khác null
        if (wayPoints != null && !isDead)
        {
            // Tính thời gian di chuyển của enemy = thời gian di chuyển cũ + thời gian thực
            navigationTime += Time.deltaTime;
            if (navigationTime > navigationUpdate)
            {
                // nếu enemy chưa đến điểm cuối cùng, di chuyển đến điểm tiếp theo
                if (target < wayPoints.Length)
                {
                    enemy.position = Vector2.MoveTowards(enemy.position, wayPoints[target].position, navigationTime);
                }
                else
                {
                    // nếu đã đến điểm cuối cùng, di chuyển đến điểm cuối
                    enemy.position = Vector2.MoveTowards(enemy.position, exitPoint.position, navigationTime);
                }
                navigationTime = 0; // reset thời gian di chuyển
            }
        }
    }

    /// <summary>
    /// Kiểm tra enemy đến điểm checkpoint hay điểm cuối cùng
    /// </summary>
    void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.tag == "checkpoint") target += 1; // nếu enemy đến checkpoint, tăng index và di chuyển đến checkpoint tiếp theo
        else if (collider2D.tag == "Finish")
        {
            // nếu enemy đến điểm cuối cùng, di chuyển đến điểm cuối
            GameManager.Instance.RoundEscaped += 1; // tăng số lượng enemy chạy thoát
            GameManager.Instance.TotalEscape += 1; // tăng số lượng enemy chạy thoát
            GameManager.Instance.UnregisterEnemy(this); // xóa enemy khỏi danh sách enemy
            GameManager.Instance.IsWaveOver(); // kiểm tra xem wave đã kết thúc chưa
        }
        else if (collider2D.tag == "projectile")
        {
            Projectile newP = collider2D.gameObject.GetComponent<Projectile>();
            try
            {
                if (newP != null) EnemyHit(newP.AttackStrength); // nếu enemy bị trúng đạn, giảm máu và xóa đạn
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning("Projectile component not found on the colliding object with tag 'projectile'." + ex.Message);
            }
            Destroy(collider2D.gameObject);
        }
    }
    /// <summary>
    /// Giảm máu của enemy khi bị trúng đạn
    /// </summary>
    public void EnemyHit(int hitPoints)
    {
        try
        {
            if (healthPoints - hitPoints > 0)
            {
                // nếu máu còn lớn hơn 0, giảm máu và chạy animation Hurt
                healthPoints -= hitPoints;
                anim.Play("Hurt");
                GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Hit);
            }
            else
            {
                // nếu máu nhỏ hơn 0, chạy animation Die
                anim.SetTrigger("didDie");
                Die();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    /// <summary>
    /// Xóa enemy khỏi danh sách enemy và thêm tiền vào tài khoản
    /// </summary>
    public void Die()
    {
        isDead = true; // đánh dấu enemy đã chết
        enemyCollider.enabled = false; // tắt collider của enemy
        GameManager.Instance.TotalKilled += 1; // tăng số lượng enemy bị giết
        GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Death); // chạy âm thanh khi enemy chết
        GameManager.Instance.AddMoney(rewardAmount); // thêm tiền vào tài khoản
        GameManager.Instance.IsWaveOver(); // kiểm tra xem wave đã kết thúc chưa

        // xóa enemy khỏi danh sách enemy sau 3 giây
        Destroy(gameObject, 3);
    }
}
