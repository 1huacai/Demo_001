using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EmojImage : MonoBehaviour
{
    Emoj emoj;
    public Emoj Emoj
    {
        set
        {
            emoj = value;
            SetImage();
        }
        get
        {
            return emoj;
        }
    }

    Image image;
    Image cachedImage
    {
        get
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }
            return image;
        }
    }

    void SetImage()
    {
        // 可以做帧动画
        cachedImage.sprite = emoj.spriteList[0];
        cachedImage.SetNativeSize();
        enabled = emoj.spriteList.Length > 1;
    }
    
    private void Update()
    {
        if (emoj.spriteList.Length > 0)
        {
            var index = (int)(Time.time / emoj.speed) % emoj.spriteList.Length;
            cachedImage.sprite = emoj.spriteList[index];
            //cachedImage.SetNativeSize();
        }
    }
}
