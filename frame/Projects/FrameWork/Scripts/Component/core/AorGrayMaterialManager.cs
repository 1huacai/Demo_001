using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Gray效果衍生材质共享管理器 (废弃... 因为置灰对象公用Material会造成诸多问题: 前后遮挡错误,Mask遮挡错误)
/// </summary>
[Obsolete]
public class AorGrayMaterialManager {

    private static AorGrayMaterialManager _instance;

    public static AorGrayMaterialManager Instance {
        get {
            if (_instance == null) {
                _instance = new AorGrayMaterialManager();
            }
            return _instance;
        }
    }
    private readonly Dictionary<string, Material> _NormalMatDic;
    private readonly Dictionary<string, Material> _GrayMatDic;
    private AorGrayMaterialManager() {
        _NormalMatDic = new Dictionary<string, Material>();
        _GrayMatDic = new Dictionary<string, Material>();
    }

    public void addMatToNormalDic(string MatName, Material material) {
        if (string.IsNullOrEmpty(MatName) || material == null)
            return;
        if (!_NormalMatDic.ContainsKey(MatName)) {
            _NormalMatDic.Add(MatName, material);
        }
    }

    public Material getMatFormNormalDic(string MatName) {
        if (string.IsNullOrEmpty(MatName))
            return null;
        if (_NormalMatDic.ContainsKey(MatName)) {
            return _NormalMatDic[MatName];
        }
        return null;
    }

    public void addMatToGrayDic(string MatName, Material material) {
        if (string.IsNullOrEmpty(MatName) || material == null)
            return;
        if (!_GrayMatDic.ContainsKey(MatName)) {
            _GrayMatDic.Add(MatName, material);
        }
    }

    public Material getMatFormGrayDic(string MatName) {
        if (string.IsNullOrEmpty(MatName))
            return null;
        if (_GrayMatDic.ContainsKey(MatName)) {
            return _GrayMatDic[MatName];
        }
        return null;
    }

}
