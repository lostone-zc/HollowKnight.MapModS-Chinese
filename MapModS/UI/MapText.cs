using MapModS.CanvasUtil;
using MapModS.Map;
using MapModS.Settings;
using System.Linq;
using UnityEngine;

namespace MapModS.UI
{
    internal class MapText
    {
        public static GameObject Canvas;

        public static bool LockToggleEnable;

        private static CanvasPanel _mapDisplayPanel;
        private static CanvasPanel _refreshDisplayPanel;

        public static void Show()
        {
            if (Canvas == null) return;

            Canvas.SetActive(true);
            LockToggleEnable = false;
            RebuildText();
        }

        public static void Hide()
        {
            if (Canvas == null) return;

            Canvas.SetActive(false);
            LockToggleEnable = false;
        }

        public static void BuildText(GameObject _canvas)
        {
            Canvas = _canvas;
            _mapDisplayPanel = new CanvasPanel
                (_canvas, GUIController.Instance.Images["ButtonsMenuBG"], new Vector2(0f, 1030f), new Vector2(1346f, 0f), new Rect(0f, 0f, 0f, 0f));
            _mapDisplayPanel.AddText("Spoilers", "", new Vector2(-540f, 0f), Vector2.zero, GUIController.Instance.TrajanNormal, 14, FontStyle.Normal, TextAnchor.UpperCenter);
            _mapDisplayPanel.AddText("Randomized", "", new Vector2(-270f, 0f), Vector2.zero, GUIController.Instance.TrajanNormal, 14, FontStyle.Normal, TextAnchor.UpperCenter);
            _mapDisplayPanel.AddText("Others", "", new Vector2(0f, 0f), Vector2.zero, GUIController.Instance.TrajanNormal, 14, FontStyle.Normal, TextAnchor.UpperCenter);
            _mapDisplayPanel.AddText("Style", "", new Vector2(270f, 0f), Vector2.zero, GUIController.Instance.TrajanNormal, 14, FontStyle.Normal, TextAnchor.UpperCenter);
            _mapDisplayPanel.AddText("Size", "", new Vector2(540f, 0f), Vector2.zero, GUIController.Instance.TrajanNormal, 14, FontStyle.Normal, TextAnchor.UpperCenter);

            _refreshDisplayPanel = new CanvasPanel
                (_canvas, GUIController.Instance.Images["ButtonsMenuBG"], new Vector2(0f, 1030f), new Vector2(1346f, 0f), new Rect(0f, 0f, 0f, 0f));
            _refreshDisplayPanel.AddText("Refresh", "", new Vector2(0f, 0f), Vector2.zero, GUIController.Instance.TrajanNormal, 14, FontStyle.Normal, TextAnchor.UpperCenter);

            _mapDisplayPanel.SetActive(false, false);
            _refreshDisplayPanel.SetActive(false, false);

            SetTexts();
        }

        public static void RebuildText()
        {
            _mapDisplayPanel.Destroy();
            _refreshDisplayPanel.Destroy();

            BuildText(Canvas);
        }

        public static void SetTexts()
        {
            if (GameManager.instance.gameMap == null || WorldMap.CustomPins == null) return;

            _mapDisplayPanel.SetActive(!LockToggleEnable && MapModS.LS.ModEnabled, false);
            _refreshDisplayPanel.SetActive(LockToggleEnable, false);

            SetSpoilers();
            SetStyle();
            SetRandomized();
            SetOthers();
            SetSize();
            SetRefresh();
        }

        private static void SetSpoilers()
        {
            _mapDisplayPanel.GetText("Spoilers").UpdateText
                (
                    MapModS.LS.SpoilerOn ? "剧透 (ctrl-1): 开" : "剧透 (ctrl-1): 关"
                );
            _mapDisplayPanel.GetText("Spoilers").SetTextColor
                (
                    MapModS.LS.SpoilerOn ? Color.green : Color.white
                );
        }

        private static void SetRandomized()
        {
            if (WorldMap.CustomPins == null) return;

            string randomizedText = "";

            if (MapModS.LS.randomizedOn)
            {
                _mapDisplayPanel.GetText("Randomized").SetTextColor(Color.green);
                randomizedText += "随机 (ctrl-2): 开";
            }
            else
            {
                _mapDisplayPanel.GetText("Randomized").SetTextColor(Color.white);
                randomizedText += "随机 (ctrl-2): 关";
            }

            if (WorldMap.CustomPins.IsRandomizedCustom())
            {
                _mapDisplayPanel.GetText("Randomized").SetTextColor(Color.yellow);
                randomizedText += "(自定义)";
            }

            _mapDisplayPanel.GetText("Randomized").UpdateText(randomizedText);
        }

        private static void SetOthers()
        {
            if (WorldMap.CustomPins == null) return;

            string othersText = "";

            if (MapModS.LS.othersOn)
            {
                _mapDisplayPanel.GetText("Others").SetTextColor(Color.green);
                othersText += "其他 (ctrl-3): 开";
            }
            else
            {
                _mapDisplayPanel.GetText("Others").SetTextColor(Color.white);
                othersText += "其他 (ctrl-3): 关";
            }

            if (WorldMap.CustomPins.IsOthersCustom())
            {
                _mapDisplayPanel.GetText("Others").SetTextColor(Color.yellow);
                othersText += "(自定义)";
            }

            _mapDisplayPanel.GetText("Others").UpdateText(othersText);
        }

        private static void SetStyle()
        {
            switch (MapModS.GS.pinStyle)
            {
                case PinStyle.Normal:
                    _mapDisplayPanel.GetText("Style").UpdateText("图针风格 (ctrl-4): 普通");
                    break;

                case PinStyle.Q_Marks_1:
                    _mapDisplayPanel.GetText("Style").UpdateText("图针风格 (ctrl-4): 问号类型:1");
                    break;

                case PinStyle.Q_Marks_2:
                    _mapDisplayPanel.GetText("Style").UpdateText("图针风格 (ctrl-4): 问号类型:2");
                    break;

                case PinStyle.Q_Marks_3:
                    _mapDisplayPanel.GetText("Style").UpdateText("图针风格 (ctrl-4): 问号类型:3");
                    break;
            }
        }

        private static void SetSize()
        {
            switch (MapModS.GS.pinSize)
            {
                case PinSize.Small:
                    _mapDisplayPanel.GetText("Size").UpdateText("图针尺寸 (ctrl-5): 小");
                    break;

                case PinSize.Medium:
                    _mapDisplayPanel.GetText("Size").UpdateText("图针尺寸 (ctrl-5): 中");
                    break;

                case PinSize.Large:
                    _mapDisplayPanel.GetText("Size").UpdateText("图针尺寸 (ctrl-5): 大");
                    break;
            }
        }

        private static void SetRefresh()
        {
            if (MapModS.LS.ModEnabled)
            {
                _refreshDisplayPanel.GetText("Refresh").UpdateText("MapMod S enabled. Close map to refresh");
            }
            else
            {
                _refreshDisplayPanel.GetText("Refresh").UpdateText("MapMod S disabled. Close map to refresh");
            }
        }
    }
}