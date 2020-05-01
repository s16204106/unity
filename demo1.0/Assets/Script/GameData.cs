using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores basic Game Data
/// </summary>
public class GameData : MonoBehaviour
{
    ///Store all available champion, all champions must be assigned from the Editor to the Script GameObject
    public Champion[] championsArray;
    public GameObject[] cameraPoint;
    public GameObject[] cameraPoint2;
    public GameObject[] camp;
    public door[] doors;
}
