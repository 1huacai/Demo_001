using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ResourceLoad
{
    public class HText : HRes
    {
        public HText()
        {
        }

        public override Type GetRealType()
        {
            return typeof(TextAsset);
        }


        public override List<string> GetExtesions()
        {
            return new List<string>() { ".txt", ".bytes", ".json" };
        }

    }
}
