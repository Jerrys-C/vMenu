using MenuAPI;

namespace vMenuClient.menus
{
    public class About
    {
        // Variables
        private Menu menu;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu("vMenu", "关于 vMenu和服务器");

            // Create menu items.
            var version = new MenuItem("vMenu 版本", $" ~b~SASDOJ~s~ 正在使用 vMenu ~b~~h~{MainMenu.Version}~h~~s~.")
            {
                Label = $"~h~{MainMenu.Version}~h~"
            };
            var credits = new MenuItem("关于 vMenu / 鸣谢", "vMenu 由 ~b~Vespura~s~ 制作。更多信息请访问 ~b~www.vespura.com/vmenu~s~。");

            var serverInfoMessage = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_message);
            if (!string.IsNullOrEmpty(serverInfoMessage))
            {
                var serverInfo = new MenuItem("服务器信息", "WIKI：wiki.sasdoj.com\nCAD：cad.sasdoj.com\nTS：ts.sasdoj.com\nFivem: fivem.sasdoj.com\nDiscord：discord.gg/4HWq5mhp2w");
                var siteUrl = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_website_url);
                if (!string.IsNullOrEmpty(siteUrl))
                {
                    serverInfo.Label = $"SASDOJ";
                }
                menu.AddMenuItem(serverInfo);
            }
            menu.AddMenuItem(version);
            menu.AddMenuItem(credits);
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
