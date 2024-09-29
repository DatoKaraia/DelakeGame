using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Properties;
using System;

public class PlayerWeapon : MonoBehaviour
{
    public PlayerAttack[] lightAttacks;
    public PlayerAttack[] heavyAttacks;
    public float weaponDamage;
    public int weaponClass;
    public bool canLink;

    [NonSerialized] public float currentDamage;
}

