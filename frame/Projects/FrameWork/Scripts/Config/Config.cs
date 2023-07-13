using CoreFrameWork;
using System;
using System.Collections.Generic;

namespace FrameWork
{
    public interface IVaryingExtension
    {
        void ApplyVaryingExtension(VaryingExtension ex);
        //bool HasVariants { get; }
    }

    public interface ICloneAble
    {
        object Clone(bool face);
    }

    [Serializable]
    public class Config : ISerializable, IVaryingExtension, ICloneAble
    {
        [ConfigComment("ID")]
        public readonly long id;
        [ConfigComment("描述")]
        public readonly string describe;
        [ConfigComment("忽略(专为监修设定，0 不可见，1 可见)")]
        public readonly int ignore;
        [NonSerialized]
        protected HashSet<string> _assets;

        public HashSet<string> Assets
        {
            get
            {
                if (_assets == null)
                    _assets = new HashSet<string>();
                return _assets;
            }
        }

        public bool Ignore
        {
            get
            {
                return 0 != ignore;
            }
        }

        [ConfigComment("是否有变体字段（ConfigLevelVar）")]
        public bool HasVariants = false;

        public virtual object Clone(bool force = false)
        {
            if (this.HasVariants || force)
            {
                Config newconfig = new Config(this);
                return newconfig;
            }
            else
                return this;
        }

        public Config()
        {

        }

        public Config(BinaryBuffer bf)
        {
            this.id = bf.ReadInt64();
            this.describe = bf.ReadString();
            this.ignore = bf.ReadInt32();
        }

        public Config(Config config)
        {
            this.id = config.id;
            this.describe = config.describe;
            this.ignore = config.ignore;
            this.HasVariants = config.HasVariants;
        }

        public virtual void Serialize(BinaryBuffer buffer)
        {
            buffer.Write(this.id);
            buffer.Write(this.describe);
            buffer.Write(this.ignore);
        }

        public virtual void ApplyVaryingExtension(VaryingExtension ex)
        {

        }

        protected void FieldApplyVaryingExtension(IVaryingExtension extension, VaryingExtension ex)
        {
            if (extension != null && ex != null)
                extension.ApplyVaryingExtension(ex);
        }

        protected void FieldApplyVaryingExtension<T>(IList<T> extensions, VaryingExtension ex) where T : IVaryingExtension
        {
            if (extensions != null && ex != null)
                for (int i = 0; i < extensions.Count; i++)
                {
                    if (extensions[i] != null)
                        extensions[i].ApplyVaryingExtension(ex);
                }
        }

        protected void FieldApplyVaryingExtension<T>(IList<IList<T>> extensions, VaryingExtension ex) where T : IVaryingExtension
        {
            if (extensions != null && ex != null)
                for (int i = 0; i < extensions.Count; i++)
                {
                    if (extensions[i] != null)
                    {
                        for (int j = 0; j < extensions[i].Count; j++)
                        {
                            if (extensions[i][j] != null)
                                extensions[i][j].ApplyVaryingExtension(ex);
                        }
                    }
                }
        }

        protected T CloneField<T>(T sources, bool force = false) where T : ICloneAble
        {
            if (sources == null)
                return default(T);
            return (T)sources.Clone(force);
        }

        protected T[] CloneField<T>(T[] sources, bool force = false)
        {
            if (sources == null)
                return default(T[]);
            Type type = typeof(T);
            if (type.IsValueType || type == typeof(string))
                return (T[])sources.Clone();
            else
            {
                T[] temp = new T[sources.Length];
                for (int i = 0; i < sources.Length; i++)
                {
                    if (sources[i] is ICloneAble)
                        temp[i] = (T)(sources[i] as ICloneAble)?.Clone(force);
                    else
                    {
                        Log.Error("don't know how to clone the field. in Class = ", this.GetType().Name, "; Type = ", sources.GetType().Name);
                        break;
                    }
                }
                return temp;
            }
        }

        protected T[][] CloneField<T>(T[][] sources, bool force = false)
        {
            if (sources == null)
                return default(T[][]);
            T[][] temp = new T[sources.Length][];
            Type type = typeof(T);
            if (type.IsValueType || type == typeof(string))
            {
                for (int i = 0; i < sources.Length; i++)
                    temp[i] = (T[])sources[i].Clone();
                return temp;
            }
            else
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    for (int j = 0; j < sources[i].Length; j++)
                    {
                        if (sources[i][j] is ICloneAble)
                            temp[i][j] = (T)(sources[i][j] as ICloneAble)?.Clone(force);
                        else
                        {
                            Log.Error("don't know how to clone the field. in Class = ", this.GetType().Name, "; Type = ", sources.GetType().Name);
                            break;
                        }
                    }
                }
                return temp;
            }
        }

        protected T[][][] CloneField<T>(T[][][] sources, bool force = false)
        {
            if (sources == null)
                return default(T[][][]);
            T[][][] temp = new T[sources.Length][][];
            Type type = typeof(T);
            if (type.IsValueType || type == typeof(string))
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    temp[i] = new T[sources[i].Length][];
                    for (int j = 0; j < sources[i].Length; j++)
                    {
                        temp[i][j] = (T[])sources[i][j].Clone();
                    }
                }
                return temp;
            }
            else
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    for (int j = 0; j < sources[i].Length; j++)
                    {
                        for (int k = 0; k < sources[i][j].Length; k++)
                        {
                            if (sources[i][j][k] is ICloneAble)
                                temp[i][j][k] = (T)(sources[i][j][k] as ICloneAble)?.Clone(force);
                            else
                            {
                                Log.Error("don't know how to clone the field. in Class = ", this.GetType().Name, "; Type = ", sources.GetType().Name);
                                return temp;
                            }
                        }
                    }
                }
                return temp;
            }
        }

        protected List<T> CloneField<T>(List<T> sources, bool force = false)
        {
            if (sources == null)
                return null;
            Type type = typeof(T);
            if (type.IsValueType || type == typeof(string))
            {
                return new List<T>(sources);
            }
            else
            {
                List<T> temp = new List<T>(sources.Count);
                for (int i = 0; i < sources.Count; i++)
                {
                    if (sources[i] is ICloneAble)
                        temp.Add((T)(sources[i] as ICloneAble)?.Clone(force));
                    else
                    {
                        Log.Error("don't know how to clone the field. in Class = ", this.GetType().Name, "; Type = ", type.Name);
                        break;
                    }
                }
                return temp;
            }
        }

        protected void Add2Asset(string path)
        {
            if (!string.IsNullOrEmpty(path))
                Assets.Add(path);
        }

        protected void Add2Asset(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (!string.IsNullOrEmpty(paths[i]))
                    Assets.Add(paths[i]);
            }
        }

        protected virtual void Add2Asset()
        {

        }
    }
    [Serializable]
    public class ConfigCommentAttribute : Attribute
    {
        public readonly string comment;
        public ConfigCommentAttribute(string comment)
        {
            this.comment = comment;
        }
    }
    [Serializable]
    public class ConfigAssetAttribute : Attribute
    {
    }
    [Serializable]
    public class ConfigPathAttribute : Attribute
    {
    }

}
