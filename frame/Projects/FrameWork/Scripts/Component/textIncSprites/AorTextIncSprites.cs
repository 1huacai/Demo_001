using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using FrameWork;
using UnityEngine.UI;

/// <summary>
/// 图文混排Text组件. 支持Unity版本 5.6
/// Author : Aorition
/// 
/// 基本功能完成. 已知问题: 
///                         1.在编辑器中非运行状态下,通过Inspector更改文本对齐方式会引发错误.
///                         2.不支持MonoSwitch序列化
/// 
/// </summary>
public class AorTextIncSprites : MaskableGraphic, ILayoutElement, IPointerClickHandler
{

    #region Text组件源码复写

    [SerializeField]
    private FontData m_FontData = FontData.defaultFontData;

    [TextArea(3, 10)]
    [SerializeField]
    protected string m_Text = String.Empty;

    private TextGenerator m_TextCache;
    private TextGenerator m_TextCacheForLayout;

    protected static Material s_DefaultText = null;

    // We use this flag instead of Unregistering/Registering the callback to avoid allocation.
    [NonSerialized]
    private bool m_DisableFontTextureRebuiltCallback = false;

    public static Action<string, AorTextIncSprites> OnAwake = null;

    private bool m_bHasReplace = false;

    public string languageKey;

    protected AorTextIncSprites()
    {
        useLegacyMeshGeneration = false;
    }

    /// <summary>
    /// Get or set the material used by this Text.
    /// </summary>

    public TextGenerator cachedTextGenerator
    {
        get { return m_TextCache ?? (m_TextCache = m_Text.Length != 0 ? new TextGenerator(m_Text.Length) : new TextGenerator()); }
    }

    public TextGenerator cachedTextGeneratorForLayout
    {
        get { return m_TextCacheForLayout ?? (m_TextCacheForLayout = new TextGenerator()); }
    }

    /// <summary>
    /// Text's texture comes from the font.
    /// </summary>
    public override Texture mainTexture
    {
        get
        {
            if (font != null && font.material != null && font.material.mainTexture != null)
                return font.material.mainTexture;

            if (m_Material != null)
                return m_Material.mainTexture;

            return base.mainTexture;
        }
    }

    public void FontTextureChanged()
    {
        // Only invoke if we are not destroyed.
        if (!this)
        {
            AorFontUpdateTracker.UntrackText(this);
            return;
        }

        if (m_DisableFontTextureRebuiltCallback)
            return;

        cachedTextGenerator.Invalidate();

        if (!IsActive())
            return;

        // this is a bit hacky, but it is currently the
        // cleanest solution....
        // if we detect the font texture has changed and are in a rebuild loop
        // we just regenerate the verts for the new UV's
        if (CanvasUpdateRegistry.IsRebuildingGraphics() || CanvasUpdateRegistry.IsRebuildingLayout())
            UpdateGeometry();
        else
            SetAllDirty();
    }

    public Font font
    {
        get { return m_FontData.font; }
        set
        {
            if (m_FontData.font == value)
                return;

            AorFontUpdateTracker.UntrackText(this);

            m_FontData.font = value;

            AorFontUpdateTracker.TrackText(this);

            SetAllDirty();
        }
    }

    /// <summary>
    /// Text that's being displayed by the Text.
    /// </summary>
    [HideInInspector]
    public string originalText = string.Empty;

#if UNITY_2018_4_36
    [HideInInspector]
    public string textWithOutFull = string.Empty;
#endif
    public virtual string text
    {
        get { return m_Text; }
        set
        {
            if (String.IsNullOrEmpty(value))
            {
                if (String.IsNullOrEmpty(m_Text))
                    return;
                originalText = m_Text = "";
                //                SetVerticesDirty();
                SetAllDirty();
            }
            else if (originalText != value)
            {
                originalText = value;
#if UNITY_2018_4_36
                textWithOutFull = DeleteRichText_Color(originalText);
#endif
                m_Text = Regex.Replace(value, "<a[^>]*>|</a>", string.Empty, RegexOptions.IgnoreCase); ;
                SetAllDirty();
            }
            m_bHasReplace = true;
        }
    }

    /// <summary>
    /// Whether this Text will support rich text.
    /// </summary>

    public bool supportRichText
    {
        get { return m_FontData.richText; }
        set
        {
            if (m_FontData.richText == value)
                return;
            m_FontData.richText = value;

            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    /// <summary>
    /// Wrap mode used by the text.
    /// </summary>

    public bool resizeTextForBestFit
    {
        get { return m_FontData.bestFit; }
        set
        {
            if (m_FontData.bestFit == value)
                return;
            m_FontData.bestFit = value;
            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    public int resizeTextMinSize
    {
        get { return m_FontData.minSize; }
        set
        {
            if (m_FontData.minSize == value)
                return;
            m_FontData.minSize = value;

            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    public int resizeTextMaxSize
    {
        get { return m_FontData.maxSize; }
        set
        {
            if (m_FontData.maxSize == value)
                return;
            m_FontData.maxSize = value;

            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    /// <summary>
    /// Alignment anchor used by the text.
    /// </summary>

    public TextAnchor alignment
    {
        get { return m_FontData.alignment; }
        set
        {
            if (m_FontData.alignment == value)
                return;
            m_FontData.alignment = value;

            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    public int fontSize
    {
        get { return m_FontData.fontSize; }
        set
        {
            if (m_FontData.fontSize == value)
                return;
            m_FontData.fontSize = value;

            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    public HorizontalWrapMode horizontalOverflow
    {
        get { return m_FontData.horizontalOverflow; }
        set
        {
            if (m_FontData.horizontalOverflow == value)
                return;
            m_FontData.horizontalOverflow = value;

            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    public VerticalWrapMode verticalOverflow
    {
        get { return m_FontData.verticalOverflow; }
        set
        {
            if (m_FontData.verticalOverflow == value)
                return;
            m_FontData.verticalOverflow = value;

            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    public float lineSpacing
    {
        get { return m_FontData.lineSpacing; }
        set
        {
            if (m_FontData.lineSpacing == value)
                return;
            m_FontData.lineSpacing = value;

            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    /// <summary>
    /// Font style used by the Text's text.
    /// </summary>

    public FontStyle fontStyle
    {
        get { return m_FontData.fontStyle; }
        set
        {
            if (m_FontData.fontStyle == value)
                return;
            m_FontData.fontStyle = value;

            //                SetVerticesDirty();
            //                SetLayoutDirty();
            SetAllDirty();
        }
    }

    public float pixelsPerUnit
    {
        get
        {
            var localCanvas = canvas;
            if (!localCanvas)
                return 1;
            // For dynamic fonts, ensure we use one pixel per pixel on the screen.
            if (!font || font.dynamic)
                return localCanvas.scaleFactor;
            // For non-dynamic fonts, calculate pixels per unit based on specified font size relative to font object's own font size.
            if (m_FontData.fontSize <= 0 || font.fontSize <= 0)
                return 1;
            return font.fontSize / (float)m_FontData.fontSize;
        }
    }

    protected override void OnDisable()
    {
        AorFontUpdateTracker.UntrackText(this);
        base.OnDisable();
    }

    protected override void UpdateGeometry()
    {
        if (font != null)
        {
            base.UpdateGeometry();
        }
    }

#if UNITY_EDITOR
        protected override void Reset()
        {
            AssignDefaultFont();
        }

#endif
    internal void AssignDefaultFont()
    {
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    public TextGenerationSettings GetGenerationSettings(Vector2 extents)
    {
        var settings = new TextGenerationSettings();

        settings.generationExtents = extents;
        if (font != null && font.dynamic)
        {
            settings.fontSize = m_FontData.fontSize;
            settings.resizeTextMinSize = m_FontData.minSize;
            settings.resizeTextMaxSize = m_FontData.maxSize;
        }

        // Other settings
        settings.textAnchor = m_FontData.alignment;
        settings.scaleFactor = pixelsPerUnit;
        settings.color = color;
        settings.font = font;
        settings.pivot = rectTransform.pivot;
        settings.richText = m_FontData.richText;
        settings.lineSpacing = m_FontData.lineSpacing;
        settings.fontStyle = m_FontData.fontStyle;
        settings.resizeTextForBestFit = m_FontData.bestFit;
        settings.updateBounds = false;
        settings.horizontalOverflow = m_FontData.horizontalOverflow;
        settings.verticalOverflow = m_FontData.verticalOverflow;

        return settings;
    }

    public static Vector2 GetTextAnchorPivot(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.LowerLeft:
                return new Vector2(0, 0);
            case TextAnchor.LowerCenter:
                return new Vector2(0.5f, 0);
            case TextAnchor.LowerRight:
                return new Vector2(1, 0);
            case TextAnchor.MiddleLeft:
                return new Vector2(0, 0.5f);
            case TextAnchor.MiddleCenter:
                return new Vector2(0.5f, 0.5f);
            case TextAnchor.MiddleRight:
                return new Vector2(1, 0.5f);
            case TextAnchor.UpperLeft:
                return new Vector2(0, 1);
            case TextAnchor.UpperCenter:
                return new Vector2(0.5f, 1);
            case TextAnchor.UpperRight:
                return new Vector2(1, 1);
            default:
                return Vector2.zero;
        }
    }

    public virtual void CalculateLayoutInputHorizontal()
    {
    }

    public virtual void CalculateLayoutInputVertical()
    {
    }

    public virtual float minWidth
    {
        get { return 0; }
    }

    public virtual float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(SupportBRTag(GetPureText()), settings) / pixelsPerUnit;
        }
    }

    public virtual float flexibleWidth
    {
        get { return -1; }
    }

    public virtual float minHeight
    {
        get { return 0; }
    }

    public virtual float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
            //                float ph = cachedTextGeneratorForLayout.GetPreferredHeight(SupportBRTag(m_Text), settings);
            //                return ph * cachedTextGeneratorForLayout.lineCount / pixelsPerUnit;
            return cachedTextGeneratorForLayout.GetPreferredHeight(SupportBRTag(GetPureText()), settings) / pixelsPerUnit;

        }
    }

    public virtual float flexibleHeight
    {
        get { return -1; }
    }

    public virtual int layoutPriority
    {
        get { return 0; }
    }

    //#if UNITY_EDITOR
    //        public override void OnRebuildRequested()
    //        {
    //            // After a Font asset gets re-imported the managed side gets deleted and recreated,
    //            // that means the delegates are not persisted.
    //            // so we need to properly enforce a consistent state here.
    //            FontUpdateTracker.UntrackText(this);
    //            FontUpdateTracker.TrackText(this);
    //
    //            // Also the textgenerator is no longer valid.
    //            cachedTextGenerator.Invalidate();
    //
    //            base.OnRebuildRequested();
    //        }
    //
    //        // The Text inspector editor can change the font, and we need a way to track changes so that we get the appropriate rebuild callbacks
    //        // We can intercept changes in OnValidate, and keep track of the previous font reference
    //        protected override void OnValidate()
    //        {
    //            if (!IsActive())
    //            {
    //                base.OnValidate();
    //                return;
    //            }
    //
    //            if (m_FontData.font != m_LastTrackedFont)
    //            {
    //                Font newFont = m_FontData.font;
    //                m_FontData.font = m_LastTrackedFont;
    //                FontUpdateTracker.UntrackText(this);
    //                m_FontData.font = newFont;
    //                FontUpdateTracker.TrackText(this);
    //
    //                m_LastTrackedFont = newFont;
    //            }
    //            base.OnValidate();
    //        }
    //
    //#endif // if UNITY_EDITOR

#endregion

    private bool _supportSpriteGraphic;

    public bool supportSpriteGraphic
    {
        get { return _supportSpriteGraphic; }
        set
        {
            if (Application.isEditor)
            {
                _supportSpriteGraphic = value;
                switchSpriteGraphic();
            }
            else
            {
                if (value != _supportSpriteGraphic)
                {
                    _supportSpriteGraphic = value;
                    switchSpriteGraphic();
                }
            }
        }
    }

    [SerializeField]
    private Material _spriteGraphic_Material;
    public Material spriteGraphic_Material
    {
        get
        {
            return _spriteGraphic_Material;
        }
        set
        {
            _spriteGraphic_Material = value;

            if (_spriteGraphic_Material != null && m_spriteGraphic != null)
            {
                m_spriteGraphic.material = _spriteGraphic_Material;
            }

        }
    }

    [SerializeField]
    private Color _spriteGraphic_Color;
    public Color spriteGraphic_Color
    {
        get
        {
            return _spriteGraphic_Color;
        }
        set
        {
            _spriteGraphic_Color = value;

            if (m_spriteGraphic != null)
            {
                m_spriteGraphic.color = _spriteGraphic_Color;
            }

        }
    }
    [SerializeField]
    private SpriteAsset _spriteGraphic_SpriteAsset;

    public SpriteAsset spriteGraphic_SpriteAsset
    {
        get
        {
            return _spriteGraphic_SpriteAsset;
        }
        set
        {
            _spriteGraphic_SpriteAsset = value;
            if (_spriteGraphic_SpriteAsset != null && m_spriteGraphic != null)
            {
                m_spriteGraphic.m_spriteAsset = _spriteGraphic_SpriteAsset;
            }
        }
    }
    // 超链接颜色
    public string _href_Color = "blue";
    /// <summary>
    /// ChatDataID
    /// </summary>
    public int id;

    private void switchSpriteGraphic()
    {
        if (_supportSpriteGraphic)
        {
            spriteGraphicInit();
        }
        else
        {
            if (m_spriteGraphic != null)
            {
                m_spriteGraphic.gameObject.Dispose();
                m_spriteGraphic = null;
            }
        }
    }

    private void spriteGraphicInit()
    {
        if (m_spriteGraphic == null)
        {
            m_spriteGraphic = transform.FindComponent<SpriteGraphic>();

            if (m_spriteGraphic == null)
            {
                GameObject sgGo = CreatePrefab_UIBase(transform, 0, 0, 0, 0, 0, 0, 1f, 1f, 0, 1f);
                sgGo.name = "spriteGraphic#";
                m_spriteGraphic = sgGo.AddComponent<SpriteGraphic>();
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        if (!string.IsNullOrEmpty(languageKey))
        {
            if (!m_bHasReplace)
            {
                if (OnAwake != null)
                {
                    OnAwake(languageKey, this);
                    return;
                }
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

#region Text 5.6 源码

        cachedTextGenerator.Invalidate();
        AorFontUpdateTracker.TrackText(this);

#endregion

        if (_CanvasRenderer == null)
        {
            _CanvasRenderer = GetComponent<CanvasRenderer>();
        }
        if (_supportSpriteGraphic)
        {
            spriteGraphicInit();
        }

        if (_spriteGraphic_SpriteAsset != null && _spriteGraphic_Material != null && m_spriteGraphic != null)
        {
            m_spriteGraphic.m_spriteAsset = _spriteGraphic_SpriteAsset;
            m_spriteGraphic.material = _spriteGraphic_Material;
            m_spriteGraphic.color = _spriteGraphic_Color;
        }

        StartCoroutine(waitingForUpdate());
    }

    private IEnumerator waitingForUpdate()
    {
        yield return new WaitForEndOfFrame();
        SetAllDirty();
    }

    /// <summary>
    /// 用正则取标签属性 名称-大小-宽度比例
    /// </summary>
    private static readonly Regex SpriteRegex =
        new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) />", RegexOptions.Singleline);

    //new Regex(@"<quad material=(\d*\.?\d+%?) size=(\d*\.?\d+%?) x=(\d*\.?\d+%?) y=(\d*\.?\d+%?) width=(\d*\.?\d+%?) height=(\d*\.?\d+%?) />", RegexOptions.Singleline);

    private CanvasRenderer _CanvasRenderer;

    /// <summary>
    /// 图片资源
    /// </summary>
    [SerializeField]
    private SpriteGraphic m_spriteGraphic;

    /// <summary>
    /// 标签的信息列表
    /// </summary>
    private List<TextPlusDataSpriteInfo> listTagInfo = new List<TextPlusDataSpriteInfo>();

    public override void SetAllDirty()
    {

        //解析标签属性
        listTagInfo = new List<TextPlusDataSpriteInfo>();
        MatchCollection mc = SpriteRegex.Matches(text);
        foreach (Match match in mc)
        {

            TextPlusDataSpriteInfo info = new TextPlusDataSpriteInfo(
                match.Index,
                match.Length,
                match.Groups[1].Value,
                int.Parse(match.Groups[2].Value)
                );
            listTagInfo.Add(info);
        }

        if (m_spriteGraphic != null)
        {
            if (listTagInfo.Count == 0)
            {
                m_spriteGraphic.gameObject.SetActive(false);
            }
            else
            {
                m_spriteGraphic.gameObject.SetActive(true);
            }
        }

        base.SetAllDirty();
    }

    /// <summary>
    /// Draw the Text.
    /// </summary>
    readonly UIVertex[] m_TempVerts = new UIVertex[4];
    protected override void OnPopulateMesh(VertexHelper toFill)
    {

#region Text 5.6 源码

        if (font == null)
            return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line... (\n)
#if UNITY_2018_4_36
        int vertCount = verts.Count;
#else
           int vertCount = verts.Count-4;
#endif
        Vector2 roundingOffset = Vector2.zero;
        if (verts.Count != 0)
            roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }

#endregion

        if (!supportRichText)
        {
            return;
        }
        UpdateHrefInfo();

        //计算图片信息
        List<UIVertex> spriteUiVertices = new List<UIVertex>();

        for (int i = 0; i < listTagInfo.Count; i++)
        {

            UIVertex[] spUiVertex = new UIVertex[4];
            int spUiVertexIndex = 0;

            //UGUIText不支持<quad/>标签，表现为乱码，我这里将他的uv全设置为0,清除乱码
            int spStartNum = listTagInfo[i].index * 4;
            int spMaxNum = listTagInfo[i].index * 4 + listTagInfo[i].length * 4;

            if (toFill.currentVertCount >= spStartNum && toFill.currentVertCount >= spMaxNum)
            {

                for (int m = spStartNum; m < spMaxNum; m++)
                {

                    //                    UIVertex tempVertex = vbo[m];
                    UIVertex tempVertex = new UIVertex();
                    toFill.PopulateUIVertex(ref tempVertex, m);
                    tempVertex.uv0 = Vector2.zero;
                    //vbo[m] = tempVertex;
                    toFill.SetUIVertex(tempVertex, m);

                    if (m >= (spMaxNum - 4))
                    {
                        int id = m - (spMaxNum - 4);
                        spUiVertex[id] = tempVertex;
                    }
                }

                //计算其uv
                if (_spriteGraphic_SpriteAsset != null)
                {
                    Rect spriteRect = _spriteGraphic_SpriteAsset.listSpriteInfo[0].rect;
                    for (int j = 0; j < _spriteGraphic_SpriteAsset.listSpriteInfo.Count; j++)
                    {
                        //通过标签的名称去索引spriteAsset里所对应的sprite的名称
                        if (listTagInfo[i].name == _spriteGraphic_SpriteAsset.listSpriteInfo[j].name)
                        {
                            spriteRect = _spriteGraphic_SpriteAsset.listSpriteInfo[j].rect;
                            Vector2 texSize = new Vector2(_spriteGraphic_SpriteAsset.texSource.width, _spriteGraphic_SpriteAsset.texSource.height);

                            spUiVertex[0].uv0 = new Vector2(spriteRect.x / texSize.x, (spriteRect.y + spriteRect.height) / texSize.y);
                            spUiVertex[1].uv0 = new Vector2((spriteRect.x + spriteRect.width) / texSize.x, (spriteRect.y + spriteRect.height) / texSize.y);
                            spUiVertex[2].uv0 = new Vector2((spriteRect.x + spriteRect.width) / texSize.x, spriteRect.y / texSize.y);
                            spUiVertex[3].uv0 = new Vector2(spriteRect.x / texSize.x, spriteRect.y / texSize.y);

                            break;
                        }
                    }
                }
                spriteUiVertices.AddRange(spUiVertex);

            }
        }

        if (spriteUiVertices.Count > 0 && m_spriteGraphic != null)
        {
            m_spriteGraphic.updateFBV(spriteUiVertices);
        }

        // 处理超链接包围框
        foreach (var hrefInfo in m_HrefInfos)
        {
            hrefInfo.boxes.Clear();
            //            if (hrefInfo.startIndex >= vbo.Count) {
            if (hrefInfo.startIndex >= toFill.currentVertCount)
            {
                continue;
            }

            // 将超链接里面的文本顶点索引坐标加入到包围框
            //            var pos = vbo[hrefInfo.startIndex].position;

            UIVertex uiv = new UIVertex();
            toFill.PopulateUIVertex(ref uiv, hrefInfo.startIndex);
            var pos = uiv.position;
            var bounds = new Bounds(pos, Vector3.zero);
            for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
            {
                if (i >= toFill.currentVertCount)
                {
                    break;
                }

                UIVertex upVt = new UIVertex();
                toFill.PopulateUIVertex(ref upVt, i);

                pos = upVt.position;
                if (pos.x < bounds.min.x) // 换行重新添加包围框
                {

                    //hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    hrefInfo.boxes.Add(new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y));
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框
                }
            }


            //hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
            hrefInfo.boxes.Add(new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y));
        }

#region Text 5.6 源码

        m_DisableFontTextureRebuiltCallback = false;

#endregion

    }

    /// <summary>
    /// 超链接信息列表
    /// </summary>
    private readonly List<HrefInfo> m_HrefInfos = new List<HrefInfo>();

    /// <summary>
    /// 文本构造器
    /// </summary>
    private static readonly StringBuilder s_TextBuilder = new StringBuilder();

    /// <summary>
    /// 超链接正则
    /// </summary>
    private static readonly Regex s_HrefRegex =
        new Regex(@"<a href=([^>\n\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

    [Serializable]
    public class HrefClickEvent : UnityEvent<string> { }

    [SerializeField]
    private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

    /// <summary>
    /// 超链接点击事件
    /// </summary>
    public HrefClickEvent onHrefClick
    {
        get { return m_OnHrefClick; }
        set { m_OnHrefClick = value; }
    }

    /// <summary>
    /// 点击事件检测是否点击到超链接文本
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        foreach (var hrefInfo in m_HrefInfos)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    m_OnHrefClick.Invoke(hrefInfo.name);
                    return;
                }
            }
        }
    }

    protected string SupportBRTag(string tex)
    {
        return tex.Replace("<br>", "\n").Replace("<br />", "\n").Replace("<br/>", "\n").Replace("<BR>", "\n").Replace("<BR />", "\n").Replace("<BR/>", "\n");
    }

    protected string GetPureText()
    {
        s_TextBuilder.Length = 0;
        m_HrefInfos.Clear();
        var indexText = 0;
        foreach (Match match in s_HrefRegex.Matches(text))
        {
            s_TextBuilder.Append(text.Substring(indexText, match.Index - indexText));
            s_TextBuilder.Append(match.Groups[2].Value);
            indexText = match.Index + match.Length;
        }
        s_TextBuilder.Append(text.Substring(indexText, text.Length - indexText));
        return s_TextBuilder.ToString();
    }

    /// <summary>
    /// 获取超链接解析后的最后输出文本
    /// </summary>
    /// <returns></returns>
    protected void UpdateHrefInfo()
    {
        s_TextBuilder.Length = 0;
        m_HrefInfos.Clear();
        var indexText = 0;
#if UNITY_2018_4_36
        foreach (Match match in s_HrefRegex.Matches(textWithOutFull))
        {
            string replaceText = textWithOutFull.Substring(indexText, match.Index - indexText);
            s_TextBuilder.Append(replaceText);
            //s_TextBuilder.Append(string.Format("<color={0}>", _href_Color)); // 超链接颜色

            var group = match.Groups[1];
            var hrefInfo = new HrefInfo
            {
                startIndex = s_TextBuilder.Length * 4, // 超链接里的文本起始顶点索引
                endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1) * 4 + 3,
                name = group.Value
            };
            m_HrefInfos.Add(hrefInfo);

            s_TextBuilder.Append(match.Groups[2].Value);
            //s_TextBuilder.Append("</color>");
            indexText = match.Index + match.Length;
        }
        s_TextBuilder.Append(textWithOutFull.Substring(indexText, textWithOutFull.Length - indexText));
        //return s_TextBuilder.ToString();
#else
              foreach (Match match in s_HrefRegex.Matches(originalText))
        {
            string replaceText = originalText.Substring(indexText, match.Index - indexText);
            s_TextBuilder.Append(replaceText);
            //s_TextBuilder.Append(string.Format("<color={0}>", _href_Color)); // 超链接颜色

            var group = match.Groups[1];
            var hrefInfo = new HrefInfo
            {
                startIndex = s_TextBuilder.Length * 4, // 超链接里的文本起始顶点索引
                endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1) * 4 + 3,
                name = group.Value
            };
            m_HrefInfos.Add(hrefInfo);

            s_TextBuilder.Append(match.Groups[2].Value);
            //s_TextBuilder.Append("</color>");
            indexText = match.Index + match.Length;
        }
        s_TextBuilder.Append(originalText.Substring(indexText, originalText.Length - indexText));
        //return s_TextBuilder.ToString();
#endif
    }

    public static string DeleteRichText_Color(string text)
    {
        Regex logComment = new Regex("<color=.+?>");
        text = logComment.Replace(text, "");
        Regex regex2 = new Regex("</color>", RegexOptions.IgnoreCase);
        text = regex2.Replace(text, "");
        return text;
    }

    /// <summary>
    /// 超链接信息类
    /// </summary>
    private class HrefInfo
    {
        public int startIndex;

        public int endIndex;

        public string name;

        public readonly List<Rect> boxes = new List<Rect>();
    }

    /// <summary>
    /// 创建基础UIGameObject
    /// 包含组件[RectTransform]
    /// </summary>
    private static GameObject CreatePrefab_UIBase(Transform parent = null,
                                                    float x = 0, float y = 0, float w = 0, float h = 0,
                                                    float anchorsMinX = 0, float anchorsMinY = 0,
                                                    float anchorsMaxX = 1f, float anchorsMaxY = 1f,
                                                    float pivotX = 0.5f, float pivotY = 0.5f)
    {
        GameObject _base = new GameObject();
        _base.layer = 5;

        if (parent != null)
        {
            _base.transform.SetParent(parent, false);
        }

        RectTransform rt = _base.AddComponent<RectTransform>();

        rt.pivot = new Vector2(pivotX, pivotY);
        rt.anchorMin = new Vector2(anchorsMinX, anchorsMinY);
        rt.anchorMax = new Vector2(anchorsMaxX, anchorsMaxY);

        rt.localPosition = new Vector3(x, y);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);

        return _base;
    }

}