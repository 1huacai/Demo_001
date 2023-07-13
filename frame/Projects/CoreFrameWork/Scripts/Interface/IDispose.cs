//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;
using UnityEngine;
namespace CoreFrameWork
{
    public interface IDispose
    {
        void SetParent(Transform parent); 
        void Dispose();

    }
}