using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Properties;

public class PlayerAttack : MonoBehaviour
{
    public string attackAniName;
    public AnimationClip attackClip;
    public PlayerAttackCollider[] attackColliders;
    public bool[] cancelableFrames;
    public bool canCharge;
    public string chargeAniName;
    public int chargeFrames;
}
