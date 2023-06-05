﻿using UnityEngine;

public enum ProjectileType
{
    rock,
    arrow,
    fireball
};

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private int attackStrength;

    [SerializeField]
    private ProjectileType projectileType;

    public int AttackStrength
    {
        get { return attackStrength; }
    }

    public ProjectileType ProjectileType
    {
        get { return projectileType; }
    }

    private void Update()
    {
        Destroy(gameObject, 3);
    }
}
