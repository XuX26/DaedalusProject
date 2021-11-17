using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCollectible : ACollectible {

    protected override void OnCollect()
    {
        Player.Instance.life++;
    }
}
