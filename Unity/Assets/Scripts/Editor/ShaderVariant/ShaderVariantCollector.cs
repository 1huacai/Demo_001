/* 
 * ==============================================================================
 * Filename: ShaderVariantCollector
 * Created:  2021 / 7 / 20 15:30
 * Author: HuaHua
 * Purpose: shader 变体 自动 收集
 * ==============================================================================
**/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class ShaderVariantCollector : Editor
{
    public static readonly string ShaderVariantPath = "Assets/Resources/ShaderCollector";
    static readonly string ResPath = "Assets/Resources";


    [MenuItem("Tools/Shader/生成shader变体")]
    public static void CollectShaders()
    {
        try
        {
            DoCollect();
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message} {e.StackTrace}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    [MenuItem("Tools/Shader/删除所有shader变体")]
    public static void ClearShaderVariant()
    {
        SafeClearDir(ShaderVariantPath);
        Debug.Log("Delete all shader variant completed!");
    }

    static void CreateDir(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }
    static void DeleteDirectory(string dirPath)
    {
        string[] files = Directory.GetFiles(dirPath);
        string[] dirs = Directory.GetDirectories(dirPath);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(dirPath, false);
    }
    static bool SafeClearDir(string folderPath)
    {
        try
        {
            if (Directory.Exists(folderPath))
            {
                DeleteDirectory(folderPath);
            }
            Directory.CreateDirectory(folderPath);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("SafeClearDir failed! path = {0} with err = {1}", folderPath, ex.Message));
            return false;
        }

    }

    static MethodInfo GetShaderVariantEntries = null;
    public struct ShaderVariantData
    {
        public int[] passTypes;
        public string[] keywordLists;
        public string[] remainingKeywords;
    }

    private static ShaderVariantData GetShaderVariantEntriesFiltered(Shader shader, string[] SelectedKeywords)
    {
        if (GetShaderVariantEntries == null)
        {
            GetShaderVariantEntries = typeof(ShaderUtil).GetMethod("GetShaderVariantEntriesFiltered", BindingFlags.NonPublic | BindingFlags.Static);
        }

        int[] types = null;
        string[] keywords = null;
        string[] remainingKeywords = null;

        object[] args = new object[]{
            shader,
            32,
            SelectedKeywords,
            new ShaderVariantCollection(),
            types,
            keywords,
            remainingKeywords
        };
        GetShaderVariantEntries?.Invoke(null, args);

        var passTypes = new List<int>();
        var allTypes = (args[4] as int[]);
        if (allTypes != null)
        {
            foreach (var type in allTypes)
            {
                if (!passTypes.Contains(type))
                {
                    passTypes.Add(type);
                }
            }
        }


        ShaderVariantData svd = new ShaderVariantData()
        {
            passTypes = passTypes.ToArray(),
            keywordLists = args[5] as string[],
            remainingKeywords = args[6] as string[]
        };

        return svd;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static Dictionary<string, List<Material>> FindAllMaterials(string path)
    {
        var materials = AssetDatabase.FindAssets("t:Material", new string[] { path });

        int idx = 0;
        var matDic = new Dictionary<string, List<Material>>();
        foreach (var guid in materials)
        {
            var matPath = AssetDatabase.GUIDToAssetPath(guid);
            EditorUtility.DisplayProgressBar($"Collect Shader {matPath}", "Find All Materials", (float)idx++ / materials.Length);
            var mat = AssetDatabase.LoadMainAssetAtPath(matPath) as Material;
            if (mat)
            {
                if (matDic.TryGetValue(mat.shader.name, out var list) == false)
                {
                    list = new List<Material>();
                    matDic.Add(mat.shader.name, list);
                }
                list.Add(mat);
            }
        }
        return matDic;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="increment">是否增量打包</param>
    public static void DoCollect(bool increment = true)
    {
        if (increment)
        {
            CreateDir(ShaderVariantPath);
        }
        else
        {
            ClearShaderVariant();
        }

        //---------------------------------------------------------------
        // find all materials
        var matDic = FindAllMaterials(ResPath);

        //---------------------------------------------------------------
        // collect all key words
        Dictionary<string, Dictionary<string, Material>> finalMats = new Dictionary<string, Dictionary<string, Material>>();
        List<string> temp = new List<string>();
        int idx = 0;
        foreach (var item in matDic)
        {
            var shaderName = item.Key;
            EditorUtility.DisplayProgressBar($"Collect Shader {shaderName}", "Collect Key words", (float)idx++ / matDic.Count);
            if (finalMats.TryGetValue(shaderName, out var matList) == false)
            {
                matList = new Dictionary<string, Material>();
                finalMats.Add(shaderName, matList);
            }

            foreach (var mat in item.Value)
            {
                temp.Clear();
                string[] keyWords = mat.shaderKeywords;
                temp.AddRange(keyWords);

                if (mat.enableInstancing)
                {
                    temp.Add("enableInstancing");
                }

                if (temp.Count == 0)
                {
                    continue;
                }

                temp.Sort();
                string pattern = string.Join("_", temp);
                if (!matList.ContainsKey(pattern))
                {
                    matList.Add(pattern, mat);
                }
            }
        }

        //---------------------------------------------------------------
        // collect all variant
        idx = 0;
        foreach (var kv in finalMats)
        {
            var shaderFullName = kv.Key;
            var matList = kv.Value;

            if (matList.Count == 0)
            {
                continue;
            }

            EditorUtility.DisplayProgressBar($"Collect Shader {shaderFullName}", "General Shader Variant", (float)idx++ / finalMats.Count);
            if (shaderFullName.Contains("InternalErrorShader"))
            {
                continue;
            }

            var shader = Shader.Find(shaderFullName);
            var path = $"{ShaderVariantPath}/{shaderFullName.Replace("/", "_")}.shadervariants";
            bool alreadyExsit = true;
            var shaderCollection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(ShaderVariantPath+ "ShaderVariant.shadervariants");
            if (shaderCollection == null)
            {
                alreadyExsit = false;
                shaderCollection = new ShaderVariantCollection();
            }


            foreach (var kv2 in matList)
            {
                var svd = GetShaderVariantEntriesFiltered(shader, kv2.Value.shaderKeywords);

                foreach (var passType in svd.passTypes)
                {
                    var shaderVariant = new ShaderVariantCollection.ShaderVariant()
                    {
                        shader = shader,
                        passType = (UnityEngine.Rendering.PassType)passType,
                        keywords = kv2.Value.shaderKeywords,
                    };
                    if (!shaderCollection.Contains(shaderVariant))
                    {
                        shaderCollection.Add(shaderVariant);
                    }
                }
            }

            if (alreadyExsit)
            {
                EditorUtility.SetDirty(shaderCollection);
            }
            else
            {
                AssetDatabase.CreateAsset(shaderCollection, ShaderVariantPath + "/ShaderVariant.shadervariants");
            }
        }

        //---------------------------------------------------------------
        // save
        EditorUtility.DisplayProgressBar("Collect Shader", "Save Assets", 1f);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        EditorUtility.ClearProgressBar();

        Debug.Log("Collect all shader variant completed!");
    }
}