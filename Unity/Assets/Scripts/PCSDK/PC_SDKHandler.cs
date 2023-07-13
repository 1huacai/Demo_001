using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Net;
using System.Text;
using System.IO;
using MiniJSON;
//using Newtonsoft.Json;

public class PC_SDKHandler
{
    private static PC_SDKHandler m_inst;
    public static PC_SDKHandler Inst
    {
        get
        {
            if (null == m_inst)
            {
                m_inst = new PC_SDKHandler();
            }
            return m_inst;
        }
    }
    public Dictionary<string, MethodInfo> m_MethodDic;
    private PC_AppInfo m_PC_AppInfo;

    /// <summary>
    /// 命令行参数
    /// </summary>
    public PC_AppInfo PC_AppInfo
    {
        get
        {
            if (null == m_PC_AppInfo)
            {
                string[] arguments = Environment.GetCommandLineArgs();
                string _auth_key = "";
                string _uid = "";
                string _extraInfo = "";
                foreach (string arg in arguments)
                {
                    if (arg.StartsWith("auth_key:"))
                    {
                        _auth_key = arg.Replace("auth_key:", "");
                    }
                    if (arg.StartsWith("uid:"))
                    {
                        _uid = arg.Replace("uid:", "");
                    }
                    if (arg.StartsWith("extraInfo:"))
                    {
                        _extraInfo = arg.Replace("extraInfo:", "");
                    }
                }
                m_PC_AppInfo = new PC_AppInfo(_auth_key, _uid, _extraInfo);
            }
            return m_PC_AppInfo;
        }
    }

    protected PC_SDKHandler()
    {
        m_MethodDic = new Dictionary<string, MethodInfo>();
        MethodInfo[] infos = GetType().GetMethods();
        for (int i = 0; i < infos.Length; i++)
        {
            if (infos[i].Name.Substring(0, 1) == "_")
            {
                if (!m_MethodDic.ContainsKey(infos[i].Name))
                {
                    m_MethodDic.Add(infos[i].Name, infos[i]);
                }
                else
                {
                    Debug.LogError(infos[i].Name);
                }
            }
        }

    }

    public void ExcuteFunction(string funcName, params object[] args)
    {
        MethodInfo _info = null;
        if (m_MethodDic.TryGetValue(funcName, out _info))
        {
            _info.Invoke(null, args);
        }
    }

    #region 相关API

    public static void _downDynamicUpdate() { }
    public static void _getDynamicUpdate(string type) { }
    public static void _gameStepInfo(string step) { }
    public static void _gameStepInfoFlag(string step, string flag) { }

    /// <summary>
    /// 进入游戏后验证一次大厅账户的合法性
    /// </summary>
    /// <param name="qid"></param>
    /// <param name="time"></param>
    /// <param name="sign"></param>
    /// <param name="server_id"></param>
    /// <param name="isAdult"></param>
    public static void _login(long qid, int time, string sign, string server_id = "S1", int isAdult = 1)
    {

        string _url = @"http://domain/[login]?qid=" + qid +
            "&server_id=" + server_id +
            "& time=" + DateTime.Now.Second +
            "&sign=" + sign +
            "& isAdult=" + isAdult;

        string _result = DoGetRequestSendData(_url);

        Dictionary<string, object> jsonData = Json.Decode(_result) as Dictionary<string, object>;
        string head = jsonData["errno"].ToString();
        string errmsg = jsonData["errmsg"].ToString();
        Debug.LogError(errmsg);
        if (head != "0")
        {
            return;
        }
        Dictionary<string, object> data = Json.Decode(jsonData["data"].ToString()) as Dictionary<string, object>;
        string uid = data["uid"].ToString();
        string auth_key = data["auth_key"].ToString();
        string zone = data["zone"].ToString();
        string extraInfo = data["extraInfo"].ToString();

    }
    public static void _loginEx(string serverSid, int flags)
    {
        CallMethod("login_success", "");

    }
    public static void _getServerList()
    {
        string _url = "http://sgzcenter.youkia.net/index.php/p371/LoginThree/pid/371/gid/45/o_system/android";
        string _result = DoGetRequestSendData(_url);

        if (_result != "error")
        {
            CallMethod("get_server_list_success", _result);
        }
        else
        {
            CallMethod("get_server_list_failed", _result);
        }
    }
    public static void _loginServer(string serverSid)
    {

    }

    public static void _loginout()
    {

    }


    public static void _startGame()
    {
        Debug.LogError("开始游戏");
    }

    public static void _enterGame(string uid, string nickName, string lv, string serverId, string serverName, string ext) { }

    public static void _createRole(string uid, string nickName, string lv, string serverId, string serverName, string ext) { }

    public static void _pushNotification(string content, int seconds, int id) { }

    public static void _cleanAllNotifi() { }

    public static void _cleanNotifi(int id) { }

    public static void _getAppInfo() { }

    public static void _buy(string productld) { }

    public static void _getGoodsList() { }

    public static void _sharePlatform(int num, string title, string message) { }

    public static void _getMaintainNotice() { }

    public static void _gameRoleInfo(string roleId, string roleName, string roleLevel, string serverId, string serverName, string extras, string createTime) { }

    public static void _setClipboard(string str) { }


    public static void _startRecordVedio(string url, string quality, string vedioMaxTime) { }

    public static void _stopRecordVedio() { }

    public static void _playVedio(string vedioUrl) { }


    public static void _buy_pro(string productld, string extra) { }

    public static void _getGoodsList_pro() { }

    public static void _getBattery() { }

    public static void _getMemory() { }


    public static void _sharePlatform_WX(string url, string msg) { }

    public static void _sharePlatform_BD(string url, string msg) { }

    public static void _svaePhotos(string _Photo) { }

    public static void _clearUpdateCache() { }

    public static void _hasDisplayCutout() { }

    // 打印更新内容（测试用）
    public static void _updateInfo() { }


    #endregion


    #region 辅助方法

    /// <summary>
    /// http请求，返回指定结果
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string DoGetRequestSendData(string url)
    {
        HttpWebRequest hwRequest = null;
        HttpWebResponse hwResponse = null;

        string strResult = string.Empty;
        try
        {
            hwRequest = (System.Net.HttpWebRequest)WebRequest.Create(url);
            //hwRequest.Timeout = 30000;
            hwRequest.Method = "GET";
            hwRequest.ContentType = "application/x-www-form-urlencoded";
        }
        catch (System.Exception err)
        {
            strResult = "error";
            return strResult;
        }
        try
        {
            hwResponse = (HttpWebResponse)hwRequest.GetResponse();
            StreamReader srReader = new StreamReader(hwResponse.GetResponseStream(), Encoding.ASCII);
            strResult = srReader.ReadToEnd();
            srReader.Close();
            hwResponse.Close();
        }
        catch (System.Exception err)
        {
            strResult = "error";
            return strResult;
        }
        return strResult;
    }

    /// <summary>
    /// 调用方法
    /// </summary>
    /// <param name="head">方法名</param>
    /// <param name="body">参数(json string)</param>
    private static void CallMethod(string head, string body)
    {
        if (string.IsNullOrEmpty(head))
        {
            return;
        }
        Dictionary<string, object> _dic = new Dictionary<string, object>();
        _dic.Add("head", head);
        _dic.Add("body", body);
        SDKManager.Inst.OnGetMessage(Json.Encode(_dic));
    }

    #endregion
}

public class PC_AppInfo
{
    public string auth_key;
    public string uid;
    public string extraInfo;

    public PC_AppInfo(string auth_key, string uid, string extraInfo)
    {
        this.auth_key = auth_key;
        this.uid = uid;
        this.extraInfo = extraInfo;
    }
}
