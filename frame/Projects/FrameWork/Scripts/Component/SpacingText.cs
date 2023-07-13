using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 艺术Text控件 (说白了就是支持了spacing,即字间距)
/// 
/// 注意: 
///     1.暂时没有对此控件写Inspector,所以spacing属性不会显示在Normal模式的Inspector内,请使用Debug模式
///     2.此控件暂不支持RichText,请手动将RichText属性置为False.
///     3.此控件暂不支持横向自动换行,请手动将HorizontalOverflow属性置为Overflow.
/// 
/// </summary>
public class SpacingText : Text
{
    [SerializeField]
    private float m_spacing = 0f;

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
/*

    protected override void OnFillVBO(List<UIVertex> vbo)
    {
        base.OnFillVBO(vbo);

        if (!IsActive()) return;

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
                int idx1 = glyphIdx * 4 + 0;
                int idx2 = glyphIdx * 4 + 1;
                int idx3 = glyphIdx * 4 + 2;
                int idx4 = glyphIdx * 4 + 3;

                // Check for truncated text (doesn't generate verts for all characters)
                if (idx4 > vbo.Count - 1) return;

                UIVertex vert1 = vbo[idx1];
                UIVertex vert2 = vbo[idx2];
                UIVertex vert3 = vbo[idx3];
                UIVertex vert4 = vbo[idx4];

                pos = Vector3.right * (letterOffset * charIdx - lineOffset);

                vert1.position += pos;
                vert2.position += pos;
                vert3.position += pos;
                vert4.position += pos;

                vbo[idx1] = vert1;
                vbo[idx2] = vert2;
                vbo[idx3] = vert3;
                vbo[idx4] = vert4;

                glyphIdx++;
            }

            // Offset for carriage return character that still generates verts
            glyphIdx++;
        }
    }*/
}