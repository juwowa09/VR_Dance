using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI text;
    private float time;
    void Start()
    {
        time = 0f;
        text.text = time.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        text.text = time.ToString("F2");
    }
}
