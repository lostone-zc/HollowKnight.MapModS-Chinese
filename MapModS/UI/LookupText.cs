﻿using MapModS.CanvasUtil;
using MapModS.Data;
using MapModS.Map;
using MapModS.Settings;
using System.Linq;
using UnityEngine;

namespace MapModS.UI
{
    internal class LookupText
    {
        public static GameObject Canvas;

        private static CanvasPanel _infoPanel;
        private static CanvasPanel _instructionPanel;

        private static string selectedLocation = "None selected";
        private static bool worldMapOpen = false;

        public static bool LookupActive()
        {
            return MapModS.LS.ModEnabled
                && (MapModS.LS.mapMode == MapMode.FullMap
                    || MapModS.LS.mapMode == MapMode.AllPins
                    || MapModS.LS.mapMode == MapMode.PinsOverMap);
        }

        public static void ShowWorldMap()
        {
            if (Canvas == null || GameManager.instance.gameMap == null || _infoPanel == null) return;

            if (!LookupActive() || TransitionText.TransitionModeActive())
            {
                Hide();
                return;
            }

            GameMap gameMap = GameManager.instance.gameMap.GetComponent<GameMap>();

            if (gameMap != null)
            {
                gameMap.panMinX = -29f;
                gameMap.panMaxX = 26f;
                gameMap.panMinY = -25f;
                gameMap.panMaxY = 20f;
            }

            worldMapOpen = true;

            Canvas.SetActive(true);

            _infoPanel.SetActive(MapModS.LS.lookupOn, MapModS.LS.lookupOn);
        }

        public static void Hide()
        {
            if (Canvas == null || _infoPanel == null) return;

            worldMapOpen = false;

            Canvas.SetActive(false);
        }

        public static void Initialize()
        {
            selectedLocation = "None selected";
        }

        public static void BuildText(GameObject _canvas)
        {
            Canvas = _canvas;

            _instructionPanel = new CanvasPanel
                (_canvas, GUIController.Instance.Images["ButtonsMenuBG"], new Vector2(10f, 20f), new Vector2(1346f, 0f), new Rect(0f, 0f, 0f, 0f));
            _instructionPanel.AddText("Control", "", new Vector2(-37f, 0f), Vector2.zero, GUIController.Instance.TrajanNormal, 14, FontStyle.Normal, TextAnchor.UpperRight);

            _instructionPanel.SetActive(true, true);

            _infoPanel = new CanvasPanel
                (_canvas, GUIController.Instance.Images["LookupBG"], new Vector2(1200f, 200f), new Vector2(GUIController.Instance.Images["LookupBG"].width, GUIController.Instance.Images["LookupBG"].height), new Rect(0f, 0f, GUIController.Instance.Images["LookupBG"].width, GUIController.Instance.Images["LookupBG"].height));
            _infoPanel.AddText("Info", "None selected", new Vector2(5f, 30f), new Vector2(GUIController.Instance.Images["LookupBG"].width - 20f, GUIController.Instance.Images["LookupBG"].height), GUIController.Instance.Perpetua, 19);

            _infoPanel.SetActive(false, false);

            SetTexts();
        }

        public static void Update()
        {
            if (Canvas == null
                || HeroController.instance == null
                || WorldMap.CustomPins == null
                || !LookupActive()
                || TransitionText.TransitionModeActive()) return;

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                && Input.GetKeyDown(KeyCode.L))
            {
                MapModS.LS.ToggleLookup();
                SetTexts();

                if (MapModS.LS.lookupOn)
                {
                    _infoPanel.SetActive(true, true);

                    UpdateSelectedPin();
                }
                else
                {
                    _infoPanel.SetActive(false, false);

                    WorldMap.CustomPins.ResizePins("None selected");
                }
            }
        }

        // Called every 0.1 seconds
        public static void UpdateSelectedPinCoroutine()
        {
            if (Canvas == null
                || WorldMap.CustomPins == null
                || !_infoPanel.Active
                || HeroController.instance == null
                || GameManager.instance.IsGamePaused()
                || !LookupActive()
                || !MapModS.LS.lookupOn)
            {
                return;
            }

            if (WorldMap.CustomPins.GetPinClosestToMiddle(selectedLocation, out selectedLocation))
            {
                UpdateSelectedPin();
            }
        }

        public static void UpdateSelectedPin()
        {
            if (!worldMapOpen) return;

            WorldMap.CustomPins.ResizePins(selectedLocation);
            SetTexts();
        }

        public static void SetTexts()
        {
            SetControlText();
            SetInstructionsText();
        }

        public static void SetControlText()
        {
            string controlText = "";

            if (MapModS.LS.lookupOn)
            {
                _instructionPanel.GetText("Control").SetTextColor(Color.green);
                controlText += "开关详细信息面板 (Ctrl-L): 开";
            }
            else
            {
                _instructionPanel.GetText("Control").SetTextColor(Color.white);
                controlText += "开关详细信息面板 (Ctrl-L): 关";
            }

            _instructionPanel.GetText("Control").UpdateText(controlText);
        }

        public static void SetInstructionsText()
        {
            string instructionsText = $"{StringUtils.ToCleanName(selectedLocation)}";

            PinDef pd = DataLoader.GetUsedPinDef(selectedLocation);

            if (pd != null)
            {
                instructionsText += $"\n\n房间: {pd.sceneName}";

                instructionsText += $"\n\n状态:";

                instructionsText += pd.pinLocationState switch
                {
                    PinLocationState.UncheckedUnreachable => " 被随机, 未检查, 无法到达",
                    PinLocationState.UncheckedReachable => " 被随机, 未检查, 可以到达",
                    PinLocationState.NonRandomizedUnchecked => " 未随机, 未检查或永存",
                    PinLocationState.OutOfLogicReachable => " 被随机, 未检查, 因为之前不在逻辑的操作而可以到达",
                    PinLocationState.Previewed => " 被随机, 已预览",
                    PinLocationState.Cleared => " 已清空",
                    PinLocationState.ClearedPersistent => " 被随机, 已清空, 永存",
                    _ => ""
                };

                if (DataLoader.IsInLogicLookup(selectedLocation))
                {
                    instructionsText += $"\n\n逻辑: {DataLoader.GetRawLogic(selectedLocation)}";
                }

                if (RandomizerMod.RandomizerMod.RS.TrackerData.previewedLocations.Contains(pd.name))
                {
                    instructionsText += $"\n\n已预览物品:";

                    string[] previewText = DataLoader.GetPreviewText(pd.abstractPlacementName);

                    if (previewText == null) return;

                    foreach (string text in previewText)
                    {
                        instructionsText += $" {StringUtils.ToCleanPreviewText(text)},";
                    }

                    instructionsText = instructionsText.Substring(0, instructionsText.Length - 1);
                }
                
                if (MapModS.LS.SpoilerOn
                    && pd.randoItems != null
                    && pd.randoItems.Any()
                    && (!RandomizerMod.RandomizerMod.RS.TrackerData.previewedLocations.Contains(pd.name)
                        || (RandomizerMod.RandomizerMod.RS.TrackerData.previewedLocations.Contains(pd.name)
                            && !pd.canPreviewItem)))
                {
                    instructionsText += $"\n\n剧透物品:";

                    foreach (ItemDef item in pd.randoItems)
                    {
                        instructionsText += $" {StringUtils.ToCleanName(item.itemName)},";
                    }

                    instructionsText = instructionsText.Substring(0, instructionsText.Length - 1);
                }
            }

            _infoPanel.GetText("Info").UpdateText(instructionsText);
        }
    }
}
