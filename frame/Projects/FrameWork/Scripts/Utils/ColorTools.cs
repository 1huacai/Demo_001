using UnityEngine;
public class ColorTools
{
    public static Color Color_White = new Color(219 / 255f, 219 / 255f, 219 / 255f);
    #region 各种颜色
    public static Color Qulity_Green = new Color(179 / 255f, 247 / 255f, 112 / 255f);
    public static Color Qulity_Blue = new Color(129 / 255f, 203 / 255f, 255 / 255f);
    public static Color Qulity_Purple = new Color(194 / 255f, 130 / 255f, 239 / 255f);
    public static Color Qulity_Orange = new Color(255 / 255f, 181 / 255f, 108 / 255f);
    public static Color Qulity_red = new Color(250 / 255f, 78 / 255f, 78 / 255f);
    #region 宝石颜色
    /// <summary>
    /// 绿色宝石文字颜色;
    /// </summary>
    public static Color32 GemColor_Green = new Color32(136, 190, 115, 255);

    /// <summary>
    /// 蓝色宝石文字颜色;
    /// </summary>
    public static Color32 GemColor_Blue = new Color(98, 175, 207, 255);

    /// <summary>
    /// 紫色宝石文字颜色;
    /// </summary>
    public static Color32 GemColor_Purple = new Color32(130, 77, 149, 255);

    /// <summary>
    /// 橙色宝石文字颜色;
    /// </summary>
    public static Color32 GemColor_Orange = new Color32(223, 186, 80, 255);
    #endregion

    #endregion

    #region 文本用颜色处理
    private static float nTof ( int n )
    {
        float t = (float)n / (float)255;
        return t;
    }
	private static int fTon ( float f )
    {
        float t = f * 255;
        return Mathf.CeilToInt(t);
    }

    //获取文本中颜色标签
	private static string GetColorStr ( Color c )
    {
        string s = "";
        string[] temp = new string[3];
        temp[0] = (System.Convert.ToString(fTon(c.r), 16));
        temp[1] = (System.Convert.ToString(fTon(c.g), 16));
        temp[2] = (System.Convert.ToString(fTon(c.b), 16));
        for (int i = 0; i < temp.Length; i++)
        {
            if (temp[i].Length < 2)
                s += "0";
            if (temp[i].Length < 1)
                s += "0";
            s += temp[i];
        }
        return s;
    }

    private static string GetTextColor(Color c)
    {
        return string.Format("<color=#{0}>", GetColorStr(c));
    }

    public static string HandlerTextAddColor(Color c, string str)
    {
        return GetTextColor(c) + str + "</color>";
    }

    public static string HandlerTextAddColor(Color32 c32, string str)
    {
        Color c = new Color(nTof(c32.r), nTof(c32.g), nTof(c32.b), nTof(c32.a));
        return HandlerTextAddColor(c, str);
    }
    #endregion
}
