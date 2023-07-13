using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class UIVertexObject
{
  private List<UIVertex> vertexList = new List<UIVertex>(4000);
  private Color32 color = new Color32((byte) 1, (byte) 1, (byte) 1, (byte) 1);
  private Vector2[] uv = new Vector2[4]
  {
    new Vector2(0.0f, 0.0f),
    new Vector2(0.0f, 1f),
    new Vector2(25f, 1f),
    new Vector2(25f, 0.0f)
  };
  private const float scaleFactor = 25f;
  private const int defaultLineCount = 1000;
  private UIVertex[] vertexs;
  private FieldInfo vertexListSizeFieldInfo;
  private FieldInfo vertexItemsFieldInfo;
  private int _lineCountRequest;

  public UIVertexObject()
  {
    System.Type type = typeof (List<UIVertex>);
    this.vertexItemsFieldInfo = type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
    this.vertexListSizeFieldInfo = type.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);
    this.vertexs = this.vertexItemsFieldInfo.GetValue((object) this.vertexList) as UIVertex[];
    this.ResetVertexArray(0, this.vertexs.Length - 1);
  }

  public UIVertex[] UIVertexListToArray(List<UIVertex> list)
  {
    return this.vertexItemsFieldInfo.GetValue((object) list) as UIVertex[];
  }

  public void GetBuffer(int lineCountRequest, out UIVertex[] vertexsBuffer)
  {
    if (lineCountRequest < 0)
      lineCountRequest = 0;
    this._lineCountRequest = lineCountRequest;
    int num = lineCountRequest * 4;
    int length = this.vertexs.Length;
    if (length < num)
    {
      UIVertex[] vertexs = this.vertexs;
      this.vertexList.Capacity = num;
      this.vertexs = this.vertexItemsFieldInfo.GetValue((object) this.vertexList) as UIVertex[];
      Array.Copy((Array) vertexs, 0, (Array) this.vertexs, 0, vertexs.Length);
      this.ResetVertexArray(length, this.vertexs.Length - 1);
    }
    vertexsBuffer = this.vertexs;
  }

  public void AddUIVertexStream(VertexHelper vh)
  {
    int num1 = this._lineCountRequest * 4;
    if (this.vertexList.Count != num1)
      this.vertexListSizeFieldInfo.SetValue((object) this.vertexList, (object) num1);
    vh.AddUIVertexStream(this.vertexList, (List<int>) null);
    for (int index = 0; index < this._lineCountRequest; ++index)
    {
      int num2 = index * 4;
      vh.AddTriangle(num2, num2 + 1, num2 + 2);
      vh.AddTriangle(num2 + 2, num2 + 3, num2);
    }
  }

  public Vector2 GetUVByIndex(int index)
  {
    return this.uv[index % 4];
  }

  private void ResetVertexArray(int start, int end)
  {
    for (int index = start; index <= end; ++index)
    {
      this.vertexs[index].color = this.color;
      this.vertexs[index].uv0 = this.uv[index % 4];
    }
  }
}
