using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class EmojiText : Text, IPointerClickHandler
{
    private const bool EMOJI_LARGE = true;
    private static Dictionary<string, EmojiInfo> EmojiIndex = null;
    private static List<string> m_nFaceNameList = new List<string>();
    private const string m_strFace_ReplaceText = "<quad name=emoji ({0})/>";

    struct EmojiInfo
    {
        public float x;
        public float y;
        public float size;
        public int len;
    }

    readonly UIVertex[] m_TempVerts = new UIVertex[4];
    [SerializeField] private float m_EmojiHeightOffset = 0;
    [SerializeField] private TextAsset m_EmojiData = null;

    /// <summary>
    /// 解析完最终的文本
    /// </summary>
    private string m_OutputText;

    public static void Release()
    {
        EmojiIndex = null;
        m_nFaceNameList.Clear();
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();

#if UNITY_EDITOR
#pragma warning disable CS0618 // 类型或成员已过时
        if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
#pragma warning restore CS0618 // 类型或成员已过时
        {
            return;
        }
#endif
        m_OutputText = GetOutputText(text);
    }

    public string emojiContent = string.Empty;

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;

        if (EmojiIndex == null)
        {
            EmojiIndex = new Dictionary<string, EmojiInfo>();
            emojiContent = m_EmojiData.text;

            //load emoji data, and you can overwrite this segment code base on your project.
            string[] lines = emojiContent.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    string[] strs = lines[i].Split('\t');
                    EmojiInfo info;
                    info.x = float.Parse(strs[3]);
                    info.y = float.Parse(strs[4]);
                    info.size = float.Parse(strs[5]);
                    info.len = 0;
                    //Debug.Log("this is " + strs[1]);
                    EmojiIndex.Add(strs[1], info);
                }
            }
        }

        Dictionary<int, EmojiInfo> emojiDic = new Dictionary<int, EmojiInfo>();
        if (supportRichText)
        {
            MatchCollection matches = Regex.Matches(text, "<quad.+?>");
            int diff = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                EmojiInfo info;
                Match numberMatch = Regex.Match(matches[i].Value, "[0-9]+");
                if (EmojiIndex.TryGetValue(numberMatch.Value, out info))
                {
                    info.len = matches[i].Length;
                    emojiDic.Add(matches[i].Index, info);
                    diff += info.len - 1;
                }
                else
                {
                    info.len = matches[i].Length;
                    emojiDic.Add(matches[i].Index, EmojiIndex["0"]);
                    diff += info.len - 1;
                }
            }
        }

        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        var orignText = m_Text;
        m_Text = m_OutputText;
        cachedTextGenerator.Populate(m_Text, settings); //重置网格
        m_Text = orignText;

        Rect inputRect = rectTransform.rect;

        // get the text alignment anchor point for the text in local space
        Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
        Vector2 refPoint = Vector2.zero;
        refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
        refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

        // Determine fraction of pixel to offset text mesh.
        Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line...
        int vertCount = verts.Count - 4;

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
                EmojiInfo info;
                int index = i / 4;
                //Debug.Log("match aa " + index);
                if (emojiDic.TryGetValue(index, out info))
                {
                    HrefInfosIndexAdjust(i); //矫正一下超链接的Index
                    //compute the distance of '[' and get the distance of emoji 
                    float charDis = (verts[i + 1].position.x - verts[i].position.x);
                    m_TempVerts[3] = verts[i]; //1
                    m_TempVerts[2] = verts[i + 1]; //2
                    m_TempVerts[1] = verts[i + 2]; //3
                    m_TempVerts[0] = verts[i + 3]; //4

                    //the real distance of an emoji
                    m_TempVerts[2].position += new Vector3(charDis, 0, 0);
                    m_TempVerts[1].position += new Vector3(charDis, 0, 0);

                    //make emoji has equal width and height
                    float fixValue = (m_TempVerts[2].position.x - m_TempVerts[3].position.x
                                                                - (m_TempVerts[2].position.y - m_TempVerts[1].position.y
                                                                ));
                    m_TempVerts[2].position -= new Vector3(fixValue, 0, 0);
                    m_TempVerts[1].position -= new Vector3(fixValue, 0, 0);


                    m_TempVerts[0].position *= unitsPerPixel;
                    m_TempVerts[1].position *= unitsPerPixel;
                    m_TempVerts[2].position *= unitsPerPixel;
                    m_TempVerts[3].position *= unitsPerPixel;
                    m_TempVerts[0].position.y += m_EmojiHeightOffset;
                    m_TempVerts[1].position.y += m_EmojiHeightOffset;
                    m_TempVerts[2].position.y += m_EmojiHeightOffset;
                    m_TempVerts[3].position.y += m_EmojiHeightOffset;

                    float pixelOffset = emojiDic[index].size / 64 / 2;
                    m_TempVerts[0].uv1 = new Vector2(emojiDic[index].x + pixelOffset, emojiDic[index].y + pixelOffset);
                    m_TempVerts[1].uv1 = new Vector2(emojiDic[index].x - pixelOffset + emojiDic[index].size,
                        emojiDic[index].y + pixelOffset);
                    m_TempVerts[2].uv1 = new Vector2(emojiDic[index].x - pixelOffset + emojiDic[index].size,
                        emojiDic[index].y - pixelOffset + emojiDic[index].size);
                    m_TempVerts[3].uv1 = new Vector2(emojiDic[index].x + pixelOffset,
                        emojiDic[index].y - pixelOffset + emojiDic[index].size);

                    toFill.AddUIVertexQuad(m_TempVerts);

                    i += 3; //4 * info.len - 1;
                }
                else
                {
                    int tempVertsIndex = i & 3;

                    m_TempVerts[tempVertsIndex] = verts[i];
                    //m_TempVerts[tempVertsIndex].position -= new Vector3(repairDistance, 0, 0);
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    if (tempVertsIndex == 3)
                        toFill.AddUIVertexQuad(m_TempVerts);
                }
            }
        }

        m_DisableFontTextureRebuiltCallback = false;

        UIVertex vert = new UIVertex();
        // 处理超链接包围框
        foreach (var hrefInfo in m_HrefInfos)
        {
            hrefInfo.boxes.Clear();
            if (hrefInfo.startIndex >= toFill.currentVertCount)
            {
                continue;
            }

            // 将超链接里面的文本顶点索引坐标加入到包围框
            toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);
            for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
            {
                if (i >= toFill.currentVertCount)
                {
                    break;
                }

                toFill.PopulateUIVertex(ref vert, i);
                pos = vert.position;
                if (pos.x < bounds.min.x) // 换行重新添加包围框
                {
                    hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框
                }
            }

            hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
        }
    }

    private void HrefInfosIndexAdjust(int imgIndex)
    {
        foreach (var hrefInfo in m_HrefInfos) //如果后面有超链接，需要把位置往前挪
        {
            if (imgIndex < hrefInfo.startIndex)
            {
                hrefInfo.startIndex -= 8;
                hrefInfo.endIndex -= 8;
            }
        }
    }

    /// <summary>
    /// 超链接正则
    /// </summary>
    private static readonly Regex s_HrefRegex =
        new Regex(@"<ahref=([^>\n\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

    /// <summary>
    /// 超链接信息列表
    /// </summary>
    private readonly List<HrefInfo> m_HrefInfos = new List<HrefInfo>();

    /// <summary>
    /// 文本构造器
    /// </summary>
    protected static readonly StringBuilder s_TextBuilder = new StringBuilder();

    /// <summary>
    /// 获取超链接解析后的最后输出文本
    /// </summary>
    /// <returns></returns>
    protected virtual string GetOutputText(string outputText)
    {
        s_TextBuilder.Length = 0;
        m_HrefInfos.Clear();
        var indexText = 0;

        foreach (Match match in s_HrefRegex.Matches(outputText))
        {
            s_TextBuilder.Append(outputText.Substring(indexText, match.Index - indexText));
            var group = match.Groups[1];
            var hrefInfo = new HrefInfo
            {
                startIndex = s_TextBuilder.Length * 4, // 超链接里的文本起始顶点索引
                endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1) * 4 + 3,
                name = group.Value
            };
            m_HrefInfos.Add(hrefInfo);

            s_TextBuilder.Append(match.Groups[2].Value);
            indexText = match.Index + match.Length;
        }

        s_TextBuilder.Append(outputText.Substring(indexText, outputText.Length - indexText));
        return s_TextBuilder.ToString();
    }

    [Serializable]
    public class HrefClickEvent : UnityEvent<string> { }

    [SerializeField] private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

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

    /// <summary>
    /// 表情解析
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string DecodeFaceText(string text)
    {
        if (m_nFaceNameList.Count == 0)
        {
            for (int i = 0; i < 33; i++)
            {
                m_nFaceNameList.Add(string.Format("[{0}]", i.ToString()));
            }
        }

        for (int i = 0; i < m_nFaceNameList.Count; i++)
        {
            if (text.Contains(m_nFaceNameList[i]))
            {
                text = text.Replace(m_nFaceNameList[i], string.Format(m_strFace_ReplaceText, i.ToString()));
            }
        }

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
}