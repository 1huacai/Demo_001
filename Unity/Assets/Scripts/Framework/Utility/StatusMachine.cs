using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo.Utility
{
    public class FSMSystem
    {
        protected IFSMStatus mCurStatus;
        protected IFSMStatus mNextStatus;


        
        public virtual void ChangeStatus(IFSMStatus nextStatus)
        {
            mNextStatus = nextStatus;
        }

        public virtual void FixedUpdate(double fTick)
        {
            if(mNextStatus!=null)
            {
                if(mCurStatus!=null)
                {
                    mCurStatus.Exit();
                }
                mCurStatus = mNextStatus;
                mCurStatus.Enter();
                mNextStatus = null;
            }
            if (mCurStatus != null)
                mCurStatus.FixedUpdate(fTick);
        }
    }

    public interface IFSMStatus
    {
        void Init();
        void Enter();
        void Exit();
        void FixedUpdate(double fTick);

        string GetType();
    }
}
