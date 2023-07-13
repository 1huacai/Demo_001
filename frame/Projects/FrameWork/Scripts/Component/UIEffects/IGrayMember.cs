using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// AorUI框架中,所有可以设置 置灰 效果的对象都应该是此接口的实现者
/// 
/// 
/// 功能依赖: Shader<Custom/Sprites/SpriteUI Alpha> 
/// 
/// </summary>
public interface IGrayMember {
        
    /// <summary>
    /// 是否置灰
    /// </summary>
    bool IsGray { get; }

    /// <summary>
    /// 置灰方法
    /// </summary>
    /// <param name="isGray"></param>
    void SetGray(bool isGray);
    
}
