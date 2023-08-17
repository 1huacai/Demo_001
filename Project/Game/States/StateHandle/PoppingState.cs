using Project;
using UnityEngine;

namespace Demo
{
    public class PoppingState : Statebase
    {
        public PoppingState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if(block == null)
                return;
            if (block.Shape == BlockShape.None)
                return;
            //己方棋子
            block.State = BlockState.Popping;
            block._Animation.PlayAnimation(string.Format("{0}_{1}",block.Shape,block.State),1,false);
            
            //对手棋子
            var otherBlock = OtherGameController.Inst.blockMatrix[block.Row, block.Col - 1];
            otherBlock._Animation.PlayAnimation(string.Format("{0}_{1}",block.Shape,block.State),1,false);
        }

        public override void Update(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            timerID = TimerMgr._Instance.Schedule(
                () =>
                {
                    //对手棋子
                    var otherBlock = OtherGameController.Inst.blockMatrix[block.Row, block.Col - 1];
                    otherBlock._Animation.StopAnimation(()=>{otherBlock.ResetOriginImg();});
                    
                    //己方棋子
                    block._Animation.StopAnimation(()=>{block.ResetOriginImg();});
                    StateManger._instance.ChangeState(BlockState.Popped, block);
                },
                ConstValues.poppingFps * ConstValues.fpsTime);
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
        }
    }
}