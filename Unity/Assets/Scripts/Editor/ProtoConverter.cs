#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// protobuf文件转换
/// </summary>
public class ProtoConverter
{
    private static readonly string SRC_DIR_ABS_PATH = Application.dataPath + "/Resources/PbProto/";    //proto文件夹
    private static readonly string DST_DIR_ABS_PATH = Application.dataPath + "/Resources/PBData/";       //pb文件夹
    private const string dstExt = ".bytes";       //pb文件后缀名
    private static string cmd = EditorHelper.AppPath + "Tools/zhanguo/protoc";        //cmd命令
    private static string annotationCmd = EditorHelper.AppPath + "Tools/ProtoAutoLuaGen/exportluaprotostructure";        //生成注释cmd命令
    private const string arg = " -I=\"{0}\" --descriptor_set_out=\"{1}\" \"{2}\"";  //cmd参数格式

    private static string PROTO_BAR_NAME = "编译Proto文件";
    /// <summary>
    /// 强制全面转换一次
    /// </summary>
    //[MenuItem("Proto编译/编译Proto文件")]
    public static void ConvertAll()
    {
        if (Directory.Exists(DST_DIR_ABS_PATH))
        {
            Directory.Delete(DST_DIR_ABS_PATH, true);
        }
        Directory.CreateDirectory(DST_DIR_ABS_PATH);
        string[] files = Directory.GetFiles(SRC_DIR_ABS_PATH, "*.proto", SearchOption.AllDirectories);
        int count = files.Length;
        for (int i = 0; i < count; i++)
        {
            string srcAbsPath = files[i];
            string srcPureName = Path.GetFileNameWithoutExtension(srcAbsPath);
            string dstAbsPath = DST_DIR_ABS_PATH + srcPureName + dstExt;
            EditorHelper.ProcessCommand(cmd, string.Format(arg, SRC_DIR_ABS_PATH, dstAbsPath, srcAbsPath));
            EditorUtility.DisplayProgressBar(PROTO_BAR_NAME, string.Format(PROTO_BAR_NAME + ":{0}/{1}", i, count), (float)i / count);
     
        }
        Application.OpenURL(annotationCmd);
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        Debug.Log("Convert All Protos, Done!");
    }
}
#endif