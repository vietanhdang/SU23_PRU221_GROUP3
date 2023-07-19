using UnityEngine;

public enum ProjectileType
{
    rock, // Đá
    arrow, // Mũi tên
    fireball // Bóng lửa
};

public class Projectile : MonoBehaviour
{
    [SerializeField]
    public int attackStrength; // Sức mạnh của projectile

    [SerializeField]
    private ProjectileType projectileType; // Loại projectile

    public int AttackStrength
    {
        get { return attackStrength; }
    }

    public int ProjectileLevel { get; set; } = 1;

    public ProjectileType ProjectileType
    {
        get { return projectileType; }
    }

    private void Update()
    {
        Destroy(gameObject, 3);
    }
}
