using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCollectible : ACollectible {

    protected override void OnCollect()
    {
        if (Player.Instance.life < Player.Instance.lifeMax)
        {
            Player.Instance.life++;
        }
    }
}
