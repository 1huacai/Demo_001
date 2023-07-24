using Project;

namespace Demo
{
    public class MatchedState:Statebase
    {
        public MatchedState(GameManger manger) : base(manger)
        {
            
        }

        public override void Enter(Block block)
        {
            base.Enter(block);
            block.State = BlockState.Matched;
           
        }

        public override void Update(Block block)
        {
            base.Update(block);
            // if (matchedTimer == 0)
            // {
            //     //匹配时间结束，退出当前状态,进入下一状态
            //     Exit(block);
            // }
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
            StateManger._instance.ChangeState(BlockState.Popping,block);
        }
    }
}