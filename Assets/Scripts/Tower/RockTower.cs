using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockTower : Tower
{
    // Start is called before the first frame update
    public override void Start()
    {
        Init(1, 2);
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
    public override void Attack()
    {
        base.Attack();
    }
}
