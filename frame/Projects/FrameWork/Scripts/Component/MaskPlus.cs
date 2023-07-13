using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MaskPlus : Mask
{
    Camera cam;
    protected override void Start()
    {
        base.Start();
        ResetShaderMaskClip();
    }

    public void ResetShaderMaskClip()
    {
        Vector3[] points = new Vector3[4];
        (transform as RectTransform).GetWorldCorners(points);

        if (cam == null)
            cam = GameObject.Find("AorUICamera#").GetComponent<Camera>();

        float x;
        float y;
        float x1;
        float y1;

        Vector3 scPos = cam.WorldToScreenPoint(points[0]);
        x = scPos.x;
        y = scPos.y;
        x1 = scPos.x;
        y1 = scPos.y;

        for (int i = 0; i < points.Length; i++)
        {
            scPos = cam.WorldToScreenPoint(points[i]);
            //取最小xy
            x = scPos.x < x ? scPos.x : x;
            y = scPos.y < y ? scPos.y : y;

            //取最大
            x1 = scPos.x > x1 ? scPos.x : x1;
            y1 = scPos.y > y1 ? scPos.y : y1;


        }


        Vector4 normalize = new Vector4(x / Screen.width, y / (float)Screen.height, x1 / (float)Screen.width, y1 / (float)Screen.height);

        Shader.SetGlobalVector("_MaskClip", normalize);

    }




}
