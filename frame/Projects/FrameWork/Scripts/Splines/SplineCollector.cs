
using FrameWork.App;
using FrameWork.SceneManger;

namespace FrameWork.Splines
{

    public class SplineCollector : MonoSwitch
    {
        public override void OnAwake()
        {
            base.OnAwake();
            int i, length = transform.childCount;
            for (i = 0; i < length; i++)
            {
                Spline spline = transform.GetChild(i).GetComponent<Spline>();
                if (spline != null)
                {
                 //  YKApplication.Instance.  GetManager< SceneManager>().AddSpline(spline.id, spline);
                }
            }

        }

    }

}


