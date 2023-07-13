using ResourceLoad;
using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Collider2D描述不规则碰撞检测
/// </summary>
[ExecuteInEditMode]
public class AorImage : Image, IGrayMember
{

    public static Action<string, AorImage, Action<AorImage,long>> LoadAorImage;

    public bool CanRaycast = true;
    private Collider2D collider;
    private long matRefID;
    private long spriteRefID;
    private string m_currenPath = string.Empty;
    private string m_currentMaterialPath = string.Empty;
    private Action<AorImage> m_callBack;
    private bool m_processing = false;

    public void LoadImage(string path, Action<AorImage> callBack = null, string materialPath = "")
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


        if (path == m_currenPath && materialPath == m_currentMaterialPath)
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
        m_currenPath = path;
        m_currentMaterialPath = materialPath;

        if (null != LoadAorImage)
        {
            LoadAorImage(path, this, (img, spriteRefID) =>
            {
                this.spriteRefID = spriteRefID;
                m_processing = false;
                Alpha = 1;
                if (null != m_callBack)
                {
                    m_callBack(img);
                }


            });
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


    /// <summary>
    /// 变灰
    /// </summary>
    /// <param name="button"></param>
    /// <param name="bo"></param>
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

    protected override void Awake()
    {
        base.Awake();

        if (sprite == null)
        {
            this.Alpha = 0;
        }



    }




    protected override void Start()
    {

        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            setCollider(collider2D);
        }

        base.Start();
    }


    public float Alpha
    {
        get
        {
            return color.a;
        }
        set
        {
            Color n = color;
            n.a = Mathf.Clamp(value, 0, 1);
            color = n;
        }
    }

    public void setCollider(Collider2D collider2D)
    {
        collider = collider2D;
    }

    override public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (!CanRaycast)
        {
            return false;
        }
        else if (collider != null)
        {
            var worldPoint = Vector3.zero;
            var isInside = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform,
                sp,
                eventCamera,
                out worldPoint
            );
            if (isInside)
                isInside = collider.OverlapPoint(worldPoint);
            return isInside;
        }
        else
        {
            return base.IsRaycastLocationValid(sp, eventCamera);
        }
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        m_callBack = null;
        ResourcesManager.Instance.Release(matRefID);
        ResourcesManager.Instance.Release(spriteRefID);
    }
}
