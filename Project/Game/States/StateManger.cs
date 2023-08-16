using System.Collections.Generic;
using Project;
using UnityEngine;

namespace Demo
{
    public class StateManger
    {
        private static StateManger _ins;
        
        public static StateManger _instance
        {
            get
            {
                if (_ins == null) _ins = new StateManger();
                return _ins;
            }
        }

        public Dictionary<BlockState, Statebase> StateHandlers;

        public void Init(Controller controller)
        {
            StateHandlers = new Dictionary<BlockState, Statebase>();
            StateHandlers.Add(BlockState.Normal,new NormalState(controller));
            StateHandlers.Add(BlockState.Swapping,new SwappingState(controller));
            StateHandlers.Add(BlockState.Matched,new MatchedState(controller));
            StateHandlers.Add(BlockState.Dimmed,new DimmedState(controller));
            StateHandlers.Add(BlockState.Hovering,new HoveringState(controller));
            StateHandlers.Add(BlockState.Falling,new FallingState(controller));
            StateHandlers.Add(BlockState.Landing,new LandingState(controller));
            StateHandlers.Add(BlockState.Popping,new PoppingState(controller));
            StateHandlers.Add(BlockState.Popped,new PoppedState(controller));
        }
        
        /// <summary>
        /// 通用状态改变处理
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="blocks"></param>
        public void ChangeState(BlockState newState,params Block[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                StateHandlers[newState].Enter(block);
                StateHandlers[newState].Update(block);
            }
        }
        
        /// <summary>
        /// 只执行block改变状态的进入
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="blocks"></param>
        public void ChangeStageEnter(BlockState newState,params Block[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                StateHandlers[newState].Enter(block);
            }
        }
        
        /// <summary>
        /// 只执行block改变状态的Update
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="blocks"></param>
        public void ChangeStageUpdate(BlockState newState,params Block[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                StateHandlers[newState].Update(block);
            }
        }
        
        
        // <summary>
        /// 通用状态改变处理
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="blocks"></param>
        public void ChangeState(BlockState newState,params PressureBlock[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                StateHandlers[newState].Enter(block);
                StateHandlers[newState].Update(block);
            }
        }
        
        /// <summary>
        /// 只执行block改变状态的进入
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="blocks"></param>
        public void ChangeStageEnter(BlockState newState,params PressureBlock[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                StateHandlers[newState].Enter(block);
            }
        }
        
        /// <summary>
        /// 只执行block改变状态的Update
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="blocks"></param>
        public void ChangeStageUpdate(BlockState newState,params PressureBlock[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                StateHandlers[newState].Update(block);
            }
        }
        
    }
}