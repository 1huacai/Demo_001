using System;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceLoad
{
    public class HFont : HRes
    {
        public HFont()
        {
        }

        public override Type GetRealType()
        {
            return typeof(Font);
        }

        public override List<string> GetExtesions()
        {
            return new List<string>() { ".ttf", ".fontsettings" };
        }
    }
}
