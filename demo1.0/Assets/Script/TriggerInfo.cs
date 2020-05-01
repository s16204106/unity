using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 帮助类->来识别地图上的玩家交互
/// </summary>
public class TriggerInfo : MonoBehaviourPun
{
    ///The type of the grid： 
    ///GRIDTYPE_OWN_INVENTORY = 0 
    ///GRIDTYPE_OPPONENT_INVENTORY = 1
    ///GRIDTYPE_HEXA_MAP = 2;
    public int gridType = -1;

    ///X position on the grid
    public int gridX = -1;

    ///Z position on the grid
    public int gridZ = -1;

}
