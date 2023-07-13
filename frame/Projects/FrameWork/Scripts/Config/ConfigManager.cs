using CoreFrameWork;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FrameWork
{
    /// <summary>
    /// 配置管理类
    /// </summary>
    public class ConfigManager : IConfigManager
    {
        private static Object lockobj = new object();
        private class ConfigSet
        {
            private class ConfigSheet
            {
                private byte[] rawBytes;
                private readonly Dictionary<long, Config> sheet;
                private Dictionary<long, int> posMap;
                private ushort totalCount;
                private Type configType;
                private BinaryBuffer bf;
                public int Count { get { return sheet.Count; } }
                public int TotalCount { get { return totalCount; } }

                internal ConfigSheet(Dictionary<long, int> map, byte[] bytes, Type type)
                {
                    rawBytes = bytes;
                    configType = type;
                    bf = new BinaryBuffer(bytes);
                    byte[] key;
                    bf.Read(out key);
                    bf.Decrypt(key, bf.Position);
                    totalCount = bf.ReadUInt16();
                    sheet = new Dictionary<long, Config>(totalCount);
                    posMap = new Dictionary<long, int>(totalCount);

                    ushort len = 0;
                    for (int i = 0; i < totalCount; i++)
                    {
                        bf.Read(out len);
                        int pos = bf.Position;
                        long id = bf.ReadInt64();
                        try
                        {
                            bf.Position += len - 8;
                            posMap.Add(id, pos);
                            map.Add(id, pos);
                        }
                        catch (Exception)
                        {
                            Log.Error(string.Format("Dump: type={0}, id={1},", type.Name, id));
                            throw;
                        }
                    }
                    bf.Position = 0;
                }

                public bool TryGetValue(long key, out Config value)
                {
                    lock (lockobj)
                    {
                        if (sheet.TryGetValue(key, out value))
                            return true;
                        int pos;
                        if (posMap.TryGetValue(key, out pos))
                        {
                            if (bf.Position != 0) //当前bf正在使用
                            {
                                var tempbf = new BinaryBuffer(rawBytes);
                                tempbf.Position = pos;
                                value = (Config)Activator.CreateInstance(configType, tempbf);
                            }
                            else
                            {
                                try
                                {
                                    bf.Position = pos;
                                    value = (Config)Activator.CreateInstance(configType, bf);
                                    bf.Position = 0;
                                }
                                catch (Exception e)
                                {
                                    Log.Error(configType.ToString());
                                }
                            }
                            sheet.Add(key, value);
                            return true;
                        }
                    }
                    return false;
                }

                public void Clear()
                {
                    sheet.Clear();
                    posMap.Clear();
                }

                public bool ContainsKey(long key)
                {
                    return posMap.ContainsKey(key);
                }

                public void Fill2Array<T>(T[] configs, ref int offset) where T : Config
                {
                    Config value;
                    foreach (var item in posMap)
                    {
                        if (TryGetValue(item.Key, out value))
                            configs[offset++] = (T)value;
                    }
                }

                public void DecodeAll()
                {
                    Config value;
                    foreach (var item in posMap)
                    {
                        TryGetValue(item.Key, out value);
                    }
                }
            }

            private readonly List<ConfigSheet> configs = new List<ConfigSheet>();
            private readonly Dictionary<long, int> id2pos_map = new Dictionary<long, int>();

            public void AddConfigSheet(byte[] bytes, Type type)
            {
                ConfigSheet sheet = new ConfigSheet(id2pos_map, bytes, type);
                configs.Add(sheet);
            }

            public void Clear()
            {
                for (int i = 0; i < configs.Count; i++)
                    configs[i].Clear();
                configs.Clear();
                id2pos_map.Clear();
            }

            public bool ContainsKey(long key)
            {
                return id2pos_map.ContainsKey(key);
            }

            public T GetConfig<T>(long id) where T : Config
            {
                return (T)GetConfig(id);
            }

            public Config GetConfig(long id)
            {
                Config temp = null;
                for (int i = 0; i < configs.Count; i++)
                {
                    if (configs[i].TryGetValue(id, out temp))
                        break;
                }
                return temp;
            }

            /*public Config[] GetConfigArray()
            {
                Config[] temp = new Config[id2pos_map.Count];
                int offset =  0;
                for (int i = 0; i < configs.Count; i++)
                    configs[i].Fill2Array(temp, ref offset);
                return temp;
            }*/

            public T[] GetConfigArray<T>() where T : Config
            {
                T[] temp = new T[id2pos_map.Count];
                int offset = 0;
                for (int i = 0; i < configs.Count; i++)
                    configs[i].Fill2Array(temp, ref offset);
                return temp;
            }

            public void DecodeAll()
            {
                for (int i = 0; i < configs.Count; i++)
                {
                    configs[i].DecodeAll();
                }
            }
        }

        /// <summary>
        /// 配置表字典
        /// </summary>
        private readonly Dictionary<int, ConfigSet> m_configDataDic = new Dictionary<int, ConfigSet>(256);

        #region 外部调用
        /// <summary>
        /// 是否打印错误提示
        /// </summary>
        public static bool IsDebug = true;

        private static ConfigManager _instance;

        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConfigManager();
                }
                return _instance;
            }
        }

        public void Clear()
        {
            m_configDataDic.Clear();
        }

        [Obsolete("this method just for test. do not invoke it in runtime!!!!")]
        /// <summary>
        /// this method just for test. do not invoke it in runtime!!!!
        /// </summary>
        public void DecodeAll()
        {
            foreach (var item in m_configDataDic)
            {
                item.Value.DecodeAll();
            }
        }

        #region 导入配置

        #region 导入二进制配置
        /// <summary>
        /// 导入二进制配置数据
        /// </summary>
        /// <param name="strInfo"></param>
        public void ImportBinaryInfo(Type type, byte[] readBytes, Type derivedType = null)
        {
            try
            {
                if (readBytes == null)
                {
                    Log.Error("导入字节配置表出错", "类型:" + derivedType == null ? type.Name : derivedType.Name, " 字节流是null");
                    return;
                }
                int keybase = type.GetHashCode();
                lock (lockobj)
                {
                    if (!m_configDataDic.ContainsKey(keybase))
                        m_configDataDic.Add(keybase, new ConfigSet());
                    if (derivedType != null)
                        m_configDataDic[derivedType.GetHashCode()] = m_configDataDic[keybase];
                    m_configDataDic[keybase].AddConfigSheet(readBytes, derivedType ?? type);
                }
            }
            catch (Exception e)
            {
                Log.Error("导入字节配置表出错", " 类型:" + type.Name, " error:" + e.Message);
            }
        }

        /// <summary>
        /// 导入二进制配置数据(泛型)
        /// </summary>
        /// <param name="strInfo"></param>
        public void ImportBinaryInfo<T>(byte[] readBytes) where T : Config
        {
            ImportBinaryInfo(typeof(T), readBytes, null);
        }

        /// <summary>
        /// 导入二进制配置数据(泛型)
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="readBytes"></param>
        public void ImportBinaryInfo<T1, T2>(byte[] readBytes) where T1 : Config where T2 : Config
        {
            ImportBinaryInfo(typeof(T1), readBytes, typeof(T2));
        }
        #endregion

        #endregion
        /// <summary>
        /// 获取配置表集合
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public Config[] GetConfigDic(Type type)
        {
            lock (lockobj)
            {
                ConfigSet configSet;
                if (type != null && m_configDataDic.TryGetValue(type.GetHashCode(), out configSet))
                    return configSet.GetConfigArray<Config>();
            }
            return null;
        }

        /// <summary>
        /// 获取配置表集合(泛型)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] GetConfigDic<T>() where T : Config
        {
            lock (lockobj)
            {
                ConfigSet configSet;
                if (m_configDataDic.TryGetValue(typeof(T).GetHashCode(), out configSet))
                    return configSet.GetConfigArray<T>();
            }
            return null;
        }

        /// <summary>
        /// 清除配置表集合(泛型) 慎用!!!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool ClearConfigDic<T>()
        {
            lock (lockobj)
            {
                ConfigSet configSet;
                if (m_configDataDic.TryGetValue(typeof(T).GetHashCode(), out configSet))
                {
                    configSet.Clear();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取某个配置表的配置
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Config GetConfigFromDic(Type type, long id)
        {
            if (type != null)
            {
                int key = type.GetHashCode();
                lock (lockobj)
                {
                    ConfigSet configSet;
                    if (m_configDataDic.TryGetValue(key, out configSet))
                    {
                        Config temp = configSet.GetConfig(id);
                        if (temp == null)
                        {
                            if (IsDebug)
                            {
                                var str = "can not find config with id=" + id + " in Type=" + type.Name;
                                if (id == 0)//兼容他们有些地方默认写个id为0但是又不自己过滤的情况
                                    Log.Info(str);
                                else
                                    Log.Error(str);
                            }
                            return null;
                        }
                        if (temp.HasVariants)
                            return (Config)temp.Clone();
                        return temp;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取某个配置表的配置(泛型)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetConfigFromDic<T>(long id) where T : Config
        {
            Type t = typeof(T);
            return (T)GetConfigFromDic(t, id);
        }

        public bool IsExistDic<T>()
        {
            return IsExistDic(typeof(T));
        }

        public bool IsExistDic(Type type)
        {
            if (type == null)
                return false;
            lock (lockobj)
            {
                int key = type.GetHashCode();
                return m_configDataDic.ContainsKey(key);
            }
        }

        public bool Contains<T>(long id) where T : Config
        {
            lock (lockobj)
            {
                int key = typeof(T).GetHashCode();
                return m_configDataDic.ContainsKey(key) && m_configDataDic[key].ContainsKey(id);
            }
        }
        #endregion
    }
}
