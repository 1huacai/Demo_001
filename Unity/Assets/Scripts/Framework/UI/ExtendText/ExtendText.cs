using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// txt 扩展
/// 1.图片<sprite=名字>
/// 2.超链接<url="链接">"中文描述"</url>
/// 创建描述文件 Asset/ExtendText/ExtendTextDatabase
/// </summary>
public class ExtendText : Text, IPointerClickHandler
{
    private static UIVertexObject UIVertexUtils = new UIVertexObject();

    private const string underline = "_";

    UIVertex[] underlineVerts = new UIVertex[4];

    public int test;
    /// <summary>
    /// 表情数据库
    /// </summary>
    [SerializeField] EmojDatabase emojDatabase;
    /// <summary>
    /// 缩放
    /// </summary>
    [SerializeField] public float emojScale = 1f;
    /// <summary>
    /// 缩放
    /// </summary>
    [SerializeField] public Vector2 textOffset = new Vector2(0f, 0f);
    /// <summary>
    /// 链接颜色
    /// </summary>
    [SerializeField] Color linkColor = Color.blue;

    /// <summary>
    /// 链接下划线颜色
    /// </summary>
    [SerializeField] public Color linkUnderlineColor = Color.blue;

    // 连接偏移
    [SerializeField] float linkOffsetY = 3;

    [SerializeField] bool richTextEx = true;

    [System.Serializable]
    public class HrefClickedEvent : UnityEvent<string>
    {
    }

    [SerializeField] HrefClickedEvent onHrefClick;

    public HrefClickedEvent OnHrefClick
    {
        get
        {
            return onHrefClick;
        }
        set
        {
            onHrefClick = value;
        }
    }

    /// <summary>
    /// 生成后的富文本 包含了自动生成的占位符<quad> <color>等标签
    /// </summary>
    string internalText;

    struct EmojTag
    {
        /// <summary>
        /// 开始字符位置 internalText Index
        /// </summary>
        public int charIndex;

        /// <summary>
        /// 哪个 Emoj
        /// </summary>
        public Emoj emoj;
    }

    class HrefTag
    {
        /// <summary>
        /// 开始字符位置 internalText Index
        /// </summary>
        public int beginCharIndex;

        /// <summary>
        /// 结束字符位置 internalText Index
        /// </summary>
        public int endCharIndex;

        /// <summary>
        /// 描述 显示用
        /// </summary>
        public string desc;

        /// <summary>
        /// 指向链接
        /// </summary>
        public string link;

        /// <summary>
        /// 碰撞盒
        /// </summary>
        public List<Rect> boxes = new List<Rect>();
    }

    /// <summary>
    /// Emoj组
    /// </summary>
    RectTransform emojRoot;

    /// <summary>
    /// emoj 标记表
    /// </summary>
    List<EmojTag> emojTags = new List<EmojTag>();

    /// <summary>
    /// 链接列表
    /// </summary>
    List<HrefTag> hrefTags = new List<HrefTag>();

    private IList<UIVertex> cachedTextGenerator_vertsList = null;
    private UIVertex[] cachedTextGenerator_vertsArray = null;

    static readonly UIVertex[] m_TempVerts = new UIVertex[4];
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.Populate(internalText, settings);

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        var verArray = UIVertexUtils.UIVertexListToArray(verts as List<UIVertex>);

        // 隐藏方块
        if (richTextEx)
        {
            ClearEmojQuads(verArray, verts.Count);
        }
        
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line... (\n)
        //int vertCount = verts.Count - 4; //新版本去掉了这4个
        int vertCount = verts.Count;
        Vector2 refPoint = Vector2.zero;
        Rect inputRect = rectTransform.rect;
        Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
        refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
        refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);
        Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;
        
        toFill.Clear();

        if (richTextEx)
        {
            // 更新表情位置
            PositionEmojs(verArray, verts.Count);
            
            // 生成链接碰撞盒
            GenerateHrefHitBox(verArray, verts.Count);
        }
        
        var preCount = 0;
        if (richTextEx)
        {
            foreach (var tag in hrefTags)
            {
                foreach (var box in tag.boxes)
                {
                    if (box.width > 0 && box.height > 0)
                    {
                        ++preCount;
                    }
                }
            }
        }

        UIVertexUtils.GetBuffer((vertCount + (preCount * 12)) / 4, out UIVertex[] vertexs);
        var vertexIndex = 0;
        
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                vertexs[vertexIndex].normal = verArray[i].normal;
                vertexs[vertexIndex].tangent = verArray[i].tangent;
                vertexs[vertexIndex].color = verArray[i].color;
                vertexs[vertexIndex].uv0 = verArray[i].uv0;
                vertexs[vertexIndex].uv1 = verArray[i].uv1;
                vertexs[vertexIndex].uv2 = verArray[i].uv2;
                vertexs[vertexIndex].uv3 = verArray[i].uv3;
                vertexs[vertexIndex].position.x = verArray[i].position.x * unitsPerPixel + roundingOffset.x + textOffset.x;
                vertexs[vertexIndex].position.y = verArray[i].position.y * unitsPerPixel + roundingOffset.y + textOffset.y;
                vertexs[vertexIndex].position.z = verArray[i].position.z * unitsPerPixel;
                ++vertexIndex;
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                vertexs[vertexIndex].normal = verArray[i].normal;
                vertexs[vertexIndex].tangent = verArray[i].tangent;
                vertexs[vertexIndex].color = verArray[i].color;
                vertexs[vertexIndex].uv0 = verArray[i].uv0;
                vertexs[vertexIndex].uv1 = verArray[i].uv1;
                vertexs[vertexIndex].uv2 = verArray[i].uv2;
                vertexs[vertexIndex].uv3 = verArray[i].uv3;
                vertexs[vertexIndex].position.x = verArray[i].position.x * unitsPerPixel + textOffset.x;
                vertexs[vertexIndex].position.y = verArray[i].position.y * unitsPerPixel + textOffset.y;
                vertexs[vertexIndex].position.z = verArray[i].position.z * unitsPerPixel;
                ++vertexIndex;
            }
        }

        m_DisableFontTextureRebuiltCallback = false;


        // 生成链接下划线
        if (preCount > 0)
        {
            GenerateUnderline(vertexs, ref vertexIndex, settings);
        }
      
        UIVertexUtils.AddUIVertexStream(toFill);
    }

    void ClearEmojQuads(UIVertex[] verts, int vertsLength)
    {
        foreach (var item in emojTags)
        {
            int vIndex = item.charIndex * 4;
            if ((vIndex + 4) > vertsLength) // 被隐藏了，表情也隐藏
                break;
            for (int i = vIndex; i < vIndex + 4; i++)
            {
                //清除Quad
                UIVertex tempVertex = verts[i];
                tempVertex.uv0 = Vector2.zero;
                tempVertex.color = new Color32(0, 0, 0, 0);
                verts[i] = tempVertex;
            }
        }
    }

    void PositionEmojs(UIVertex[] verts, int verts_Length)
    {
        float unitsPerPixel = 1 / pixelsPerUnit;
        for (int i = 0; i < emojTags.Count; i++)
        {
            var child = emojRoot.GetChild(i);

            var childGraphic = child.GetComponent<Graphic>();

            int vIndex = emojTags[i].charIndex * 4;
            if ((vIndex) >= verts_Length) // 最后四个顶点
            {
                // 直接改alpha会引起Trying to XXX for graphic rebuild while we are already inside a graphic rebuild loop. This is not supported.
                // 这里改alpha为0 隐藏
                childGraphic.CrossFadeAlpha(0, 0f, true);
                continue;
            }

            Vector2 min = verts[vIndex].position;
            Vector2 max = verts[vIndex].position;

            for (int j = vIndex; j < vIndex + 4; j++)
            {
                Vector2 pos = verts[j].position;
                min.x = Mathf.Min(min.x, pos.x);
                min.y = Mathf.Min(min.y, pos.y);

                max.x = Mathf.Max(max.x, pos.x);
                max.y = Mathf.Max(max.y, pos.y);
            }
            childGraphic.CrossFadeAlpha(1, 0f, true);

            child.localPosition = (min + (max - min) / 2) * unitsPerPixel + emojTags[i].emoj.offset;
        }
    }

    static System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

    /// <summary>
    /// 用正则取标签属性 名称-大小-宽度比例
    /// </summary>
    private static readonly Regex SpriteRegex =
        new Regex(@"<sprite=(.+?)>", RegexOptions.Singleline);

    /// <summary>
    /// 超链接正则
    /// </summary>
    private static readonly Regex s_HrefRegex =
        new Regex(@"<url=([^>\n\s]+)>(.*?)(</url>)", RegexOptions.Singleline);

    /// <summary>
    /// 分析处理文本，得到tag和渲染文本
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <returns>渲染文本</returns>
    void ProcessText(string text)
    {
        if (string.IsNullOrEmpty(text) == true)
        {
            internalText = "";
            return;
        }
        emojTags.Clear();
        hrefTags.Clear();

        // clear string buffer
        stringBuilder.Length = 0;

        var indexText = 0;
        if (emojDatabase != null)
        {
            MatchCollection mc = SpriteRegex.Matches(text);
            foreach (Match match in mc)
            {
                string val = match.Value;
                stringBuilder.Append(text.Substring(indexText, match.Index - indexText));
                indexText = match.Index + val.Length;
                var name = match.Groups[1].Value;
                var emojInfo = emojDatabase.GetEmoj(name);

                if (emojInfo != null)
                {
                    int charIndex = stringBuilder.Length;
                    emojTags.Add(new EmojTag() { charIndex = charIndex, emoj = emojInfo });
                    Vector2 emojSize = emojInfo.Size;
                    stringBuilder.AppendFormat("<quad size={0}>", Mathf.Max(emojSize.x * this.emojScale, emojSize.y * this.emojScale).ToString());
                }
                else
                {
                    stringBuilder.Append(val);
                }
            }
        }
        else
        {
            internalText = text;
        }

        if (!richTextEx)
        {
            return;
        }

        var t = text.Substring(indexText, text.Length - indexText);
        indexText = 0;
        MatchCollection textmc = s_HrefRegex.Matches(t);
        foreach (Match match in textmc)
        {
            stringBuilder.Append(t.Substring(indexText, match.Index - indexText));
            indexText = match.Index + match.Length;
            var desc = match.Groups[2].Value;
            var link = match.Groups[1].Value;
            int charIndex = stringBuilder.Length;
            Color32 linkColor = this.linkColor;
            string color = string.Format("{0}{1}{2}{3}", linkColor.r.ToString("x2"), linkColor.g.ToString("x2"), linkColor.b.ToString("x2"), linkColor.a.ToString("x2"));
            stringBuilder.AppendFormat("<color=#{0}>{1}</color>", color, desc);
            hrefTags.Add(new HrefTag() { beginCharIndex = charIndex, endCharIndex = stringBuilder.Length, desc = desc, link = link });
        }
        stringBuilder.Append(t.Substring(indexText, t.Length - indexText));
        internalText = stringBuilder.ToString();
        GenerateEmojs();
    }

    void GenerateEmojs()
    {
        if (emojRoot == null)
        {
            emojRoot = rectTransform;
        }

        for (int i = 0; i < emojTags.Count; i++)
        {
            RectTransform rect = null;
            if (i < emojRoot.childCount)
            {
                rect = emojRoot.GetChild(i).GetComponent<RectTransform>();
                rect.gameObject.SetActive(true);
            }
            else
            {
                rect = new GameObject().AddComponent<RectTransform>();
                rect.gameObject.hideFlags = HideFlags.HideAndDontSave;
                rect.SetParent(emojRoot, false);
                rect.gameObject.AddComponent<EmojImage>();
                rect.GetComponent<Graphic>().raycastTarget = false;
            }
            rect.GetComponent<EmojImage>().Emoj = emojTags[i].emoj;
            rect.transform.localScale = new Vector3(this.emojScale, this.emojScale, 1);
        }

        for (int i = emojTags.Count; i < emojRoot.childCount; i++)
        {
            emojRoot.GetChild(i).gameObject.SetActive(false);
        }
    }

    void GenerateHrefHitBox(UIVertex[] verts, int verts_Length)
    {
        foreach (var tag in hrefTags)
        {
            int begin = tag.beginCharIndex * 4;
            int end = tag.endCharIndex * 4;

            if (verts_Length < begin)
            {
                // 后面的顶点被隐藏了
                break;
            }

            var boundsList = tag.boxes;
            boundsList.Clear();

            Bounds bounds = new Bounds(verts[begin].position, Vector3.zero);

            for (int i = begin; i < end && i < verts_Length - 4; i++) // 最后4个顶点是换行符
            {
                var pos = verts[i].position;

                if (pos.x < bounds.min.x) // 换行
                {
                    if (bounds.size.x > 0 && bounds.size.y > 0) // 检测不是个空盒子
                    {
                        tag.boxes.Add(new Rect(bounds.min, bounds.size));
                    }
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos);
                }
            }

            if (bounds.size.x > 0 && bounds.size.y > 0) // 检测不是个空盒子
            {
                tag.boxes.Add(new Rect(bounds.min, bounds.size));
            }
        }
    }

    void GenerateUnderline(UIVertex[] vertexs, ref int vertexIndex, TextGenerationSettings settings)
    {
        UIVertex[] underlineVerts = GetUnderlineVerts(settings);
          
        foreach (var tag in hrefTags)
        {
            foreach (var box in tag.boxes)
            {
                if (box.width > 0 && box.height > 0)
                {
                    FillUnderline(box, vertexs, ref vertexIndex, underlineVerts);
                }
            }
        }
    }
    
    void FillUnderline(Rect box, UIVertex[] vertexs, ref int vertexIndex, UIVertex[] underlineVerts)
    {
        /* 面片结构
         * p0-------p1
         *  |       | 
         *  |       |
         * p3-------p2
         * */

        box.position -= new Vector2(0, linkOffsetY); //向下偏移一个单位

        Vector3 p0 = underlineVerts[0].position;
        Vector3 p1 = underlineVerts[1].position;
        Vector3 p2 = underlineVerts[2].position;

        float height = p1.y - p2.y;
        float width = p1.x - p0.x;

        Vector2 uv0 = underlineVerts[0].uv0;
        Vector2 uv1 = underlineVerts[1].uv0;
        Vector2 uv2 = underlineVerts[2].uv0;
        Vector2 uv3 = underlineVerts[3].uv0;

        //顶部中心uv
        Vector2 topCenterUv = uv0 + (uv1 - uv0) * 0.5f;
        //底部中心uv
        Vector2 bottomCenterUv = uv3 + (uv2 - uv3) * 0.5f;

        //m_TempVerts[0] = underlineVerts[0];
        //m_TempVerts[1] = underlineVerts[1];
        //m_TempVerts[2] = underlineVerts[2];
        //m_TempVerts[3] = underlineVerts[3];

        //m_TempVerts[0].color = linkUnderlineColor;
        //m_TempVerts[1].color = linkUnderlineColor;
        //m_TempVerts[2].color = linkUnderlineColor;
        //m_TempVerts[3].color = linkUnderlineColor;

        float xMin = box.xMin;
        float yMin = box.yMin;

        float xMax = box.xMax;
        float yMax = box.yMin + height; // 高度

        float unitsPerPixel = 1 / pixelsPerUnit;
        {
            vertexs[vertexIndex] = underlineVerts[0];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMin, yMax) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = uv0;
            ++vertexIndex;

            vertexs[vertexIndex] = underlineVerts[1];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMin + width * 0.5f, yMax) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = topCenterUv;
            ++vertexIndex;

            vertexs[vertexIndex] = underlineVerts[2];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMin + width * 0.5f, yMin) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = bottomCenterUv;
            ++vertexIndex;

            vertexs[vertexIndex] = underlineVerts[3];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMin, yMin) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = uv3;
            ++vertexIndex;
        }
        {
            vertexs[vertexIndex] = underlineVerts[0];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMin + width * 0.5f, yMax) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = topCenterUv;
            ++vertexIndex;

            vertexs[vertexIndex] = underlineVerts[1];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMax - width * 0.5f, yMax) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = topCenterUv;
            ++vertexIndex;

            vertexs[vertexIndex] = underlineVerts[2];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMax - width * 0.5f, yMin) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = bottomCenterUv;
            ++vertexIndex;

            vertexs[vertexIndex] = underlineVerts[3];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMin + width * 0.5f, yMin) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = bottomCenterUv;
            ++vertexIndex;
        }

        {
            vertexs[vertexIndex] = underlineVerts[0];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMax - width * 0.5f, yMax) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = topCenterUv;
            ++vertexIndex;

            vertexs[vertexIndex] = underlineVerts[1];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMax, yMax) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = uv1;
            ++vertexIndex;

            vertexs[vertexIndex] = underlineVerts[2];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMax, yMin) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = uv2;
            ++vertexIndex;

            vertexs[vertexIndex] = underlineVerts[3];
            vertexs[vertexIndex].color = linkUnderlineColor;
            vertexs[vertexIndex].position = new Vector3(xMax - width * 0.5f, yMin) * unitsPerPixel;
            vertexs[vertexIndex].uv0 = bottomCenterUv;
            ++vertexIndex;
        }
    }

    UIVertex[] GetUnderlineVerts(TextGenerationSettings settings)
    {
        cachedTextGenerator.Populate(underline, settings);
        var verts = cachedTextGenerator.verts;

        for (int i = 0; i < verts.Count && i < 4; i++)
        {
            underlineVerts[i] = verts[i];
        }

        return underlineVerts;
    }

    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(internalText, settings) / pixelsPerUnit;
        }
    }

    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(internalText, settings) / pixelsPerUnit;
        }
    }

    public override void SetVerticesDirty()
    {
        ProcessText(text);
        base.SetVerticesDirty();
    }

    protected override void OnDisable()
    {
        SetEnableChild(false);
        base.OnDisable();
    }

    protected override void OnEnable()
    {
        ForceRichTextAndAlignByGeometry();
        base.OnEnable();
        SetEnableChild(true);
    }

    void SetEnableChild(bool enable)
    {
        if (richTextEx)
        {
            for (int i = 0; emojRoot != null && i < emojRoot.childCount; i++)
            {
                emojRoot.GetChild(i).GetComponent<Graphic>().enabled = enable;
            }
        }
    }

    protected void ForceRichTextAndAlignByGeometry()
    {
        if (richTextEx)
        {
            supportRichText = true;
            alignByGeometry = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        float unitsPerPixel = 1 / pixelsPerUnit;
        lp = lp / unitsPerPixel;
        
        foreach (var hrefInfo in hrefTags)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    OnHrefClick.Invoke(hrefInfo.link);
                    return;
                }
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        ForceRichTextAndAlignByGeometry();
        base.OnValidate();
    }
#endif
}
