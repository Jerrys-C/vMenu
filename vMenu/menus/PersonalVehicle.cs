using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class PersonalVehicle
    {
        // Variables
        private Menu menu;
        public bool EnableVehicleBlip { get; private set; } = UserDefaults.PVEnableVehicleBlip;

        // Empty constructor
        public PersonalVehicle() { }

        public Vehicle CurrentPersonalVehicle { get; internal set; } = null;

        public Menu VehicleDoorsMenu { get; internal set; } = null;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Menu
            menu = new Menu(GetSafePlayerName(Game.Player.Name), "个人车辆选项");
            // menu items
            var setVehice = new MenuItem("设置车辆", "将当前车辆设置为个人车辆。如果已经设置了个人车辆，则会覆盖之前的设置。") { Label = "当前车辆: 无" };
            var toggleEngine = new MenuItem("切换引擎", "切换引擎的开启或关闭，即使你不在车内。其他人使用你的车辆时此功能无效。");
            var toggleLights = new MenuListItem("设置车辆灯光", new List<string> { "强制开启", "强制关闭", "重置" }, 0, "启用或禁用车辆前照灯。引擎需要启动。");
            var toggleStance = new MenuListItem("车辆姿态", new List<string> { "默认", "降低" }, 0, "选择个人车辆的姿态。");
            var kickAllPassengers = new MenuItem("踢出乘客", "将所有乘客从个人车辆中移除。");
            var lockDoors = new MenuItem("锁定车辆车门", "锁定所有车辆车门，对所有玩家生效。已在车内的玩家仍可以离开车辆，即使车门已锁定。");
            var unlockDoors = new MenuItem("解锁车辆车门", "解锁所有车辆车门，对所有玩家生效。");
             var doorsMenuBtn = new MenuItem("车辆车门", "在这里打开、关闭、移除和恢复车辆车门。")
            {
                Label = "→→→"
            };
            var soundHorn = new MenuItem("鸣喇叭", "鸣响车辆喇叭。");
            var toggleAlarm = new MenuItem("切换警报声音", "切换车辆警报声音的开启或关闭状态。此操作不会设置警报，只是切换警报的当前响声状态。");
            var enableBlip = new MenuCheckboxItem("为个人车辆添加标记", "启用或禁用在标记车辆为个人车辆时添加的标记。", EnableVehicleBlip) { Style = MenuCheckboxItem.CheckboxStyle.Cross };
            var exclusiveDriver = new MenuCheckboxItem("专属司机", "如果启用，则只有你可以进入驾驶座位。其他玩家不能驾驶车辆，但仍可以作为乘客。", false) { Style = MenuCheckboxItem.CheckboxStyle.Cross };

            // 子菜单
            VehicleDoorsMenu = new Menu("车辆车门", "车辆车门管理");
            MenuController.AddSubmenu(menu, VehicleDoorsMenu);
            MenuController.BindMenuItem(menu, VehicleDoorsMenu, doorsMenuBtn);

            // This is always allowed if this submenu is created/allowed.
            menu.AddMenuItem(setVehice);

            // Add conditional features.

            // Toggle engine.
            if (IsAllowed(Permission.PVToggleEngine))
            {
                menu.AddMenuItem(toggleEngine);
            }

            // Toggle lights
            if (IsAllowed(Permission.PVToggleLights))
            {
                menu.AddMenuItem(toggleLights);
            }

            // Toggle stance
            if (IsAllowed(Permission.PVToggleStance))
            {
                menu.AddMenuItem(toggleStance);
            }

            // Kick vehicle passengers
            if (IsAllowed(Permission.PVKickPassengers))
            {
                menu.AddMenuItem(kickAllPassengers);
            }

            // Lock and unlock vehicle doors
            if (IsAllowed(Permission.PVLockDoors))
            {
                menu.AddMenuItem(lockDoors);
                menu.AddMenuItem(unlockDoors);
            }

            if (IsAllowed(Permission.PVDoors))
            {
                menu.AddMenuItem(doorsMenuBtn);
            }

            // Sound horn
            if (IsAllowed(Permission.PVSoundHorn))
            {
                menu.AddMenuItem(soundHorn);
            }

            // Toggle alarm sound
            if (IsAllowed(Permission.PVToggleAlarm))
            {
                menu.AddMenuItem(toggleAlarm);
            }

            // Enable blip for personal vehicle
            if (IsAllowed(Permission.PVAddBlip))
            {
                menu.AddMenuItem(enableBlip);
            }

            if (IsAllowed(Permission.PVExclusiveDriver))
            {
                menu.AddMenuItem(exclusiveDriver);
            }


            // Handle list presses
            menu.OnListItemSelect += (sender, item, itemIndex, index) =>
            {
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists())
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("你当前不能控制这辆车。是否有其他玩家在驾驶你的车辆？请确保其他玩家没有控制你的车辆后重试。");
                            return;
                        }
                    }

                    if (item == toggleLights)
                    {
                        PressKeyFob(CurrentPersonalVehicle);
                        if (itemIndex == 0)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 3);
                        }
                        else if (itemIndex == 1)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 1);
                        }
                        else
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 0);
                        }
                    }
                    else if (item == toggleStance)
                    {
                        PressKeyFob(CurrentPersonalVehicle);
                        if (itemIndex == 0)
                        {
                            SetReduceDriftVehicleSuspension(CurrentPersonalVehicle.Handle, false);
                        }
                        else if (itemIndex == 1)
                        {
                            SetReduceDriftVehicleSuspension(CurrentPersonalVehicle.Handle, true);
                        }
                    }

                }
                else
                {
                    Notify.Error("你尚未选择个人车辆，或者你的车辆已被删除。请设置个人车辆后再使用这些选项。");
                }
            };

            // Handle checkbox changes
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == enableBlip)
                {
                    EnableVehicleBlip = _checked;
                    if (EnableVehicleBlip)
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                        {
                            if (CurrentPersonalVehicle.AttachedBlip == null || !CurrentPersonalVehicle.AttachedBlip.Exists())
                            {
                                CurrentPersonalVehicle.AttachBlip();
                            }
                            CurrentPersonalVehicle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                            CurrentPersonalVehicle.AttachedBlip.Name = "个人车辆";
                        }
                        else
                        {
                            Notify.Error("你尚未选择个人车辆，或者你的车辆已被删除。请设置个人车辆后再使用这些选项。");
                        }

                    }
                    else
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists() && CurrentPersonalVehicle.AttachedBlip != null && CurrentPersonalVehicle.AttachedBlip.Exists())
                        {
                            CurrentPersonalVehicle.AttachedBlip.Delete();
                        }
                    }
                }
                else if (item == exclusiveDriver)
                {
                    if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                    {
                        if (NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (_checked)
                            {
                                // SetVehicleExclusiveDriver, but the current version is broken in C# so we manually execute it.
                                CitizenFX.Core.Native.Function.Call((CitizenFX.Core.Native.Hash)0x41062318F23ED854, CurrentPersonalVehicle, true);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, Game.PlayerPed.Handle, 1);
                            }
                            else
                            {
                                // SetVehicleExclusiveDriver, but the current version is broken in C# so we manually execute it.
                                CitizenFX.Core.Native.Function.Call((CitizenFX.Core.Native.Hash)0x41062318F23ED854, CurrentPersonalVehicle, false);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, 0, 1);
                            }
                        }
                        else
                        {
                            item.Checked = !_checked;
                            Notify.Error("你当前不能控制这辆车。是否有其他玩家在驾驶你的车辆？请确保其他玩家没有控制你的车辆后重试。");
                        }
                    }
                }
            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == setVehice)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        if (veh != null && veh.Exists())
                        {
                            if (Game.PlayerPed == veh.Driver)
                            {
                                CurrentPersonalVehicle = veh;
                                veh.PreviouslyOwnedByPlayer = true;
                                veh.IsPersistent = true;
                                if (EnableVehicleBlip && IsAllowed(Permission.PVAddBlip))
                                {
                                    if (veh.AttachedBlip == null || !veh.AttachedBlip.Exists())
                                    {
                                        veh.AttachBlip();
                                    }
                                    veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    veh.AttachedBlip.Name = "个人车辆";
                                }
                                var name = GetLabelText(veh.DisplayName);
                                if (string.IsNullOrEmpty(name) || name.ToLower() == "null")
                                {
                                    name = veh.DisplayName;
                                }
                                item.Label = $"当前车辆: {name}";
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
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                else if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                {
                    if (item == kickAllPassengers)
                    {
                        if (CurrentPersonalVehicle.Occupants.Count() > 0 && CurrentPersonalVehicle.Occupants.Any(p => p != Game.PlayerPed))
                        {
                            var netId = VehToNet(CurrentPersonalVehicle.Handle);
                            TriggerServerEvent("vMenu:GetOutOfCar", netId, Game.Player.ServerId);
                        }
                        else
                        {
                            Notify.Info("没有其他玩家在你的车辆中需要被踢出。");
                        }
                    }
                    else
                    {
                        if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                            {
                                Notify.Error("你目前无法控制这辆车。是否有其他玩家正在驾驶你的车？请确认没有其他玩家在控制你的车后再试一次。");
                                return;
                            }
                        }

                        if (item == toggleEngine)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            SetVehicleEngineOn(CurrentPersonalVehicle.Handle, !CurrentPersonalVehicle.IsEngineRunning, true, true);
                        }

                        else if (item == lockDoors || item == unlockDoors)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            var _lock = item == lockDoors;
                            LockOrUnlockDoors(CurrentPersonalVehicle, _lock);
                        }

                        else if (item == soundHorn)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            SoundHorn(CurrentPersonalVehicle);
                        }

                        else if (item == toggleAlarm)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            ToggleVehicleAlarm(CurrentPersonalVehicle);
                        }
                    }
                }
                else
                {
                    Notify.Error("你尚未选择个人车辆，或您的车辆已被删除。请先设置个人车辆后才能使用这些选项。");
                }
            };

            #region Doors submenu 
            var openAll = new MenuItem("打开所有车门", "打开所有车门。");
            var closeAll = new MenuItem("关闭所有车门", "关闭所有车门。");
            var LF = new MenuItem("左前门", "打开/关闭左前门。");
            var RF = new MenuItem("右前门", "打开/关闭右前门。");
            var LR = new MenuItem("左后门", "打开/关闭左后门。");
            var RR = new MenuItem("右后门", "打开/关闭右后门。");
            var HD = new MenuItem("引擎盖", "打开/关闭引擎盖。");
            var TR = new MenuItem("后备箱", "打开/关闭后备箱。");
            var E1 = new MenuItem("额外车门 1", "打开/关闭额外车门（#1）。请注意，这扇门在大多数车辆上不存在。");
            var E2 = new MenuItem("额外车门 2", "打开/关闭额外车门（#2）。请注意，这扇门在大多数车辆上不存在。");
            var BB = new MenuItem("炸弹舱", "打开/关闭炸弹舱。仅在某些飞机上可用。");
            var doors = new List<string>() { "左前门", "右前门", "左后门", "右后门", "引擎盖", "后备箱", "额外车门 1", "额外车门 2", "炸弹舱" };
            var removeDoorList = new MenuListItem("移除车门", doors, 0, "完全移除指定的车门。");
            var deleteDoors = new MenuCheckboxItem("删除已移除的车门", "启用时，通过上方列表移除的车门将从世界中删除。如果禁用，车门将掉落在地上。", false);

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
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists())
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("您目前无法控制这辆车。是否有其他人正在驾驶您的车辆？请确保其他玩家没有控制您的车辆后再试一次。");
                            return;
                        }
                    }

                    if (item == removeDoorList)
                    {
                        PressKeyFob(veh);
                        SetVehicleDoorBroken(veh.Handle, index, deleteDoors.Checked);
                    }
                }
            };

            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists() && !veh.IsDead)
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("您目前无法控制这辆车。是否有其他人正在驾驶您的车辆？请确保没有其他玩家控制您的车辆后再试一次。");
                            return;
                        }
                    }

                    if (index < 8)
                    {
                        var open = GetVehicleDoorAngleRatio(veh.Handle, index) > 0.1f;
                        PressKeyFob(veh);
                        if (open)
                        {
                            SetVehicleDoorShut(veh.Handle, index, false);
                        }
                        else
                        {
                            SetVehicleDoorOpen(veh.Handle, index, false, false);
                        }
                    }
                    else if (item == openAll)
                    {
                        PressKeyFob(veh);
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorOpen(veh.Handle, door, false, false);
                        }
                    }
                    else if (item == closeAll)
                    {
                        PressKeyFob(veh);
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorShut(veh.Handle, door, false);
                        }
                    }
                    else if (item == BB && veh.HasBombBay)
                    {
                        PressKeyFob(veh);
                        var bombBayOpen = AreBombBayDoorsOpen(veh.Handle);
                        if (bombBayOpen)
                        {
                            veh.CloseBombBay();
                        }
                        else
                        {
                            veh.OpenBombBay();
                        }
                    }
                    else
                    {
                        Notify.Error("您尚未选择个人车辆，或您的车辆已被删除。请先设置个人车辆，然后才能使用这些选项。");
                    }
                }
            };
            #endregion
        }



        private async void SoundHorn(Vehicle veh)
        {
            if (veh != null && veh.Exists())
            {
                var timer = GetGameTimer();
                while (GetGameTimer() - timer < 1000)
                {
                    SoundVehicleHornThisFrame(veh.Handle);
                    await Delay(0);
                }
            }
        }

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
