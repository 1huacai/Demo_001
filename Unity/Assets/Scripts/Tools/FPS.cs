using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// 帧率统计器
/// </summary>

public class FPS : MonoBehaviour
{
    private GUIStyle Style;

    void Awake()
    {
        Style = new GUIStyle();
        Style.fontSize = (int)((Screen.width < Screen.height ? Screen.width : Screen.height) / 48);
        Style.normal.textColor = new Color(1f, 0f, 0f, 1f);
    }

    private Queue<float> _timeQueue = new Queue<float>();
    private float _fps;

    public void Update()
    {
        int fNum = Math.Max((int)_fps, 10);

        _timeQueue.Enqueue(Time.deltaTime);
        while (_timeQueue.Count > fNum)
        {
            _timeQueue.Dequeue();
        }
        float t = 0;
        foreach (float dt in _timeQueue)
        {
            t += dt;
        }
        _fps = (int)(_timeQueue.Count / t);
        if (_fps >= 25)
        {
            Style.normal.textColor = Color.green;
        }
        else if (_fps >= 20)
        {
            Style.normal.textColor = Color.yellow;
        }
        else
        {
            Style.normal.textColor = Color.red;
        }
        sb.Clear();
        sb.AppendFormat("FPS：{0}\n", _fps);
        //sb.AppendFormat("lua：{0}\n", XLuaManager.Instance.GetLuaMemory() / 1024 / 1024);
        //sb.AppendFormat("Instancing：{0}\n", YoukiaEngine.GPUDeviceInfo.SupportsInstancing());

#if ENABLE_PROFILER
            sb.AppendFormat("Unity：{0}M\n",  (Profiler.usedHeapSizeLong / 1024 / 1024f).ToString("f1"));
            sb.AppendFormat("Mono：{0}M\n", (Profiler.GetMonoUsedSizeLong() / 1024 / 1024f).ToString("f1"));
            sb.AppendFormat("GfxDriver：{0}M\n",  (Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024f).ToString("f1"));
            sb.AppendFormat("Mono Reserved：{0}M\n",  (Profiler.GetMonoHeapSizeLong() / 1024 / 1024f).ToString("f1")); 
#endif


        info = sb.ToString();
    }

    private StringBuilder sb = new StringBuilder();
    private string info = string.Empty;

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 100, 20), info, Style);
    }

}
