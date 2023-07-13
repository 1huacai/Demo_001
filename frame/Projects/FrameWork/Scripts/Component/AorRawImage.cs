using ResourceLoad;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Image实现使用PolygonCollider2D描述不规则碰撞检测
/// </summary>
[ExecuteInEditMode]
public class AorRawImage : RawImage, IGrayMember
{

    public static Action<string, AorRawImage, Action<AorRawImage,long>> LoadAorRawImage;
    public static Action<string, AorRawImage, Action<long>> LoadAorRawImageMat;
    public string m_currentPath = string.Empty;
    private string m_currentMaterialPath = string.Empty;
    private Action<AorRawImage> m_callBack;

    private long matRefID;
    private long textureRefID;
    private bool m_processing = false;

    public void LoadImage(string path, Action<AorRawImage> callBack = null, string materialPath = "")
    {

        Alpha = 0;
        m_callBack = callBack;

        if (string.IsNullOrEmpty(path))
        {
            if (Application.isEditor)
            {
                //Log.Warning("The path can not be null.");
            }

            if (null != m_callBack)
            {
                m_callBack(null);
            }

            return;
        }

        if (m_currentPath == path && m_currentMaterialPath == materialPath)
        {
            if (!m_processing)
            {
                Alpha = 1;
                if (null != m_callBack)
                {
                    m_callBack(this);
                }
            }
            return;
        }


        m_processing = true;
        m_currentPath = path;
      
        m_currentMaterialPath = materialPath;

        if (!string.IsNullOrEmpty(materialPath))
        {
            if (null != LoadAorRawImageMat)
            {
                LoadAorRawImageMat(materialPath, this, (matRefID) =>
                {
                    this.matRefID = matRefID;
                    if (null != LoadAorRawImage)
                    {
                        LoadAorRawImage(path, this, (img,textureRefID) =>
                        {
                            this.textureRefID = textureRefID;
                            m_processing = false;
                            Alpha = 1;
                            if (null != m_callBack)
                            {
                                m_callBack(img);
                            }
                        });

                    }

                });
            }
        }
        else
        {
            if (null != LoadAorRawImage)
            {
                LoadAorRawImage(path, this, (img, textureRefID) =>
                {
                    this.textureRefID = textureRefID;
                    m_processing = false;
                    Alpha = 1;
                    if (null != m_callBack)
                    {
                        m_callBack(img);
                    }
                });

            }

        }


    }

    protected override void Awake()
    {
        base.Awake();
        if (texture == null)
        {
            Alpha = 0;
        }

    }



    private bool _isGray;

    public bool IsGray
    {
        get
        {
            return _isGray;
        }
    }

    private Color oldColor;

    public void SetGray(bool isGray)
    {
        if (_isGray == isGray)
            return;

        switch (material.shader.name)
        {
            case "UI/Default":

                if (GrayManager.DefImageMaterial == null)
                {
                    Debug.LogWarning("AorRawImage.setGray :: can not find the Shader<Custom/RawImage/RawImage Gray>");
                    return;
                }
                material = GrayManager.DefImageMaterial;
                SetMaterialDirty();
                break;

        }


        _isGray = isGray;
        if (isGray)
        {
            oldColor = color;
            color = new Color(0, 0, 0, color.a);
        }
        else
        {
            color = new Color(oldColor.r, oldColor.g, oldColor.b, Alpha);
        }


    }


    public float Alpha
    {
        get { return color.a; }
        set
        {
            Color n = color;
            n.a = Mathf.Clamp(value, 0, 1);
            color = n;


        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        m_callBack = null;
        ResourcesManager.Instance.Release(matRefID);
        ResourcesManager.Instance.Release(textureRefID);
    }

}
