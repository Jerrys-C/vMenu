using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using vMenuShared;

using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class WeatherOptions
    {
        // 变量
        private Menu menu;
        public MenuCheckboxItem dynamicWeatherEnabled;
        public MenuCheckboxItem blackout;
        public MenuCheckboxItem snowEnabled;
        public static readonly List<string> weatherTypes = new()
        {
            "EXTRASUNNY",
            "CLEAR",
            "NEUTRAL",
            "SMOG",
            "FOGGY",
            "CLOUDS",
            "OVERCAST",
            "CLEARING",
            "RAIN",
            "THUNDER",
            "BLIZZARD",
            "SNOW",
            "SNOWLIGHT",
            "XMAS",
            "HALLOWEEN"
        };

        private void CreateMenu()
        {
            // 创建菜单
            menu = new Menu(Game.Player.Name, "天气选项");

            dynamicWeatherEnabled = new MenuCheckboxItem("切换动态天气", "启用或禁用动态天气变化。", EventManager.DynamicWeatherEnabled);
            blackout = new MenuCheckboxItem("切换停电", "这会禁用或启用地图上的所有灯光。", EventManager.IsBlackoutEnabled);
            snowEnabled = new MenuCheckboxItem("启用雪效", "这会强制地面出现雪，并为行人和车辆启用雪粒子效果。与X-MAS或轻雪天气配合使用效果最佳。", ConfigManager.GetSettingsBool(ConfigManager.Setting.vmenu_enable_snow));
            var extrasunny = new MenuItem("极其晴朗", "将天气设置为~y~极其晴朗~s~！") { ItemData = "EXTRASUNNY" };
            var clear = new MenuItem("晴朗", "将天气设置为~y~晴朗~s~！") { ItemData = "CLEAR" };
            var neutral = new MenuItem("中性", "将天气设置为~y~中性~s~！") { ItemData = "NEUTRAL" };
            var smog = new MenuItem("雾霾", "将天气设置为~y~雾霾~s~！") { ItemData = "SMOG" };
            var foggy = new MenuItem("多雾", "将天气设置为~y~多雾~s~！") { ItemData = "FOGGY" };
            var clouds = new MenuItem("多云", "将天气设置为~y~多云~s~！") { ItemData = "CLOUDS" };
            var overcast = new MenuItem("阴天", "将天气设置为~y~阴天~s~！") { ItemData = "OVERCAST" };
            var clearing = new MenuItem("放晴", "将天气设置为~y~放晴~s~！") { ItemData = "CLEARING" };
            var rain = new MenuItem("下雨", "将天气设置为~y~下雨~s~！") { ItemData = "RAIN" };
            var thunder = new MenuItem("雷暴", "将天气设置为~y~雷暴~s~！") { ItemData = "THUNDER" };
            var blizzard = new MenuItem("暴风雪", "将天气设置为~y~暴风雪~s~！") { ItemData = "BLIZZARD" };
            var snow = new MenuItem("雪", "将天气设置为~y~雪~s~！") { ItemData = "SNOW" };
            var snowlight = new MenuItem("轻雪", "将天气设置为~y~轻雪~s~！") { ItemData = "SNOWLIGHT" };
            var xmas = new MenuItem("圣诞雪", "将天气设置为~y~圣诞~s~！") { ItemData = "XMAS" };
            var halloween = new MenuItem("万圣节", "将天气设置为~y~万圣节~s~！") { ItemData = "HALLOWEEN" };
            var removeclouds = new MenuItem("移除所有云层", "移除天空中的所有云层！");
            var randomizeclouds = new MenuItem("随机云层", "向天空中添加随机云层！");

            if (IsAllowed(Permission.WODynamic))
            {
                menu.AddMenuItem(dynamicWeatherEnabled);
            }
            if (IsAllowed(Permission.WOBlackout))
            {
                menu.AddMenuItem(blackout);
            }
            if (IsAllowed(Permission.WOSetWeather))
            {
                menu.AddMenuItem(snowEnabled);
                menu.AddMenuItem(extrasunny);
                menu.AddMenuItem(clear);
                menu.AddMenuItem(neutral);
                menu.AddMenuItem(smog);
                menu.AddMenuItem(foggy);
                menu.AddMenuItem(clouds);
                menu.AddMenuItem(overcast);
                menu.AddMenuItem(clearing);
                menu.AddMenuItem(rain);
                menu.AddMenuItem(thunder);
                menu.AddMenuItem(blizzard);
                menu.AddMenuItem(snow);
                menu.AddMenuItem(snowlight);
                menu.AddMenuItem(xmas);
                menu.AddMenuItem(halloween);
            }
            if (IsAllowed(Permission.WORandomizeClouds))
            {
                menu.AddMenuItem(randomizeclouds);
            }

            if (IsAllowed(Permission.WORemoveClouds))
            {
                menu.AddMenuItem(removeclouds);
            }

            menu.OnItemSelect += (sender, item, index2) =>
            {
                if (item == removeclouds)
                {
                    ModifyClouds(true);
                }
                else if (item == randomizeclouds)
                {
                    ModifyClouds(false);
                }
                else if (item.ItemData is string weatherType)
                {
                    Notify.Custom($"天气将被更改为~y~{item.Text}~s~。这将需要{EventManager.WeatherChangeTime}秒。");
                    UpdateServerWeather(weatherType, EventManager.IsBlackoutEnabled, EventManager.DynamicWeatherEnabled, EventManager.IsSnowEnabled);
                }
            };

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == dynamicWeatherEnabled)
                {
                    Notify.Custom($"动态天气变化现在{(_checked ? "~g~启用" : "~r~禁用")}~s~。");
                    UpdateServerWeather(EventManager.GetServerWeather, EventManager.IsBlackoutEnabled, _checked, EventManager.IsSnowEnabled);
                }
                else if (item == blackout)
                {
                    Notify.Custom($"停电模式现在{(_checked ? "~g~启用" : "~r~禁用")}~s~。");
                    UpdateServerWeather(EventManager.GetServerWeather, _checked, EventManager.DynamicWeatherEnabled, EventManager.IsSnowEnabled);
                }
                else if (item == snowEnabled)
                {
                    Notify.Custom($"雪效现在强制{(_checked ? "~g~启用" : "~r~禁用")}~s~。");
                    UpdateServerWeather(EventManager.GetServerWeather, EventManager.IsBlackoutEnabled, EventManager.DynamicWeatherEnabled, _checked);
                }
            };
        }

        /// <summary>
        /// 如果菜单不存在，则创建菜单，然后返回菜单。
        /// </summary>
        /// <returns>菜单</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
