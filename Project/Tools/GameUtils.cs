using System;
using UnityEngine;
using FrameWork;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using CoreFrameWork;


public static class GameUtils
{
    public static byte[] EncryptyFile(byte[] filedata, int code)
    {
        int filelen = filedata.Length;
        for (int i = 0; i < filelen; ++i)
        {
            if (i % 2 == 0)
            {
                filedata[i] = (byte)(filedata[i] ^ code);
            }
            else
            {
                filedata[i] = filedata[i];
            }

        }
        return filedata;
    }

}