using CitizenFX.Core;

using MenuAPI;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;

namespace vMenuClient.menus
{
    public class Recording
    {
        // 变量
        private Menu menu;

        private void CreateMenu()
        {
            AddTextEntryByHash(0x86F10CE6, "上传到 Cfx.re 论坛"); // 替换图库中的“上传到社交俱乐部”按钮
            AddTextEntry("ERROR_UPLOAD", "您确定要将这张照片上传到 Cfx.re 论坛吗？"); // 替换上传警告消息文本

            // 创建菜单。
            menu = new Menu("录制", "录制选项");

            var takePic = new MenuItem("拍摄照片", "拍摄一张照片并保存到暂停菜单图库。");
            var openPmGallery = new MenuItem("打开图库", "打开暂停菜单图库。");
            var startRec = new MenuItem("开始录制", "使用 GTA V 内置录制功能开始新的游戏录制。");
            var stopRec = new MenuItem("停止录制", "停止并保存当前的录制。");
            var openEditor = new MenuItem("Rockstar 编辑器", "打开 Rockstar 编辑器，请注意您可能需要先退出会话以避免一些问题。");

            // menu.AddMenuItem(takePic);
            // menu.AddMenuItem(openPmGallery);
            menu.AddMenuItem(startRec);
            menu.AddMenuItem(stopRec);
            menu.AddMenuItem(openEditor);

            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == startRec)
                {
                    if (IsRecording())
                    {
                        Notify.Alert("您已经在录制剪辑，需要先停止录制才能重新开始录制！");
                    }
                    else
                    {
                        StartRecording(1);
                    }
                }
                else if (item == openPmGallery)
                {
                    ActivateFrontendMenu((uint)GetHashKey("FE_MENU_VERSION_MP_PAUSE"), true, 3);
                }
                else if (item == takePic)
                {
                    BeginTakeHighQualityPhoto();
                    SaveHighQualityPhoto(-1);
                    FreeMemoryForHighQualityPhoto();
                }
                else if (item == stopRec)
                {
                    if (!IsRecording())
                    {
                        Notify.Alert("您当前没有录制剪辑，需要先开始录制才能停止并保存剪辑。");
                    }
                    else
                    {
                        StopRecordingAndSaveClip();
                    }
                }
                else if (item == openEditor)
                {
                    if (GetSettingsBool(Setting.vmenu_quit_session_in_rockstar_editor))
                    {
                        QuitSession();
                    }
                    ActivateRockstarEditor();
                    // 等待编辑器关闭。
                    while (IsPauseMenuActive())
                    {
                        await BaseScript.Delay(0);
                    }
                    // 然后屏幕渐显。
                    DoScreenFadeIn(1);
                    Notify.Alert("您在进入 Rockstar 编辑器之前已退出了当前会话。请重新启动游戏以重新加入服务器的主会话。", true, true);
                }
            };

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
