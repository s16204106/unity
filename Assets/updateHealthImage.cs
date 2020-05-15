using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class updateHealthImage : MonoBehaviour
{
    Image healthbar;
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        healthbar = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        healthbar.fillAmount = float.Parse(text.text.Substring(0, text.text.Length-1)) / 100;
    }
}
