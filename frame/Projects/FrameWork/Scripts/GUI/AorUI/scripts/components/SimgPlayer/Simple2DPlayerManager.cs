using System.Collections.Generic;
using UnityEngine;
using FrameWork.GUI.AorUI.Core;
using ResourceFrameWork;

public class Simple2DPlayerManager
{
    private static Simple2DPlayerManager m_inst;
    public static Simple2DPlayerManager Inst
    {

        get
        {
            if (null == m_inst)
            {
                m_inst = new Simple2DPlayerManager();
            }
            return m_inst;
        }
    }

    private Dictionary<string, Simple2DData> m_FResourceRefDic = new Dictionary<string, Simple2DData>();

    public Simple2DPlayerManager()
    {

    }


    public void Get(string path, string clipName, CallBack<List<Sprite>, FResourceRef> callBack = null)
    {
        if (string.IsNullOrEmpty(path))
        {
            if (null != callBack)
            {
                callBack(null, null);
            }
            return;
        }

        if (m_FResourceRefDic.ContainsKey(path))
        {
            if (null == m_FResourceRefDic[path])
            {
                m_FResourceRefDic.Remove(path);
            }

            if (null != callBack)
            {
                callBack(m_FResourceRefDic[path].GetSpriteList(clipName), m_FResourceRefDic[path].AtlasRef);
            }
           
        }
        else
        {
            AorUIAssetLoader.LoadAllSprites(path, (sps, objs) =>
            {
                if (!m_FResourceRefDic.ContainsKey(path))
                {
                    m_FResourceRefDic.Add(path, new Simple2DData()
                    {
                        AtlasPath = path,
                        AtlasRef = objs[0] as FResourceRef
                    });
                    m_FResourceRefDic[path].AddSpriteList(sps);
                }
                if (null != callBack)
                {
                    callBack(m_FResourceRefDic[path].GetSpriteList(clipName), m_FResourceRefDic[path].AtlasRef);
                }



            });


        }

    }


    public void AddCount(string path)
    {
        if (m_FResourceRefDic.ContainsKey(path))
        {
            m_FResourceRefDic[path].AddRefrence();
        }
    }

    public void ReduceCount(string path)
    {
        if (m_FResourceRefDic.ContainsKey(path))
        {
            m_FResourceRefDic[path].ReduceRefrence();
            if(m_FResourceRefDic[path].Count<=0)
            {
                m_FResourceRefDic.Remove(path);
            }
        }

    }
}

public class Simple2DData
{
    public string AtlasPath;

    public FResourceRef AtlasRef;
    public Dictionary<string, List<Sprite>> AnimationDic;

    public int Count;
    private Sprite[] allSprite;

    public Simple2DData()
    {
        this.AtlasPath = string.Empty;
        this.AtlasRef = null;
        this.AnimationDic = new Dictionary<string, List<Sprite>>();
        this.Count = 0;
    }

    public void AddSpriteList(Sprite[] allSprite)
    {
        List<Sprite> _list = null;

        int _index = -1;
        string _key = string.Empty;
        string _name = string.Empty;
        int _length = allSprite.Length;
        for (int i = 0; i < _length; ++i)
        {
            Sprite _sprite = allSprite[i];
            _name = _sprite.name;
            _index = _name.LastIndexOf("_");
            if (-1 == _index)
            {
                continue;
            }
            _key = _name.Substring(0, _index);


            if (!AnimationDic.TryGetValue(_key, out _list))
            {
                _list = new List<Sprite>();
                _list.Add(_sprite);
                AnimationDic.Add(_key, _list);
            }
            else
            {
                if (!_list.Contains(_sprite))
                {
                    _list.Add(_sprite);
                }
            }

        }
    }

    public List<Sprite> GetSpriteList(string clipName)
    {
        List<Sprite> _list = new List<Sprite>();
        if (string.IsNullOrEmpty(clipName))
        {
            return _list;
        }

        if (!AnimationDic.TryGetValue(clipName, out _list))
        {

            int _index = -1;

            string _name = string.Empty;
            int _length = allSprite.Length;
            for (int i = 0; i < _length; ++i)
            {
                Sprite _sprite = allSprite[i];
                _name = _sprite.name;
                _index = _name.IndexOf(clipName);
                if (0 == _index)
                {
                    _list.Add(_sprite);
                }
            }

            AnimationDic.Add(clipName, _list);

        }
        return _list;
    }

    public void AddRefrence()
    {
        Count++;
    }

    public void ReduceRefrence()
    {
        Count--;
    }

}


