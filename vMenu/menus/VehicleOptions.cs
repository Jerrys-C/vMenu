using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class VehicleOptions
    {
        #region Variables
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        // Submenus
        public Menu VehicleModMenu { get; private set; }
        public Menu VehicleDoorsMenu { get; private set; }
        public Menu VehicleWindowsMenu { get; private set; }
        public Menu VehicleComponentsMenu { get; private set; }
        public Menu VehicleLiveriesMenu { get; private set; }
        public Menu VehicleColorsMenu { get; private set; }
        public Menu DeleteConfirmMenu { get; private set; }
        public Menu VehicleUnderglowMenu { get; private set; }

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = UserDefaults.VehicleGodMode;
        public bool VehicleGodInvincible { get; private set; } = UserDefaults.VehicleGodInvincible;
        public bool VehicleGodEngine { get; private set; } = UserDefaults.VehicleGodEngine;
        public bool VehicleGodVisual { get; private set; } = UserDefaults.VehicleGodVisual;
        public bool VehicleGodStrongWheels { get; private set; } = UserDefaults.VehicleGodStrongWheels;
        public bool VehicleGodRamp { get; private set; } = UserDefaults.VehicleGodRamp;
        public bool VehicleGodAutoRepair { get; private set; } = UserDefaults.VehicleGodAutoRepair;

        public bool VehicleNeverDirty { get; private set; } = UserDefaults.VehicleNeverDirty;
        public bool VehicleEngineAlwaysOn { get; private set; } = UserDefaults.VehicleEngineAlwaysOn;
        public bool VehicleNoSiren { get; private set; } = UserDefaults.VehicleNoSiren;
        public bool VehicleNoBikeHelemet { get; private set; } = UserDefaults.VehicleNoBikeHelmet;
        public bool FlashHighbeamsOnHonk { get; private set; } = UserDefaults.VehicleHighbeamsOnHonk;
        public bool DisablePlaneTurbulence { get; private set; } = UserDefaults.VehicleDisablePlaneTurbulence;
        public bool DisableHelicopterTurbulence { get; private set; } = UserDefaults.VehicleDisableHelicopterTurbulence;
        public bool VehicleBikeSeatbelt { get; private set; } = UserDefaults.VehicleBikeSeatbelt;
        public bool VehicleInfiniteFuel { get; private set; } = false;
        public bool VehicleShowHealth { get; private set; } = false;
        public bool VehicleFrozen { get; private set; } = false;
        public bool VehicleTorqueMultiplier { get; private set; } = false;
        public bool VehiclePowerMultiplier { get; private set; } = false;
        public float VehicleTorqueMultiplierAmount { get; private set; } = 2f;
        public float VehiclePowerMultiplierAmount { get; private set; } = 2f;

        private readonly Dictionary<MenuItem, int> vehicleExtras = new();
        #endregion

        #region CreateMenu()
        /// <summary>
        /// Create menu creates the vehicle options menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "车辆选项");

            #region 菜单项变量
            // 车辆无敌模式菜单
            var vehGodMenu = new Menu("车辆无敌模式", "车辆无敌模式选项");
            var vehGodMenuBtn = new MenuItem("无敌模式选项", "启用或禁用特定的伤害类型。") { Label = "→→→" };
            MenuController.AddSubmenu(menu, vehGodMenu);

            // 创建复选框。
            var vehicleGod = new MenuCheckboxItem("车辆无敌模式", "使您的车辆不受任何伤害。注意，您需要进入下面的无敌模式菜单以选择要禁用的伤害类型。", VehicleGodMode);
            var vehicleNeverDirty = new MenuCheckboxItem("保持车辆清洁", "如果车辆的污垢水平超过 0，这将不断清洁您的车辆。请注意，这只会清洁 ~o~灰尘~s~ 或 ~o~污垢~s~。这不会清洁泥土、雪或其他 ~r~损坏标记~s~。请修理您的车辆以去除它们。", VehicleNeverDirty);
            var vehicleBikeSeatbelt = new MenuCheckboxItem("自行车安全带", "防止您从自行车、摩托车、四轮摩托车或类似车辆上被撞下。", VehicleBikeSeatbelt);
            var vehicleEngineAO = new MenuCheckboxItem("引擎常开", "当您离开车辆时，保持车辆引擎开启。", VehicleEngineAlwaysOn);
            var vehicleNoTurbulence = new MenuCheckboxItem("禁用飞机颠簸", "禁用所有飞机的颠簸。", DisablePlaneTurbulence);
            var vehicleNoTurbulenceHeli = new MenuCheckboxItem("禁用直升机颠簸", "禁用所有直升机的颠簸。", DisableHelicopterTurbulence);
            var vehicleNoSiren = new MenuCheckboxItem("禁用警报", "禁用您车辆的警报。仅适用于实际有警报的车辆。", VehicleNoSiren);
            var vehicleNoBikeHelmet = new MenuCheckboxItem("无摩托车头盔", "骑上自行车或四轮摩托车时不再自动装备头盔。", VehicleNoBikeHelemet);
            var vehicleFreeze = new MenuCheckboxItem("冻结车辆", "冻结您车辆的位置。", VehicleFrozen);
            var torqueEnabled = new MenuCheckboxItem("启用扭矩倍增器", "启用从下面列表中选择的扭矩倍增器。", VehicleTorqueMultiplier);
            var powerEnabled = new MenuCheckboxItem("启用功率倍增器", "启用从下面列表中选择的功率倍增器。", VehiclePowerMultiplier);
            var highbeamsOnHonk = new MenuCheckboxItem("按喇叭时闪烁远光灯", "按喇叭时开启您的车辆远光灯。白天灯光关闭时无效。", FlashHighbeamsOnHonk);
            var showHealth = new MenuCheckboxItem("显示车辆健康", "在屏幕上显示车辆健康。", VehicleShowHealth);
            var infiniteFuel = new MenuCheckboxItem("无限燃料", "启用或禁用车辆的无限燃料，仅在安装了 FRFuel 时有效。", VehicleInfiniteFuel);

            // 创建按钮。
            var fixVehicle = new MenuItem("修复车辆", "修复您车辆上的任何视觉和物理损坏。");
            var cleanVehicle = new MenuItem("清洗车辆", "清洁您的车辆。");
            var toggleEngine = new MenuItem("切换引擎开/关", "打开/关闭您的引擎。");
            var setLicensePlateText = new MenuItem("设置车牌文字", "为您的车辆输入自定义车牌。");
            var modMenuBtn = new MenuItem("改装菜单", "在这里调整和自定义您的车辆。")
            {
                Label = "→→→"
            };
            var doorsMenuBtn = new MenuItem("车辆门", "在这里打开、关闭、拆除和恢复车辆门。")
            {
                Label = "→→→"
            };
            var windowsMenuBtn = new MenuItem("车辆窗户", "在这里升降窗户或拆除/恢复车辆窗户。")
            {
                Label = "→→→"
            };
            var componentsMenuBtn = new MenuItem("车辆配件", "添加/移除车辆配件/额外配置。")
            {
                Label = "→→→"
            };
            var liveriesMenuBtn = new MenuItem("车辆涂装", "用精美的涂装装饰您的车辆！")
            {
                Label = "→→→"
            };
            var colorsMenuBtn = new MenuItem("车辆颜色", "通过给车辆上点 ~g~炫酷~s~的颜色来进一步装饰！")
            {
                Label = "→→→"
            };
            var underglowMenuBtn = new MenuItem("车辆霓虹灯", "让您的车辆闪耀着一些炫酷的霓虹灯光！")
            {
                Label = "→→→"
            };

            var vehicleInvisible = new MenuItem("切换车辆可见性", "使您的车辆可见/不可见。~r~您的车辆将在您离开车辆时再次可见。否则您将无法重新进入。");
            var flipVehicle = new MenuItem("翻转车辆", "将您当前的车辆设置为四个轮子着地。");
            var vehicleAlarm = new MenuItem("切换车辆警报", "启动/停止您的车辆警报。");
            var cycleSeats = new MenuItem("切换车辆座位", "切换可用的车辆座位。");
            var lights = new List<string>()
            {
                "危险警告灯",
                "左侧转向灯",
                "右侧转向灯",
                "内饰灯",
                //"出租车灯", // 这个似乎不管怎样都不起作用。
                "直升机探照灯",
            };
            var vehicleLights = new MenuListItem("车辆灯光", lights, 0, "打开/关闭车辆灯光。");

            var stationNames = new List<string>();

            foreach (var radioStationName in Enum.GetNames(typeof(RadioStation)))
            {
                stationNames.Add(radioStationName);
            }

            var radioIndex = UserDefaults.VehicleDefaultRadio;

            if (radioIndex == (int)RadioStation.RadioOff)
            {
                var stations = (RadioStation[])Enum.GetValues(typeof(RadioStation));
                var index = Array.IndexOf(stations, RadioStation.RadioOff);
                radioIndex = index;
            }

            var radioStations = new MenuListItem("默认广播电台", stationNames, radioIndex, "选择一个默认广播电台，作为新车生成时的广播电台。");

            var tiresList = new List<string>() { "所有轮胎", "轮胎 #1", "轮胎 #2", "轮胎 #3", "轮胎 #4", "轮胎 #5", "轮胎 #6", "轮胎 #7", "轮胎 #8" };
            var vehicleTiresList = new MenuListItem("修复 / 摧毁轮胎", tiresList, 0, "修复或摧毁特定车辆轮胎，或一次性修复所有轮胎。注意，并非所有索引对于所有车辆都是有效的，某些车辆可能不会有任何效果。");

            var destroyEngine = new MenuItem("摧毁引擎", "摧毁您的车辆引擎。");

            var deleteBtn = new MenuItem("~r~删除车辆", "删除您的车辆，这~r~不能被撤销~s~！")
            {
                LeftIcon = MenuItem.Icon.WARNING,
                Label = "→→→"
            };
            var deleteNoBtn = new MenuItem("NO，取消", "NO，不要删除我的车辆，返回！");
            var deleteYesBtn = new MenuItem("~r~YES，删除", "是的，我确定，请删除我的车辆，我理解这不能被撤销。")
            {
                LeftIcon = MenuItem.Icon.WARNING
            };

            // Create lists.
            var dirtlevel = new List<string> { "无污垢", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            var setDirtLevel = new MenuListItem("设置污垢等级", dirtlevel, 0, "选择要在车辆上显示的污垢等级，按~r~回车~s~应用所选等级。");
            var licensePlates = new List<string> { GetLabelText("CMOD_PLA_0"), GetLabelText("CMOD_PLA_1"), GetLabelText("CMOD_PLA_2"), GetLabelText("CMOD_PLA_3"),
                GetLabelText("CMOD_PLA_4"), "北扬克顿" };
            var setLicensePlateType = new MenuListItem("车牌类型", licensePlates, 0, "选择一个车牌类型并按~r~回车 ~s~将其应用到您的车辆。");
            var torqueMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            var torqueMultiplier = new MenuListItem("设置引擎扭矩倍增器", torqueMultiplierList, 0, "设置引擎扭矩倍增器。");
            var powerMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            var powerMultiplier = new MenuListItem("设置引擎功率倍增器", powerMultiplierList, 0, "设置引擎功率倍增器。");
            var speedLimiterOptions = new List<string>() { "设置", "重置", "自定义速度限制" };
            var speedLimiter = new MenuListItem("速度限制器", speedLimiterOptions, 0, "将您的车辆的最大速度设置为当前速度。重置车辆的最大速度将把当前车辆的最大速度恢复为默认值。此选项仅影响当前车辆。");
            #endregion

            #region Submenus
            // Submenu's
            VehicleModMenu = new Menu("改装菜单", "车辆改装");
            VehicleModMenu.InstructionalButtons.Add(Control.Jump, "切换车辆门");
            VehicleModMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_PRESSED, new Action<Menu, Control>((m, c) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    var open = GetVehicleDoorAngleRatio(veh.Handle, 0) < 0.1f;
                    if (open)
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            SetVehicleDoorOpen(veh.Handle, i, false, false);
                        }
                    }
                    else
                    {
                        SetVehicleDoorsShut(veh.Handle, false);
                    }
                }
            }), false));
             VehicleDoorsMenu = new Menu("车门", "车门管理");
            VehicleWindowsMenu = new Menu("车窗", "车窗管理");
            VehicleComponentsMenu = new Menu("车辆附加组件", "车辆附加组件/部件");
            VehicleLiveriesMenu = new Menu("车辆涂装", "车辆涂装");
            VehicleColorsMenu = new Menu("车辆颜色", "车辆颜色");
            DeleteConfirmMenu = new Menu("确认操作", "删除车辆，您确定吗？");
            VehicleUnderglowMenu = new Menu("车辆霓虹灯", "车辆霓虹灯选项");

            MenuController.AddSubmenu(menu, VehicleModMenu);
            MenuController.AddSubmenu(menu, VehicleDoorsMenu);
            MenuController.AddSubmenu(menu, VehicleWindowsMenu);
            MenuController.AddSubmenu(menu, VehicleComponentsMenu);
            MenuController.AddSubmenu(menu, VehicleLiveriesMenu);
            MenuController.AddSubmenu(menu, VehicleColorsMenu);
            MenuController.AddSubmenu(menu, DeleteConfirmMenu);
            MenuController.AddSubmenu(menu, VehicleUnderglowMenu);
            #endregion

            #region Add items to the menu.
            // Add everything to the menu. (based on permissions)
            if (IsAllowed(Permission.VOGod)) // GOD MODE
            {
                menu.AddMenuItem(vehicleGod);
                menu.AddMenuItem(vehGodMenuBtn);
                MenuController.BindMenuItem(menu, vehGodMenu, vehGodMenuBtn);

                var godInvincible = new MenuCheckboxItem("无敌", "使车辆无敌。包括火灾伤害、爆炸伤害、碰撞伤害等。", VehicleGodInvincible);
                var godEngine = new MenuCheckboxItem("引擎伤害", "禁用引擎受伤害。", VehicleGodEngine);
                var godVisual = new MenuCheckboxItem("视觉伤害", "防止车辆出现划痕和其他损伤贴花。这不会防止（车身）变形伤害。", VehicleGodVisual);
                var godStrongWheels = new MenuCheckboxItem("强力轮胎", "防止轮胎变形并导致操控性下降。这不会使轮胎防弹。", VehicleGodStrongWheels);
                var godRamp = new MenuCheckboxItem("斜坡伤害", "禁用如 Ramp Buggy 这样的车辆在使用斜坡时受到伤害。", VehicleGodRamp);
                var godAutoRepair = new MenuCheckboxItem("~r~自动修复", "当车辆有任何类型的损伤时自动修复。建议保持关闭以防止出现故障。", VehicleGodAutoRepair);

                vehGodMenu.AddMenuItem(godInvincible);
                vehGodMenu.AddMenuItem(godEngine);
                vehGodMenu.AddMenuItem(godVisual);
                vehGodMenu.AddMenuItem(godStrongWheels);
                vehGodMenu.AddMenuItem(godRamp);
                vehGodMenu.AddMenuItem(godAutoRepair);

                vehGodMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    if (item == godInvincible)
                    {
                        VehicleGodInvincible = _checked;
                    }
                    else if (item == godEngine)
                    {
                        VehicleGodEngine = _checked;
                    }
                    else if (item == godVisual)
                    {
                        VehicleGodVisual = _checked;
                    }
                    else if (item == godStrongWheels)
                    {
                        VehicleGodStrongWheels = _checked;
                    }
                    else if (item == godRamp)
                    {
                        VehicleGodRamp = _checked;
                    }
                    else if (item == godAutoRepair)
                    {
                        VehicleGodAutoRepair = _checked;
                    }
                };

            }
            if (IsAllowed(Permission.VORepair)) // REPAIR VEHICLE
            {
                menu.AddMenuItem(fixVehicle);
            }
            if (IsAllowed(Permission.VOKeepClean))
            {
                menu.AddMenuItem(vehicleNeverDirty);
            }
            if (IsAllowed(Permission.VOWash))
            {
                menu.AddMenuItem(cleanVehicle); // CLEAN VEHICLE
                menu.AddMenuItem(setDirtLevel); // SET DIRT LEVEL
            }
            if (IsAllowed(Permission.VOMod)) // MOD MENU
            {
                menu.AddMenuItem(modMenuBtn);
            }
            if (IsAllowed(Permission.VOColors)) // COLORS MENU
            {
                menu.AddMenuItem(colorsMenuBtn);
            }
            if (IsAllowed(Permission.VOUnderglow)) // UNDERGLOW EFFECTS
            {
                menu.AddMenuItem(underglowMenuBtn);
                MenuController.BindMenuItem(menu, VehicleUnderglowMenu, underglowMenuBtn);
            }
            if (IsAllowed(Permission.VOLiveries)) // LIVERIES MENU
            {
                menu.AddMenuItem(liveriesMenuBtn);
            }
            if (IsAllowed(Permission.VOComponents)) // COMPONENTS MENU
            {
                menu.AddMenuItem(componentsMenuBtn);
            }
            if (IsAllowed(Permission.VOEngine)) // TOGGLE ENGINE ON/OFF
            {
                menu.AddMenuItem(toggleEngine);
            }
            if (IsAllowed(Permission.VOChangePlate))
            {
                menu.AddMenuItem(setLicensePlateText); // SET LICENSE PLATE TEXT
                menu.AddMenuItem(setLicensePlateType); // SET LICENSE PLATE TYPE
            }
            if (IsAllowed(Permission.VODoors)) // DOORS MENU
            {
                menu.AddMenuItem(doorsMenuBtn);
            }
            if (IsAllowed(Permission.VOWindows)) // WINDOWS MENU
            {
                menu.AddMenuItem(windowsMenuBtn);
            }
            if (IsAllowed(Permission.VOBikeSeatbelt))
            {
                menu.AddMenuItem(vehicleBikeSeatbelt);
            }
            if (IsAllowed(Permission.VOSpeedLimiter)) // SPEED LIMITER
            {
                menu.AddMenuItem(speedLimiter);
            }
            if (IsAllowed(Permission.VOTorqueMultiplier))
            {
                menu.AddMenuItem(torqueEnabled); // TORQUE ENABLED
                menu.AddMenuItem(torqueMultiplier); // TORQUE LIST
            }
            if (IsAllowed(Permission.VOPowerMultiplier))
            {
                menu.AddMenuItem(powerEnabled); // POWER ENABLED
                menu.AddMenuItem(powerMultiplier); // POWER LIST
            }
            if (IsAllowed(Permission.VODisableTurbulence))
            {
                menu.AddMenuItem(vehicleNoTurbulence);
                menu.AddMenuItem(vehicleNoTurbulenceHeli);
            }
            if (IsAllowed(Permission.VOFlip)) // FLIP VEHICLE
            {
                menu.AddMenuItem(flipVehicle);
            }
            if (IsAllowed(Permission.VOAlarm)) // TOGGLE VEHICLE ALARM
            {
                menu.AddMenuItem(vehicleAlarm);
            }
            if (IsAllowed(Permission.VOCycleSeats)) // CYCLE THROUGH VEHICLE SEATS
            {
                menu.AddMenuItem(cycleSeats);
            }
            if (IsAllowed(Permission.VOLights)) // VEHICLE LIGHTS LIST
            {
                menu.AddMenuItem(vehicleLights);
            }
            if (IsAllowed(Permission.VOFixOrDestroyTires))
            {
                menu.AddMenuItem(vehicleTiresList);
            }
            if (IsAllowed(Permission.VODestroyEngine))
            {
                menu.AddMenuItem(destroyEngine);
            }
            if (IsAllowed(Permission.VOFreeze)) // FREEZE VEHICLE
            {
                menu.AddMenuItem(vehicleFreeze);
            }
            if (IsAllowed(Permission.VOInvisible)) // MAKE VEHICLE INVISIBLE
            {
                menu.AddMenuItem(vehicleInvisible);
            }
            if (IsAllowed(Permission.VOEngineAlwaysOn)) // LEAVE ENGINE RUNNING
            {
                menu.AddMenuItem(vehicleEngineAO);
            }
            if (IsAllowed(Permission.VOInfiniteFuel)) // INFINITE FUEL
            {
                menu.AddMenuItem(infiniteFuel);
            }
            // always allowed
            menu.AddMenuItem(showHealth); // SHOW VEHICLE HEALTH

            // I don't really see why would you want to disable this so I will not add useless permissions
            menu.AddMenuItem(radioStations);

            if (IsAllowed(Permission.VONoSiren) && !GetSettingsBool(Setting.vmenu_use_els_compatibility_mode)) // DISABLE SIREN
            {
                menu.AddMenuItem(vehicleNoSiren);
            }
            if (IsAllowed(Permission.VONoHelmet)) // DISABLE BIKE HELMET
            {
                menu.AddMenuItem(vehicleNoBikeHelmet);
            }
            if (IsAllowed(Permission.VOFlashHighbeamsOnHonk)) // FLASH HIGHBEAMS ON HONK
            {
                menu.AddMenuItem(highbeamsOnHonk);
            }

            if (IsAllowed(Permission.VODelete)) // DELETE VEHICLE
            {
                menu.AddMenuItem(deleteBtn);
            }
            #endregion

            #region delete vehicle handle stuff
            DeleteConfirmMenu.AddMenuItem(deleteNoBtn);
            DeleteConfirmMenu.AddMenuItem(deleteYesBtn);
            DeleteConfirmMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteNoBtn)
                {
                    DeleteConfirmMenu.GoBack();
                }
                else
                {
                    var veh = GetVehicle();
                    if (veh != null && veh.Exists() && GetVehicle().Driver == Game.PlayerPed)
                    {
                        SetVehicleHasBeenOwnedByPlayer(veh.Handle, false);
                        SetEntityAsMissionEntity(veh.Handle, false, false);
                        veh.Delete();
                    }
                    else
                    {
                        if (!Game.PlayerPed.IsInVehicle())
                        {
                            Notify.Alert(CommonErrors.NoVehicle);
                        }
                        else
                        {
                            Notify.Alert("你需要在驾驶座上才能删除车辆。");
                        }

                    }
                    DeleteConfirmMenu.GoBack();
                    menu.GoBack();
                }
            };
            #endregion

            #region Bind Submenus to their buttons.
            MenuController.BindMenuItem(menu, VehicleModMenu, modMenuBtn);
            MenuController.BindMenuItem(menu, VehicleDoorsMenu, doorsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleWindowsMenu, windowsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleComponentsMenu, componentsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleLiveriesMenu, liveriesMenuBtn);
            MenuController.BindMenuItem(menu, VehicleColorsMenu, colorsMenuBtn);
            MenuController.BindMenuItem(menu, DeleteConfirmMenu, deleteBtn);
            #endregion

            #region Handle button presses
            // Manage button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteBtn) // reset the index so that "no" / "cancel" will always be selected by default.
                {
                    DeleteConfirmMenu.RefreshIndex();
                }
                // If the player is actually in a vehicle, continue.
                if (GetVehicle() != null && GetVehicle().Exists())
                {
                    // Create a vehicle object.
                    var vehicle = GetVehicle();

                    // Check if the player is the driver of the vehicle, if so, continue.
                    if (vehicle.GetPedOnSeat(VehicleSeat.Driver) == new Ped(Game.PlayerPed.Handle))
                    {
                        // Repair vehicle.
                        if (item == fixVehicle)
                        {
                            vehicle.Repair();
                        }
                        // Clean vehicle.
                        else if (item == cleanVehicle)
                        {
                            vehicle.Wash();
                        }
                        // Flip vehicle.
                        else if (item == flipVehicle)
                        {
                            SetVehicleOnGroundProperly(vehicle.Handle);
                        }
                        // Toggle alarm.
                        else if (item == vehicleAlarm)
                        {
                            ToggleVehicleAlarm(vehicle);
                        }
                        // Toggle engine
                        else if (item == toggleEngine)
                        {
                            SetVehicleEngineOn(vehicle.Handle, !vehicle.IsEngineRunning, false, true);
                        }
                        // Set license plate text
                        else if (item == setLicensePlateText)
                        {
                            SetLicensePlateCustomText();
                        }
                        // Make vehicle invisible.
                        else if (item == vehicleInvisible)
                        {
                            if (vehicle.IsVisible)
                            {
                                // Check the visibility of all peds inside before setting the vehicle as invisible.
                                var visiblePeds = new Dictionary<Ped, bool>();
                                foreach (var p in vehicle.Occupants)
                                {
                                    visiblePeds.Add(p, p.IsVisible);
                                }

                                // Set the vehicle invisible or invincivble.
                                vehicle.IsVisible = !vehicle.IsVisible;

                                // Restore visibility for each ped.
                                foreach (var pe in visiblePeds)
                                {
                                    pe.Key.IsVisible = pe.Value;
                                }
                            }
                            else
                            {
                                // Set the vehicle invisible or invincivble.
                                vehicle.IsVisible = !vehicle.IsVisible;
                            }
                        }
                        // Destroy vehicle engine
                        else if (item == destroyEngine)
                        {
                            SetVehicleEngineHealth(vehicle.Handle, -4000);
                        }
                    }

                    // If the player is not the driver seat and a button other than the option below (cycle seats) was pressed, notify them.
                    else if (item != cycleSeats)
                    {
                        Notify.Error("你必须是车辆的驾驶员才能访问此菜单！", true, false);
                    }

                    // Cycle vehicle seats
                    if (item == cycleSeats)
                    {
                        CycleThroughSeats();
                    }
                }
            };
            #endregion

            #region Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // Create a vehicle object.
                var vehicle = GetVehicle();

                if (item == vehicleGod) // God Mode Toggled
                {
                    VehicleGodMode = _checked;
                }
                else if (item == vehicleFreeze) // Freeze Vehicle Toggled
                {
                    VehicleFrozen = _checked;
                    if (!_checked)
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            FreezeEntityPosition(vehicle.Handle, false);
                        }
                    }
                }
                else if (item == torqueEnabled) // Enable Torque Multiplier Toggled
                {
                    VehicleTorqueMultiplier = _checked;
                }
                else if (item == powerEnabled) // Enable Power Multiplier Toggled
                {
                    VehiclePowerMultiplier = _checked;
                    if (_checked)
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, VehiclePowerMultiplierAmount);
                        }
                    }
                    else
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, 1f);
                        }
                    }
                }
                else if (item == vehicleEngineAO) // Leave Engine Running (vehicle always on) Toggled
                {
                    VehicleEngineAlwaysOn = _checked;
                }
                else if (item == showHealth) // show vehicle health on screen.
                {
                    VehicleShowHealth = _checked;
                }
                else if (item == vehicleNoSiren) // Disable Siren Toggled
                {
                    VehicleNoSiren = _checked;
                    if (vehicle != null && vehicle.Exists())
                    {
                        vehicle.IsSirenSilent = _checked;
                    }
                }
                else if (item == vehicleNoBikeHelmet) // No Helemet Toggled
                {
                    VehicleNoBikeHelemet = _checked;
                }
                else if (item == highbeamsOnHonk)
                {
                    FlashHighbeamsOnHonk = _checked;
                }
                else if (item == vehicleNoTurbulence)
                {
                    DisablePlaneTurbulence = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsPlane)
                    {
                        if (MainMenu.VehicleOptionsMenu.DisablePlaneTurbulence)
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 0f);
                        }
                        else
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 1.0f);
                        }
                    }
                }
                else if (item == vehicleNoTurbulenceHeli)
                {
                    DisableHelicopterTurbulence = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsHelicopter)
                    {
                        if (MainMenu.VehicleOptionsMenu.DisableHelicopterTurbulence)
                        {
                            SetHeliTurbulenceScalar(vehicle.Handle, 0f);
                        }
                        else
                        {
                            SetHeliTurbulenceScalar(vehicle.Handle, 1f);
                        }
                    }
                }
                else if (item == vehicleNeverDirty)
                {
                    VehicleNeverDirty = _checked;
                }
                else if (item == vehicleBikeSeatbelt)
                {
                    VehicleBikeSeatbelt = _checked;
                }
                else if (item == infiniteFuel)
                {
                    VehicleInfiniteFuel = _checked;
                }
            };
            #endregion

            #region Handle List Changes.
            // Handle list changes.
            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (GetVehicle() != null && GetVehicle().Exists())
                {
                    var veh = GetVehicle();
                    // If the torque multiplier changed. Change the torque multiplier to the new value.
                    if (item == torqueMultiplier)
                    {
                        // Get the selected value and remove the "x" in the string with nothing.
                        var value = torqueMultiplierList[newIndex].ToString().Replace("x", "");
                        // Convert the value to a float and set it as a public variable.
                        VehicleTorqueMultiplierAmount = float.Parse(value);
                    }
                    // If the power multiplier is changed. Change the power multiplier to the new value.
                    else if (item == powerMultiplier)
                    {
                        // Get the selected value. Remove the "x" from the string.
                        var value = powerMultiplierList[newIndex].ToString().Replace("x", "");
                        // Conver the string into a float and set it to be the value of the public variable.
                        VehiclePowerMultiplierAmount = float.Parse(value);
                        if (VehiclePowerMultiplier)
                        {
                            SetVehicleEnginePowerMultiplier(veh.Handle, VehiclePowerMultiplierAmount);
                        }
                    }
                    else if (item == setLicensePlateType)
                    {
                        // Set the license plate style.
                        switch (newIndex)
                        {
                            case 0:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite1;
                                break;
                            case 1:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite2;
                                break;
                            case 2:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite3;
                                break;
                            case 3:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;
                                break;
                            case 4:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlack;
                                break;
                            case 5:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.NorthYankton;
                                break;
                            default:
                                break;
                        }
                    }
                }
            };
            #endregion

            #region Handle List Items Selected
            menu.OnListItemSelect += async (sender, item, listIndex, itemIndex) =>
            {
                // Set dirt level
                if (item == setDirtLevel)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        GetVehicle().DirtLevel = float.Parse(listIndex.ToString());
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Toggle vehicle lights
                else if (item == vehicleLights)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        // We need to do % 4 because this seems to be some sort of flags system. For a taxi, this function returns 65, 66, etc.
                        // So % 4 takes care of that.
                        var state = GetVehicleIndicatorLights(veh.Handle) % 4; // 0 = none, 1 = left, 2 = right, 3 = both

                        if (listIndex == 0) // Hazard lights
                        {
                            if (state != 3) // either all lights are off, or one of the two (left/right) is off.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else // both are on.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 1) // left indicator
                        {
                            if (state != 1) // Left indicator is (only) off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 2) // right indicator
                        {
                            if (state != 2) // Right indicator (only) is off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 3) // Interior lights
                        {
                            SetVehicleInteriorlight(veh.Handle, !IsVehicleInteriorLightOn(veh.Handle));
                            //CommonFunctions.Log("Something cool here.");
                        }
                        //else if (listIndex == 4) // taxi light
                        //{
                        //    veh.IsTaxiLightOn = !veh.IsTaxiLightOn;
                        //    //    SetTaxiLights(veh, true);
                        //    //    SetTaxiLights(veh, false);
                        //    //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //    //SetTaxiLights(veh, true);
                        //    //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //    //SetTaxiLights(veh, false);
                        //    //    //SetTaxiLights(veh, !IsTaxiLightOn(veh));
                        //    //    CommonFunctions.Log
                        //}
                        else if (listIndex == 4) // helicopter spotlight
                        {
                            SetVehicleSearchlight(veh.Handle, !IsVehicleSearchlightOn(veh.Handle), true);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Speed Limiter
                else if (item == speedLimiter)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var vehicle = GetVehicle();

                        if (vehicle != null && vehicle.Exists())
                        {
                            if (listIndex == 0) // Set
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                SetEntityMaxSpeed(vehicle.Handle, vehicle.Speed);

                                if (ShouldUseMetricMeasurements()) // 公里每小时
                                {
                                    Notify.Info($"车辆速度现在限制为 ~b~{Math.Round(vehicle.Speed * 3.6f, 1)} 公里每小时~s~。");
                                }
                                else // 英里每小时
                                {
                                    Notify.Info($"车辆速度现在限制为 ~b~{Math.Round(vehicle.Speed * 2.237f, 1)} 英里每小时~s~。");
                                }

                            }
                            else if (listIndex == 1) // Reset
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f); // 默认最大速度似乎对所有车辆都有效。
                                Notify.Info("车辆速度现在不再受限制。");
                            }
                            else if (listIndex == 2) // custom speed
                            {
                                var inputSpeed = await GetUserInput("Enter a speed (in meters/sec)", "20.0", 5);
                                if (!string.IsNullOrEmpty(inputSpeed))
                                {
                                    if (float.TryParse(inputSpeed, out var outFloat))
                                    {
                                        //vehicle.MaxSpeed = outFloat;
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outFloat + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // 公里每小时
                                        {
                                            Notify.Info($"车辆速度现在限制为 ~b~{Math.Round(outFloat * 3.6f, 1)} 公里每小时~s~。");
                                        }
                                        else // 英里每小时
                                        {
                                            Notify.Info($"车辆速度现在限制为 ~b~{Math.Round(outFloat * 2.237f, 1)} 英里每小时~s~。");
                                        }
                                    }
                                    else if (int.TryParse(inputSpeed, out var outInt))
                                    {
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outInt + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // 公里每小时
                                        {
                                            Notify.Info($"车辆速度现在限制为 ~b~{Math.Round(outInt * 3.6f, 1)} 公里每小时~s~。");
                                        }
                                        else // 英里每小时
                                        {
                                            Notify.Info($"车辆速度现在限制为 ~b~{Math.Round(outInt * 2.237f, 1)} 英里每小时~s~。");
                                        }
                                    }
                                    else
                                    {
                                        Notify.Error("这不是有效的数字。请输入有效的速度（米/秒）。");
                                    }
                                }
                                else
                                {
                                    Notify.Error(CommonErrors.InvalidInput);
                                }
                            }
                        }
                    }
                }
                else if (item == vehicleTiresList)
                {
                    //bool fix = item == vehicleTiresList;

                    var veh = GetVehicle();
                    if (veh != null && veh.Exists())
                    {
                        if (Game.PlayerPed == veh.Driver)
                        {
                            if (listIndex == 0)
                            {
                                if (IsVehicleTyreBurst(veh.Handle, 0, false))
                                {
                                    for (var i = 0; i < 8; i++)
                                    {
                                        SetVehicleTyreFixed(veh.Handle, i);
                                    }
                                    Notify.Success("所有车辆轮胎已修复。");
                                }
                                else
                                {
                                    for (var i = 0; i < 8; i++)
                                    {
                                        SetVehicleTyreBurst(veh.Handle, i, false, 1f);
                                    }
                                    Notify.Success("所有车辆轮胎已销毁。");
                                }
                            }
                            else
                            {
                                var index = listIndex - 1;
                                if (IsVehicleTyreBurst(veh.Handle, index, false))
                                {
                                    SetVehicleTyreFixed(veh.Handle, index);
                                    Notify.Success($"车辆轮胎 #{listIndex} 已修复。");
                                }
                                else
                                {
                                    SetVehicleTyreBurst(veh.Handle, index, false, 1f);
                                    Notify.Success($"车辆轮胎 #{listIndex} 已销毁。");
                                }
                            }
                        }
                        else
                        {
                            Notify.Error(CommonErrors.NeedToBeTheDriver);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                else if (item == radioStations)
                {
                    var newStation = (RadioStation)Enum.GetValues(typeof(RadioStation)).GetValue(listIndex);

                    var veh = GetVehicle();
                    if (veh != null && veh.Exists())
                    {
                        veh.RadioStation = newStation;
                    }

                    UserDefaults.VehicleDefaultRadio = (int)newStation;
                }
            };
            #endregion

            #region Vehicle Colors Submenu Stuff
            var primaryColorsMenu = new Menu("车辆颜色", "主色调");
            MenuController.AddSubmenu(VehicleColorsMenu, primaryColorsMenu);

            var primaryColorsBtn = new MenuItem("主色调") { Label = "→→→" };
            VehicleColorsMenu.AddMenuItem(primaryColorsBtn);
            MenuController.BindMenuItem(VehicleColorsMenu, primaryColorsMenu, primaryColorsBtn);

            // 副菜单
            var secondaryColorsMenu = new Menu("车辆颜色", "副色调");
            MenuController.AddSubmenu(VehicleColorsMenu, secondaryColorsMenu);

            var secondaryColorsBtn = new MenuItem("副色调") { Label = "→→→" };
            VehicleColorsMenu.AddMenuItem(secondaryColorsBtn);
            MenuController.BindMenuItem(VehicleColorsMenu, secondaryColorsMenu, secondaryColorsBtn);

            // color lists
            var classic = new List<string>();
            var matte = new List<string>();
            var metals = new List<string>();
            var util = new List<string>();
            var worn = new List<string>();
            var chameleon = new List<string>();
            var wheelColors = new List<string>() { "默认合金" };

            // Just quick and dirty solution to put this in a new enclosed section so that we can still use 'i' as a counter in the other code parts.
            {
                var i = 0;
                foreach (var vc in VehicleData.ClassicColors)
                {
                    classic.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.ClassicColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MatteColors)
                {
                    matte.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MatteColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MetalColors)
                {
                    metals.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MetalColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.UtilColors)
                {
                    util.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.UtilColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.WornColors)
                {
                    worn.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.WornColors.Count})");
                    i++;
                }

                if (GetSettingsBool(Setting.vmenu_using_chameleon_colours))
                {
                    i = 0;
                    foreach (var vc in VehicleData.ChameleonColors)
                    {
                        chameleon.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.ChameleonColors.Count})");
                        i++;
                    }
                }

                wheelColors.AddRange(classic);
            }

            var wheelColorsList = new MenuListItem("车轮颜色", wheelColors, 0);
            var dashColorList = new MenuListItem("仪表盘颜色", classic, 0);
            var intColorList = new MenuListItem("内饰/修边颜色", classic, 0);
            var vehicleEnveffScale = new MenuSliderItem("车辆环境效果比例", "仅对某些车辆有效，例如 Besra。它会“淡化”某些涂层。", 0, 20, 10, true);

            var chrome = new MenuItem("铬");
            VehicleColorsMenu.AddMenuItem(chrome);
            VehicleColorsMenu.AddMenuItem(vehicleEnveffScale);

            VehicleColorsMenu.OnItemSelect += (sender, item, index) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    if (item == chrome)
                    {
                        SetVehicleColours(veh.Handle, 120, 120); // chrome is index 120
                    }
                }
                else
                {
                    Notify.Error("您需要是可驾驶车辆的驾驶员才能更改此项。");
                }
            };
            VehicleColorsMenu.OnSliderPositionChange += (m, sliderItem, oldPosition, newPosition, itemIndex) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Driver == Game.PlayerPed && !veh.IsDead)
                {
                    if (sliderItem == vehicleEnveffScale)
                    {
                        SetVehicleEnveffScale(veh.Handle, newPosition / 20f);
                    }
                }
                else
                {
                    Notify.Error("您需要是可驾驶车辆的驾驶员才能更改此滑块。");
                }
            };

            VehicleColorsMenu.AddMenuItem(dashColorList);
            VehicleColorsMenu.AddMenuItem(intColorList);
            VehicleColorsMenu.AddMenuItem(wheelColorsList);

            VehicleColorsMenu.OnListIndexChange += HandleListIndexChanges;

            void HandleListIndexChanges(Menu sender, MenuListItem listItem, int oldIndex, int newIndex, int itemIndex)
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    var primaryColor = 0;
                    var secondaryColor = 0;
                    var pearlColor = 0;
                    var wheelColor = 0;
                    var dashColor = 0;
                    var intColor = 0;

                    GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);
                    GetVehicleExtraColours(veh.Handle, ref pearlColor, ref wheelColor);
                    GetVehicleDashboardColour(veh.Handle, ref dashColor);
                    GetVehicleInteriorColour(veh.Handle, ref intColor);

                    if (sender == primaryColorsMenu)
                    {
                        if (itemIndex == 1)
                        {
                            pearlColor = VehicleData.ClassicColors[newIndex].id;
                        }
                        else
                        {
                            pearlColor = 0;
                        }

                        switch (itemIndex)
                        {
                            case 0:
                            case 1:
                                primaryColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 2:
                                primaryColor = VehicleData.MatteColors[newIndex].id;
                                break;
                            case 3:
                                primaryColor = VehicleData.MetalColors[newIndex].id;
                                break;
                            case 4:
                                primaryColor = VehicleData.UtilColors[newIndex].id;
                                break;
                            case 5:
                                primaryColor = VehicleData.WornColors[newIndex].id;
                                break;
                        }

                        if (GetSettingsBool(Setting.vmenu_using_chameleon_colours))
                        {
                            if (itemIndex == 6)
                            {
                                primaryColor = VehicleData.ChameleonColors[newIndex].id;
                                secondaryColor = VehicleData.ChameleonColors[newIndex].id;

                                SetVehicleModKit(veh.Handle, 0);
                            }
                        }

                        SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
                    }
                    else if (sender == secondaryColorsMenu)
                    {
                        switch (itemIndex)
                        {
                            case 0:
                                pearlColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 1:
                            case 2:
                                secondaryColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 3:
                                secondaryColor = VehicleData.MatteColors[newIndex].id;
                                break;
                            case 4:
                                secondaryColor = VehicleData.MetalColors[newIndex].id;
                                break;
                            case 5:
                                secondaryColor = VehicleData.UtilColors[newIndex].id;
                                break;
                            case 6:
                                secondaryColor = VehicleData.WornColors[newIndex].id;
                                break;
                        }
                        SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
                    }
                    else if (sender == VehicleColorsMenu)
                    {
                        if (listItem == wheelColorsList)
                        {
                            if (newIndex == 0)
                            {
                                wheelColor = 156; // default alloy color.
                            }
                            else
                            {
                                wheelColor = VehicleData.ClassicColors[newIndex - 1].id;
                            }
                        }
                        else if (listItem == dashColorList)
                        {
                            dashColor = VehicleData.ClassicColors[newIndex].id;
                            // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                            // this should actually be called SetVehicleDashboardColour
                            SetVehicleInteriorColour(veh.Handle, dashColor);
                        }
                        else if (listItem == intColorList)
                        {
                            intColor = VehicleData.ClassicColors[newIndex].id;
                            // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                            // this should actually be called SetVehicleInteriorColour
                            SetVehicleDashboardColour(veh.Handle, intColor);
                        }
                    }

                    SetVehicleExtraColours(veh.Handle, pearlColor, wheelColor);
                }
                else
                {
                    Notify.Error("您需要是车辆的驾驶员才能更改车辆颜色。");
                }
            }

            for (var i = 0; i < 2; i++)
            {
                var pearlescentList = new MenuListItem("珍珠光泽", classic, 0);
                var classicList = new MenuListItem("经典", classic, 0);
                var metallicList = new MenuListItem("金属", classic, 0);
                var matteList = new MenuListItem("哑光", matte, 0);
                var metalList = new MenuListItem("金属质感", metals, 0);
                var utilList = new MenuListItem("实用", util, 0);
                var wornList = new MenuListItem("磨损", worn, 0);

                if (i == 0)
                {
                    primaryColorsMenu.AddMenuItem(classicList);
                    primaryColorsMenu.AddMenuItem(metallicList);
                    primaryColorsMenu.AddMenuItem(matteList);
                    primaryColorsMenu.AddMenuItem(metalList);
                    primaryColorsMenu.AddMenuItem(utilList);
                    primaryColorsMenu.AddMenuItem(wornList);

                    if (GetSettingsBool(Setting.vmenu_using_chameleon_colours))
                    {
                        var chameleonList = new MenuListItem("Chameleon", chameleon, 0);

                        primaryColorsMenu.AddMenuItem(chameleonList);
                    }

                    primaryColorsMenu.OnListIndexChange += HandleListIndexChanges;
                }
                else
                {
                    secondaryColorsMenu.AddMenuItem(pearlescentList);
                    secondaryColorsMenu.AddMenuItem(classicList);
                    secondaryColorsMenu.AddMenuItem(metallicList);
                    secondaryColorsMenu.AddMenuItem(matteList);
                    secondaryColorsMenu.AddMenuItem(metalList);
                    secondaryColorsMenu.AddMenuItem(utilList);
                    secondaryColorsMenu.AddMenuItem(wornList);

                    secondaryColorsMenu.OnListIndexChange += HandleListIndexChanges;
                }
            }
            #endregion

            #region Vehicle Doors Submenu Stuff
            var openAll = new MenuItem("打开所有车门", "打开所有车辆车门。");
            var closeAll = new MenuItem("关闭所有车门", "关闭所有车辆车门。");
            var LF = new MenuItem("左前车门", "打开/关闭左前车门。");
            var RF = new MenuItem("右前车门", "打开/关闭右前车门。");
            var LR = new MenuItem("左后车门", "打开/关闭左后车门。");
            var RR = new MenuItem("右后车门", "打开/关闭右后车门。");
            var HD = new MenuItem("引擎盖", "打开/关闭引擎盖。");
            var TR = new MenuItem("后备箱", "打开/关闭后备箱。");
            var E1 = new MenuItem("额外门 1", "打开/关闭额外门 (#1)。请注意，这种门在大多数车辆上不存在。");
            var E2 = new MenuItem("额外门 2", "打开/关闭额外门 (#2)。请注意，这种门在大多数车辆上不存在。");
            var BB = new MenuItem("炸弹舱", "打开/关闭炸弹舱。仅在某些飞机上可用。");
            var doors = new List<string>() { "前左", "前右", "后左", "后右", "引擎盖", "后备箱", "额外门 1", "额外门 2" };
            var removeDoorList = new MenuListItem("移除车门", doors, 0, "完全移除指定的车辆车门。");
            var deleteDoors = new MenuCheckboxItem("删除移除的车门", "启用时，通过上面的列表移除的车门将从世界中删除。如果禁用，则车门将只是掉落在地面上。", false);

            VehicleDoorsMenu.AddMenuItem(LF);
            VehicleDoorsMenu.AddMenuItem(RF);
            VehicleDoorsMenu.AddMenuItem(LR);
            VehicleDoorsMenu.AddMenuItem(RR);
            VehicleDoorsMenu.AddMenuItem(HD);
            VehicleDoorsMenu.AddMenuItem(TR);
            VehicleDoorsMenu.AddMenuItem(E1);
            VehicleDoorsMenu.AddMenuItem(E2);
            VehicleDoorsMenu.AddMenuItem(BB);
            VehicleDoorsMenu.AddMenuItem(openAll);
            VehicleDoorsMenu.AddMenuItem(closeAll);
            VehicleDoorsMenu.AddMenuItem(removeDoorList);
            VehicleDoorsMenu.AddMenuItem(deleteDoors);

            VehicleDoorsMenu.OnListItemSelect += (sender, item, index, itemIndex) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists())
                {
                    if (veh.Driver == Game.PlayerPed)
                    {
                        if (item == removeDoorList)
                        {
                            SetVehicleDoorBroken(veh.Handle, index, deleteDoors.Checked);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NeedToBeTheDriver);
                    }
                }
                else
                {
                    Notify.Error(CommonErrors.NoVehicle);
                }
            };

            // Handle button presses.
            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                // Get the vehicle.
                var veh = GetVehicle();
                // If the player is in a vehicle, it's not dead and the player is the driver, continue.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    // If button 0-5 are pressed, then open/close that specific index/door.
                    if (index < 8)
                    {
                        // If the door is open.
                        var open = GetVehicleDoorAngleRatio(veh.Handle, index) > 0.1f;

                        if (open)
                        {
                            // Close the door.
                            SetVehicleDoorShut(veh.Handle, index, false);
                        }
                        else
                        {
                            // Open the door.
                            SetVehicleDoorOpen(veh.Handle, index, false, false);
                        }
                    }
                    // If the index >= 8, and the button is "openAll": open all doors.
                    else if (item == openAll)
                    {
                        // Loop through all doors and open them.
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorOpen(veh.Handle, door, false, false);
                        }
                        if (veh.HasBombBay)
                        {
                            veh.OpenBombBay();
                        }
                    }
                    // If the index >= 8, and the button is "closeAll": close all doors.
                    else if (item == closeAll)
                    {
                        // Close all doors.
                        SetVehicleDoorsShut(veh.Handle, false);
                        if (veh.HasBombBay)
                        {
                            veh.CloseBombBay();
                        }
                    }
                    // If bomb bay doors button is pressed and the vehicle has bomb bay doors.
                    else if (item == BB && veh.HasBombBay)
                    {
                        var bombBayOpen = AreBombBayDoorsOpen(veh.Handle);
                        // If open, close them.
                        if (bombBayOpen)
                        {
                            veh.CloseBombBay();
                        }
                        // Otherwise, open them.
                        else
                        {
                            veh.OpenBombBay();
                        }
                    }
                }
                else
                {
                    Notify.Alert(CommonErrors.NoVehicle, placeholderValue: "打开或关闭载具门");
                }
            };

            #endregion

            #region Vehicle Windows Submenu Stuff
            var fwu = new MenuItem("~y~↑~s~ 升起前窗", "升起前面两个窗户。");
            var fwd = new MenuItem("~o~↓~s~ 降下前窗", "降下前面两个窗户。");
            var rwu = new MenuItem("~y~↑~s~ 升起后窗", "升起后面两个窗户。");
            var rwd = new MenuItem("~o~↓~s~ 降下后窗", "降下后面两个窗户。");            VehicleWindowsMenu.AddMenuItem(fwu);
            VehicleWindowsMenu.AddMenuItem(fwd);
            VehicleWindowsMenu.AddMenuItem(rwu);
            VehicleWindowsMenu.AddMenuItem(rwd);
            VehicleWindowsMenu.OnItemSelect += (sender, item, index) =>
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead)
                {
                    if (item == fwu)
                    {
                        RollUpWindow(veh.Handle, 0);
                        RollUpWindow(veh.Handle, 1);
                    }
                    else if (item == fwd)
                    {
                        RollDownWindow(veh.Handle, 0);
                        RollDownWindow(veh.Handle, 1);
                    }
                    else if (item == rwu)
                    {
                        RollUpWindow(veh.Handle, 2);
                        RollUpWindow(veh.Handle, 3);
                    }
                    else if (item == rwd)
                    {
                        RollDownWindow(veh.Handle, 2);
                        RollDownWindow(veh.Handle, 3);
                    }
                }
            };
            #endregion

            #region Vehicle Liveries Submenu Stuff
            menu.OnItemSelect += (sender, item, idex) =>
            {
                // If the liverys menu button is selected.
                if (item == liveriesMenuBtn)
                {
                    // Get the player's vehicle.
                    var veh = GetVehicle();
                    // If it exists, isn't dead and the player is in the drivers seat continue.
                    if (veh != null && veh.Exists() && !veh.IsDead)
                    {
                        if (veh.Driver == Game.PlayerPed)
                        {
                            VehicleLiveriesMenu.ClearMenuItems();
                            SetVehicleModKit(veh.Handle, 0);
                            var liveryCount = GetVehicleLiveryCount(veh.Handle);

                            if (liveryCount > 0)
                            {
                                var liveryList = new List<string>();
                                for (var i = 0; i < liveryCount; i++)
                                {
                                    var livery = GetLiveryName(veh.Handle, i);
                                    livery = GetLabelText(livery) != "NULL" ? GetLabelText(livery) : $"Livery #{i}";
                                    liveryList.Add(livery);
                                }
                                var liveryListItem = new MenuListItem("设置涂装", liveryList, GetVehicleLivery(veh.Handle), "Choose a livery for this vehicle.");
                                VehicleLiveriesMenu.AddMenuItem(liveryListItem);
                                VehicleLiveriesMenu.OnListIndexChange += (_menu, listItem, oldIndex, newIndex, itemIndex) =>
                                {
                                    if (listItem == liveryListItem)
                                    {
                                        veh = GetVehicle();
                                        SetVehicleLivery(veh.Handle, newIndex);
                                    }
                                };
                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                            else
                            {
                                Notify.Error("这辆车没有任何涂装。");
                                VehicleLiveriesMenu.CloseMenu();
                                menu.OpenMenu();
                                var backBtn = new MenuItem("没有可用的涂装 :(", "点击我返回。")
                                {
                                    Label = "返回"
                                };
                                VehicleLiveriesMenu.AddMenuItem(backBtn);
                                VehicleLiveriesMenu.OnItemSelect += (sender2, item2, index2) =>
                                {
                                    if (item2 == backBtn)
                                    {
                                        VehicleLiveriesMenu.GoBack();
                                    }
                                };

                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                        }
                        else
                        {
                            Notify.Error("你必须是车辆的驾驶员才能访问此菜单。");
                        }
                    }
                    else
                    {
                        Notify.Error("你必须是车辆的驾驶员才能访问此菜单。");
                    }
                }
            };
            #endregion

            #region Vehicle Mod Submenu Stuff
            menu.OnItemSelect += (sender, item, index) =>
            {
                // When the mod submenu is openend, reset all items in there.
                if (item == modMenuBtn)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        UpdateMods();
                    }
                    else
                    {
                        VehicleModMenu.CloseMenu();
                        menu.OpenMenu();
                    }

                }
            };
            #endregion

            #region Vehicle Components Submenu
            // when the components menu is opened.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If the components menu is opened.
                if (item == componentsMenuBtn)
                {
                    // Empty the menu in case there were leftover buttons from another vehicle.
                    if (VehicleComponentsMenu.Size > 0)
                    {
                        VehicleComponentsMenu.ClearMenuItems();
                        vehicleExtras.Clear();
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();
                    }

                    // Get the vehicle.
                    var veh = GetVehicle();

                    // Check if the vehicle exists, it's actually a vehicle, it's not dead/broken and the player is in the drivers seat.
                    if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                    {
                        //List<int> extraIds = new List<int>();
                        // Loop through all possible extra ID's (AFAIK: 0-14).
                        for (var extra = 0; extra < 14; extra++)
                        {
                            // If this extra exists...
                            if (veh.ExtraExists(extra))
                            {
                                // Add it's ID to the list.
                                //extraIds.Add(extra);

                                // Create a checkbox for it.
                                var extraCheckbox = new MenuCheckboxItem($"额外的 #{extra}", extra.ToString(), veh.IsExtraOn(extra));
                                // Add the checkbox to the menu.
                                VehicleComponentsMenu.AddMenuItem(extraCheckbox);

                                // Add it's ID to the dictionary.
                                vehicleExtras[extraCheckbox] = extra;
                            }
                        }



                        if (vehicleExtras.Count > 0)
                        {
                            var backBtn = new MenuItem("返回", "返回到车辆选项菜单。");
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        else
                        {
                            var backBtn = new MenuItem("没有可用的额外选项 :(", "返回到车辆选项菜单。")
                            {
                                Label = "返回"
                            };
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        // And update the submenu to prevent weird glitches.
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();

                    }
                }
            };
            // when a checkbox in the components menu changes
            VehicleComponentsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // When a checkbox is checked/unchecked, get the selected checkbox item index and use that to get the component ID from the list.
                // Then toggle that extra.
                if (vehicleExtras.TryGetValue(item, out var extra))
                {
                    var veh = GetVehicle();
                    veh.ToggleExtra(extra, _checked);
                }
            };
            #endregion

            #region Underglow Submenu
            var underglowFront = new MenuCheckboxItem("启用前灯", "启用或禁用车辆前侧的霓虹灯。注意，并非所有车辆都有灯光。", false);
            var underglowBack = new MenuCheckboxItem("启用后灯", "启用或禁用车辆后侧的霓虹灯。注意，并非所有车辆都有灯光。", false);
            var underglowLeft = new MenuCheckboxItem("启用左侧灯", "启用或禁用车辆左侧的霓虹灯。注意，并非所有车辆都有灯光。", false);
            var underglowRight = new MenuCheckboxItem("启用右侧灯", "启用或禁用车辆右侧的霓虹灯。注意，并非所有车辆都有灯光。", false);
            var underglowColorsList = new List<string>();
            for (var i = 0; i < 13; i++)
            {
                underglowColorsList.Add(GetLabelText($"CMOD_NEONCOL_{i}"));
            }
            var underglowColor = new MenuListItem(GetLabelText("CMOD_NEON_1"), underglowColorsList, 0, "Select the color of the neon underglow.");

            VehicleUnderglowMenu.AddMenuItem(underglowFront);
            VehicleUnderglowMenu.AddMenuItem(underglowBack);
            VehicleUnderglowMenu.AddMenuItem(underglowLeft);
            VehicleUnderglowMenu.AddMenuItem(underglowRight);

            VehicleUnderglowMenu.AddMenuItem(underglowColor);

            menu.OnItemSelect += (sender, item, index) =>
            {
                #region reset checkboxes state when opening the menu.
                if (item == underglowMenuBtn)
                {
                    var veh = GetVehicle();
                    if (veh != null)
                    {
                        if (veh.Mods.HasNeonLights)
                        {
                            underglowFront.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Front) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front);
                            underglowBack.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Back) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back);
                            underglowLeft.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Left) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left);
                            underglowRight.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Right) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right);

                            underglowFront.Enabled = true;
                            underglowBack.Enabled = true;
                            underglowLeft.Enabled = true;
                            underglowRight.Enabled = true;

                            underglowFront.LeftIcon = MenuItem.Icon.NONE;
                            underglowBack.LeftIcon = MenuItem.Icon.NONE;
                            underglowLeft.LeftIcon = MenuItem.Icon.NONE;
                            underglowRight.LeftIcon = MenuItem.Icon.NONE;
                        }
                        else
                        {
                            underglowFront.Checked = false;
                            underglowBack.Checked = false;
                            underglowLeft.Checked = false;
                            underglowRight.Checked = false;

                            underglowFront.Enabled = false;
                            underglowBack.Enabled = false;
                            underglowLeft.Enabled = false;
                            underglowRight.Enabled = false;

                            underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                            underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                            underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                            underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                        }
                    }
                    else
                    {
                        underglowFront.Checked = false;
                        underglowBack.Checked = false;
                        underglowLeft.Checked = false;
                        underglowRight.Checked = false;

                        underglowFront.Enabled = false;
                        underglowBack.Enabled = false;
                        underglowLeft.Enabled = false;
                        underglowRight.Enabled = false;

                        underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                        underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                        underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                        underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                    }

                    underglowColor.ListIndex = GetIndexFromColor();
                }
                #endregion
            };
            // handle item selections
            VehicleUnderglowMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    var veh = GetVehicle();
                    if (veh.Mods.HasNeonLights)
                    {
                        veh.Mods.NeonLightsColor = GetColorFromIndex(underglowColor.ListIndex);
                        if (item == underglowLeft)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Left, veh.Mods.HasNeonLight(VehicleNeonLight.Left) && _checked);
                        }
                        else if (item == underglowRight)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Right, veh.Mods.HasNeonLight(VehicleNeonLight.Right) && _checked);
                        }
                        else if (item == underglowBack)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Back, veh.Mods.HasNeonLight(VehicleNeonLight.Back) && _checked);
                        }
                        else if (item == underglowFront)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Front, veh.Mods.HasNeonLight(VehicleNeonLight.Front) && _checked);
                        }
                    }
                }
            };

            VehicleUnderglowMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == underglowColor)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        if (veh.Mods.HasNeonLights)
                        {
                            veh.Mods.NeonLightsColor = GetColorFromIndex(newIndex);
                        }
                    }
                }
            };
            #endregion

            #region Handle menu-opening refreshing license plate
            menu.OnMenuOpen += (sender) =>
            {
                menu.GetMenuItems().ForEach((item) =>
                {
                    var veh = GetVehicle(true);

                    if (item == setLicensePlateType && item is MenuListItem listItem && veh != null && veh.Exists())
                    {
                        // Set the license plate style.
                        switch (veh.Mods.LicensePlateStyle)
                        {
                            case LicensePlateStyle.BlueOnWhite1:
                                listItem.ListIndex = 0;
                                break;
                            case LicensePlateStyle.BlueOnWhite2:
                                listItem.ListIndex = 1;
                                break;
                            case LicensePlateStyle.BlueOnWhite3:
                                listItem.ListIndex = 2;
                                break;
                            case LicensePlateStyle.YellowOnBlue:
                                listItem.ListIndex = 3;
                                break;
                            case LicensePlateStyle.YellowOnBlack:
                                listItem.ListIndex = 4;
                                break;
                            case LicensePlateStyle.NorthYankton:
                                listItem.ListIndex = 5;
                                break;
                            default:
                                break;
                        }
                    }
                });
            };
            #endregion

        }
        #endregion

        /// <summary>
        /// Public get method for the menu. Checks if the menu exists, if not create the menu first.
        /// </summary>
        /// <returns>Returns the Vehicle Options menu.</returns>
        public Menu GetMenu()
        {
            // If menu doesn't exist. Create one.
            if (menu == null)
            {
                CreateMenu();
            }
            // Return the menu.
            return menu;
        }

        #region Update Vehicle Mods Menu
        /// <summary>
        /// Refreshes the mods page. The selectedIndex allows you to go straight to a specific index after refreshing the menu.
        /// This is used because when the wheel type is changed, the menu is refreshed to update the available wheels list.
        /// </summary>
        /// <param name="selectedIndex">Pass this if you want to go straight to a specific mod/index.</param>
        public void UpdateMods(int selectedIndex = 0)
        {
            // If there are items, remove all of them.
            if (VehicleModMenu.Size > 0)
            {
                if (selectedIndex != 0)
                {
                    VehicleModMenu.ClearMenuItems(true);
                }
                else
                {
                    VehicleModMenu.ClearMenuItems(false);
                }

            }

            // Get the vehicle.
            var veh = GetVehicle();

            // Check if the vehicle exists, is still drivable/alive and it's actually a vehicle.
            if (veh != null && veh.Exists() && !veh.IsDead)
            {
                #region initial setup & dynamic vehicle mods setup
                // Set the modkit so we can modify the car.
                SetVehicleModKit(veh.Handle, 0);

                // Get all mods available on this vehicle.
                var mods = veh.Mods.GetAllMods();

                // Loop through all the mods.
                foreach (var mod in mods)
                {
                    veh = GetVehicle();

                    // Get the proper localized mod type (suspension, armor, etc) name.
                    var typeName = mod.LocalizedModTypeName;

                    // Create a list to all available upgrades for this modtype.
                    var modlist = new List<string>();

                    // Get the current item index ({current}/{max upgrades})
                    var currentItem = $"[1/{mod.ModCount + 1}]";

                    // Add the stock value for this mod.
                    var name = $"Stock {typeName} {currentItem}";
                    modlist.Add(name);

                    // Loop through all available upgrades for this specific mod type.
                    for (var x = 0; x < mod.ModCount; x++)
                    {
                        // Create the item index.
                        currentItem = $"[{2 + x}/{mod.ModCount + 1}]";

                        // Create the name (again, converting to proper case), then add the name.
                        name = mod.GetLocalizedModName(x) != "" ? $"{ToProperString(mod.GetLocalizedModName(x))} {currentItem}" : $"{typeName} #{x} {currentItem}";
                        modlist.Add(name);
                    }

                    // Create the MenuListItem for this mod type.
                    var currIndex = GetVehicleMod(veh.Handle, (int)mod.ModType) + 1;
                    var modTypeListItem = new MenuListItem(
                        typeName,
                        modlist,
                        currIndex,
                        $"选择一个 ~y~{typeName}~s~ 升级，它将自动应用到您的车辆上。"
                    )
                    {
                        ItemData = (int)mod.ModType
                    };

                    // Add the list item to the menu.
                    VehicleModMenu.AddMenuItem(modTypeListItem);
                }
                #endregion

                #region more variables and setup
                veh = GetVehicle();
                // Create the wheel types list & listitem and add it to the menu.
                var wheelTypes = new List<string>()
                {
                    "运动型",       // 0
                    "肌肉车",       // 1
                    "低底盘",       // 2
                    "SUV",          // 3
                    "越野",         // 4
                    "改装车",         // 5
                    "摩托车轮胎",   // 6
                    "高端",         // 7
                    "班尼改装 (1)", // 8
                    "班尼改装 (2)", // 9
                    "开轮式",       // 10
                    "街头"          // 11
                };
                var vehicleWheelType = new MenuListItem("轮胎类型", wheelTypes, MathUtil.Clamp(GetVehicleWheelType(veh.Handle), 0, 11), $"选择您的车辆的~y~轮胎类型~s~。");
                if (!veh.Model.IsBoat && !veh.Model.IsHelicopter && !veh.Model.IsPlane && !veh.Model.IsBicycle && !veh.Model.IsTrain)
                {
                    VehicleModMenu.AddMenuItem(vehicleWheelType);
                }

                // Create the checkboxes for some options.
                var toggleCustomWheels = new MenuCheckboxItem("切换自定义轮胎", "按此键以添加或移除~y~自定义~s~轮胎。", GetVehicleModVariation(veh.Handle, 23));
                var xenonHeadlights = new MenuCheckboxItem("氙气大灯", "启用或禁用~b~氙气~s~大灯。", IsToggleModOn(veh.Handle, 22));
                var turbo = new MenuCheckboxItem("涡轮增压", "启用或禁用此车辆的~y~涡轮增压~s~。", IsToggleModOn(veh.Handle, 18));
                var bulletProofTires = new MenuCheckboxItem("防弹轮胎", "启用或禁用此车辆的~y~防弹轮胎~s~。", !GetVehicleTyresCanBurst(veh.Handle));
                var lowGripTires = new MenuCheckboxItem("低抓地力轮胎", "启用或禁用此车辆的~y~低抓地力轮胎~s~。", GetDriftTyresEnabled(veh.Handle));

                // Add the checkboxes to the menu.
                VehicleModMenu.AddMenuItem(toggleCustomWheels);
                VehicleModMenu.AddMenuItem(xenonHeadlights);
                var currentHeadlightColor = GetHeadlightsColorForVehicle(veh);
                if (currentHeadlightColor is < 0 or > 12)
                {
                    currentHeadlightColor = 13;
                }
                var headlightColor = new MenuListItem("大灯颜色", new List<string>() { "白色", "蓝色", "电光蓝", "薄荷绿", "酸橙绿", "黄色", "金色", "橙色", "红色", "小马粉", "热粉", "紫色", "黑光", "默认氙气" }, currentHeadlightColor, "新功能：带色大灯。请先启用氙气大灯。");
                VehicleModMenu.AddMenuItem(headlightColor);
                VehicleModMenu.AddMenuItem(turbo);
                VehicleModMenu.AddMenuItem(bulletProofTires);
                VehicleModMenu.AddMenuItem(lowGripTires);
                // Create a list of tire smoke options.
                var tireSmokes = new List<string>() { "红色", "橙色", "黄色", "金色", "浅绿", "深绿", "浅蓝", "深蓝", "紫色", "粉色", "黑色" };
                var tireSmokeColors = new Dictionary<string, int[]>()
                {
                    ["红色"] = new int[] { 244, 65, 65 },
                    ["橙色"] = new int[] { 244, 167, 66 },
                    ["黄色"] = new int[] { 244, 217, 65 },
                    ["金色"] = new int[] { 181, 120, 0 },
                    ["浅绿"] = new int[] { 158, 255, 84 },
                    ["深绿"] = new int[] { 44, 94, 5 },
                    ["浅蓝"] = new int[] { 65, 211, 244 },
                    ["深蓝"] = new int[] { 24, 54, 163 },
                    ["紫色"] = new int[] { 108, 24, 192 },
                    ["粉色"] = new int[] { 192, 24, 172 },
                    ["黑色"] = new int[] { 1, 1, 1 }
                };
                int smoker = 0, smokeg = 0, smokeb = 0;
                GetVehicleTyreSmokeColor(veh.Handle, ref smoker, ref smokeg, ref smokeb);
                var item = tireSmokeColors.ToList().Find((f) => { return f.Value[0] == smoker && f.Value[1] == smokeg && f.Value[2] == smokeb; });
                var index = tireSmokeColors.ToList().IndexOf(item);
                if (index < 0)
                {
                    index = 0;
                }

                var tireSmoke = new MenuListItem("轮胎烟雾颜色", tireSmokes, index, $"选择您的车辆的~y~轮胎烟雾颜色~s~。");
                VehicleModMenu.AddMenuItem(tireSmoke);

                // 创建启用/禁用轮胎烟雾的复选框。
                var tireSmokeEnabled = new MenuCheckboxItem("轮胎烟雾", "启用或禁用~y~轮胎烟雾~s~。~h~~r~重要：~s~禁用轮胎烟雾后，需要开车一段时间才能生效。", IsToggleModOn(veh.Handle, 20));
                VehicleModMenu.AddMenuItem(tireSmokeEnabled);

                // 创建窗户贴膜列表
                var windowTints = new List<string>() { "原装 [1/7]", "无 [2/7]", "豪华 [3/7]", "浅烟雾 [4/7]", "深烟雾 [5/7]", "纯黑 [6/7]", "绿色 [7/7]" };

                var currentTint = GetVehicleWindowTint(veh.Handle);
                if (currentTint == -1)
                {
                    currentTint = 4; // stock
                }

                // Convert window tint to the correct index of the list above.
                switch (currentTint)
                {
                    case 0:
                        currentTint = 1; // None
                        break;
                    case 1:
                        currentTint = 5; // Pure Black
                        break;
                    case 2:
                        currentTint = 4; // Dark Smoke
                        break;
                    case 3:
                        currentTint = 3; // Light Smoke
                        break;
                    case 4:
                        currentTint = 0; // Stock
                        break;
                    case 5:
                        currentTint = 2; // Limo
                        break;
                    case 6:
                        currentTint = 6; // Green
                        break;
                    default:
                        break;
                }

                var windowTint = new MenuListItem("窗户贴膜", windowTints, currentTint, "为您的窗户应用贴膜。");
                VehicleModMenu.AddMenuItem(windowTint);

                #endregion

                #region Checkbox Changes
                // Handle checkbox changes.
                VehicleModMenu.OnCheckboxChange += (sender2, item2, index2, _checked) =>
                {
                    veh = GetVehicle();

                    // Xenon Headlights
                    if (item2 == xenonHeadlights)
                    {
                        ToggleVehicleMod(veh.Handle, 22, _checked);
                    }
                    // Turbo
                    else if (item2 == turbo)
                    {
                        ToggleVehicleMod(veh.Handle, 18, _checked);
                    }
                    // Bullet Proof Tires
                    else if (item2 == bulletProofTires)
                    {
                        SetVehicleTyresCanBurst(veh.Handle, !_checked);
                    }
                    // Low Grip Tyres
                    else if (item2 == lowGripTires)
                    {
                        SetDriftTyresEnabled(veh.Handle, _checked);
                    }
                    // Custom Wheels
                    else if (item2 == toggleCustomWheels)
                    {
                        SetVehicleMod(veh.Handle, 23, GetVehicleMod(veh.Handle, 23), !GetVehicleModVariation(veh.Handle, 23));

                        // If the player is on a motorcycle, also change the back wheels.
                        if (IsThisModelABike((uint)GetEntityModel(veh.Handle)))
                        {
                            SetVehicleMod(veh.Handle, 24, GetVehicleMod(veh.Handle, 24), GetVehicleModVariation(veh.Handle, 23));
                        }
                    }
                    // Toggle Tire Smoke
                    else if (item2 == tireSmokeEnabled)
                    {
                        // If it should be enabled:
                        if (_checked)
                        {
                            // Enable it.
                            ToggleVehicleMod(veh.Handle, 20, true);
                            // Get the selected color values.
                            var r = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][0];
                            var g = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][1];
                            var b = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][2];
                            // Set the color.
                            SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                        }
                        // If it should be disabled:
                        else
                        {
                            // Set the smoke to white.
                            SetVehicleTyreSmokeColor(veh.Handle, 255, 255, 255);
                            // Disable it.
                            ToggleVehicleMod(veh.Handle, 20, false);
                            // Remove the mod.
                            RemoveVehicleMod(veh.Handle, 20);
                        }
                    }
                };
                #endregion

                #region List Changes
                // Handle list selections
                VehicleModMenu.OnListIndexChange += (sender2, item2, oldIndex, newIndex, itemIndex) =>
                {
                    // Get the vehicle and set the mod kit.
                    veh = GetVehicle();
                    SetVehicleModKit(veh.Handle, 0);

                    #region handle the dynamic (vehicle-specific) mods
                    // If the affected list is actually a "dynamically" generated list, continue. If it was one of the manual options, go to else.
                    if (item2.ItemData is int modType)
                    {
                        var selectedUpgrade = item2.ListIndex - 1;
                        var customWheels = GetVehicleModVariation(veh.Handle, 23);

                        SetVehicleMod(veh.Handle, modType, selectedUpgrade, customWheels);
                    }
                    #endregion
                    // If it was not one of the lists above, then it was one of the manual lists/options selected, 
                    // either: vehicle Wheel Type, tire smoke color, or window tint:
                    #region Handle the items available on all vehicles.
                    // Wheel types
                    else if (item2 == vehicleWheelType)
                    {
                        var vehicleClass = GetVehicleClass(veh.Handle);
                        var isBikeOrOpenWheel = (newIndex == 6 && veh.Model.IsBike) || (newIndex == 10 && vehicleClass == 22);
                        var isNotBikeNorOpenWheel = newIndex != 6 && !veh.Model.IsBike && newIndex != 10 && vehicleClass != 22;
                        var isCorrectVehicleType = isBikeOrOpenWheel || isNotBikeNorOpenWheel;
                        if (!isCorrectVehicleType)
                        {
                            // Go past the index if it's not a bike.
                            if (!veh.Model.IsBike && vehicleClass != 22)
                            {
                                if (newIndex > oldIndex)
                                {
                                    item2.ListIndex++;
                                }
                                else
                                {
                                    item2.ListIndex--;
                                }
                            }
                            // Reset the index to 6 if it is a bike
                            else
                            {
                                item2.ListIndex = veh.Model.IsBike ? 6 : 10;
                            }
                        }
                        // Set the wheel type
                        SetVehicleWheelType(veh.Handle, item2.ListIndex);

                        var customWheels = GetVehicleModVariation(veh.Handle, 23);

                        // Reset the wheel mod index for front wheels
                        SetVehicleMod(veh.Handle, 23, -1, customWheels);

                        // If the model is a bike, do the same thing for the rear wheels.
                        if (veh.Model.IsBike)
                        {
                            SetVehicleMod(veh.Handle, 24, -1, customWheels);
                        }

                        // Refresh the menu with the item index so that the view doesn't change
                        UpdateMods(selectedIndex: itemIndex);
                    }
                    // Tire smoke
                    else if (item2 == tireSmoke)
                    {
                        // Get the selected color values.
                        var r = tireSmokeColors[tireSmokes[newIndex]][0];
                        var g = tireSmokeColors[tireSmokes[newIndex]][1];
                        var b = tireSmokeColors[tireSmokes[newIndex]][2];

                        // Set the color.
                        SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                    }
                    // Window Tint
                    else if (item2 == windowTint)
                    {
                        // Stock = 4,
                        // None = 0,
                        // Limo = 5,
                        // LightSmoke = 3,
                        // DarkSmoke = 2,
                        // PureBlack = 1,
                        // Green = 6,

                        switch (newIndex)
                        {
                            case 1:
                                SetVehicleWindowTint(veh.Handle, 0); // None
                                break;
                            case 2:
                                SetVehicleWindowTint(veh.Handle, 5); // Limo
                                break;
                            case 3:
                                SetVehicleWindowTint(veh.Handle, 3); // Light Smoke
                                break;
                            case 4:
                                SetVehicleWindowTint(veh.Handle, 2); // Dark Smoke
                                break;
                            case 5:
                                SetVehicleWindowTint(veh.Handle, 1); // Pure Black
                                break;
                            case 6:
                                SetVehicleWindowTint(veh.Handle, 6); // Green
                                break;
                            case 0:
                            default:
                                SetVehicleWindowTint(veh.Handle, 4); // Stock
                                break;
                        }
                    }
                    else if (item2 == headlightColor)
                    {
                        if (newIndex == 13) // default
                        {
                            SetHeadlightsColorForVehicle(veh, 255);
                        }
                        else if (newIndex is > (-1) and < 13)
                        {
                            SetHeadlightsColorForVehicle(veh, newIndex);
                        }
                    }
                    #endregion
                };

                #endregion
            }
            // Refresh Index and update the scaleform to prevent weird broken menus.
            if (selectedIndex == 0)
            {
                VehicleModMenu.RefreshIndex();
            }

            //VehicleModMenu.UpdateScaleform();

            // Set the selected index to the provided index (0 by default)
            // Used for example, when the wheelstype is changed, the menu is refreshed and we want to set the
            // selected item back to the "wheelsType" list so the user doesn't have to scroll down each time they
            // change the wheels type.
            //VehicleModMenu.CurrentIndex = selectedIndex;
        }

        internal static void SetHeadlightsColorForVehicle(Vehicle veh, int newIndex)
        {

            if (veh != null && veh.Exists() && veh.Driver == Game.PlayerPed)
            {
                if (newIndex is > (-1) and < 13)
                {
                    SetVehicleHeadlightsColour(veh.Handle, newIndex);
                }
                else
                {
                    SetVehicleHeadlightsColour(veh.Handle, -1);
                }
            }
        }

        internal static int GetHeadlightsColorForVehicle(Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Exists())
            {
                if (IsToggleModOn(vehicle.Handle, 22))
                {
                    var val = GetVehicleHeadlightsColour(vehicle.Handle);
                    if (val is > (-1) and < 13)
                    {
                        return val;
                    }
                    return -1;
                }
            }
            return -1;
        }
        #endregion

        #region GetColorFromIndex function (underglow)

        private readonly List<int[]> _VehicleNeonLightColors = new()
        {
            { new int[3] { 255, 255, 255 } },   // White
            { new int[3] { 2, 21, 255 } },      // Blue
            { new int[3] { 3, 83, 255 } },      // Electric blue
            { new int[3] { 0, 255, 140 } },     // Mint Green
            { new int[3] { 94, 255, 1 } },      // Lime Green
            { new int[3] { 255, 255, 0 } },     // Yellow
            { new int[3] { 255, 150, 5 } },     // Golden Shower
            { new int[3] { 255, 62, 0 } },      // Orange
            { new int[3] { 255, 0, 0 } },       // Red
            { new int[3] { 255, 50, 100 } },    // Pony Pink
            { new int[3] { 255, 5, 190 } },     // Hot Pink
            { new int[3] { 35, 1, 255 } },      // Purple
            { new int[3] { 15, 3, 255 } },      // Blacklight
        };

        /// <summary>
        /// Converts a list index to a <see cref="System.Drawing.Color"/> struct.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private System.Drawing.Color GetColorFromIndex(int index)
        {
            if (index is >= 0 and < 13)
            {
                return System.Drawing.Color.FromArgb(_VehicleNeonLightColors[index][0], _VehicleNeonLightColors[index][1], _VehicleNeonLightColors[index][2]);
            }
            return System.Drawing.Color.FromArgb(255, 255, 255);
        }

        /// <summary>
        /// Returns the color index that is applied on the current vehicle. 
        /// If a color is active on the vehicle which is not in the list, it'll return the default index 0 (white).
        /// </summary>
        /// <returns></returns>
        private int GetIndexFromColor()
        {
            var veh = GetVehicle();

            if (veh == null || !veh.Exists() || !veh.Mods.HasNeonLights)
            {
                return 0;
            }

            int r = 255, g = 255, b = 255;

            GetVehicleNeonLightsColour(veh.Handle, ref r, ref g, ref b);

            if (r == 255 && g == 0 && b == 255) // default return value when the vehicle has no neon kit selected.
            {
                return 0;
            }

            if (_VehicleNeonLightColors.Any(a => { return a[0] == r && a[1] == g && a[2] == b; }))
            {
                return _VehicleNeonLightColors.FindIndex(a => { return a[0] == r && a[1] == g && a[2] == b; });
            }

            return 0;
        }
        #endregion
    }
}
