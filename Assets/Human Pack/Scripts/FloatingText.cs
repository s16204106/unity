using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Floating text display damage numbers champions take
/// </summary>
public class FloatingText : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Vector3 moveDirection;
    private float timer = 0;

    ///浮动速度
    public float speed = 3;

    ///渐隐时间
    public float fadeOutTime = 1f;

    /// Start is called before the first frame update
    void Start()
    {

    }

    /// Update is called once per frame
    void Update()
    {
        this.transform.position = this.transform.position + moveDirection * speed * Time.deltaTime;

        timer = timer + Time.deltaTime;
        float fade = (fadeOutTime - timer) / fadeOutTime;

        canvasGroup.alpha = fade;

        if (fade <= 0)
            Destroy(this.gameObject);
    }
    /// <summary>
    /// 初始化冒字
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="v"></param>
    public void Init(Vector3 startPosition, float v)
    {
        this.transform.position = startPosition;

        canvasGroup = this.GetComponent<CanvasGroup>();
        //将浮点数四舍五入为整型
        this.GetComponent<Text>().text = Mathf.Round(v).ToString();
        //确保冒字有浮动，不重叠。
        moveDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1, Random.Range(-0.5f, 0.5f)).normalized;
    }
}
