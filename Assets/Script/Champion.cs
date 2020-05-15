using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all the stats and information of a champion character
/// </summary>
[CreateAssetMenu(fileName = "DefaultChampion", menuName = "Champion", order = 1)]
public class Champion : ScriptableObject
{
    //预制体
    public GameObject prefab;

    //头像
    public Sprite sprite;

    //子弹
    public GameObject attackProjectile;

    //角色名
    public string uiname;

    //价格
    public int cost;

    //数量
    public int number;

    //生命值
    public int health = 100;

    //伤害
    public float damage = 10;

    //攻击距离
    public float attackRange = 1;

    //发现距离
    public float findRange = 100;
}


