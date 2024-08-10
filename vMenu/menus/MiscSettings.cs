using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class MiscSettings
    {
        // Variables
        private Menu menu;
        private Menu teleportOptionsMenu;
        private Menu developerToolsMenu;
        private Menu entitySpawnerMenu;

        public bool ShowSpeedoKmh { get; private set; } = UserDefaults.MiscSpeedKmh;
        public bool ShowSpeedoMph { get; private set; } = UserDefaults.MiscSpeedMph;
        public bool ShowCoordinates { get; private set; } = false;
        public bool HideHud { get; private set; } = false;
        public bool HideRadar { get; private set; } = false;
        public bool ShowLocation { get; private set; } = UserDefaults.MiscShowLocation;
        public bool DeathNotifications { get; private set; } = UserDefaults.MiscDeathNotifications;
        public bool JoinQuitNotifications { get; private set; } = UserDefaults.MiscJoinQuitNotifications;
        public bool LockCameraX { get; private set; } = false;
        public bool LockCameraY { get; private set; } = false;
        public bool ShowLocationBlips { get; private set; } = UserDefaults.MiscLocationBlips;
        public bool ShowPlayerBlips { get; private set; } = UserDefaults.MiscShowPlayerBlips;
        public bool MiscShowOverheadNames { get; private set; } = UserDefaults.MiscShowOverheadNames;
        public bool ShowVehicleModelDimensions { get; private set; } = false;
        public bool ShowPedModelDimensions { get; private set; } = false;
        public bool ShowPropModelDimensions { get; private set; } = false;
        public bool ShowEntityHandles { get; private set; } = false;
        public bool ShowEntityModels { get; private set; } = false;
        public bool ShowEntityNetOwners { get; private set; } = false;
        public bool MiscRespawnDefaultCharacter { get; private set; } = UserDefaults.MiscRespawnDefaultCharacter;
        public bool RestorePlayerAppearance { get; private set; } = UserDefaults.MiscRestorePlayerAppearance;
        public bool RestorePlayerWeapons { get; private set; } = UserDefaults.MiscRestorePlayerWeapons;
        public bool DrawTimeOnScreen { get; internal set; } = UserDefaults.MiscShowTime;
        public bool MiscRightAlignMenu { get; private set; } = UserDefaults.MiscRightAlignMenu;
        public bool MiscDisablePrivateMessages { get; private set; } = UserDefaults.MiscDisablePrivateMessages;
        public bool MiscDisableControllerSupport { get; private set; } = UserDefaults.MiscDisableControllerSupport;

        internal bool TimecycleEnabled { get; private set; } = false;
        internal int LastTimeCycleModifierIndex { get; private set; } = UserDefaults.MiscLastTimeCycleModifierIndex;
        internal int LastTimeCycleModifierStrength { get; private set; } = UserDefaults.MiscLastTimeCycleModifierStrength;


        // keybind states
        public bool KbTpToWaypoint { get; private set; } = UserDefaults.KbTpToWaypoint;
        public int KbTpToWaypointKey { get; } = vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key) != -1
            ? vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key)
            : 168; // 168 (F7 by default)
        public bool KbDriftMode { get; private set; } = UserDefaults.KbDriftMode;
        public bool KbRecordKeys { get; private set; } = UserDefaults.KbRecordKeys;
        public bool KbRadarKeys { get; private set; } = UserDefaults.KbRadarKeys;
        public bool KbPointKeys { get; private set; } = UserDefaults.KbPointKeys;

        internal static List<vMenuShared.ConfigManager.TeleportLocation> TpLocations = new();

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            MenuController.MenuAlignment = MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
            if (MenuController.MenuAlignment != (MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left))
            {
                Notify.Error(CommonErrors.RightAlignedNotSupported);

                // (re)set the default to left just in case so they don't get this error again in the future.
                MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                MiscRightAlignMenu = false;
                UserDefaults.MiscRightAlignMenu = false;
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, "杂项设置");
            teleportOptionsMenu = new Menu(Game.Player.Name, "传送选项");
            developerToolsMenu = new Menu(Game.Player.Name, "开发工具");
            entitySpawnerMenu = new Menu(Game.Player.Name, "实体生成");

            // teleport menu
            var teleportMenu = new Menu(Game.Player.Name, "传送位置");
            var teleportMenuBtn = new MenuItem("传送位置", "传送到服务器所有者预先配置的位置。");
            MenuController.AddSubmenu(menu, teleportMenu);
            MenuController.BindMenuItem(menu, teleportMenu, teleportMenuBtn);

            // keybind settings menu
            var keybindMenu = new Menu(Game.Player.Name, "键绑定设置");
            var keybindMenuBtn = new MenuItem("键绑定设置", "启用或禁用某些选项的键绑定。");
            MenuController.AddSubmenu(menu, keybindMenu);
            MenuController.BindMenuItem(menu, keybindMenu, keybindMenuBtn);

            // keybind settings menu items
            var kbTpToWaypoint = new MenuCheckboxItem("传送到航点", "按下键绑定时传送到您的航点。默认情况下，此键绑定设置为 ~r~F7~s~，服务器所有者可以更改此设置，所以如果您不知道它是什么，请询问他们。", KbTpToWaypoint);
            var kbDriftMode = new MenuCheckboxItem("漂移模式", "按住键盘上的左Shift键或控制器上的X键时，使您的车辆几乎没有牵引力。", KbDriftMode);
            var kbRecordKeys = new MenuCheckboxItem("录制控件", "启用或禁用键盘和控制器上的录制（Rockstar编辑器的游戏录制）热键。", KbRecordKeys);
            var kbRadarKeys = new MenuCheckboxItem("小地图控件", "按下多人游戏信息键（键盘上的z键，控制器上的下箭头键）在扩展雷达和普通雷达之间切换。", KbRadarKeys);
            var kbPointKeysCheckbox = new MenuCheckboxItem("手指指向控件", "启用手指指向切换键。默认的QWERTY键盘映射为'B'，控制器则是快速双击右摇杆。", KbPointKeys);
            var backBtn = new MenuItem("返回");

            // Create the menu items.
            var rightAlignMenu = new MenuCheckboxItem("右对齐菜单", "如果您希望vMenu显示在屏幕的左侧，请禁用此选项。此选项会立即保存。您无需点击保存偏好。", MiscRightAlignMenu);
            var disablePms = new MenuCheckboxItem("禁用私信", "阻止其他人通过在线玩家菜单向您发送私信。这也阻止您向其他玩家发送消息。", MiscDisablePrivateMessages);
            var disableControllerKey = new MenuCheckboxItem("禁用控制器支持", "这会禁用控制器菜单切换键。这不会禁用导航按钮。", MiscDisableControllerSupport);
            var speedKmh = new MenuCheckboxItem("显示速度 KM/H", "在屏幕上显示速度计，指示您的速度（单位：KM/H）。", ShowSpeedoKmh);
            var speedMph = new MenuCheckboxItem("显示速度 MPH", "在屏幕上显示速度计，指示您的速度（单位：MPH）。", ShowSpeedoMph);
            var coords = new MenuCheckboxItem("显示坐标", "在屏幕顶部显示您当前的坐标。", ShowCoordinates);
            var hideRadar = new MenuCheckboxItem("隐藏雷达", "隐藏雷达/小地图。", HideRadar);
            var hideHud = new MenuCheckboxItem("隐藏HUD", "隐藏所有HUD元素。", HideHud);
            var showLocation = new MenuCheckboxItem("显示位置", "显示您当前的位置和方向，以及最近的交叉路口。类似于PLD。~r~警告：此功能在60 Hz下运行时可能会占用多达4.6 FPS。", ShowLocation) { LeftIcon = MenuItem.Icon.WARNING };
            var drawTime = new MenuCheckboxItem("在屏幕上显示时间", "在屏幕上显示当前时间。", DrawTimeOnScreen);
            var saveSettings = new MenuItem("保存个人设置", "保存您当前的设置。所有保存操作都在客户端进行，如果您重新安装Windows，您将丢失设置。设置在使用vMenu的所有服务器之间共享。")
            {
                RightIcon = MenuItem.Icon.TICK
            };
            var exportData = new MenuItem("导出/导入数据", "即将推出（TM）：导入和导出您的保存数据的功能。");
            var joinQuitNotifs = new MenuCheckboxItem("加入/退出通知", "有人加入或离开服务器时接收通知。", JoinQuitNotifications);
            var deathNotifs = new MenuCheckboxItem("死亡通知", "有人死亡或被杀时接收通知。", DeathNotifications);
            var nightVision = new MenuCheckboxItem("切换夜视", "启用或禁用夜视。", false);
            var thermalVision = new MenuCheckboxItem("切换热成像", "启用或禁用热成像。", false);
            var vehModelDimensions = new MenuCheckboxItem("显示车辆尺寸", "绘制当前靠近您的每辆车的模型轮廓。", ShowVehicleModelDimensions);
            var propModelDimensions = new MenuCheckboxItem("显示道具尺寸", "绘制当前靠近您的每个道具的模型轮廓。", ShowPropModelDimensions);
            var pedModelDimensions = new MenuCheckboxItem("显示行人尺寸", "绘制当前靠近您的每个行人的模型轮廓。", ShowPedModelDimensions);
            var showEntityHandles = new MenuCheckboxItem("显示实体句柄", "绘制所有靠近实体的实体句柄（您必须启用上面的轮廓功能才能使其工作）。", ShowEntityHandles);
            var showEntityModels = new MenuCheckboxItem("显示实体模型", "绘制所有靠近实体的实体模型（您必须启用上面的轮廓功能才能使其工作）。", ShowEntityModels);
            var showEntityNetOwners = new MenuCheckboxItem("显示网络所有者", "绘制所有靠近实体的实体网络所有者（您必须启用上面的轮廓功能才能使其工作）。", ShowEntityNetOwners);
            var dimensionsDistanceSlider = new MenuSliderItem("显示尺寸半径", "显示实体模型/句柄/尺寸绘制范围。", 0, 20, 20, false);

            var clearArea = new MenuItem("清理区域", "清理您周围的区域（100米）。伤害、污垢、行人、道具、车辆等一切都将被清理、修复并重置为默认世界状态。");
            var lockCamX = new MenuCheckboxItem("锁定摄像机水平旋转", "锁定摄像机的水平旋转。我想这在直升机中可能有用。", false);
            var lockCamY = new MenuCheckboxItem("锁定摄像机垂直旋转", "锁定摄像机的垂直旋转。我想这在直升机中可能有用。", false);
            // Entity spawner
            var spawnNewEntity = new MenuItem("生成新实体", "在世界中生成实体并让您设置其位置和旋转。");
             var confirmEntityPosition = new MenuItem("确认实体位置", "停止放置实体并将其设置在当前位置。");
            var cancelEntity = new MenuItem("取消", "删除当前实体并取消其放置。");
            var confirmAndDuplicate = new MenuItem("确认实体位置并复制", "停止放置实体并将其设置在当前位置，并创建新的实体进行放置。");

            var connectionSubmenu = new Menu(Game.Player.Name, "连接选项");
            var connectionSubmenuBtn = new MenuItem("连接选项", "服务器连接/退出游戏选项。");

            var quitSession = new MenuItem("退出会话", "将您留在服务器上，但退出网络会话。~r~当您是主持人时无法使用。");
            var rejoinSession = new MenuItem("重新加入会话", "这在所有情况下可能不起作用，但如果您想在点击“退出会话”后重新加入以前的会话，可以尝试使用此选项。");
            var quitGame = new MenuItem("退出游戏", "5秒后退出游戏。");
            var disconnectFromServer = new MenuItem("从服务器断开连接", "将您从服务器断开连接并返回服务器列表。~r~不推荐使用此功能，完全退出游戏并重新启动以获得更好的体验。");
            connectionSubmenu.AddMenuItem(quitSession);
            connectionSubmenu.AddMenuItem(rejoinSession);
            connectionSubmenu.AddMenuItem(quitGame);
            connectionSubmenu.AddMenuItem(disconnectFromServer);

            var enableTimeCycle = new MenuCheckboxItem("启用时间周期修改器", "启用或禁用下面列表中的时间周期修改器。", TimecycleEnabled);
            var timeCycleModifiersListData = TimeCycles.Timecycles.ToList();
            for (var i = 0; i < timeCycleModifiersListData.Count; i++)
            {
                timeCycleModifiersListData[i] += $" ({i + 1}/{timeCycleModifiersListData.Count})";
            }
            var timeCycles = new MenuListItem("时间周期修改器", timeCycleModifiersListData, MathUtil.Clamp(LastTimeCycleModifierIndex, 0, Math.Max(0, timeCycleModifiersListData.Count - 1)), "选择一个时间周期修改器并启用上面的复选框。");
            var timeCycleIntensity = new MenuSliderItem("时间周期修改器强度", "设置时间周期修改器的强度。", 0, 20, LastTimeCycleModifierStrength, true);

            var locationBlips = new MenuCheckboxItem("位置标记", "在地图上显示一些常见位置的标记。", ShowLocationBlips);
            var playerBlips = new MenuCheckboxItem("显示玩家标记", "在地图上显示所有玩家的标记。~y~注意：当服务器使用 OneSync Infinity 时，这对远离你的玩家无效。", ShowPlayerBlips);
            var playerNames = new MenuCheckboxItem("显示玩家名字", "启用或禁用玩家头顶的名字。", MiscShowOverheadNames);
            var respawnDefaultCharacter = new MenuCheckboxItem("重生为默认 MP 角色", "启用此选项后，你将作为默认保存的 MP 角色重生。注意，服务器所有者可以全局禁用此选项。要设置默认角色，请前往你保存的 MP 角色之一并点击“设置为默认角色”按钮。", MiscRespawnDefaultCharacter);
            var restorePlayerAppearance = new MenuCheckboxItem("恢复玩家外观", "每次重生后恢复玩家的外观。重新加入服务器不会恢复你之前的外观。", RestorePlayerAppearance);
            var restorePlayerWeapons = new MenuCheckboxItem("恢复玩家武器", "每次重生后恢复玩家的武器。重新加入服务器不会恢复你之前的武器。", RestorePlayerWeapons);

            MenuController.AddSubmenu(menu, connectionSubmenu);
            MenuController.BindMenuItem(menu, connectionSubmenu, connectionSubmenuBtn);

            keybindMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == kbTpToWaypoint)
                {
                    KbTpToWaypoint = _checked;
                }
                else if (item == kbDriftMode)
                {
                    KbDriftMode = _checked;
                }
                else if (item == kbRecordKeys)
                {
                    KbRecordKeys = _checked;
                }
                else if (item == kbRadarKeys)
                {
                    KbRadarKeys = _checked;
                }
                else if (item == kbPointKeysCheckbox)
                {
                    KbPointKeys = _checked;
                }
            };
            keybindMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == backBtn)
                {
                    keybindMenu.GoBack();
                }
            };

            connectionSubmenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == quitGame)
                {
                    CommonFunctions.QuitGame();
                }
                else if (item == quitSession)
                {
                    if (NetworkIsSessionActive())
                    {
                        if (NetworkIsHost())
                        {
                            Notify.Error("对不起，你不能在作为主机时离开会话。这会阻止其他玩家加入/留在服务器上。");
                        }
                        else
                        {
                            QuitSession();
                        }
                    }
                    else
                    {
                        Notify.Error("你当前不在任何会话中。");
                    }
                }
                else if (item == rejoinSession)
                {
                    if (NetworkIsSessionActive())
                    {
                        Notify.Error("你已经连接到一个会话。");
                    }
                    else
                    {
                        Notify.Info("尝试重新加入会话。");
                        NetworkSessionHost(-1, 32, false);
                    }
                }
                else if (item == disconnectFromServer)
                {
                    RegisterCommand("退出", new Action<dynamic, dynamic, dynamic>((a, b, c) => { }), false);
                    ExecuteCommand("退出");
                }
            };

            // Teleportation options
            if (IsAllowed(Permission.MSTeleportToWp) || IsAllowed(Permission.MSTeleportLocations) || IsAllowed(Permission.MSTeleportToCoord))
            {
                var teleportOptionsMenuBtn = new MenuItem("传送选项", "各种传送选项。") { Label = "→→→" };
                menu.AddMenuItem(teleportOptionsMenuBtn);
                MenuController.BindMenuItem(menu, teleportOptionsMenu, teleportOptionsMenuBtn);

                var tptowp = new MenuItem("传送到标记点", "传送到你地图上的标记点。");
                var tpToCoord = new MenuItem("传送到坐标", "输入x, y, z坐标，你将被传送到该位置。");
                var saveLocationBtn = new MenuItem("保存传送位置", "将你当前的位置添加到传送位置菜单，并在服务器上保存。");
                teleportOptionsMenu.OnItemSelect += async (sender, item, index) =>
                {
                    // Teleport to waypoint.
                    if (item == tptowp)
                    {
                        TeleportToWp();
                    }
                    else if (item == tpToCoord)
                    {
                        var x = await GetUserInput("输入X坐标。");
                        if (string.IsNullOrEmpty(x))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        var y = await GetUserInput("输入Y坐标。");
                        if (string.IsNullOrEmpty(y))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        var z = await GetUserInput("输入Z坐标。");
                        if (string.IsNullOrEmpty(z))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }

                        if (!float.TryParse(x, out var posX))
                            {
                            if (int.TryParse(x, out var intX))
                            {
                                posX = intX;
                            }
                            else
                            {
                                Notify.Error("你输入的X坐标无效。");
                                return;
                            }
                        }
                        if (!float.TryParse(y, out var posY))
                        {
                            if (int.TryParse(y, out var intY))
                            {
                                posY = intY;
                            }
                            else
                            {
                                Notify.Error("你输入的Y坐标无效。");
                                return;
                            }
                        }
                        if (!float.TryParse(z, out var posZ))
                        {
                            if (int.TryParse(z, out var intZ))
                            {
                                posZ = intZ;
                            }
                            else
                            {
                                Notify.Error("你输入的Z坐标无效。");
                                return;
                            }
                        }

                        await TeleportToCoords(new Vector3(posX, posY, posZ), true);
                    }
                    else if (item == saveLocationBtn)
                    {
                        SavePlayerLocationToLocationsFile();
                    }
                };

                if (IsAllowed(Permission.MSTeleportToWp))
                {
                    teleportOptionsMenu.AddMenuItem(tptowp);
                    keybindMenu.AddMenuItem(kbTpToWaypoint);
                }
                if (IsAllowed(Permission.MSTeleportToCoord))
                {
                    teleportOptionsMenu.AddMenuItem(tpToCoord);
                }
                if (IsAllowed(Permission.MSTeleportLocations))
                {
                    teleportOptionsMenu.AddMenuItem(teleportMenuBtn);

                    MenuController.AddSubmenu(teleportOptionsMenu, teleportMenu);
                    MenuController.BindMenuItem(teleportOptionsMenu, teleportMenu, teleportMenuBtn);
                    teleportMenuBtn.Label = "→→→";

                    teleportMenu.OnMenuOpen += (sender) =>
                    {
                        if (teleportMenu.Size != TpLocations.Count())
                        {
                            teleportMenu.ClearMenuItems();
                            foreach (var location in TpLocations)
                            {
                                var x = Math.Round(location.coordinates.X, 2);
                                var y = Math.Round(location.coordinates.Y, 2);
                                var z = Math.Round(location.coordinates.Z, 2);
                                var heading = Math.Round(location.heading, 2);
                                var tpBtn = new MenuItem(location.name, $"传送到 ~y~{location.name}~n~~s~x: ~y~{x}~n~~s~y: ~y~{y}~n~~s~z: ~y~{z}~n~~s~heading: ~y~{heading}") { ItemData = location };
                                teleportMenu.AddMenuItem(tpBtn);
                            }
                        }
                    };

                    teleportMenu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item.ItemData is vMenuShared.ConfigManager.TeleportLocation tl)
                        {
                            await TeleportToCoords(tl.coordinates, true);
                            SetEntityHeading(Game.PlayerPed.Handle, tl.heading);
                            SetGameplayCamRelativeHeading(0f);
                        }
                    };

                    if (IsAllowed(Permission.MSTeleportSaveLocation))
                    {
                        teleportOptionsMenu.AddMenuItem(saveLocationBtn);
                    }
                }

            }

            #region dev tools menu

            var devToolsBtn = new MenuItem("开发者工具", "各种开发/调试工具。") { Label = "→→→" };
            menu.AddMenuItem(devToolsBtn);
            MenuController.AddSubmenu(menu, developerToolsMenu);
            MenuController.BindMenuItem(menu, developerToolsMenu, devToolsBtn);

            // clear area and coordinates
            if (IsAllowed(Permission.MSClearArea))
            {
                developerToolsMenu.AddMenuItem(clearArea);
            }
            if (IsAllowed(Permission.MSShowCoordinates))
            {
                developerToolsMenu.AddMenuItem(coords);
            }

            // model outlines
            if ((!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_disable_entity_outlines_tool)) && (IsAllowed(Permission.MSDevTools)))
            {
                developerToolsMenu.AddMenuItem(vehModelDimensions);
                developerToolsMenu.AddMenuItem(propModelDimensions);
                developerToolsMenu.AddMenuItem(pedModelDimensions);
                developerToolsMenu.AddMenuItem(showEntityHandles);
                developerToolsMenu.AddMenuItem(showEntityModels);
                developerToolsMenu.AddMenuItem(showEntityNetOwners);
                developerToolsMenu.AddMenuItem(dimensionsDistanceSlider);
            }


            // timecycle modifiers
            developerToolsMenu.AddMenuItem(timeCycles);
            developerToolsMenu.AddMenuItem(enableTimeCycle);
            developerToolsMenu.AddMenuItem(timeCycleIntensity);

            developerToolsMenu.OnSliderPositionChange += (sender, item, oldPos, newPos, itemIndex) =>
            {
                if (item == timeCycleIntensity)
                {
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        var intensity = newPos / 20f;
                        SetTimecycleModifierStrength(intensity);
                    }
                    UserDefaults.MiscLastTimeCycleModifierIndex = timeCycles.ListIndex;
                    UserDefaults.MiscLastTimeCycleModifierStrength = timeCycleIntensity.Position;
                }
                else if (item == dimensionsDistanceSlider)
                {
                    FunctionsController.entityRange = newPos / 20f * 2000f; // max radius = 2000f;
                }
            };

            developerToolsMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == timeCycles)
                {
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        var intensity = timeCycleIntensity.Position / 20f;
                        SetTimecycleModifierStrength(intensity);
                    }
                    UserDefaults.MiscLastTimeCycleModifierIndex = timeCycles.ListIndex;
                    UserDefaults.MiscLastTimeCycleModifierStrength = timeCycleIntensity.Position;
                }
            };

            developerToolsMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == clearArea)
                {
                    var pos = Game.PlayerPed.Position;
                    BaseScript.TriggerServerEvent("vMenu:ClearArea", pos.X, pos.Y, pos.Z);
                }
            };

            developerToolsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == vehModelDimensions)
                {
                    ShowVehicleModelDimensions = _checked;
                }
                else if (item == propModelDimensions)
                {
                    ShowPropModelDimensions = _checked;
                }
                else if (item == pedModelDimensions)
                {
                    ShowPedModelDimensions = _checked;
                }
                else if (item == showEntityHandles)
                {
                    ShowEntityHandles = _checked;
                }
                else if (item == showEntityModels)
                {
                    ShowEntityModels = _checked;
                }
                else if (item == showEntityNetOwners)
                {
                    ShowEntityNetOwners = _checked;
                }
                else if (item == enableTimeCycle)
                {
                    TimecycleEnabled = _checked;
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        var intensity = timeCycleIntensity.Position / 20f;
                        SetTimecycleModifierStrength(intensity);
                    }
                }
                else if (item == coords)
                {
                    ShowCoordinates = _checked;
                }
            };

            if (IsAllowed(Permission.MSEntitySpawner))
            {
                var entSpawnerMenuBtn = new MenuItem("实体生成器", "生成和移动实体") { Label = "→→→" };
                developerToolsMenu.AddMenuItem(entSpawnerMenuBtn);
                MenuController.BindMenuItem(developerToolsMenu, entitySpawnerMenu, entSpawnerMenuBtn);

                entitySpawnerMenu.AddMenuItem(spawnNewEntity);
                entitySpawnerMenu.AddMenuItem(confirmEntityPosition);
                entitySpawnerMenu.AddMenuItem(confirmAndDuplicate);
                entitySpawnerMenu.AddMenuItem(cancelEntity);

                entitySpawnerMenu.OnItemSelect += async (sender, item, index) =>
                {
                    if (item == spawnNewEntity)
                    {
                        if (EntitySpawner.CurrentEntity != null || EntitySpawner.Active)
                        {
                            Notify.Error("您已经在放置一个实体，请设置其位置或取消并重试！");
                            return;
                        }

                        var result = await GetUserInput(windowTitle: "输入模型名称");

                        if (string.IsNullOrEmpty(result))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }

                        EntitySpawner.SpawnEntity(result, Game.PlayerPed.Position);
                    }
                    else if (item == confirmEntityPosition || item == confirmAndDuplicate)
                    {
                        if (EntitySpawner.CurrentEntity != null)
                        {
                            EntitySpawner.FinishPlacement(item == confirmAndDuplicate);
                        }
                        else
                        {
                            Notify.Error("没有实体可以确认位置！");
                        }
                    }
                    else if (item == cancelEntity)
                    {
                        if (EntitySpawner.CurrentEntity != null)
                        {
                            EntitySpawner.CurrentEntity.Delete();
                        }
                        else
                        {
                            Notify.Error("没有实体可以取消！");
                        }
                    }
                };
            }

            #endregion


            // Keybind options
            if (IsAllowed(Permission.MSDriftMode))
            {
                keybindMenu.AddMenuItem(kbDriftMode);
            }
            // always allowed keybind menu options
            keybindMenu.AddMenuItem(kbRecordKeys);
            keybindMenu.AddMenuItem(kbRadarKeys);
            keybindMenu.AddMenuItem(kbPointKeysCheckbox);
            keybindMenu.AddMenuItem(backBtn);

            // Always allowed
            menu.AddMenuItem(rightAlignMenu);
            menu.AddMenuItem(disablePms);
            menu.AddMenuItem(disableControllerKey);
            menu.AddMenuItem(speedKmh);
            menu.AddMenuItem(speedMph);
            menu.AddMenuItem(keybindMenuBtn);
            keybindMenuBtn.Label = "→→→";
            if (IsAllowed(Permission.MSConnectionMenu))
            {
                menu.AddMenuItem(connectionSubmenuBtn);
                connectionSubmenuBtn.Label = "→→→";
            }
            if (IsAllowed(Permission.MSShowLocation))
            {
                menu.AddMenuItem(showLocation);
            }
            menu.AddMenuItem(drawTime); // always allowed
            if (IsAllowed(Permission.MSJoinQuitNotifs))
            {
                menu.AddMenuItem(joinQuitNotifs);
            }
            if (IsAllowed(Permission.MSDeathNotifs))
            {
                menu.AddMenuItem(deathNotifs);
            }
            if (IsAllowed(Permission.MSNightVision))
            {
                menu.AddMenuItem(nightVision);
            }
            if (IsAllowed(Permission.MSThermalVision))
            {
                menu.AddMenuItem(thermalVision);
            }
            if (IsAllowed(Permission.MSLocationBlips))
            {
                menu.AddMenuItem(locationBlips);
                ToggleBlips(ShowLocationBlips);
            }
            if (IsAllowed(Permission.MSPlayerBlips))
            {
                menu.AddMenuItem(playerBlips);
            }
            if (IsAllowed(Permission.MSOverheadNames))
            {
                menu.AddMenuItem(playerNames);
            }
            // always allowed, it just won't do anything if the server owner disabled the feature, but players can still toggle it.
            menu.AddMenuItem(respawnDefaultCharacter);
            if (IsAllowed(Permission.MSRestoreAppearance))
            {
                menu.AddMenuItem(restorePlayerAppearance);
            }
            if (IsAllowed(Permission.MSRestoreWeapons))
            {
                menu.AddMenuItem(restorePlayerWeapons);
            }

            // Always allowed
            menu.AddMenuItem(hideRadar);
            menu.AddMenuItem(hideHud);
            menu.AddMenuItem(lockCamX);
            menu.AddMenuItem(lockCamY);
            if (MainMenu.EnableExperimentalFeatures)
            {
                menu.AddMenuItem(exportData);
            }
            menu.AddMenuItem(saveSettings);

            // Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == rightAlignMenu)
                {

                    MenuController.MenuAlignment = _checked ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
                    MiscRightAlignMenu = _checked;
                    UserDefaults.MiscRightAlignMenu = MiscRightAlignMenu;

                    if (MenuController.MenuAlignment != (_checked ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left))
                    {
                        Notify.Error(CommonErrors.RightAlignedNotSupported);
                        // (re)set the default to left just in case so they don't get this error again in the future.
                        MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                        MiscRightAlignMenu = false;
                        UserDefaults.MiscRightAlignMenu = false;
                    }

                }
                else if (item == disablePms)
                {
                    MiscDisablePrivateMessages = _checked;
                }
                else if (item == disableControllerKey)
                {
                    MiscDisableControllerSupport = _checked;
                    MenuController.EnableMenuToggleKeyOnController = !_checked;
                }
                else if (item == speedKmh)
                {
                    ShowSpeedoKmh = _checked;
                }
                else if (item == speedMph)
                {
                    ShowSpeedoMph = _checked;
                }
                else if (item == hideHud)
                {
                    HideHud = _checked;
                    DisplayHud(!_checked);
                }
                else if (item == hideRadar)
                {
                    HideRadar = _checked;
                    if (!_checked)
                    {
                        DisplayRadar(true);
                    }
                }
                else if (item == showLocation)
                {
                    ShowLocation = _checked;
                }
                else if (item == drawTime)
                {
                    DrawTimeOnScreen = _checked;
                }
                else if (item == deathNotifs)
                {
                    DeathNotifications = _checked;
                }
                else if (item == joinQuitNotifs)
                {
                    JoinQuitNotifications = _checked;
                }
                else if (item == nightVision)
                {
                    SetNightvision(_checked);
                }
                else if (item == thermalVision)
                {
                    SetSeethrough(_checked);
                }
                else if (item == lockCamX)
                {
                    LockCameraX = _checked;
                }
                else if (item == lockCamY)
                {
                    LockCameraY = _checked;
                }
                else if (item == locationBlips)
                {
                    ToggleBlips(_checked);
                    ShowLocationBlips = _checked;
                }
                else if (item == playerBlips)
                {
                    ShowPlayerBlips = _checked;
                }
                else if (item == playerNames)
                {
                    MiscShowOverheadNames = _checked;
                }
                else if (item == respawnDefaultCharacter)
                {
                    MiscRespawnDefaultCharacter = _checked;
                }
                else if (item == restorePlayerAppearance)
                {
                    RestorePlayerAppearance = _checked;
                }
                else if (item == restorePlayerWeapons)
                {
                    RestorePlayerWeapons = _checked;
                }

            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // export data
                if (item == exportData)
                {
                    MenuController.CloseAllMenus();
                    var vehicles = GetSavedVehicles();
                    var normalPeds = StorageManager.GetSavedPeds();
                    var mpPeds = StorageManager.GetSavedMpPeds();
                    var weaponLoadouts = WeaponLoadouts.GetSavedWeapons();
                    var data = JsonConvert.SerializeObject(new
                    {
                        saved_vehicles = vehicles,
                        normal_peds = normalPeds,
                        mp_characters = mpPeds,
                        weapon_loadouts = weaponLoadouts
                    });
                    SendNuiMessage(data);
                    SetNuiFocus(true, true);
                }
                // save settings
                else if (item == saveSettings)
                {
                    UserDefaults.SaveSettings();
                }
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

        private readonly struct Blip
        {
            public readonly Vector3 Location;
            public readonly int Sprite;
            public readonly string Name;
            public readonly int Color;
            public readonly int blipID;

            public Blip(Vector3 Location, int Sprite, string Name, int Color, int blipID)
            {
                this.Location = Location;
                this.Sprite = Sprite;
                this.Name = Name;
                this.Color = Color;
                this.blipID = blipID;
            }
        }

        private readonly List<Blip> blips = new();

        /// <summary>
        /// Toggles blips on/off.
        /// </summary>
        /// <param name="enable"></param>
        private void ToggleBlips(bool enable)
        {
            if (enable)
            {
                try
                {
                    foreach (var bl in vMenuShared.ConfigManager.GetLocationBlipsData())
                    {
                        var blipID = AddBlipForCoord(bl.coordinates.X, bl.coordinates.Y, bl.coordinates.Z);
                        SetBlipSprite(blipID, bl.spriteID);
                        BeginTextCommandSetBlipName("STRING");
                        AddTextComponentSubstringPlayerName(bl.name);
                        EndTextCommandSetBlipName(blipID);
                        SetBlipColour(blipID, bl.color);
                        SetBlipAsShortRange(blipID, true);

                        var b = new Blip(bl.coordinates, bl.spriteID, bl.name, bl.color, blipID);
                        blips.Add(b);
                    }
                }
                catch (JsonReaderException ex)
                {
                    Debug.Write($"\n\n[vMenu] 加载 locations.json 文件时发生错误。请联系服务器所有者以解决此问题。\n联系所有者时，请提供以下错误详情：\n{ex.Message}。\n\n\n");
                }
            }
            else
            {
                if (blips.Count > 0)
                {
                    foreach (var blip in blips)
                    {
                        var id = blip.blipID;
                        if (DoesBlipExist(id))
                        {
                            RemoveBlip(ref id);
                        }
                    }
                }
                blips.Clear();
            }
        }

    }
}
