﻿using MapModS.CanvasUtil;
using MapModS.Data;
using MapModS.Map;
using MapModS.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MapModS.UI
{
    // All the following was modified from the GUI implementation of BenchwarpMod by homothetyhk
    internal class PauseMenu
    {
        public static GameObject Canvas;

        private static readonly Dictionary<string, (UnityAction<string>, Vector2)> _mainButtons = new()
        {
            ["Spoilers"] = (SpoilersClicked, new Vector2(100f, 0f)),
            ["Randomized"] = (RandomizedClicked, new Vector2(200f, 0f)),
            ["Others"] = (OthersClicked, new Vector2(300f, 0f)),
            ["Style"] = (StyleClicked, new Vector2(0f, 30f)),
            ["Size"] = (SizeClicked, new Vector2(100f, 30f)),
            ["Mode"] = (ModeClicked, new Vector2(200f, 30f)),
        };

        private static readonly Dictionary<string, (UnityAction<string>, Vector2)> _poolPanelAuxButtons = new()
        {
            ["GroupBy"] = (GroupByClicked, new Vector2(-200f, 60f)),
            ["Persistent"] = (PersistentClicked, new Vector2(-100f, 60f))
        };

        private static CanvasPanel _mapControlPanel;

        public static void BuildMenu(GameObject _canvas)
        {
            Canvas = _canvas;

            _mapControlPanel = new CanvasPanel
                (_canvas, GUIController.Instance.Images["ButtonsMenuBG"], new Vector2(10f, 865f), new Vector2(1346f, 0f), new Rect(0f, 0f, 0f, 0f));
            _mapControlPanel.AddText("MapModLabel", "MapModS", new Vector2(0f, -25f), Vector2.zero, GUIController.Instance.TrajanNormal, 18);

            Rect buttonRect = new(0, 0, GUIController.Instance.Images["ButtonRect"].width, GUIController.Instance.Images["ButtonRect"].height);

            // Main settings
            // Toggle the mod on or off
            _mapControlPanel.AddButton
                (
                    "Enable",
                    GUIController.Instance.Images["ButtonRect"],
                    new Vector2(0f, 0f),
                    Vector2.zero,
                    EnableClicked,
                    buttonRect,
                    GUIController.Instance.TrajanBold,
                    "Enable",
                    fontSize: 10
                );

            if (!MapModS.LS.ModEnabled)
            {
                UpdateEnable();

                if (GameManager.instance.IsGamePaused())
                {
                    _mapControlPanel.SetActive(true, false);
                }

                return;
            }

            foreach (KeyValuePair<string, (UnityAction<string>, Vector2)> pair in _mainButtons)
            {
                _mapControlPanel.AddButton
                (
                    pair.Key,
                    GUIController.Instance.Images["ButtonRect"],
                    pair.Value.Item2,
                    Vector2.zero,
                    pair.Value.Item1,
                    buttonRect,
                    GUIController.Instance.TrajanBold,
                    pair.Key,
                    fontSize: 10
                );
            }

            // New panel for pool buttons
            CanvasPanel pools = _mapControlPanel.AddPanel
            (
                "PoolsPanel",
                GUIController.Instance.Images["ButtonRectEmpty"],
                new Vector2(400f, 0f),
                Vector2.zero,
                new Rect(0f, 0f, GUIController.Instance.Images["DropdownBG"].width, 270f)
            );
            _mapControlPanel.AddButton
            (
                "PoolsToggle",
                GUIController.Instance.Images["ButtonRect"],
                new Vector2(300f, 30f),
                Vector2.zero,
                s => PoolsPanelClicked(),
                buttonRect,
                GUIController.Instance.TrajanBold,
                "自定义\n图针",
                fontSize: 10
            );

            pools.SetActive(false, true);

            // Pool buttons
            foreach (PoolGroup group in Enum.GetValues(typeof(PoolGroup)))
            {
                if (group == PoolGroup.Unknown) continue;

                float x_offset = (float)(group - 1) % 9 * 90;
                float y_offset = (int)(group - 1) / 9 * 30;

                pools.AddButton
                (
                    group.ToString(),
                    GUIController.Instance.Images["ButtonRectEmpty"],
                    new Vector2(x_offset, y_offset),
                    Vector2.zero,
                    PoolClicked,
                    buttonRect,
                    GUIController.Instance.TrajanBold,
                    StringUtils.ToButtonGroup(group),
                    fontSize: 10
                );
            }

            int poolGroupCount = Enum.GetNames(typeof(PoolGroup)).Length;

            pools.AddButton
            (
                "Benches",
                GUIController.Instance.Images["ButtonRectEmpty"],
                new Vector2((float)(poolGroupCount - 1) % 9 * 90, (int)(poolGroupCount - 1) / 9 * 30),
                Vector2.zero,
                BenchClicked,
                buttonRect,
                GUIController.Instance.TrajanBold,
                "椅子",
                fontSize: 10
            );

            foreach (KeyValuePair<string, (UnityAction<string>, Vector2)> pair in _poolPanelAuxButtons)
            {
                pools.AddButton
                (
                    pair.Key,
                    GUIController.Instance.Images["ButtonRectEmpty"],
                    pair.Value.Item2,
                    Vector2.zero,
                    pair.Value.Item1,
                    buttonRect,
                    GUIController.Instance.TrajanBold,
                    pair.Key,
                    fontSize: 10
                );
            }

            UpdateGUI();

            _mapControlPanel.SetActive(false, true); // collapse all subpanels

            if (GameManager.instance.IsGamePaused())
            {
                _mapControlPanel.SetActive(true, false);
            }
        }

        // Called every frame
        public static void Update()
        {
            if (_mapControlPanel == null || GameManager.instance == null)
            {
                return;
            }

            if (HeroController.instance == null || !GameManager.instance.IsGameplayScene() || !GameManager.instance.IsGamePaused())
            {
                // Any time we aren't at the Pause Menu / don't want to show the UI otherwise
                if (_mapControlPanel.Active) _mapControlPanel.SetActive(false, true);
                return;
            }

            // On the frame that we enter the Pause Menu
            if (!_mapControlPanel.Active)
            {
                _mapControlPanel.Destroy();
                BuildMenu(Canvas);
            }
        }

        // Update all the buttons (text, color)
        public static void UpdateGUI()
        {
            if (GameManager.instance.gameMap == null) return;

            UpdateEnable();
            UpdateSpoilers();
            UpdateRandomized();
            UpdateOthers();
            UpdateStyle();
            UpdateSize();
            UpdateMode();
            UpdatePoolsPanel();

            foreach (PoolGroup group in Enum.GetValues(typeof(PoolGroup)))
            {
                if (group == PoolGroup.Unknown) continue;

                UpdatePool(group);
            }

            UpdateBench();
            UpdateGroupBy();
            UpdatePersistent();
        }

        public static void EnableClicked(string buttonName)
        {
            if (!(MapText.LockToggleEnable && MapText.Canvas.activeSelf))
            {
                MapModS.LS.ToggleModEnabled();

                if (!GameManager.instance.IsGamePaused() && !HeroController.instance.controlReqlinquished)
                {
                    UIManager.instance.checkpointSprite.Show();
                    UIManager.instance.checkpointSprite.Hide();
                }

                if (!MapModS.LS.ModEnabled
                    && GameManager.instance.gameMap != null
                    && SettingsUtil.IsTransitionRando())
                {
                    Transition.ResetMapColors(GameManager.instance.gameMap);
                }

                _mapControlPanel.Destroy();
                BuildMenu(Canvas);
                MapText.LockToggleEnable = true;
                MapText.RebuildText();
                //TransitionText.LockToggleEnable = true;
                TransitionText.SetTexts();
            }
        }

        private static void UpdateEnable()
        {
            _mapControlPanel.GetButton("Enable").SetTextColor
                (
                    MapModS.LS.ModEnabled ? Color.green : Color.red
                );
            _mapControlPanel.GetButton("Enable").UpdateText
                (
                    MapModS.LS.ModEnabled ? ("Mod\n开") : ("Mod\n关")
                );
        }

        public static void SpoilersClicked(string buttonName)
        {
            MapModS.LS.ToggleSpoilers();
            WorldMap.CustomPins.SetSprites();
            
            UpdateGUI();
            MapText.SetTexts();
            LookupText.UpdateSelectedPin();
        }

        private static void UpdateSpoilers()
        {
            _mapControlPanel.GetButton("Spoilers").SetTextColor
                (
                    MapModS.LS.SpoilerOn ? Color.green : Color.white
                );
            _mapControlPanel.GetButton("Spoilers").UpdateText
                (
                    MapModS.LS.SpoilerOn ? ("剧透:\n开") : ("剧透:\n关")
                );
        }

        public static void RandomizedClicked(string buttonName)
        {
            MapModS.LS.ToggleRandomizedOn();
            WorldMap.CustomPins.ResetPoolSettings();
            WorldMap.CustomPins.SetPinsActive();

            UpdateGUI();
            MapText.SetTexts();
        }

        private static void UpdateRandomized()
        {
            if (WorldMap.CustomPins == null) return;

            string randomizedText = "";

            if (MapModS.LS.randomizedOn)
            {
                _mapControlPanel.GetButton("Randomized").SetTextColor(Color.green);
                randomizedText += "随机:\n开";
            }
            else
            {
                _mapControlPanel.GetButton("Randomized").SetTextColor(Color.white);
                randomizedText += "随机:\n关";
            }

            if (WorldMap.CustomPins.IsRandomizedCustom())
            {
                _mapControlPanel.GetButton("Randomized").SetTextColor(Color.yellow);
                randomizedText += "(自定义)";
            }

            _mapControlPanel.GetButton("Randomized").UpdateText(randomizedText);
        }

        public static void OthersClicked(string buttonName)
        {
            MapModS.LS.ToggleOthersOn();
            WorldMap.CustomPins.ResetPoolSettings();
            WorldMap.CustomPins.SetPinsActive();

            UpdateGUI();
            MapText.SetTexts();
        }

        private static void UpdateOthers()
        {
            if (WorldMap.CustomPins == null) return;

            string othersText = "";

            if (MapModS.LS.othersOn)
            {
                _mapControlPanel.GetButton("Others").SetTextColor(Color.green);
                othersText += "其他:\n开";
            }
            else
            {
                _mapControlPanel.GetButton("Others").SetTextColor(Color.white);
                othersText += "其他:\n关";
            }

            if (WorldMap.CustomPins.IsOthersCustom())
            {
                _mapControlPanel.GetButton("Others").SetTextColor(Color.yellow);
                othersText += "(自定义)";
            }

            _mapControlPanel.GetButton("Others").UpdateText(othersText);
        }

        public static void StyleClicked(string buttonName)
        {
            MapModS.GS.TogglePinStyle();
            WorldMap.CustomPins.SetSprites();

            UpdateGUI();
            MapText.SetTexts();
        }

        private static void UpdateStyle()
        {
            switch (MapModS.GS.pinStyle)
            {
                case PinStyle.Normal:
                    _mapControlPanel.GetButton("Style").UpdateText("图针风格:\n普通");
                    break;

                case PinStyle.Q_Marks_1:
                    _mapControlPanel.GetButton("Style").UpdateText("图针风格:\n问号类型:1");
                    break;

                case PinStyle.Q_Marks_2:
                    _mapControlPanel.GetButton("Style").UpdateText("图针风格:\n问号类型:2");
                    break;

                case PinStyle.Q_Marks_3:
                    _mapControlPanel.GetButton("Style").UpdateText("图针风格:\n问号类型:3");
                    break;
            }
        }

        public static void SizeClicked(string buttonName)
        {
            MapModS.GS.TogglePinSize();

            if (WorldMap.CustomPins != null)
            {
                WorldMap.CustomPins.ResizePins("None selected");
            }

            UpdateGUI();
            MapText.SetTexts();

            if (MapModS.LS.lookupOn)
            {
                LookupText.UpdateSelectedPin();
            }
        }

        private static void UpdateSize()
        {
            switch (MapModS.GS.pinSize)
            {
                 case PinSize.Small:
                    _mapControlPanel.GetButton("Size").UpdateText("图针尺寸:\n小");
                    break;

                case PinSize.Medium:
                    _mapControlPanel.GetButton("Size").UpdateText("图针尺寸:\n中");
                    break;

                case PinSize.Large:
                    _mapControlPanel.GetButton("Size").UpdateText("图针尺寸:\n大");
                    break;
            }
        }

        public static void ModeClicked(string buttonName)
        {
            if (GameManager.instance.gameMap != null
                && (RandomizerMod.RandomizerMod.RS.GenerationSettings.TransitionSettings.Mode == RandomizerMod.Settings.TransitionSettings.TransitionMode.RoomRandomizer
                    && MapModS.LS.mapMode == MapMode.TransitionRando)
                || ((RandomizerMod.RandomizerMod.RS.GenerationSettings.TransitionSettings.Mode == RandomizerMod.Settings.TransitionSettings.TransitionMode.FullAreaRandomizer
                        || RandomizerMod.RandomizerMod.RS.GenerationSettings.TransitionSettings.Mode == RandomizerMod.Settings.TransitionSettings.TransitionMode.MapAreaRandomizer)
                    && MapModS.LS.mapMode == MapMode.TransitionRandoAlt))
            {
                Transition.ResetMapColors(GameManager.instance.gameMap);
            }

            MapModS.LS.ToggleFullMap();

            UpdateGUI();
            MapText.SetTexts();
        }

        private static void UpdateMode()
        {
            switch (MapModS.LS.mapMode)
            {
                case MapMode.FullMap:
                    _mapControlPanel.GetButton("Mode").SetTextColor(Color.green);
                    _mapControlPanel.GetButton("Mode").UpdateText("模式:\n所有地图");
                    break;

                case MapMode.AllPins:
                    _mapControlPanel.GetButton("Mode").SetTextColor(Color.white);
                    _mapControlPanel.GetButton("Mode").UpdateText("模式:\n所有图针");
                    break;

                case MapMode.PinsOverMap:
                    _mapControlPanel.GetButton("Mode").SetTextColor(Color.white);
                    _mapControlPanel.GetButton("Mode").UpdateText("模式:\n当前地图");
                    break;

                case MapMode.TransitionRando:
                    _mapControlPanel.GetButton("Mode").SetTextColor(Color.cyan);
                    _mapControlPanel.GetButton("Mode").UpdateText("模式:\n连接类型:1");
                    break;

                case MapMode.TransitionRandoAlt:
                    _mapControlPanel.GetButton("Mode").SetTextColor(Color.cyan);
                    _mapControlPanel.GetButton("Mode").UpdateText("模式:\n连接类型:2");
                    break;
            }
        }

        public static void PoolsPanelClicked()
        {
            _mapControlPanel.TogglePanel("PoolsPanel");

            UpdateGUI();
        }

        private static void UpdatePoolsPanel()
        {
            _mapControlPanel.GetButton("PoolsToggle").SetTextColor
                (
                _mapControlPanel.GetPanel("PoolsPanel").Active? Color.yellow : Color.white
                );;
        }

        public static void PoolClicked(string buttonName)
        {
            MapModS.LS.TogglePoolGroupState(buttonName);

            WorldMap.CustomPins.GetRandomizedGroups();

            UpdateGUI();
            MapText.SetTexts();
        }

        private static void UpdatePool(PoolGroup pool)
        {
            if (WorldMap.CustomPins == null) return;

            if (pool == PoolGroup.GeoRocks && !RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.GeoRocks)
            {
                _mapControlPanel.GetPanel("PoolsPanel").GetButton(pool.ToString()).UpdateText
                    (
                        "钱堆:\n"
                        + MapModS.LS.GeoRockCounter + " / " + "207"
                    );
            }

            switch (MapModS.LS.GetPoolGroupState(pool))
            {
                case PoolGroupState.Off:
                    _mapControlPanel.GetPanel("PoolsPanel").GetButton(pool.ToString()).SetTextColor(Color.white);
                    break;
                case PoolGroupState.On:
                    _mapControlPanel.GetPanel("PoolsPanel").GetButton(pool.ToString()).SetTextColor(Color.green);
                    break;
                case PoolGroupState.Mixed:
                    _mapControlPanel.GetPanel("PoolsPanel").GetButton(pool.ToString()).SetTextColor(Color.yellow);
                    break;
            }
        }

        public static void BenchClicked(string buttonName)
        {
            if (!PlayerData.instance.hasPinBench) return;

            MapModS.LS.ToggleBench();

            UpdateGUI();
        }

        public static void UpdateBench()
        {
            if (!PlayerData.instance.hasPinBench)
            {
                _mapControlPanel.GetPanel("PoolsPanel").GetButton("Benches").SetTextColor(Color.red);
                return;
            }

            _mapControlPanel.GetPanel("PoolsPanel").GetButton("Benches").SetTextColor
                (
                    MapModS.LS.showBenchPins ? Color.green : Color.white
                );
        }

        public static void GroupByClicked(string buttonName)
        {
            MapModS.LS.ToggleGroupBy();

            WorldMap.CustomPins.GetRandomizedGroups();
            WorldMap.CustomPins.ResetPoolSettings();

            UpdateGUI();
        }

        private static void UpdateGroupBy()
        {
            switch (MapModS.LS.groupBy)
            {
                case GroupBy.Location:
                    _mapControlPanel.GetPanel("PoolsPanel").GetButton("GroupBy").UpdateText("分组方式:\n随机点");
                    break;

                case GroupBy.Item:
                    _mapControlPanel.GetPanel("PoolsPanel").GetButton("GroupBy").UpdateText("分组方式:\n物品");
                    break;
            }
        }

        public static void PersistentClicked(string buttonName)
        {
            MapModS.GS.TogglePersistentOn();

            UpdateGUI();
        }

        private static void UpdatePersistent()
        {
            if (MapModS.GS.persistentOn)
            {
                _mapControlPanel.GetPanel("PoolsPanel").GetButton("Persistent").UpdateText("永存物品:\n开");
                _mapControlPanel.GetPanel("PoolsPanel").GetButton("Persistent").SetTextColor(Color.green);
            }
            else
            {
                _mapControlPanel.GetPanel("PoolsPanel").GetButton("Persistent").UpdateText("永存物品:\n关");
                _mapControlPanel.GetPanel("PoolsPanel").GetButton("Persistent").SetTextColor(Color.white);
            }
        }
    }
}