using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ResourceLoad
{
    public class HShader : HRes
    {
        public HShader()
        {
        }

        public override Type GetRealType()
        {
            return typeof(Shader);
        }

        public override List<string> GetExtesions()
        {
            return new List<string>() { ".shader" };
        }
    }
}
