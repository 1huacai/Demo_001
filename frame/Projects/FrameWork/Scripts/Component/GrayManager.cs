using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;


public static class GrayManager
{
    public static Dictionary<Object, Color> oldColorsForGray = new Dictionary<Object, Color>();

    public static Material _DefFontMaterial;
    public static Material _DefImageMaterial;
    public static Color ColorGray = new Color(0, 0, 0, 1);


    public static Material DefFontMaterial
    {

        get
        {
            if (_DefFontMaterial == null)
                _DefFontMaterial = AorMaterialCreater.CreateMaterial("DefFontMaterial", "Custom/Fonts/Default Font");

            return _DefFontMaterial;
        }
    }

    public static Material DefImageMaterial
    {

        get
        {
            if (_DefImageMaterial == null)
                _DefImageMaterial = AorMaterialCreater.CreateMaterial("defImageMaterial", "Custom/Sprites/SpriteUI");

            return _DefImageMaterial;


        }
    }


    static void ClearDictionary()
    {

        Object[] ops = oldColorsForGray.Keys.ToArray();

        for (int i = 0; i < ops.Length; i++)
        {
            if (!ops[i])
            {
                oldColorsForGray.Remove(ops[i]);
            }

        }

    }

    /// <summary>
    /// 处理Text的Gray
    /// </summary>
    public static void SetGray(this RawImage image, bool isGray)
    {
        ClearDictionary();
        bool _isGray = false;

        if (oldColorsForGray.ContainsKey(image) || image.color == Color.black)
        {
            _isGray = true;

        }




        if (_isGray == isGray)
            return;




        switch (image.material.shader.name)
        {

            case "UI/Default":


                if (DefImageMaterial == null)
                    return;

                image.material = DefImageMaterial;
                image.SetMaterialDirty();
                break;
        }

        if (isGray)
        {
            oldColorsForGray.Add(image, image.color);
            image.color = new Color(0, 0, 0, image.color.a);
        }
        else if (oldColorsForGray.ContainsKey(image))
        {
            image.color = oldColorsForGray[image];
            oldColorsForGray.Remove(image);
        }

    }



    /// <summary>
    /// 处理Text的Gray
    /// </summary>
    public static void SetGray(this Image image, bool isGray)
    {
        ClearDictionary();
        bool _isGray = false;

        if (oldColorsForGray.ContainsKey(image) || image.color == Color.black)
        {
            _isGray = true;

        }

        if (_isGray == isGray)
            return;




        switch (image.material.shader.name)
        {

            case "UI/Default":

                if (DefImageMaterial == null)
                    return;


                image.material = DefImageMaterial;
                image.SetMaterialDirty();
                break;
        }

        if (isGray)
        {
            oldColorsForGray.Add(image, image.color);
            image.color = new Color(0, 0, 0, image.color.a);
        }
        else if (oldColorsForGray.ContainsKey(image))
        {
            image.color = oldColorsForGray[image];
            oldColorsForGray.Remove(image);
        }

    }



    /// <summary>
    /// 处理Text的Gray
    /// </summary>
    public static void ProcessTextSetGray(Text text, bool isGray)
    {


        ClearDictionary();

        if (isGray)
        {


            if (oldColorsForGray == null)
            {
                oldColorsForGray = new Dictionary<Object, Color>();
            }

            switch (text.material.shader.name)
            {

                case "UI/Default Font":

                    if (DefFontMaterial == null)
                    {
                        Debug.LogWarning("AorButtom.setGray :: can not find the Shader<Custom/Fonts/Default Font>");
                        return;
                    }

                    text.material = DefFontMaterial;
                    break;
                case "Custom/Fonts/SpriteUI CustomFont":



                case "Custom/Fonts/SpriteUI CustomFont Gradient":


                    break;
                default:
                    if (Application.isEditor)
                    {
                        //Debug.Log("AorButtom.setGray :: subChild : Text[" + text.transform.getHierarchyPath() +
                        //            "]Not use supported Shader. setGray Faild.");
                    }
                    break;
            }


            Color oldcol = text.color;

            if (!oldColorsForGray.ContainsKey(text))
                oldColorsForGray.Add(text, oldcol);

            text.color = new Color(1, 1, 1, text.color.a);
        }
        else if (oldColorsForGray.ContainsKey(text))
        {

            text.color = oldColorsForGray[text];
            oldColorsForGray.Remove(text);
        }


    }
    /// <summary>
    /// 处理outline的Gray
    /// </summary>
    public static void ProcessOutlineSetGray(Outline line, bool isGray)
    {


        ClearDictionary();

        if (isGray)
        {


            if (oldColorsForGray == null)
            {
                oldColorsForGray = new Dictionary<Object, Color>();
            }

            Color oldcol = line.effectColor;

            if (!oldColorsForGray.ContainsKey(line))
                oldColorsForGray.Add(line, oldcol);

            line.effectColor = new Color(0.02f, 0.02f, 0.02f, oldcol.a);
        }
        else if (oldColorsForGray.ContainsKey(line))
        {

            line.effectColor = oldColorsForGray[line];
            oldColorsForGray.Remove(line);
        }


    }

    /// <summary>
    /// 处理AorTextIncSprites的Gray
    /// </summary>
    public static void ProcessTextSetGray(AorTextIncSprites text, bool isGray)
    {

        ClearDictionary();
        if (isGray)
        {


            if (oldColorsForGray == null)
            {
                oldColorsForGray = new Dictionary<Object, Color>();
            }

            switch (text.material.shader.name)
            {

                case "UI/Default Font":


                    if (DefFontMaterial == null)
                    {
                        Debug.LogWarning("AorButtom.setGray :: can not find the Shader<Custom/Fonts/Default Font>");
                        return;
                    }

                    text.material = DefFontMaterial;
                    break;
                case "Custom/Fonts/SpriteUI CustomFont":



                case "Custom/Fonts/SpriteUI CustomFont Gradient":


                case "Custom/Fonts/Default Font":


                default:
                    if (Application.isEditor)
                    {

                        //Debug.Log("AorButtom.setGray :: subChild : Text[" + text.transform.getHierarchyPath() +
                        //                 "]Not use supported Shader. setGray Faild.");
                    }
                    return;
            }


            Color oldcol = text.color;

            if (!oldColorsForGray.ContainsKey(text))
                oldColorsForGray.Add(text, oldcol);

            text.color = new Color(1, 1, 1, text.color.a);
        }
        else if (oldColorsForGray.ContainsKey(text))
        {
            text.color = oldColorsForGray[text];
            oldColorsForGray.Remove(text);

        }

    }

    public static void SetGaryWithAllChildren(Transform trans, bool bGray)
    {

        setChildGrayLoop(trans, bGray);
    }


    private static void setChildGrayLoop(Transform t, bool isGray)
    {
        bool _do = false;

        IGrayMember[] ims = t.GetInterfaces<IGrayMember>();
        if (ims != null && ims.Length > 0)
        {
            for (int i = 0; i < ims.Length; i++)
            {
                if (!(ims[i] is AorButton))
                {
                    if (ims[i].IsGray != isGray)
                    {
                        _do = true;//可以操作
                        ims[i].SetGray(isGray);
                    }
                }
            }
        }

        int j, len = t.childCount;

        for (j = 0; j < len; j++)
        {
            setChildGrayLoop(t.GetChild(j), isGray);

        }

        if (_do)
        {

            //单独处理Text
            Text text = t.GetComponent<Text>();
            if (text != null)
            {
                ProcessTextSetGray(text, isGray);
            }

            Outline line = t.GetComponent<Outline>();
            if (line != null)
            {
                ProcessOutlineSetGray(line, isGray);
            }

            //单独处理AorTextIncSprites;
            AorTextIncSprites aorText = t.GetComponent<AorTextIncSprites>();
            if (aorText != null)
            {
                ProcessTextSetGray(aorText, isGray);
            }

        }
    }
}

