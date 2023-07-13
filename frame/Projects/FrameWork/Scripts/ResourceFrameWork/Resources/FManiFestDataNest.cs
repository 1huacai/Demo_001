using System;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceFrameWork
{

    [PreferBinarySerialization]
    public class FManiFestDataNest : ScriptableObject
    {
        public List<string> m_typeList = new List<string>();
        public List<int> m_typeCountList = new List<int>();
        public List<FManiFestData> m_list = new List<FManiFestData>();

    }

}
