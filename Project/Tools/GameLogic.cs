using System;
using UnityEngine;

namespace Demo.Tools
{
    public class GameLogic:MonoBehaviour
    {
        private void Awake()
        {
            
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            TimerMgr._Instance.Update();
            // GameManger.Inst.FiexdUpdate();
        }

        private void FixedUpdate()
        {
            GameManger.Inst.FiexdUpdate();
        }
    }
}