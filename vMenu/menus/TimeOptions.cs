using System.Collections.Generic;
using CitizenFX.Core;
using MenuAPI;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class TimeOptions
    {
        // Variables
        private Menu menu;
        public MenuItem freezeTimeToggle;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "时间选项");

            // Create menu items.
            freezeTimeToggle = new MenuItem("冻结/解冻时间", "启用或禁用时间冻结。");
            var earlymorning = new MenuItem("清晨", "将时间设置为 06:00.") { Label = "06:00" };
            var morning = new MenuItem("早晨", "将时间设置为 09:00.") { Label = "09:00" };
            var noon = new MenuItem("中午", "将时间设置为 12:00.") { Label = "12:00" };
            var earlyafternoon = new MenuItem("下午早些时候", "将时间设置为 15:00.") { Label = "15:00" };
            var afternoon = new MenuItem("下午", "将时间设置为 18:00.") { Label = "18:00" };
            var evening = new MenuItem("晚上", "将时间设置为 21:00.") { Label = "21:00" };
            var midnight = new MenuItem("午夜", "将时间设置为 00:00.") { Label = "00:00" };
            var night = new MenuItem("夜晚", "将时间设置为 03:00.") { Label = "03:00" };

            var hours = new List<string> { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            var minutes = new List<string> { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            for (var i = 10; i < 60; i++)
            {
                if (i < 24) hours.Add(i.ToString());
                minutes.Add(i.ToString());
            }
            var manualHour = new MenuListItem("设置自定义小时", hours, 0);
            var manualMinute = new MenuListItem("设置自定义分钟", minutes, 0);

            // Add items to the menu.
            if (IsAllowed(Permission.TOFreezeTime))
                menu.AddMenuItem(freezeTimeToggle);

            if (IsAllowed(Permission.TOSetTime))
            {
                menu.AddMenuItem(earlymorning);
                menu.AddMenuItem(morning);
                menu.AddMenuItem(noon);
                menu.AddMenuItem(earlyafternoon);
                menu.AddMenuItem(afternoon);
                menu.AddMenuItem(evening);
                menu.AddMenuItem(midnight);
                menu.AddMenuItem(night);
                menu.AddMenuItem(manualHour);
                menu.AddMenuItem(manualMinute);
            }

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == freezeTimeToggle)
                {
                    Subtitle.Info($"时间现在将 { (EventManager.IsServerTimeFrozen ? "~y~继续" : "~o~冻结")} ~s~.", prefix: "信息:");
                    UpdateServerTime(EventManager.GetServerHours, EventManager.GetServerMinutes, !EventManager.IsServerTimeFrozen);
                }
                else
                {
                    var newHour = (IsAllowed(Permission.TOFreezeTime) 
                        ? (index * 3) + 3 
                        : ((index + 1) * 3) + 3) % 24;

                    var newMinute = 0;
                    Subtitle.Info($"时间设置为 ~y~{(newHour < 10 ? $"0{newHour}" : newHour.ToString())}~s~:~y~" +
                        $"{(newMinute < 10 ? $"0{newMinute}" : newMinute.ToString())}~s~.", prefix: "信息:");
                    UpdateServerTime(newHour, newMinute, EventManager.IsServerTimeFrozen);
                }
            };

            menu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
            {
                var newHour = EventManager.GetServerHours;
                var newMinute = EventManager.GetServerMinutes;
                if (item == manualHour) newHour = item.ListIndex;
                else if (item == manualMinute) newMinute = item.ListIndex;

                Subtitle.Info($"时间设置为 ~y~{(newHour < 10 ? $"0{newHour}" : newHour.ToString())}~s~:~y~" +
                        $"{(newMinute < 10 ? $"0{newMinute}" : newMinute.ToString())}~s~.", prefix: "信息:");
                UpdateServerTime(newHour, newMinute, EventManager.IsServerTimeFrozen);
            };
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
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
