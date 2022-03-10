using MapModS.Data;
using MapModS.Settings;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Code borrowed from homothety: https://github.com/homothetyhk/RandomizerMod/
namespace MapModS.Map
{
    internal static class SpriteManager
    {
        private static Dictionary<string, Sprite> _sprites;

        public static void LoadEmbeddedPngs(string prefix)
        {
            Assembly a = typeof(SpriteManager).Assembly;
            _sprites = new Dictionary<string, Sprite>();

            foreach (string name in a.GetManifestResourceNames().Where(name => name.Substring(name.Length - 3).ToLower() == "png"))
            {
                string altName = prefix != null ? name.Substring(prefix.Length) : name;
                altName = altName.Remove(altName.Length - 4);
                altName = altName.Replace(".", "");
                Sprite sprite = FromStream(a.GetManifestResourceStream(name));
                _sprites[altName] = sprite;
            }
        }

        public static Sprite GetSpriteFromPool(PoolGroup pool, PinBorderColor color)
        {
            string spriteName = "undefined";

            switch (MapModS.GS.pinStyle)
            {
                case PinStyle.Normal:
                    spriteName = pool switch
                    {
                        PoolGroup.守梦者 => "pinDreamer",
                        PoolGroup.技能 => "pinSkill",
                        PoolGroup.护符 => "pinCharm",
                        PoolGroup.钥匙 => "pinKey",
                        PoolGroup.面具碎片 => "pinMask",
                        PoolGroup.容器碎片 => "pinVessel",
                        PoolGroup.护符槽 => "pinNotch",
                        PoolGroup.苍白矿石 => "pinOre",
                        PoolGroup.钱箱 => "pinGeo",
                        PoolGroup.腐臭蛋 => "pinEgg",
                        PoolGroup.古董 => "pinRelic",
                        PoolGroup.低语之根 => "pinRoot",
                        PoolGroup.Boss精华 => "pinEssenceBoss",
                        PoolGroup.幼虫 => "pinGrub",
                        PoolGroup.假虫子 => "pinGrub",
                        PoolGroup.地图 => "pinMap",
                        PoolGroup.鹿角站 => "pinStag",
                        PoolGroup.生命血茧 => "pinCocoon",
                        PoolGroup.格林火焰 => "pinFlame",
                        PoolGroup.猎人日志 => "pinLore",
                        PoolGroup.GeoRocks => "pinRock",
                        PoolGroup.Boss吉欧 => "pinGeo",
                        PoolGroup.灵魂图腾 => "pinTotem",
                        PoolGroup.碑文 => "pinLore",
                        PoolGroup.商店 => "pinShop",
                        PoolGroup.拉干 => "pinLever",
                        _ => "pinUnknown",
                    };
                    break;

                case PinStyle.Q_Marks_1:
                    spriteName = pool switch
                    {
                        PoolGroup.商店 => "pinShop",
                        _ => "pinUnknown",
                    };
                    break;

                case PinStyle.Q_Marks_2:
                    spriteName = pool switch
                    {
                        PoolGroup.幼虫 => "pinUnknown_GrubInv",
                        PoolGroup.假虫子 => "pinUnknown_GrubInv",
                        PoolGroup.生命血茧 => "pinUnknown_LifebloodInv",
                        PoolGroup.GeoRocks => "pinUnknown_GeoRockInv",
                        PoolGroup.灵魂图腾 => "pinUnknown_TotemInv",
                        PoolGroup.商店 => "pinShop",
                        _ => "pinUnknown",
                    };
                    break;

                case PinStyle.Q_Marks_3:
                    spriteName = pool switch
                    {
                        PoolGroup.幼虫 => "pinUnknown_Grub",
                        PoolGroup.假虫子 => "pinUnknown_Grub",
                        PoolGroup.生命血茧 => "pinUnknown_Lifeblood",
                        PoolGroup.GeoRocks => "pinUnknown_GeoRock",
                        PoolGroup.灵魂图腾 => "pinUnknown_Totem",
                        PoolGroup.商店 => "pinShop",
                        _ => "pinUnknown",
                    };
                    break;
            }

            switch (color)
            {
                case PinBorderColor.Previewed:
                    spriteName += "Green";
                    break;
                case PinBorderColor.OutOfLogic:
                    spriteName += "Red";
                    break;
                case PinBorderColor.Persistent:
                    spriteName += "Cyan";
                    break;
            }

            return GetSprite(spriteName);
        }

        public static Sprite GetSprite(string name)
        {
            if (_sprites.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }

            MapModS.Instance.LogWarn("Failed to load sprite named '" + name + "'");

            return _sprites["pinUnknown"];
        }

        private static Sprite FromStream(Stream s)
        {
            Texture2D tex = new(1, 1);
            byte[] buffer = ToArray(s);
            _ = tex.LoadImage(buffer, markNonReadable: true);
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 55);
        }

        private static byte[] ToArray(Stream s)
        {
            using MemoryStream ms = new();
            s.CopyTo(ms);
            return ms.ToArray();
        }
    }
}