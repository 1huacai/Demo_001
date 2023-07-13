using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Demo
{
    public class GameManger
    {
        private static GameManger s_inst;
        public static GameManger Inst
        {
            get { if (s_inst == null) s_inst = new GameManger();return s_inst; }
        }
    }
}
