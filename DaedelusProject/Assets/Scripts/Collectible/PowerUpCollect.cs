using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpCollect : ACollectible
{
    protected override void OnCollect()
    {
        if (Player.Instance.life < Player.Instance.lifeMax)
        {
            Player.Instance.attackPower = 3;
        }
    }
}
