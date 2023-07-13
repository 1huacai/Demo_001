using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Text.RegularExpressions;

/// <summary>
/// 携带语言表数据的Key;
/// </summary>
[ExecuteInEditMode]
public class AorText : Text
{
    public float m_spacing = 0f;

    public string languageKey;
    public override string text
    {
        set
        {
            base.text = value;
            if (Application.isPlaying)
            {
                m_bHasReplace = true;
            }
        }
        get
        {
            return base.m_Text;
        }
    }

    public float spacing
    {
        get { return m_spacing; }
        set
        {
            if (m_spacing == value) return;
            m_spacing = value;
            this.SetVerticesDirty();
        }
    }
    public static Action<string, AorText> OnAwake = null;

    public bool HasReplace
    {
        get { return m_bHasReplace; }
        set { m_bHasReplace = value; }
    }
    public bool m_bHasReplace = false;

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
                    if (Application.isPlaying)
                    {
                        m_bHasReplace = true;
                    }
                    return;
                }
            }
        }
    }



    protected override void OnPopulateMesh(VertexHelper toFill)
    {

#if Japan
        if (!IsActive()) return;
        base.OnPopulateMesh(toFill);
        if (spacing==0) return;
#else
        base.OnPopulateMesh(toFill);

        if (!IsActive()) return;
#endif

        List<UIVertex> vbo = new List<UIVertex>();
        toFill.GetUIVertexStream(vbo);

        string[] lines = text.Split('\n');
        Vector3 pos;
        float letterOffset = spacing * (float)fontSize / 100f;
        float alignmentFactor = 0;
        int glyphIdx = 0;

        switch (alignment)
        {
            case TextAnchor.LowerLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.UpperLeft:
                alignmentFactor = 0f;
                break;

            case TextAnchor.LowerCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.UpperCenter:
                alignmentFactor = 0.5f;
                break;

            case TextAnchor.LowerRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.UpperRight:
                alignmentFactor = 1f;
                break;
        }

        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            string line = lines[lineIdx];
            float lineOffset = (line.Length - 1) * letterOffset * alignmentFactor;

            for (int charIdx = 0; charIdx < line.Length; charIdx++)
            {
                int idx1 = glyphIdx * 6 + 0;
                int idx2 = glyphIdx * 6 + 1;
                int idx3 = glyphIdx * 6 + 2;
                int idx4 = glyphIdx * 6 + 3;
                int idx5 = glyphIdx * 6 + 4;
                int idx6 = glyphIdx * 6 + 5;

                // Check for truncated text (doesn't generate verts for all characters)
                if (idx6 > vbo.Count - 1) return;

                UIVertex vert1 = vbo[idx1];
                UIVertex vert2 = vbo[idx2];
                UIVertex vert3 = vbo[idx3];
                UIVertex vert4 = vbo[idx4];
                UIVertex vert5 = vbo[idx5];
                UIVertex vert6 = vbo[idx6];

                pos = Vector3.right * (letterOffset * charIdx - lineOffset);

                vert1.position += pos;
                vert2.position += pos;
                vert3.position += pos;
                vert4.position += pos;
                vert5.position += pos;
                vert6.position += pos;

                vbo[idx1] = vert1;
                vbo[idx2] = vert2;
                vbo[idx3] = vert3;
                vbo[idx4] = vert4;
                vbo[idx5] = vert5;
                vbo[idx6] = vert6;

                glyphIdx++;
            }

            // Offset for carriage return character that still generates verts
            glyphIdx++;
        }
        toFill.Clear();
        toFill.AddUIVertexTriangleStream(vbo);
    }
}
