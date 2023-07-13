using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FrameWork;

namespace T4Template.ConfigTemplates
{
    public partial class ConfigTemplate
    {
        private Type TargetType;
        private bool UseVarDouble;
        private List<FieldInfo> FieldInfos = new List<FieldInfo>();

        public ConfigTemplate(Type type, bool useVarDouble)
        {
            this.TargetType = type;
            this.UseVarDouble = useVarDouble;
            FieldInfos.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        }

        private bool IsConfigClass(Type type)
        {
            if (type.IsValueType)
                return false;
            while (type.BaseType != null)
            {
                if (type == typeof(Config))
                    return true;
                type = type.BaseType;
            }
            return false;
        }

        public string GetWriteMethod(FieldInfo fieldinfo)
        {
            Type fieldtype = fieldinfo.FieldType;
            var fieldName = fieldinfo.Name;
            if (fieldtype.IsArray)
            {
                int i = 0;
                while (fieldtype.IsArray)
                {
                    i++;
                    fieldtype = fieldtype.GetElementType();
                }
                var Astring = "".PadRight(i, 'A');
                if (fieldtype.IsEnum)
                    return "Enum" + Astring + "(this." + fieldName + ", t=>(long)t)";
                else if (IsConfigClass(fieldtype))
                    return "Config" + Astring + "(this." + fieldName + ")";
            }
            else if (fieldtype.IsGenericType)
            {
                fieldtype = fieldtype.GetGenericArguments()[0];
                if (fieldtype.IsEnum)
                    return "EnumList(this." + fieldName + ", t=>(long)t)";
                else if (IsConfigClass(fieldtype))
                    return "ConfigList(this." + fieldName + ")";
            }
            else if (fieldtype.IsEnum)
            {
                return "Enum" + "(this." + fieldName + ", t=>(long)t)";
            }
            else if (IsConfigClass(fieldtype))
            {
                return "Config" + "(this." + fieldName + ")";
            }
            return "(this." + fieldName + ")";
        }

        public string GetReadMethod(Type fieldtype)
        {
            if (fieldtype.IsArray)
            {
                int i = 0;
                while (fieldtype.IsArray)
                {
                    i++;
                    fieldtype = fieldtype.GetElementType();
                }
                var Astring = "".PadRight(i, 'A');
                if (fieldtype.IsEnum)
                {
                    return "Enum" + Astring + "(t=>(" + fieldtype.FullName.Replace('+','.') + ")t)";
                }
                else if (fieldtype.IsValueType || fieldtype == typeof(string))
                {
                    switch (fieldtype.Name)
                    {
                        case "Double":
                            return UseVarDouble ? "VarDouble" + Astring + "(ref HasVariants)" : "Double" + Astring + "()";
                        default:
                            return fieldtype.Name + Astring + "()";
                    }
                }
                else if (IsConfigClass(fieldtype))
                {
                    return "Config" + Astring + "<" + fieldtype.Name + ">(ref HasVariants)";
                }
            }
            else if (fieldtype.IsGenericType)
            {
                fieldtype = fieldtype.GetGenericArguments()[0];
                if (fieldtype.IsEnum)
                {
                    return "EnumList" + "(t=>(" + fieldtype.FullName.Replace('+', '.') + ")t)";
                }
                else if (fieldtype.IsValueType || fieldtype == typeof(string))
                {
                    switch (fieldtype.Name)
                    {
                        case "Double":
                            return UseVarDouble ? "VarDoubleList"  + "(ref HasVariants)" : "DoubleList"  + "()";
                        case "VarDouble":
                        case "VarDoubleA":
                        case "VarDoubleAA":
                            return fieldtype.Name + "List(ref HasVariants)";
                        default:
                            return fieldtype.Name  + "List()";
                    }
                }
                else if (fieldtype.IsClass)
                {
                    if (IsConfigClass(fieldtype))
                        return "ConfigList<" + fieldtype.Name + ">(ref HasVariants)";
                    switch (fieldtype.Name)
                    {
                        case "VarDouble":
                        case "VarDoubleA":
                        case "VarDoubleAA":
                            return fieldtype.Name + "List(ref HasVariants)";
                        default:
                            return fieldtype.Name + "List()";
                    }
                }
            }
            else if (fieldtype.IsEnum)
            {
                return "Enum(t=>(" + fieldtype.FullName.Replace('+', '.') + ")t)";
            }
            else if (fieldtype.IsValueType || fieldtype == typeof(string))
            {
                switch (fieldtype.Name)
                {
                    case "Double":
                        return UseVarDouble ? "VarDouble(ref HasVariants)" : "Double()";
                    case "VarDouble":
                    case "VarDoubleA":
                    case "VarDoubleAA":
                        return fieldtype.Name + "(ref HasVariants)";
                    default:
                        return fieldtype.Name + "()";
                }
            }
            else if (fieldtype.IsClass)
            {
                if (IsConfigClass(fieldtype))
                    return "Config<" + fieldtype.Name + ">(ref HasVariants)";
                switch (fieldtype.Name)
                {
                    case "VarDouble":
                    case "VarDoubleA":
                    case "VarDoubleAA":
                        return fieldtype.Name + "(ref HasVariants)";
                }
            }
            return "";
        }

        public string GetCloneMethod(FieldInfo fieldinfo)
        {
            var fieldtype = fieldinfo.FieldType;
            if (UseVarDouble && (fieldtype == typeof(VarDoubleA)
                || fieldtype == typeof(double)
                || fieldtype == typeof(double[])
                || fieldtype == typeof(double[][])
                || fieldtype == typeof(VarDouble)
                || fieldtype == typeof(VarDoubleAA)))
            { 
                return $"CloneField(instance.{fieldinfo.Name})";
            }
            else if (fieldtype.IsArray
                || fieldtype.IsGenericType
                || IsConfigClass(fieldtype))
            {
                return $"CloneField(instance.{fieldinfo.Name})";
            }
            return "instance." + fieldinfo.Name;
        }

        public bool IsVaryingExtensionField(Type fieldtype)
        {
            if (fieldtype.IsArray)
            {
                while (fieldtype.IsArray)
                {
                    fieldtype = fieldtype.GetElementType();
                }
                return IsVaryingExtensionField(fieldtype);
            }
            else if (fieldtype.IsGenericType)
            {
                fieldtype = fieldtype.GetGenericArguments()[0];
                return IsVaryingExtensionField(fieldtype);
            }
            else if (IsConfigClass(fieldtype))
            {
                return true;
            }
            else
            {
                if (fieldtype.Name.Contains("VarDouble"))
                    return true;
                if (UseVarDouble && fieldtype == typeof(double))
                    return true;
            }
            return false;
        }

        public bool IsAssetField(FieldInfo fieldinfo)
        {
            return fieldinfo.GetCustomAttributes(typeof(ConfigAssetAttribute), false).Length > 0;
        }

    }

}
