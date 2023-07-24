using Project;

namespace Demo
{
    public class Statebase
    {
        protected GameManger _gameManger;

        public Statebase(GameManger manger)
        {
            _gameManger = manger;
        }

        public virtual void OnBlockOperation(int row, int col, BlockOperation operation)
        {
            
        }

        public virtual void OnDestroy()
        {
            _gameManger = null;
        }


        #region 状态逻辑更新

        public virtual void Enter(Block block)
        {
            
        }

        public virtual void Update(Block block)
        {
            
        }

        public virtual void Exit(Block block)
        {
            
        }

        #endregion
        
    }
}