using Project;

namespace Demo
{
    public class HoveringState:Statebase
    {
        public HoveringState(GameManger manger) : base(manger)
        {
        }

        public override void Enter(Block block)
        {
            base.Enter(block);
            block.State = BlockState.Hovering;
        }

        public override void Update(Block block)
        {
            base.Update(block);
            timerID = TimerMgr._Instance.Schedule(() =>
            {
                var downBlock = _gameManger.blockMatrix[block.Row - 1, block.Col - 1];
                if (downBlock.type == BlockType.None)
                {
                    StateManger._instance.ChangeState(BlockState.Falling, block);
                }
                else if (downBlock.type != BlockType.None && downBlock.State == BlockState.Hovering)
                {
                    StateManger._instance.ChangeState(BlockState.Hovering, block);
                }
                else if (downBlock.type != BlockType.None && downBlock.State == BlockState.Normal)
                {
                    StateManger._instance.ChangeState(BlockState.Landing, block);
                }

                Exit(block);
            }, ConstValues.hoveringFps * ConstValues.fpsTime);
           
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
        }
    }
}