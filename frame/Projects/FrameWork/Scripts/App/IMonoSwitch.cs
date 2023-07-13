using UnityEngine;
// 打包流程 IMonoSwitch->ScriptDataStrage->打预制体+reslink
//读取流程 reslink读取设置资源->ScriptDataStragezhuan转IMonoSwitch-> IresLoadBeforeInst预读 ->实例化
namespace FrameWork
{
    public interface IMonoSwitch
    {

        GameObject gameObject { get; }

        string ExportData();

        void ImportData(string stringData);

        void SetOtherParma(string target, string stringData);



        void RemoveCall(string className);
        void OnEditorAwake();
        void OnAwake();

    }
}

