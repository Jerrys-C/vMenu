using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class PlayerOptions
    {
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        // Public variables (getters only), return the private variables.
        public bool PlayerGodMode { get; private set; } = UserDefaults.PlayerGodMode;
        public bool PlayerInvisible { get; private set; } = false;
        public bool PlayerStamina { get; private set; } = UserDefaults.UnlimitedStamina;
        public bool PlayerFastRun { get; private set; } = UserDefaults.FastRun;
        public bool PlayerFastSwim { get; private set; } = UserDefaults.FastSwim;
        public bool PlayerSuperJump { get; private set; } = UserDefaults.SuperJump;
        public bool PlayerNoRagdoll { get; private set; } = UserDefaults.NoRagdoll;
        public bool PlayerNeverWanted { get; private set; } = UserDefaults.NeverWanted;
        public bool PlayerIsIgnored { get; private set; } = UserDefaults.EveryoneIgnorePlayer;
        public bool PlayerStayInVehicle { get; private set; } = UserDefaults.PlayerStayInVehicle;
        public bool PlayerFrozen { get; private set; } = false;
        private readonly Menu CustomDrivingStyleMenu = new("Driving Style", "Custom Driving Style");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create menu and menu items
            // Create the menu.
            menu = new Menu(Game.Player.Name, "玩家选项");

            // 创建所有复选框。
            var playerGodModeCheckbox = new MenuCheckboxItem("上帝模式", "让你变得无敌。", PlayerGodMode);
            var invisibleCheckbox = new MenuCheckboxItem("隐身", "使你对自己和其他人都不可见。", PlayerInvisible);
            var unlimitedStaminaCheckbox = new MenuCheckboxItem("无限耐力", "允许你永远跑而不会减速或受伤。", PlayerStamina);
            var fastRunCheckbox = new MenuCheckboxItem("快速奔跑", "获得 ~g~豹子~s~ 能力，跑得非常快！", PlayerFastRun);
            SetRunSprintMultiplierForPlayer(Game.Player.Handle, PlayerFastRun && IsAllowed(Permission.POFastRun) ? 1.49f : 1f);
            var fastSwimCheckbox = new MenuCheckboxItem("快速游泳", "获得 ~g~鲨鱼 2.0~s~ 能力，游得超级快！", PlayerFastSwim);
            SetSwimMultiplierForPlayer(Game.Player.Handle, PlayerFastSwim && IsAllowed(Permission.POFastSwim) ? 1.49f : 1f);
            var superJumpCheckbox = new MenuCheckboxItem("超级跳跃", "获得 ~g~兔子 3.0~s~ 能力，跳得像个冠军！", PlayerSuperJump);
            var noRagdollCheckbox = new MenuCheckboxItem("无跌落", "禁用玩家跌落效果，使你不再从自行车上摔下来。", PlayerNoRagdoll);
            var neverWantedCheckbox = new MenuCheckboxItem("从未被通缉", "禁用所有通缉等级。", PlayerNeverWanted);
            var everyoneIgnoresPlayerCheckbox = new MenuCheckboxItem("所有人忽视玩家", "所有人都会离开你。", PlayerIsIgnored);
            var playerStayInVehicleCheckbox = new MenuCheckboxItem("留在车辆中", "启用此选项时，如果 NPC 生气，他们将无法将你拖出车辆。", PlayerStayInVehicle);
            var playerFrozenCheckbox = new MenuCheckboxItem("冻结玩家", "冻结你当前的位置。", PlayerFrozen);


            // Wanted level options
           var wantedLevelList = new List<string> { "无通缉等级", "1", "2", "3", "4", "5" };
            var setWantedLevel = new MenuListItem("设置通缉等级", wantedLevelList, GetPlayerWantedLevel(Game.Player.Handle), "通过选择一个值并按下回车键来设置你的通缉等级。");
            var setArmorItem = new MenuListItem("设置护甲类型", new List<string> { "无护甲", GetLabelText("WT_BA_0"), GetLabelText("WT_BA_1"), GetLabelText("WT_BA_2"), GetLabelText("WT_BA_3"), GetLabelText("WT_BA_4"), }, 0, "设置你玩家的护甲等级/类型。");

            var healPlayerBtn = new MenuItem("治疗玩家", "给予玩家最大生命值。");
            var cleanPlayerBtn = new MenuItem("清洁玩家衣物", "清洁你的玩家衣物。");
            var dryPlayerBtn = new MenuItem("晾干玩家衣物", "让你的玩家衣物干燥。");
            var wetPlayerBtn = new MenuItem("弄湿玩家衣物", "让你的玩家衣物变湿。");
            var suicidePlayerBtn = new MenuItem("~r~自杀", "通过服药或使用手枪（如果你有）来杀死自己。");

            var vehicleAutoPilot = new Menu("自动驾驶", "车辆自动驾驶选项。");


            MenuController.AddSubmenu(menu, vehicleAutoPilot);

            var vehicleAutoPilotBtn = new MenuItem("车辆自动驾驶菜单", "管理车辆自动驾驶选项。")
            {
                Label = "→→→"
            };

           var drivingStyles = new List<string>() { "正常", "急速", "避免高速公路", "倒车", "自定义" };
            var drivingStyle = new MenuListItem("驾驶风格", drivingStyles, 0, "设置用于前往标记点和随机驾驶功能的驾驶风格。");

            // 场景（列表可以在 PedScenarios 类中找到）
            var playerScenarios = new MenuListItem("玩家场景", PedScenarios.Scenarios, 0, "选择一个场景并按回车键开始。如果你已经在执行选定的场景，再次选择它将停止当前场景。");
            var stopScenario = new MenuItem("强制停止场景", "这将强制立即停止正在执行的场景，而无需等待完成'停止'动画。");
            #endregion

            #region add items to menu based on permissions
            // Add all checkboxes to the menu. (keeping permissions in mind)
            if (IsAllowed(Permission.POGod))
            {
                menu.AddMenuItem(playerGodModeCheckbox);
            }
            if (IsAllowed(Permission.POInvisible))
            {
                menu.AddMenuItem(invisibleCheckbox);
            }
            if (IsAllowed(Permission.POUnlimitedStamina))
            {
                menu.AddMenuItem(unlimitedStaminaCheckbox);
            }
            if (IsAllowed(Permission.POFastRun))
            {
                menu.AddMenuItem(fastRunCheckbox);
            }
            if (IsAllowed(Permission.POFastSwim))
            {
                menu.AddMenuItem(fastSwimCheckbox);
            }
            if (IsAllowed(Permission.POSuperjump))
            {
                menu.AddMenuItem(superJumpCheckbox);
            }
            if (IsAllowed(Permission.PONoRagdoll))
            {
                menu.AddMenuItem(noRagdollCheckbox);
            }
            if (IsAllowed(Permission.PONeverWanted))
            {
                menu.AddMenuItem(neverWantedCheckbox);
            }
            if (IsAllowed(Permission.POSetWanted))
            {
                menu.AddMenuItem(setWantedLevel);
            }
            if (IsAllowed(Permission.POIgnored))
            {
                menu.AddMenuItem(everyoneIgnoresPlayerCheckbox);
            }
            if (IsAllowed(Permission.POStayInVehicle))
            {
                menu.AddMenuItem(playerStayInVehicleCheckbox);
            }
            if (IsAllowed(Permission.POMaxHealth))
            {
                menu.AddMenuItem(healPlayerBtn);
            }
            if (IsAllowed(Permission.POMaxArmor))
            {
                menu.AddMenuItem(setArmorItem);
            }
            if (IsAllowed(Permission.POCleanPlayer))
            {
                menu.AddMenuItem(cleanPlayerBtn);
            }
            if (IsAllowed(Permission.PODryPlayer))
            {
                menu.AddMenuItem(dryPlayerBtn);
            }
            if (IsAllowed(Permission.POWetPlayer))
            {
                menu.AddMenuItem(wetPlayerBtn);
            }

            menu.AddMenuItem(suicidePlayerBtn);

            if (IsAllowed(Permission.POVehicleAutoPilotMenu))
            {
                menu.AddMenuItem(vehicleAutoPilotBtn);
                MenuController.BindMenuItem(menu, vehicleAutoPilot, vehicleAutoPilotBtn);

                vehicleAutoPilot.AddMenuItem(drivingStyle);

                var startDrivingWaypoint = new MenuItem("前往标记点", "让你的角色驾驶车辆前往标记点。");
                var startDrivingRandomly = new MenuItem("随机驾驶", "让你的角色随机驾驶车辆在地图上移动。");
                var stopDriving = new MenuItem("停止驾驶", "角色将寻找一个合适的位置停车。当车辆到达合适的停车位置后，任务将停止。");
                var forceStopDriving = new MenuItem("强制停止驾驶", "立即停止驾驶任务，无需寻找停车位置。");
                var customDrivingStyle = new MenuItem("自定义驾驶风格", "选择自定义驾驶风格。确保在驾驶风格列表中选择 '自定义' 驾驶风格来启用它。") { Label = "→→→" };
                MenuController.AddSubmenu(vehicleAutoPilot, CustomDrivingStyleMenu);
                vehicleAutoPilot.AddMenuItem(customDrivingStyle);
                MenuController.BindMenuItem(vehicleAutoPilot, CustomDrivingStyleMenu, customDrivingStyle);
                var knownNames = new Dictionary<int, string>()
                {
                    { 0, "在车辆前停车" },
                    { 1, "在行人前停车" },
                    { 2, "避开车辆" },
                    { 3, "避开空车" },
                    { 4, "避开行人" },
                    { 5, "避开物体" },

                    { 7, "在红绿灯前停车" },
                    { 8, "使用转向灯" },
                    { 9, "允许逆行" },
                    { 10, "倒车" },

                    { 18, "使用最短路径" },

                    { 22, "忽略道路" },

                    { 24, "忽略所有路径规划" },

                    { 29, "尽可能避免高速公路" },
                };

                for (var i = 0; i < 31; i++)
                {
                    var name = "~r~未知标志";
                    if (knownNames.ContainsKey(i))
                    {
                        name = knownNames[i];
                    }
                    var checkbox = new MenuCheckboxItem(name, "切换此驾驶风格标志。", false);
                    CustomDrivingStyleMenu.AddMenuItem(checkbox);
                }
                CustomDrivingStyleMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                    CustomDrivingStyleMenu.MenuSubtitle = $"自定义风格: {style}";
                    if (drivingStyle.ListIndex == 4)
                    {
                        Notify.Custom("驾驶风格已更新。");
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                    }
                    else
                    {
                        Notify.Custom("驾驶风格未更新，因为您在之前的菜单中没有启用自定义驾驶风格。");
                    }
                };


                vehicleAutoPilot.AddMenuItem(startDrivingWaypoint);
                vehicleAutoPilot.AddMenuItem(startDrivingRandomly);
                vehicleAutoPilot.AddMenuItem(stopDriving);
                vehicleAutoPilot.AddMenuItem(forceStopDriving);

                vehicleAutoPilot.RefreshIndex();

                vehicleAutoPilot.OnItemSelect += async (sender, item, index) =>
                {
                    if (Game.PlayerPed.IsInVehicle() && item != stopDriving && item != forceStopDriving)
                    {
                        if (Game.PlayerPed.CurrentVehicle != null && Game.PlayerPed.CurrentVehicle.Exists() && !Game.PlayerPed.CurrentVehicle.IsDead && Game.PlayerPed.CurrentVehicle.IsDriveable)
                        {
                            if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                            {
                                if (item == startDrivingWaypoint)
                                {
                                    if (IsWaypointActive())
                                    {
                                        var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                        DriveToWp(style);
                                        Notify.Info("您的角色现在正在为您驾驶车辆。您可以随时通过按下“停止驾驶”按钮来取消。车辆将在到达目的地时停下。");
                                    }
                                    else
                                    {
                                        Notify.Error("您需要一个航点才能前往它！");
                                    }
                                }
                                else if (item == startDrivingRandomly)
                                {
                                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                    DriveWander(style);
                                    Notify.Info("AI现在正在为您驾驶车辆。您可以随时按停止驾驶按钮取消。");
                                }
                            }
                            else
                            {
                                Notify.Error("你必须是这辆车的司机！");
                            }
                        }
                        else
                        {
                            Notify.Error("您的车辆损坏或不存在！");
                        }
                    }
                    else if (item != stopDriving && item != forceStopDriving)
                    {
                        Notify.Error("你需要先上车！");
                    }
                    if (item == stopDriving)
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            var veh = GetVehicle();
                            if (veh != null && veh.Exists() && !veh.IsDead)
                            {
                                var outPos = new Vector3();
                                if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
                                {
                                    Notify.Info("AI将找到一个合适的地方停车，然后停止驾驶。请稍候。");
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    TaskVehiclePark(Game.PlayerPed.Handle, veh.Handle, outPos.X, outPos.Y, outPos.Z, Game.PlayerPed.Heading, 3, 60f, true);
                                    while (Game.PlayerPed.Position.DistanceToSquared2D(outPos) > 3f)
                                    {
                                        await BaseScript.Delay(0);
                                    }
                                    SetVehicleHalt(veh.Handle, 3f, 0, false);
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    Notify.Info("AI已经停下了。");
                                }
                            }
                        }
                        else
                        {
                            ClearPedTasks(Game.PlayerPed.Handle);
                            Notify.Alert("你不在驾驶车辆！");
                        }
                    }
                    else if (item == forceStopDriving)
                    {
                        ClearPedTasks(Game.PlayerPed.Handle);
                        Notify.Info("驾驶任务已取消。");
                    }
                };

                vehicleAutoPilot.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
                {
                    if (item == drivingStyle)
                    {
                        var style = GetStyleFromIndex(listIndex);
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                        Notify.Info($"驾驶方式现在设置为： ~r~{drivingStyles[listIndex]}~s~.");
                    }
                };
            }

            if (IsAllowed(Permission.POFreeze))
            {
                menu.AddMenuItem(playerFrozenCheckbox);
            }
            if (IsAllowed(Permission.POScenarios))
            {
                menu.AddMenuItem(playerScenarios);
                menu.AddMenuItem(stopScenario);
            }
            #endregion

            #region handle all events
            // Checkbox changes.
            menu.OnCheckboxChange += (sender, item, itemIndex, _checked) =>
            {
                // God Mode toggled.
                if (item == playerGodModeCheckbox)
                {
                    PlayerGodMode = _checked;
                }
                // Invisibility toggled.
                else if (item == invisibleCheckbox)
                {
                    PlayerInvisible = _checked;
                    SetEntityVisible(Game.PlayerPed.Handle, !PlayerInvisible, false);
                }
                // Unlimited Stamina toggled.
                else if (item == unlimitedStaminaCheckbox)
                {
                    PlayerStamina = _checked;
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), _checked ? 100 : 0, true);
                }
                // Fast run toggled.
                else if (item == fastRunCheckbox)
                {
                    PlayerFastRun = _checked;
                    SetRunSprintMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Fast swim toggled.
                else if (item == fastSwimCheckbox)
                {
                    PlayerFastSwim = _checked;
                    SetSwimMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Super jump toggled.
                else if (item == superJumpCheckbox)
                {
                    PlayerSuperJump = _checked;
                }
                // No ragdoll toggled.
                else if (item == noRagdollCheckbox)
                {
                    PlayerNoRagdoll = _checked;
                }
                // Never wanted toggled.
                else if (item == neverWantedCheckbox)
                {
                    PlayerNeverWanted = _checked;
                    if (!_checked)
                    {
                        SetMaxWantedLevel(5);
                    }
                    else
                    {
                        SetMaxWantedLevel(0);
                    }
                }
                // Everyone ignores player toggled.
                else if (item == everyoneIgnoresPlayerCheckbox)
                {
                    PlayerIsIgnored = _checked;

                    // Manage player is ignored by everyone.
                    SetEveryoneIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPoliceIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPlayerCanBeHassledByGangs(Game.Player.Handle, !PlayerIsIgnored);
                }
                else if (item == playerStayInVehicleCheckbox)
                {
                    PlayerStayInVehicle = _checked;
                }
                // Freeze player toggled.
                else if (item == playerFrozenCheckbox)
                {
                    PlayerFrozen = _checked;

                    if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                    else if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                }
            };

            // List selections
            menu.OnListItemSelect += (sender, listItem, listIndex, itemIndex) =>
            {
                // Set wanted Level
                if (listItem == setWantedLevel)
                {
                    SetPlayerWantedLevel(Game.Player.Handle, listIndex, false);
                    SetPlayerWantedLevelNow(Game.Player.Handle, false);
                }
                // Player Scenarios 
                else if (listItem == playerScenarios)
                {
                    PlayScenario(PedScenarios.ScenarioNames[PedScenarios.Scenarios[listIndex]]);
                }
                else if (listItem == setArmorItem)
                {
                    Game.PlayerPed.Armor = listItem.ListIndex * 20;
                }
            };

            // button presses
            menu.OnItemSelect += (sender, item, index) =>
            {
                // 强制停止场景按钮
                if (item == stopScenario)
                {
                    // 播放一个名为 "forcestop" 的新场景（这个场景不存在，但 "Play" 函数会检查
                    // 字符串 "forcestop"，如果提供了这个场景名称，则会强制清除玩家任务。
                    PlayScenario("forcestop");
                }
                else if (item == healPlayerBtn)
                {
                    Game.PlayerPed.Health = Game.PlayerPed.MaxHealth;
                    Notify.Success("玩家已被治愈。");
                }
                else if (item == cleanPlayerBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Notify.Success("玩家衣物已被清洗。");
                }
                else if (item == dryPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 0f;
                    Notify.Success("玩家现在是干的。");
                }
                else if (item == wetPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 2f;
                    Notify.Success("玩家现在是湿的。");
                }
                else if (item == suicidePlayerBtn)
                {
                    CommitSuicide();
                }
            };

            #endregion

        }

        private int GetCustomDrivingStyle()
        {
            var items = CustomDrivingStyleMenu.GetMenuItems();
            var flags = new int[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is MenuCheckboxItem checkbox)
                {
                    flags[i] = checkbox.Checked ? 1 : 0;
                }
            }
            var binaryString = "";
            var reverseFlags = flags.Reverse();
            foreach (var i in reverseFlags)
            {
                binaryString += i;
            }
            var binaryNumber = Convert.ToUInt32(binaryString, 2);
            return (int)binaryNumber;
        }

        private int GetStyleFromIndex(int index)
        {
            var style = index switch
            {
                0 => 443,// normal
                1 => 575,// rushed
                2 => 536871355,// Avoid highways
                3 => 1467,// Go in reverse
                4 => GetCustomDrivingStyle(),// custom driving style;
                _ => 0,// no style (impossible, but oh well)
            };
            return style;
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Player Options Menu</returns>
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
