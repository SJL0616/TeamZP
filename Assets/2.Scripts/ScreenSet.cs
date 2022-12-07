using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSet : MonoBehaviour
{
    public int[] size;
    public float scale;
    public RectTransform[] rect;

    // Start is called before the first frame update
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        rect[0].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size[0] * scale);
        rect[0].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size[1] * scale);

        rect[1].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size[0]);
        rect[1].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size[1]);

    }

    // Update is called once per frame
    void Update()
    {
        size[0] = Screen.width;
        size[1] = Screen.height;
    }

}
