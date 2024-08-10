using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class VoiceChat
    {
        // 变量
        private Menu menu;
        public bool EnableVoicechat = UserDefaults.VoiceChatEnabled;
        public bool ShowCurrentSpeaker = UserDefaults.ShowCurrentSpeaker;
        public bool ShowVoiceStatus = UserDefaults.ShowVoiceStatus;
        public float currentProximity = (GetSettingsFloat(Setting.vmenu_override_voicechat_default_range) != 0.0) ? GetSettingsFloat(Setting.vmenu_override_voicechat_default_range) : UserDefaults.VoiceChatProximity;
        public List<string> channels = new()
        {
            "频道 1 (默认)",
            "频道 2",
            "频道 3",
            "频道 4",
        };
        public string currentChannel;
        private readonly List<float> proximityRange = new()
        {
            5f, // 5米
            10f, // 10米
            15f, // 15米
            20f, // 20米
            100f, // 100米
            300f, // 300米
            1000f, // 1.000公里
            2000f, // 2.000公里
            0f, // 全局
        };


        private void CreateMenu()
        {
            currentChannel = channels[0];
            if (IsAllowed(Permission.VCStaffChannel))
            {
                channels.Add("员工频道");
            }

            // 创建菜单
            menu = new Menu(Game.Player.Name, "语音聊天设置");

            var voiceChatEnabled = new MenuCheckboxItem("启用语音聊天", "启用或禁用语音聊天。", EnableVoicechat);
            var showCurrentSpeaker = new MenuCheckboxItem("显示当前发言者", "显示当前正在讲话的人。", ShowCurrentSpeaker);
            var showVoiceStatus = new MenuCheckboxItem("显示麦克风状态", "显示您的麦克风是打开还是静音。", ShowVoiceStatus);

            var proximity = new List<string>()
            {
                "5 米",
                "10 米",
                "15 米",
                "20 米",
                "100 米",
                "300 米",
                "1 公里",
                "2 公里",
                "全局",
            };
            var voiceChatProximity = new MenuItem("语音聊天接收范围 (" + ConvertToMetric(currentProximity) + ")", "设置语音聊天接收范围（以米为单位）。设置为 0 以启用全局范围。");
            var voiceChatChannel = new MenuListItem("语音聊天频道", channels, channels.IndexOf(currentChannel), "设置语音聊天频道。");

            if (IsAllowed(Permission.VCEnable))
            {
                menu.AddMenuItem(voiceChatEnabled);

                // 嵌套权限，因为如果没有启用语音聊天，您将无法使用这些设置。
                if (IsAllowed(Permission.VCShowSpeaker))
                {
                    menu.AddMenuItem(showCurrentSpeaker);
                }

                menu.AddMenuItem(voiceChatProximity);
                menu.AddMenuItem(voiceChatChannel);
                menu.AddMenuItem(showVoiceStatus);
            }

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == voiceChatEnabled)
                {
                    EnableVoicechat = _checked;
                }
                else if (item == showCurrentSpeaker)
                {
                    ShowCurrentSpeaker = _checked;
                }
                else if (item == showVoiceStatus)
                {
                    ShowVoiceStatus = _checked;
                }
            };

            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == voiceChatChannel)
                {
                    currentChannel = channels[newIndex];
                    Subtitle.Custom($"新的语音聊天频道设置为: ~b~{channels[newIndex]}~s~.");
                }
            };
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == voiceChatProximity)
                {
                    var result = await GetUserInput(windowTitle: $"输入接收范围（米）。当前: ({ConvertToMetric(currentProximity)})", maxInputLength: 6);

                    if (float.TryParse(result, out var resultfloat))
                    {
                        currentProximity = resultfloat;
                        Subtitle.Custom($"新的语音聊天接收范围设置为: ~b~{ConvertToMetric(currentProximity)}~s~.");
                        voiceChatProximity.Text = ("语音聊天接收范围 (" + ConvertToMetric(currentProximity) + ")");
                    }
                }
            };

        }
        static string ConvertToMetric(float input)
        {
            string val = "0m";
            if (input < 1.0)
            {
                val = (input * 100) + "cm";
            }
            else if (input >= 1.0)
            {
                if (input < 1000)
                {
                    val = input + "m";
                }
                else
                {
                    val = (input / 1000) + "km";
                }
            }
            if (input == 0)
            {
                val = "全局";
            }
            return val;
        }
        /// <summary>
        /// 如果菜单不存在，则创建菜单，然后返回它。
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
