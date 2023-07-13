using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Emoj
{
    public string name;
    
    /// <summary>
    /// 表情帧，用于实现帧动画
    /// </summary>
    public Sprite[] spriteList;
    /// <summary>
    /// 表情偏移
    /// </summary>
    public Vector2 offset;

    /// <summary>
    /// 缩放
    /// </summary>
    public float scale = 1.0f;
    public Vector2 _size;
    public Vector2 Size
    {
        get
        {
            return _size;
        }
    }

    /// <summary>
    /// 表情帧动画速度, 多少时间切换一次
    /// </summary>
    public float speed = 0.3f;

    public void InitSize()
    {
        if (spriteList.Length == 0)
        {
            _size = Vector2.one;
            return;
        }


        Vector2 sz = Vector2.zero;
        for (int i = 0; i < spriteList.Length; i++)
        {
            sz = Vector2.Max(sz, spriteList[i].rect.size);
        }
        _size = sz * scale;
    }
}

[CreateAssetMenu(menuName = "ExtendText/ExtendTextDatabase")]
public class EmojDatabase : ScriptableObject
{
    public Emoj[] emojs;

    private Dictionary<string, Emoj> _dic = null;

    public Emoj GetEmoj(string name)
    {
        if (_dic == null)
        {
            if (Application.isEditor)
            {
                // 编辑器下不进行缓存, 加图片， 改大小
                for (int i = 0; i < emojs.Length; i++)
                {
                    var emoj = emojs[i];
                    if (emoj.name == name)
                    {
                        emoj.InitSize();
                        return emoj;
                    }
                }
                return null;
            }
            _dic = new Dictionary<string, Emoj>();
            for (int i = 0; i < emojs.Length; i++)
            {
                var emoj = emojs[i];
                emoj.InitSize();
                _dic.Add(emoj.name, emoj);
            }
        }
        _dic.TryGetValue(name, out var em);
        return em;
    }


}
