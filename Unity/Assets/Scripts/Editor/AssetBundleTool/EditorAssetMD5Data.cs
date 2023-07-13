using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using System.IO;


public  enum MD5StateType
{
    /// <summary>
    ///  新增
    /// </summary>
    New = 0,
    /// <summary>
    /// 修改
    /// </summary>
    Modify,
    /// <summary>
    /// 删除
    /// </summary>
    Delet,

    /// <summary>
    /// 重打
    /// </summary>
    RePack,
    /// <summary>
    /// 
    /// </summary>
    Other,

    end,
}
public class EditorAssetMD5Data
{
    public string Path;
    public string MD5;
    public MD5StateType state = MD5StateType.end;

    public EditorAssetMD5Data( )
    {

    }
    public EditorAssetMD5Data(string path, string md5)
    {
        Path = path;
        MD5 = md5;
    }
    public EditorAssetMD5Data(string path, string md5, MD5StateType state )
    {
        Path = path;
        MD5 = md5;
        this.state = state;
    }

}
