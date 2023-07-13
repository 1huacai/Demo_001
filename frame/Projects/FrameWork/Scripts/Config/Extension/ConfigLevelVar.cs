using CoreFrameWork;
using System;
namespace FrameWork
{
    public class VarDouble : ISerializable, IVaryingExtension, IEquatable<VarDouble>, ICloneAble
    {
        public static readonly VarDouble Defaulte = new VarDouble();
        public double Value { get; private set; }
        public bool HasVariants { get; private set; }

        private int configID;

        public VarDouble()
        {
            this.Value = 0d;
            this.HasVariants = false;
            this.configID = 0;
        }

        public VarDouble(double value, int config = 0)
        {
            this.Value = value;
            this.configID = config;
            HasVariants = config > 0;
        }

        public VarDouble(VarDouble sources):this(sources.Value, sources.configID)
        {
        }

        public object Clone(bool force)
        {
            return new VarDouble(this);
        }

        public void Deserialize(BinaryBuffer r)
        {
            HasVariants = r.ReadBoolean();
            if (HasVariants)
                configID = r.ReadInt32();
            else
                Value = r.ReadDouble();
        }

        public void Serialize(BinaryBuffer w)
        {
            w.Write(HasVariants);
            if (HasVariants)
                w.Write(configID);
            else
                w.Write(Value);
        }

        public void ApplyVaryingExtension(VaryingExtension ex)
        {
            if (ex != null && configID > 0)
            {
                var config = ConfigManager.Instance.GetConfigFromDic<ConfigLevelVar>(configID);
                if (config != null)
                    Value = config.GetValue(ex);
            }
        }

        public bool Equals(VarDouble other)
        {
            return other.configID == this.configID && other.Value == this.Value;
        }

        public static implicit operator double(VarDouble varDouble)
        {
            return varDouble.Value;
        }

        public static implicit operator VarDouble(int varDouble)
        {
            return new VarDouble() { Value = varDouble, configID = 0 };
        }

        public static implicit operator VarDouble(double varDouble)
        {
            return new VarDouble() { Value = varDouble, configID = 0 };
        }

        public static VarDouble Pare(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                int id;
                double value;
                if (data.StartsWith("@level:"))
                {
                    string idStr = data.Substring(7);

                    if (int.TryParse(idStr, out id))
                        return new VarDouble(0, id);
                }
                if (data.StartsWith("@-level:"))
                {
                    string idStr = data.Substring(8);
                    if (int.TryParse(idStr, out id))
                        return new VarDouble(0, id);
                }
                data = data.Replace('_', '-');
                if (double.TryParse(data, out value))
                    return new VarDouble(value, 0);
            }
            return Defaulte;
        }
    }

    public class VarDoubleA : ISerializable, IVaryingExtension, ICloneAble
    {
        public static readonly VarDoubleA Defaulte = new VarDoubleA();
        public double[] Value { get; private set; }
        private int[] configIDs;
        public bool HasVariants { get; private set; }

        public VarDoubleA():this(null, null)
        {
        }

        public VarDoubleA(double[] value, int[] configs = null)
        {
            if (value == null)
            {
                Value = new double[0];
                configIDs = new int[0];
                return;
            }
            if (configs != null && value.Length != configs.Length)
                throw new System.ArgumentException("value and configs length not matched!");
            Value = (double[])value.Clone();
            configIDs = configs == null ? new int[value.Length] : (int[])configs.Clone();
            HasVariants = false;
            for (int i = 0; i < configIDs.Length; i++)
            {
                if (configIDs[i] > 0)
                {
                    HasVariants = true;
                    break;
                }
            }
        }

        public VarDoubleA(VarDoubleA sources)
        {
            Value = (double[])sources.Value.Clone();
            configIDs = (int[])sources.configIDs.Clone();
            HasVariants = sources.HasVariants;
        }

        public object Clone(bool force)
        {
            return new VarDoubleA(this);
        }

        public double this[int index]
        {
            get
            {
                if (index < 0 || index > Value.Length - 1)
                    throw new IndexOutOfRangeException("VarDoubleA length = " + Value.Length + " index = " + index);
                return Value[index];
            }
            set
            {
                if (index < 0 || index > Value.Length - 1)
                    throw new IndexOutOfRangeException("VarDoubleA length = " + Value.Length + " index = " + index);
                Value[index] = value;
            }
        }

        public void Deserialize(BinaryBuffer r)
        {
            byte len = r.ReadByte();
            Value = new double[len];
            configIDs = new int[len];
            for (int i = 0; i < len; i++)
            {
                bool isConfig = r.ReadBoolean();
                if (isConfig)
                    configIDs[i] = r.ReadInt32();
                else
                    Value[i] = r.ReadDouble();
                HasVariants |= isConfig;
            }
        }

        public void Serialize(BinaryBuffer w)
        {
            int len = Value == null ? 0 : Value.Length;
            w.Write((byte)len);
            for (int i = 0; i < len; i++)
            {
                bool isConfig = configIDs[i] > 0;
                w.Write(isConfig);
                if (isConfig)
                    w.Write(configIDs[i]);
                else
                    w.Write(this.Value[i]);
            }
        }

        public void ApplyVaryingExtension(VaryingExtension ex)
        {
            if (configIDs != null && ex != null)
            {
                for (int i = 0; i < configIDs.Length; i++)
                {
                    if (configIDs[i] > 0)
                    {
                        var config = ConfigManager.Instance.GetConfigFromDic<ConfigLevelVar>(configIDs[i]);
                        config.ApplyVaryingExtension(ex);
                        Value[i] = config.GetValue(ex);
                    }
                }
            }
        }

        public static implicit operator double[] (VarDoubleA varNumber)
        {
            return varNumber.Value;
        }

        public static VarDoubleA Pare(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                string[] v1 = data.Split('+');
                double[] values = new double[v1.Length];
                int[] configs = new int[v1.Length];

                int id;
                double value;

                for (int i = 0; i < v1.Length; i++)
                {
                    if (v1[i].StartsWith("@level:"))
                    {
                        string idStr = v1[i].Substring(7);
                        if (int.TryParse(idStr, out id))
                            configs[i] = id;
                    }
                    else if (v1[i].StartsWith("@-level:"))
                    {
                        string idStr = v1[i].Substring(8);
                        if (int.TryParse(idStr, out id))
                            configs[i] = id;
                    }
                    else
                    {
                        v1[i] = v1[i].Replace('_', '-');
                        if (double.TryParse(v1[i], out value))
                            values[i] = value;
                    }
                }
                return new VarDoubleA(values, configs);
            }
            return Defaulte;
        }
    }

    public class VarDoubleAA : ISerializable, IVaryingExtension, ICloneAble
    {
        public static readonly VarDoubleAA Defaulte = new VarDoubleAA();
        public double[][] Value { get; private set; }
        private int[][] configIDs;
        public bool HasVariants { get; private set; }

        public VarDoubleAA():this(null, null)
        {
        }

        public VarDoubleAA(VarDoubleAA sources):this(sources.Value, sources.configIDs)
        {
            //Value = (double[][])sources.Value.Clone();
            //configIDs = (int[][])sources.configIDs.Clone();
            //HasVariants = sources.HasVariants;
        }

        public VarDoubleAA(double[][] value, int[][] configs = null)
        {
            if (value == null)
            {
                Value = new double[0][];
                configIDs = new int[0][];
                return;
            }
            if (configs != null && value.Length != configs.Length)
                throw new System.ArgumentException("value and configs length not matched!");
            HasVariants = false;
            Value = new double[value.Length][];
            configIDs = new int[value.Length][];
            for (int i = 0; i < Value.Length; i++)
            {
                Value[i] = (double[])value[i].Clone();
                configIDs[i] = configs == null ? new int[value[i].Length] : (int[])configs[i].Clone();
            }
            for (int i = 0; i < configIDs.Length; i++)
            {
                for (int j = 0; j < configIDs[i].Length; j++)
                {
                    if (configIDs[i][j] > 0)
                    {
                        HasVariants = true;
                        break;
                    }
                }
            }
        }

        public object Clone(bool force)
        {
            return new VarDoubleAA(this);
        }

        public void Deserialize(BinaryBuffer r)
        {
            byte len = r.ReadByte();
            Value = new double[len][];
            configIDs = new int[len][];
            for (int i = 0; i < len; i++)
            {
                byte len1 = r.ReadByte();
                Value[i] = new double[len1];
                configIDs[i] = new int[len1];
                for (int j = 0; j < len1; j++)
                {
                    bool isConfig = r.ReadBoolean();
                    if (isConfig)
                        configIDs[i][j] = r.ReadInt32();
                    else
                        Value[i][j] = r.ReadDouble();
                    HasVariants |= isConfig;
                }
            }
        }

        public double[] this[int index]
        {
            get
            {
                if (index < 0 || index > Value.Length - 1)
                    throw new IndexOutOfRangeException("VarDoubleA length = " + Value.Length + " index = " + index);
                return Value[index];
            }
            set
            {
                if (index < 0 || index > Value.Length - 1)
                    throw new IndexOutOfRangeException("VarDoubleA length = " + Value.Length + " index = " + index);
                Value[index] = value;
            }
        }

        public void Serialize(BinaryBuffer w)
        {
            int len = Value == null ? 0 : Value.Length;
            w.Write((byte)len);
            for (int i = 0; i < len; i++)
            {
                VarDoubleA doubleA = new VarDoubleA(Value[i], configIDs[i]);
                doubleA.Serialize(w);
            }
        }

        public void ApplyVaryingExtension(VaryingExtension ex)
        {
            if (configIDs != null && ex != null)
            {
                for (int i = 0; i < configIDs.Length; i++)
                {
                    for (int j = 0; j < configIDs[i].Length; j++)
                    {
                        if (configIDs[i][j] > 0)
                        {
                            var config = ConfigManager.Instance.GetConfigFromDic<ConfigLevelVar>(configIDs[i][j]);
                            config.ApplyVaryingExtension(ex);
                            Value[i][j] = config.GetValue(ex);
                        }
                    }
                }
            }
        }

        public static implicit operator double[][] (VarDoubleAA varNumber)
        {
            return varNumber.Value;
        }

        public static VarDoubleAA Pare(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                string[] v1 = data.Split('|');
                double[][] values = new double[v1.Length][];
                int[][] configs = new int[v1.Length][];
                int id;
                double value;

                for (int i = 0; i < v1.Length; i++)
                {
                    string[] v2 = v1[i].Split('+');
                    values[i] = new double[v2.Length];
                    configs[i] = new int[v2.Length];
                    for (int j = 0; j < v2.Length; j++)
                    {
                        if (v2[j].StartsWith("@level:"))
                        {
                            string idStr = v2[j].Substring(7);
                            if (int.TryParse(idStr, out id))
                                configs[i][j] = id;
                        }
                        else if (v2[j].StartsWith("@-level:"))
                        {
                            string idStr = v2[j].Substring(8);
                            if (int.TryParse(idStr, out id))
                                configs[i][j] = id;
                        }
                        else
                        {
                            v2[j] = v2[j].Replace('_', '-');
                            if (double.TryParse(v2[j], out value))
                                values[i][j] = value;
                        }
                    }
                }
                return new VarDoubleAA(values, configs);
            }
            return Defaulte;
        }
    }
    /// <summary>
    /// 用于附加等级计算的专用类，使用格式为@level:id，作用类型为double
    /// </summary>
    public class ConfigLevelVar : ConfigVaryingNumber
    {
        [ConfigComment("名称")] 
        public readonly string Name;
        [ConfigComment("等级初始值")] 
        public readonly double FirstItem;
        [ConfigComment("等级公差")]
        public readonly double Tolerance;
        [ConfigComment("等级数组")]
        public readonly double[] ExtendArray;

        public override double GetValue(VaryingExtension ex)
        {
            int index = 0;
//            int grade = 0;

            if (ex is LevelVaryingExtension)
            {
                LevelVaryingExtension lvEx = (LevelVaryingExtension) ex;
                //TODO:等级关联默认都为0
                index = lvEx.Lv[0] - 1;
//                grade = lvEx.Grade - 1;
            }

            if (index < 0) return FirstItem;

            double result = FirstItem + Tolerance * (index);

            if (ExtendArray != null && ExtendArray.Length > 0)
                result += ExtendArray[MYMath.Clamp(index, 0, ExtendArray.Length - 1)];

            return result;
        }

        public double GetValue(int grade, params int[] lv)
        {
//            return GetValue(new LevelVaryingExtension(lv, 0));
            return GetValue(new LevelVaryingExtension(lv));
        }

        public ConfigLevelVar() { }

        public ConfigLevelVar(BinaryBuffer buffer):base(buffer)
        {
            this.FirstItem = buffer.ReadDouble();
            this.Tolerance = buffer.ReadDouble();
            this.ExtendArray = buffer.ReadDoubleA();
        }

        public override void Serialize(BinaryBuffer buffer)
        {
            base.Serialize(buffer);
            buffer.Write(this.FirstItem);
            buffer.Write(this.Tolerance);
            buffer.Write(this.ExtendArray);
        }
    }

    public class LevelVaryingExtension : VaryingExtension
    {
        public readonly int[] Lv;
//        public readonly int Grade;

//        public LevelVaryingExtension(int[] lv, int grade)
//        {
//            Lv = lv;
//            Grade = grade;
//        }

        public LevelVaryingExtension(params int[] lv)
        {
            Lv = lv;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = 0; i < Lv.Length; i++)
            {
                hash = hash * 3 + Lv[i].GetHashCode();
            }

//            hash = hash * 3 + Grade.GetHashCode();

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            LevelVaryingExtension ins = (LevelVaryingExtension)obj;

            if (Lv.Length != ins.Lv.Length)
            {
                return false;
            }
            for (int i = 0; i < Lv.Length; i++)
            {
                if (!Lv[i].Equals(ins.Lv[i]))
                {
                    return false;
                }
            }

//            if (!Grade.Equals(ins.Grade))
//            {
//                return false;
//            }

            return true;
        }
    }
}