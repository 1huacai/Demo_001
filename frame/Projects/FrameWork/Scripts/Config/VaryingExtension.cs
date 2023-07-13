using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FrameWork
{
    //TODO:注意此处出于便利性考虑使用反射做通用实现,注意效率瓶颈问题
    //注意:要求子类所有字段readonly!

    /// <summary>
    /// 变形扩展
    /// </summary>
    public abstract class VaryingExtension
    {
        private static VaryingExtension _defaultEx;

        public static VaryingExtension DefaultEx
        {
            get
            {
                if (_defaultEx == null)
                {
                    _defaultEx = new DummyVaryingExtension();
                }
                return _defaultEx;
            }
        }

        //注意:默认方法只能处理子类字段全为值类型,对于包含引用类型的子类必须手动重写
        public override int GetHashCode()
        {
            FieldInfo[] fieldInfos = GetType().GetFields();

            int hash = 17;
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo field = fieldInfos[i];
                hash = hash*3 + field.GetValue(this).GetHashCode();
            }
            return hash;
        }

        //注意:默认方法只能处理子类字段全为值类型,对于包含引用类型的子类必须手动重写
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            FieldInfo[] fieldInfos = GetType().GetFields();

            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo field = fieldInfos[i];
                if (!field.GetValue(this).Equals(field.GetValue(obj)))
                {
                    return false;
                }
            }
            return true;
        }

        private class DummyVaryingExtension : VaryingExtension
        {

            public override int GetHashCode()
            {

                return 9527;
            }

            //注意:默认方法只能处理子类字段全为值类型,对于包含引用类型的子类必须手动重写
            public override bool Equals(object obj)
            {

                return true;
            }

        }
    }
}