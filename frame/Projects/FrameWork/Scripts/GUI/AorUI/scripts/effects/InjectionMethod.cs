using System;
using UnityEngine;
using UnityEngine.UI;

namespace FrameWork.GUI.AorUI.effects {
    public static class InjectionMethod
    {
        /// <summary>
        /// 变灰
        /// </summary>
        /// <param name="button"></param>
        /// <param name="bo"></param>
        public static void SetGray(this Image image, bool bo)
        {
            Material mat = new Material(image.material);
            mat.name = image.GetHashCode() + "_Gray";

            if (bo)
            {
                mat.SetFloat("_Gray", 1);

            }
            else
            {
                mat.SetFloat("_Gray", 0);
            }

            image.material = mat;
            image.SetMaterialDirty();

        }
        /// <summary>
        /// 变灰
        /// </summary>
        /// <param name="button"></param>
        /// <param name="bo"></param>
        public static void SetGray(this RawImage image, bool bo)
        {
            Material mat = new Material(image.material);
            mat.name = image.GetHashCode() + "_Gray";

            if (bo)
            {
                mat.SetFloat("_Gray", 1);

            }
            else
            {
                mat.SetFloat("_Gray", 0);
            }

            image.material = mat;
            image.SetMaterialDirty();

        }

    }
}
