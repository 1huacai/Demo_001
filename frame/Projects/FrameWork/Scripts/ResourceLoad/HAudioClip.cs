using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ResourceLoad
{
    public class HAudioCilp : HRes
    {
        public HAudioCilp()
        { 
        }

        public override Type GetRealType()
        {
            return typeof(AudioClip);
        }

        public override List<string> GetExtesions()
        {
            return new List<string>() { ".ogg", ".mp3", ".wav"};
        }
    }
}
