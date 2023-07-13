using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AorUI框架获取创建Material处理器
/// 
/// author Aorition
/// 
/// </summary>
public class AorMaterialCreater {

    /// <summary>
    /// 获取shader的自定义注入方法
    /// </summary>
    public static Func<string,Shader> CustomGetShaderFunc;

    /// <summary>
    /// 获取Shader
    /// </summary>
    public static Shader GetShader(string ShaderName) {

        if (CustomGetShaderFunc != null) {
            return CustomGetShaderFunc(ShaderName);
        }

        //default
        Shader shader = Shader.Find(ShaderName);
        if (shader != null) {
            return shader;
        }
        return null;
    }

    public static Material CreateMaterial(string MatName, string ShaderName) {
        return CreateMaterial(MatName, GetShader(ShaderName));
    }

    public static Material CreateMaterial(string MatName, Shader shader) {
        if (shader != null) {
            Material m = new Material(shader);
            m.name = MatName;
            return m;
        }
        return null;
    }
}
