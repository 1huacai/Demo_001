using UnityEngine;
using UnityEngine.UI;

namespace FrameWork.GUI.AorUI.Core
{
    public class AorUiRuntimeUtility
    {
        /// <summary>
        /// 创建AorUIManger
        /// </summary>
        /// <returns></returns>
        public static GameObject CreatePrefab_AorUIManger(bool isOrthographicCamera = true)
        {
            GameObject obj = new GameObject();
            obj.name = "AorUIManager";
            obj.layer = 5;

            //camera
            GameObject uicamaeraGo = new GameObject();
            uicamaeraGo.transform.SetParent(obj.transform, false);
            uicamaeraGo.layer = 5;
            uicamaeraGo.name = "AorUICamera#";
            Camera uicamaera = uicamaeraGo.AddComponent<Camera>();
            uicamaera.clearFlags = CameraClearFlags.Nothing;
            uicamaera.cullingMask = 1 << 5;
            uicamaera.fieldOfView = 63.25f;
            uicamaera.orthographicSize = 3.6f;
            uicamaera.orthographic = isOrthographicCamera;
            uicamaera.nearClipPlane = 10f;
            uicamaera.farClipPlane = 1000f;
            uicamaera.depth = 100f;
            uicamaera.useOcclusionCulling = false;

            //FloatBackUICanvas
            GameObject _UIBackCanv = CreatePrefab_UIBase(obj.transform, 0, 0, 512f, 512f, 0.5f, 0.5f, 0.5f, 0.5f);

            _UIBackCanv.name = "AorFloatBackUICanvas#";
            Canvas _canvas = _UIBackCanv.AddComponent<Canvas>();
            _canvas.worldCamera = uicamaera;
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.planeDistance = (isOrthographicCamera ? 50f : 100f);
            _canvas.sortingOrder = -10;
            //_UIBackCanv.AddComponent<GraphicRaycaster>();

            //UIBackLayer
            GameObject _uiback = CreatePrefab_UIBase(_UIBackCanv.transform);
            _uiback.name = "FloatUIBackLayer#";
            if (Application.isEditor && !Application.isPlaying)
            {
                _uiback.SetActive(false);
            }




            //UICanvas
            GameObject canv = CreatePrefab_UIBase(obj.transform, 0, 0, 512f, 512f, 0.5f, 0.5f, 0.5f, 0.5f);

            canv.name = "AorUICanvas#";
            Canvas canvas = canv.AddComponent<Canvas>();
            canvas.worldCamera = uicamaera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = (isOrthographicCamera ? 50f : 100f);

            canv.AddComponent<GraphicRaycaster>();



            //BG
            GameObject bg = CreatePrefab_UIBase(canv.transform, 0, 0, 512f, 512f, 0.5f, 0.5f, 0.5f, 0.5f);
            bg.name = "AorUIBG#";
           // bg.AddComponent<CanvasGroup>();
            bg.AddComponent<RawImage>().color = new Color(0, 0, 0, 1f);

            //RT
           // GameObject rt = CreatePrefab_UIBase(obj.transform, 0, 0, 512f, 512f, 0.5f, 0.5f, 0.5f, 0.5f);
            GameObject rt = CreatePrefab_UIBase(canv.transform, 0, 0, 0f, 0f, 0f, 0f, 1f, 1f);
           rt.name = "AorRT#";
         //   rt.AddComponent<CanvasGroup>();
            RawImage img = rt.AddComponent<RawImage>();
            img.color = new Color(1, 1,1,0);
            rt.SetActive(false);

            //UIBackLayer
            GameObject uiback = CreatePrefab_UIBase (canv.transform);
            uiback.name = "UIBackLayer#";
            if (Application.isEditor && !Application.isPlaying)
            {
                uiback.SetActive (false);
            }



            //FloatFrontUICanvas
            GameObject _UIFrontCanv = CreatePrefab_UIBase(obj.transform, 0, 0, 512f, 512f, 0.5f, 0.5f, 0.5f, 0.5f);

            _UIFrontCanv.name = "AorFloatFrontUICanvas#";
              _canvas = _UIFrontCanv.AddComponent<Canvas>();
            _canvas.worldCamera = uicamaera;
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.planeDistance = (isOrthographicCamera ? 50f : 100f);
            _canvas.sortingOrder = 10;
            //_UIFrontCanv.AddComponent<GraphicRaycaster>();

            //UIBackLayer
              _uiback = CreatePrefab_UIBase(_UIFrontCanv.transform);
            _uiback.name = "FloatFrontUIBackLayer#";
            if (Application.isEditor && !Application.isPlaying)
            {
                _uiback.SetActive(false);
            }








            //FloatingLayer_A
            GameObject flb_A = CreatePrefab_UIBase (canv.transform.parent);
            flb_A.name = "FloatingLayer_A#";
            if (Application.isEditor && !Application.isPlaying)
            {
                flb_A.SetActive (false);
            }
            Canvas _canvas_A = flb_A.AddComponent<Canvas> ();
            _canvas_A.overrideSorting = true;
            _canvas_A.sortingOrder = -100;
            _canvas_A.worldCamera = uicamaera;
            _canvas_A.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas_A.planeDistance = (isOrthographicCamera ? 50f : 100f);

            //FloatingLayer_B
            GameObject flb_B = CreatePrefab_UIBase(canv.transform.parent);
            flb_B.name = "FloatingLayer_B#";
            if (Application.isEditor && !Application.isPlaying)
            {
                flb_B.SetActive(false);
            }
            Canvas _canvas_B = flb_B.AddComponent<Canvas> ();
            _canvas_B.overrideSorting = true;
            _canvas_B.sortingOrder = -90;
            _canvas_B.worldCamera = uicamaera;
            _canvas_B.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas_B.planeDistance = (isOrthographicCamera ? 50f : 100f);

            //FloatingLayer_C
            GameObject flb_C = CreatePrefab_UIBase (canv.transform.parent);
            flb_C.name = "FloatingLayer_C#";
            if (Application.isEditor && !Application.isPlaying)
            {
                flb_C.SetActive (false);
            }
            Canvas _canvas_C = flb_C.AddComponent<Canvas> ();
            _canvas_C.overrideSorting = true;
            _canvas_C.sortingOrder = -80;
            _canvas_C.worldCamera = uicamaera;
            _canvas_C.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas_C.planeDistance = (isOrthographicCamera ? 50f : 100f);

            //FloatingLayer_X
            GameObject flb_X = CreatePrefab_UIBase(canv.transform.parent);
            flb_X.name = "FloatingLayer_X#";
            if (Application.isEditor && !Application.isPlaying)
            {
                flb_X.SetActive(false);
            }
            Canvas _canvas_X = flb_X.AddComponent<Canvas>();
            _canvas_X.overrideSorting = true;
            _canvas_X.sortingOrder =100;
            _canvas_X.worldCamera = uicamaera;
            _canvas_X.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas_X.planeDistance = (isOrthographicCamera ? 50f : 100f);




            //AorUIStage
            GameObject stage = CreatePrefab_UIBase(canv.transform, 0, 0, 512f, 512f, 0.5f, 0.5f, 0.5f, 0.5f);
            stage.name = "AorUIStage#";
            //- stage的Mask现在不作为必要组件了,因为舞台Mask的开销不太划算,如有需要可以在Manager初始化后自行添加
            //stage.AddComponent<Image>().color = new Color(0, 0.65f, 1f, 1f);
            //stage.AddComponent<Mask>().showMaskGraphic = false;


            //AorUIStage.mainLayer
            GameObject ml = CreatePrefab_UIBase(stage.transform);
            ml.name = "MainLayer";

            //AorUIStage.mainLayer
            GameObject tl = CreatePrefab_UIBase(stage.transform);
            tl.name = "TopLayer";

            //FloatingLayer_T
            GameObject flt = CreatePrefab_UIBase(canv.transform);
            flt.name = "FloatingLayer_T#";
            if (Application.isEditor && !Application.isPlaying)
            {
                flt.SetActive(false);
            }

            return obj;
        }
        //--------------------------------------------------------- 基础类prefab
		
		public static RectTransform ResetRectTransform(RectTransform rectTransform) {
            rectTransform.pivot = new Vector2(.5f, .5f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);

            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            return rectTransform;
        }
		
        /// <summary>
        /// 创建Camera
        /// </summary>
        /// <returns></returns>
        public static Camera CreatePrefab_Camera()
        {
            GameObject uc = new GameObject();
            uc.name = "Camera";
            Camera c = uc.AddComponent<Camera>();
            return c;
        }

        //public static GameObject CreatePrefab_BaseGameObject()

        /// <summary>
        /// 创建基础UIGameObject
        /// 包含组件[RectTransform]
        /// </summary>
        public static GameObject CreatePrefab_UIBase(   Transform parent = null,
                                                        float x = 0, float y = 0, float w = 0, float h = 0,
                                                        float anchorsMinX = 0, float anchorsMinY = 0,
                                                        float anchorsMaxX = 1f, float anchorsMaxY = 1f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f) 
        {
            GameObject _base = new GameObject();
            _base.layer = 5;

            if (parent != null) {
                _base.transform.SetParent(parent, false);
            }

            RectTransform rt = _base.AddComponent<RectTransform>();
            
            rt.pivot = new Vector2(pivotX, pivotY);
            rt.anchorMin = new Vector2(anchorsMinX, anchorsMinY);
            rt.anchorMax = new Vector2(anchorsMaxX, anchorsMaxY);
            
            rt.localPosition = new Vector3(x, y);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);

            return _base;
        }
        //// <summary>
        ///  创建一个Text Prefab;
        /// 包含组件[Text]
        /// </summary>
        public static Text CreatePrefab_Text(     Transform parent = null,
                                                        float x = 0, float y = 0, float w = 0, float h = 0,
                                                        float anchorsMinX = 0, float anchorsMinY = 0,
                                                        float anchorsMaxX = 1f, float anchorsMaxY = 1f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f)
        {
            GameObject obj = CreatePrefab_UIBase(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            obj.name = "Text";
            Text txt = obj.AddComponent<Text>();
            return txt;
        }
        //// <summary>
        ///  创建一个Image Prefab;
        /// 包含组件[Image]
        /// </summary>
        public static Image CreatePrefab_Image(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 0, float h = 0,
                                                        float anchorsMinX = 0, float anchorsMinY = 0,
                                                        float anchorsMaxX = 1f, float anchorsMaxY = 1f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f)
        {
            GameObject obj = CreatePrefab_UIBase(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            obj.name = "Image";
            Image img = obj.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.65f);
            return img;
        }

        /// <summary>
        ///  创建一个Button Prefab;
        ///  包含组件[Image, Button]
        /// </summary>
        public static Button CreatePrefab_Button(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 0, float h = 0,
                                                        float anchorsMinX = 0, float anchorsMinY = 0,
                                                        float anchorsMaxX = 1f, float anchorsMaxY = 1f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f)
        {
            Image img = CreatePrefab_Image(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            img.gameObject.name = "Button";

            Button btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            return btn;
        }
        /// <summary>
        ///  创建一个AorScrollRect Prefab;
        /// 包含组件[Image, Mask, ScrollRect]
        /// </summary>
        public static AorScrollRect CreatePrefab_AorScrollRect(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 0, float h = 0,
                                                        float anchorsMinX = 0, float anchorsMinY = 0,
                                                        float anchorsMaxX = 1f, float anchorsMaxY = 1f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f ) 
        {

            GameObject obj = CreatePrefab_UIBase(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.207f, 0.207f, 0.207f, 1f);

            Mask mk = obj.AddComponent<Mask>();
            mk.showMaskGraphic = false;

            AorScrollRect sr = obj.AddComponent<AorScrollRect>();

            return sr;
        }
        /// <summary>
        ///  创建一个ScrollRect Prefab;
        /// 包含组件[Image, Mask, ScrollRect]
        /// </summary>
        public static ScrollRect CreatePrefab_ScrollRect(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 0, float h = 0,
                                                        float anchorsMinX = 0, float anchorsMinY = 0,
                                                        float anchorsMaxX = 1f, float anchorsMaxY = 1f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f) {

            GameObject obj = CreatePrefab_UIBase(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.207f, 0.207f, 0.207f, 1f);

            Mask mk = obj.AddComponent<Mask>();
            mk.showMaskGraphic = false;

            ScrollRect sr = obj.AddComponent<ScrollRect>();

            return sr;
        }
        //--------------------------------------------------------- 容器类prefab
        //AorPage
        public static GameObject CreatePrefab_AorPage(Transform parent = null) {
            Image img = CreatePrefab_Image(parent);
            img.gameObject.name = "AorPage";
            return img.gameObject;
        }
        //AorWindow
        public static GameObject CreatePrefab_AorWindow(Transform parent = null) 
        {
            Image img = CreatePrefab_Image(parent);
            img.gameObject.name = "AorWindow";
            img.color = new Color(0f, 0f, 0f, .75f);

            //window
            Image win_img = CreatePrefab_Image(img.transform, 0, 0, 640f, 480f, .5f, .5f, .5f, .5f);
            win_img.gameObject.name = "Window#";
            win_img.gameObject.AddComponent<CanvasGroup>();

            //closeBtn
            Button closeBtn_btn = CreatePrefab_Button(win_img.transform, 0, 0, 64f, 64f, 1f, 1f);
            closeBtn_btn.gameObject.name = "closeBtn";

            //Text
            Text txt = CreatePrefab_Text(closeBtn_btn.transform);
            txt.fontSize = 16;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0, 0, 0, 1f);
            txt.text = "Close..";

            Outline ol = txt.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(1, 1, 1, 0.45f);
            ol.effectDistance = new Vector2(1f, -1f);

            return img.gameObject;
        }
        //--------------------------------------------------------- 组件prefab

        //BloodBar
        public static GameObject CreatePrefab_BloodBar(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 200, float h = 20,
                                                        float anchorsMinX = .5f, float anchorsMinY = .5f,
                                                        float anchorsMaxX = .5f, float anchorsMaxY = .5f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f) {

            Image img = CreatePrefab_Image(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            img.gameObject.name = "BloodBar";
            img.color = new Color(0.3294f, 0.3294f, 0.3294f, 1f);

            //MColorL#
            Image mcl_img = CreatePrefab_Image(img.transform, 0, 0, 0, 0, 0, 0, 0.9f, 1f, 0f);
            mcl_img.gameObject.name = "MColorL#";
            mcl_img.color = new Color(0.588f, 0, 0, 1f);

            //TColorL#
            Image tcl_img = CreatePrefab_Image(img.transform, 0, 0, 0, 0, 0, 0, 0.75f, 1f, 0f);
            tcl_img.gameObject.name = "TColorL#";
            tcl_img.color = new Color(0.1921f, 0.7882f, 0.36862f, 1f);

            //ValueEcho#
            Text txt = CreatePrefab_Text(img.transform);
            txt.gameObject.name = "ValueEcho#";
            txt.text = "100 / 100";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.resizeTextForBestFit = true;
            txt.fontSize = 14;
            txt.resizeTextMinSize = 6;
            txt.resizeTextMaxSize = 64;
            txt.color = new Color(1f, 1f, 1f, 1f);

            Outline ol = txt.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.3529f, 0.3529f, 0.3529f, 0.501f);
            ol.effectDistance = new Vector2(1f, -1f);
            ol.useGraphicAlpha = true;

            return img.gameObject;
        }
        //ProgressBar
        public static GameObject CreatePrefab_ProgressBar(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 400, float h = 20,
                                                        float anchorsMinX = .5f, float anchorsMinY = .5f,
                                                        float anchorsMaxX = .5f, float anchorsMaxY = .5f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f) 
        {
            GameObject obj = CreatePrefab_UIBase(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            obj.name = "ProgressBar";

            //pgb_bg#
            Image pbg_img = CreatePrefab_Image(obj.transform);
            pbg_img.gameObject.name = "pgb_bg#";
            pbg_img.color = new Color(0.36862f, 0.36862f, 0.36862f, 1f);

            //pgb_fillArea#
            GameObject filla = CreatePrefab_UIBase(pbg_img.transform);
            filla.name = "pgb_fillArea#";

            //pgb_fill#
            Image pbg_fill_img = CreatePrefab_Image(filla.transform, 0, 0, 0, 0, 0, 0, 0.75f);
            pbg_fill_img.gameObject.name = "pgb_fill#";
            pbg_fill_img.color = new Color(1f, 0.51f, 0.51f, 1f);

            //ValueEcho#
            Text txt = CreatePrefab_Text(pbg_img.transform);
            txt.gameObject.name = "ValueEcho#";
            txt.text = "0";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.resizeTextForBestFit = true;
            txt.fontSize = 14;
            txt.resizeTextMinSize = 6;
            txt.resizeTextMaxSize = 64;
            txt.color = new Color(1f, 1f, 1f, 1f);
            Outline ol = txt.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.3529f, 0.3529f, 0.3529f, 0.501f);
            ol.effectDistance = new Vector2(1f, -1f);
            ol.useGraphicAlpha = true;

            //pgb_point#
            Image pt_img = CreatePrefab_Image(pbg_img.transform, 0, 0, 64f, 64f, 0, 0.5f, 0, 0.5f);
            pt_img.gameObject.name = "pgb_point#";
            pt_img.color = new Color(0.4549f, 0.9125f, 0.9125f, 0.3921f);

            return obj;
        }
        //ImageMapping
        public static GameObject CreatePrefab_ImageMapping(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 64, float h = 64,
                                                        float anchorsMinX = .5f, float anchorsMinY = .5f,
                                                        float anchorsMaxX = .5f, float anchorsMaxY = .5f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f) 
        {
            Image img = CreatePrefab_Image(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            img.gameObject.name = "ImageMapping";
            img.color = new Color(1f, 1f, 1f, 1f);

            return img.gameObject;
        }
        //NumFollow
        public static GameObject CreatePrefab_NumFollow(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 100, float h = 25,
                                                        float anchorsMinX = .5f, float anchorsMinY = .5f,
                                                        float anchorsMaxX = .5f, float anchorsMaxY = .5f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f) 
        {
            Text txt = CreatePrefab_Text(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            txt.name = "NumFollow";
            txt.text = "0";
            txt.alignment = TextAnchor.MiddleLeft;
            txt.fontSize = 16;
            txt.color = new Color(1f, 1f, 1f, 1f);

            return txt.gameObject;
        }
        //TipBox
        public static GameObject CreatePrefab_TipBox(Transform parent = null,
                                                        float x = 100, float y = 100, float w = 0, float h = 0,
                                                        float anchorsMinX = 0, float anchorsMinY = 0,
                                                        float anchorsMaxX = 0, float anchorsMaxY = 0,
                                                        float pivotX = 0, float pivotY = 0) 
        {
            GameObject obj = CreatePrefab_UIBase(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            obj.name = "TipBox";
            
            //bg#
            Image bg_img = CreatePrefab_Image(obj.transform, -20f, 15f, 110f, 35f, 0, 0, 0, 0, 0, 0);
            bg_img.gameObject.name = "bg#";
            bg_img.color = new Color(0, 0, 0, 0.545f); 

            //echo#
            Text echo_text = CreatePrefab_Text(bg_img.transform, 5f, 5f, 0, 0, 0, 0, 0, 0, 0, 0);
            echo_text.gameObject.name = "echo#";
            echo_text.text = "tip message";
            echo_text.fontSize = 18;
            echo_text.color = new Color(1f, 1f, 1f, 1f);
            Outline echo_outline = echo_text.gameObject.AddComponent<Outline>();
            echo_outline.effectColor = new Color(0, 0, 0, 0.6274f);
            echo_outline.effectDistance = new Vector2(1f, -1f);
            echo_outline.useGraphicAlpha = true;

            ContentSizeFitter csf = echo_text.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return obj;
        }
        //ScrollList
        public static GameObject CreatePrefab_ScrollList(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 300, float h = 100,
                                                        float anchorsMinX = .5f, float anchorsMinY = .5f,
                                                        float anchorsMaxX = .5f, float anchorsMaxY =.5f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f) 
        {

//            ScrollRect sr = CreatePrefab_ScrollRect(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
//            sr.gameObject.name = "ScrollList";
            AorScrollRect sr = CreatePrefab_AorScrollRect(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            sr.gameObject.name = "ScrollList";

            //Panel#
            GameObject panel = CreatePrefab_UIBase(sr.transform, 0, 0, 0, 225f, 0, 1f, 1f, 1f, 0f, 1f);
            panel.name = "Panel#";
            
            sr.content = panel.GetComponent<RectTransform>();
            sr.horizontal = false;
            sr.vertical = true;
          //  sr.movementType = ScrollRect.MovementType.Clamped;
            
            //ListItem
            GameObject li = CreatePrefab_ListItem(panel.transform);
            li.name = "ListItem[CreatePrefabOrDontDelete]";

            return sr.gameObject;
        }
        //ListItem
        public static GameObject CreatePrefab_ListItem(Transform parent = null,
                                                        float x = 0, float y = -25, float w = 0, float h = 50,
                                                        float anchorsMinX = 0, float anchorsMinY = 1f,
                                                        float anchorsMaxX = 1f, float anchorsMaxY = 1f,
                                                        float pivotX = 0f, float pivotY = 1f) 
        {
            Button btn = CreatePrefab_Button(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            btn.gameObject.name = "ListItem";
            
            ColorBlock cb = new ColorBlock();
            cb.normalColor = new Color(0,0,0,1f);
            cb.highlightedColor = new Color(0, 0, 0, 1f);
            cb.pressedColor = new Color(0.57f,0.3882f,0.1176f,1f);
            cb.disabledColor = new Color(0.7844f, 0.7844f, 0.7844f, 0.5f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            Text txt = CreatePrefab_Text(btn.transform, 0, 0, 0, 0, 0.1f, 0.3f, 0.9f, 0.7f);
            txt.text = "0";
            txt.fontSize = 14;
            txt.color = new Color(1f, 1f, 1f, 1f);

            return btn.gameObject;
        }
        //ComboBox
        public static GameObject CreatePrefab_ComboBox(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 420, float h = 50,
                                                        float anchorsMinX = .5f, float anchorsMinY = .5f,
                                                        float anchorsMaxX = .5f, float anchorsMaxY = .5f,
                                                        float pivotX = 0.5f, float pivotY = 1f) 
        {
            GameObject obj = CreatePrefab_UIBase(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            obj.name = "ComboBox";

            //Background#
            Image bg_img = CreatePrefab_Image(obj.transform, 0, 0, 0, 0, 0, 0, 1f, 1f, .5f, 1f);
            bg_img.gameObject.name = "Background#";
            bg_img.color = new Color(0.447f, 0.447f, 0.447f, 0.8862f);

            //SelectItemPos#
            GameObject sip = CreatePrefab_UIBase(obj.transform, 0, 0, 0, 0, 0, 0, 1f, 1f, .5f, 1f);
            sip.name = "SelectItemPos#";

            //ScrollListBackground#
            Image slbg_img = CreatePrefab_Image(obj.transform, 0, 0, 0, 50, 0, 0, 1f, 0, .5f, 1f);
            slbg_img.gameObject.name = "ScrollListBackground#";
            slbg_img.color = new Color(1f, 1f, 1f, 1f);

            //InteractiveBtn#
            Button itbtn_btn = CreatePrefab_Button(obj.transform);
            itbtn_btn.gameObject.name = "InteractiveBtn#";
            itbtn_btn.interactable = true;
            itbtn_btn.transition = Selectable.Transition.None;

            //ScrollList#
            GameObject sub = CreatePrefab_ScrollList(obj.transform, 0, 0, 0, 100f, 0, 0, 1f, 0, .5f, 1f);
            sub.name = "ScrollList#";

            return obj;
        }

        //DialoguesSystem
        public static GameObject CreatePrefab_DialoguesSystem(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 0, float h = 0,
                                                        float anchorsMinX = 0, float anchorsMinY = 0,
                                                        float anchorsMaxX = 1f, float anchorsMaxY = 1f,
                                                        float pivotX = 0.5f, float pivotY = 0.5f) 
        {
            GameObject obj = CreatePrefab_UIBase(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            obj.name = "DialoguesSystem";

            //backround#
            Image bg_img = CreatePrefab_Image(obj.transform, 0, 0, 0, 0, 0, 0, 1f, 0.35f);
            bg_img.gameObject.name = "backround#";
            bg_img.color = new Color(1f, 1f, 1f, 0.65f);

            //PortraitContentA#
            GameObject pca = CreatePrefab_UIBase(obj.transform, 130f, 240f, 240f, 320f, 0, 0, 0f, 0f);
            pca.name = "PortraitContentA#";

            //PortraitContentB#
            GameObject pcb = CreatePrefab_UIBase(obj.transform, -130f, 240f, 240f, 320f, 1f, 0f, 1f, 0f);
            pcb.name = "PortraitContentB#";

            //TitleBox#
            Text tlb_text = CreatePrefab_Text(obj.transform, 0, 195f, 720f, 70f, .5f, 0, .5f, 0);
            tlb_text.gameObject.name = "TitleBox#";
            tlb_text.fontSize = 46;
            tlb_text.color = new Color(1f, 1f, 1f, 1f);
            tlb_text.text = "Title";

            //DialogBox#
            Text dlb_text = CreatePrefab_Text(obj.transform, 0, 80f, 720f, 140f, .5f, 0, .5f, 0);
            dlb_text.name = "DialogBox#";
            dlb_text.fontSize = 34;
            dlb_text.color = new Color(1f, 1f, 1f, 1f);
            dlb_text.text = "New Text";

            //SkipButton#
            Button itbtn_btn = CreatePrefab_Button(obj.transform, -130f, 40f, 240f, 55f, 1f, 0, 1f, 0);
            itbtn_btn.gameObject.name = "SkipButton#";

            //SkipButtonText
            Text siBtn_text = CreatePrefab_Text(itbtn_btn.transform);
            siBtn_text.gameObject.name = "Text";
            siBtn_text.fontSize = 14;
            siBtn_text.color = new Color(.2f, .2f, .2f, 1f);
            siBtn_text.alignment = TextAnchor.MiddleCenter;

            return obj;
        }
        //GridPanel
        public static GameObject CreatePrefab_GridPanel(Transform parent = null,
                                                        float x = 0, float y = 0, float w = 100, float h = 100,
                                                        float anchorsMinX = .5f, float anchorsMinY = .5f,
                                                        float anchorsMaxX = .5f, float anchorsMaxY = .5f,
                                                        float pivotX = .5f, float pivotY = .5f) 
        {
            Image img = CreatePrefab_Image(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            img.gameObject.name = "GridView";
            img.color = new Color(0,.6274f,1f,0.84f);
            Mask mk = img.gameObject.AddComponent<Mask>();
            mk.showMaskGraphic = false;

            //Panel#
            GameObject pl = CreatePrefab_UIBase(img.transform, 0, 0, 0, 0, 0, 1f, 0, 1f, 0, 1f);
            pl.name = "Panel#";

            //GridViewItem
            GameObject gvi = CreatePrefab_GridPanelItem(pl.transform);
            gvi.name = "GridViewItem[CreatePrefabOrDontDelete]";

            return img.gameObject;
        }
        //GridViewItem
        public static GameObject CreatePrefab_GridPanelItem(Transform parent = null,
                                                        float x = 25, float y = -25, float w = 50, float h = 50,
                                                        float anchorsMinX = 0, float anchorsMinY = 1f,
                                                        float anchorsMaxX = 0f, float anchorsMaxY = 1f,
                                                        float pivotX = .5f, float pivotY = .5f) 
        {
            Image img = CreatePrefab_Image(parent, x, y, w, h, anchorsMinX, anchorsMinY, anchorsMaxX, anchorsMaxY, pivotX, pivotY);
            img.gameObject.name = "GridViewItem";
            img.color = new Color(.698f, .698f, .698f, 1f);

            //Text
            Text txt = CreatePrefab_Text(img.transform);
            txt.gameObject.name = "Text";
            txt.text = "没有导入数据";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 13;
            txt.color = new Color(.196f, .196f, .196f, 1f);

            return img.gameObject;
        }

    }
}
