using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceLoad
{
    public class HShaderVariantCollection : HRes
    {
        public HShaderVariantCollection()
        {
        }

        public override Type GetRealType()
        {
            return typeof(ShaderVariantCollection);
        }

        public override List<string> GetExtesions()
        {
            return new List<string>() { ".shadervariants" };
        }
    }
}