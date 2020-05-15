using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FPS显示
/// </summary>
public class ShowFPS : MonoBehaviour
{

    /// <summary>
    /// 更新间隔
    /// </summary>
    private float updateInterval = 0.5f;

    /// <summary>
    /// 累加值
    /// </summary>
    private float accum = 0;

    /// <summary>
    /// 帧频
    /// </summary>
    private int frames = 0;

    /// <summary>
    /// 计时数值
    /// </summary>
    private float timeLeft;

    /// <summary>
    /// FPS显示字符
    /// </summary>
    [HideInInspector]
    public string strFPSValue;


    // Start is called before the first frame update
    void Start()
    {
        timeLeft = updateInterval;
    }

    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;  //每秒执行了多少帧
        ++frames;                                  // 帧频
        if (timeLeft <= 0.0)
        {
            float fps = accum / frames;             //多次后取平均值
            string formate = System.String.Format("{0:F2} FPS", fps);
            strFPSValue = formate;
            timeLeft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }
}
