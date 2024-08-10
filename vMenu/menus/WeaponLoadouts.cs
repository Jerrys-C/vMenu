using System.Collections.Generic;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class WeaponLoadouts
    {
        // 变量
        private Menu menu = null;
        private readonly Menu SavedLoadoutsMenu = new("已保存的武器", "已保存的武器列表");
        private readonly Menu ManageLoadoutMenu = new("管理武器", "管理已保存的武器");
        public bool WeaponLoadoutsSetLoadoutOnRespawn { get; private set; } = UserDefaults.WeaponLoadoutsSetLoadoutOnRespawn;

        private readonly Dictionary<string, List<ValidWeapon>> SavedWeapons = new();

        public static Dictionary<string, List<ValidWeapon>> GetSavedWeapons()
        {
            var handle = StartFindKvp("vmenu_string_saved_weapon_loadout_");
            var saves = new Dictionary<string, List<ValidWeapon>>();
            while (true)
            {
                var kvp = FindKvp(handle);
                if (string.IsNullOrEmpty(kvp))
                {
                    break;
                }
                saves.Add(kvp, JsonConvert.DeserializeObject<List<ValidWeapon>>(GetResourceKvpString(kvp)));
            }
            EndFindKvp(handle);
            return saves;
        }

        private string SelectedSavedLoadoutName { get; set; } = "";
        // vmenu_temp_weapons_loadout_before_respawn
        // vmenu_string_saved_weapon_loadout_

        /// <summary>
        /// 返回已保存的武器列表，并设置 <see cref="SavedWeapons"/> 变量。
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<ValidWeapon>> RefreshSavedWeaponsList()
        {
            if (SavedWeapons.Count > 0)
            {
                SavedWeapons.Clear();
            }

            var handle = StartFindKvp("vmenu_string_saved_weapon_loadout_");
            var saves = new List<string>();
            while (true)
            {
                var kvp = FindKvp(handle);
                if (string.IsNullOrEmpty(kvp))
                {
                    break;
                }
                saves.Add(kvp);
            }
            EndFindKvp(handle);

            foreach (var save in saves)
            {
                SavedWeapons.Add(save, JsonConvert.DeserializeObject<List<ValidWeapon>>(GetResourceKvpString(save)));
            }

            return SavedWeapons;
        }

        /// <summary>
        /// 如果菜单不存在，则创建菜单并设置事件处理程序。
        /// </summary>
        public void CreateMenu()
        {
            menu = new Menu(Game.Player.Name, "武器管理");

            MenuController.AddSubmenu(menu, SavedLoadoutsMenu);
            MenuController.AddSubmenu(SavedLoadoutsMenu, ManageLoadoutMenu);

            var saveLoadout = new MenuItem("保存武器", "将当前武器保存到新的武器槽中。");
            var savedLoadoutsMenuBtn = new MenuItem("管理武器", "管理已保存的武器。") { Label = "→→→" };
            var enableDefaultLoadouts = new MenuCheckboxItem("重生时恢复默认武器", "如果您将某个武器设置为默认武器，那么每当您重生时，您的武器将自动装备。", WeaponLoadoutsSetLoadoutOnRespawn);

            menu.AddMenuItem(saveLoadout);
            menu.AddMenuItem(savedLoadoutsMenuBtn);
            MenuController.BindMenuItem(menu, SavedLoadoutsMenu, savedLoadoutsMenuBtn);
            if (IsAllowed(Permission.WLEquipOnRespawn))
            {
                menu.AddMenuItem(enableDefaultLoadouts);

                menu.OnCheckboxChange += (sender, checkbox, index, _checked) =>
                {
                    WeaponLoadoutsSetLoadoutOnRespawn = _checked;
                };
            }


            void RefreshSavedWeaponsMenu()
            {
                var oldCount = SavedLoadoutsMenu.Size;
                SavedLoadoutsMenu.ClearMenuItems(true);

                RefreshSavedWeaponsList();

                foreach (var sw in SavedWeapons)
                {
                    var btn = new MenuItem(sw.Key.Replace("vmenu_string_saved_weapon_loadout_", ""), "点击管理此武器。") { Label = "→→→" };
                    SavedLoadoutsMenu.AddMenuItem(btn);
                    MenuController.BindMenuItem(SavedLoadoutsMenu, ManageLoadoutMenu, btn);
                }

                if (oldCount > SavedWeapons.Count)
                {
                    SavedLoadoutsMenu.RefreshIndex();
                }
            }


            var spawnLoadout = new MenuItem("装备武器", "装备此已保存的武器。这将移除您当前的所有武器，并用此已保存的槽替换它们。");
            var renameLoadout = new MenuItem("重命名武器", "重命名此已保存的武器。");
            var cloneLoadout = new MenuItem("克隆武器", "将此已保存的武器克隆到新的槽中。");
            var setDefaultLoadout = new MenuItem("设为默认武器", "将此武器设置为您每次重生时的默认载具。这将覆盖‘Misc Settings’菜单中的‘恢复武器’选项。您可以在主武器菜单中切换此选项。");
            var replaceLoadout = new MenuItem("~r~替换武器", "~r~这将用您当前库存中的武器替换此已保存的槽。此操作不能撤销！");
            var deleteLoadout = new MenuItem("~r~删除武器", "~r~这将删除此已保存的武器。此操作不能撤销！");

            if (IsAllowed(Permission.WLEquip))
            {
                ManageLoadoutMenu.AddMenuItem(spawnLoadout);
            }

            ManageLoadoutMenu.AddMenuItem(renameLoadout);
            ManageLoadoutMenu.AddMenuItem(cloneLoadout);
            ManageLoadoutMenu.AddMenuItem(setDefaultLoadout);
            ManageLoadoutMenu.AddMenuItem(replaceLoadout);
            ManageLoadoutMenu.AddMenuItem(deleteLoadout);

            // 保存武器载具
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == saveLoadout)
                {
                    var name = await GetUserInput("输入保存名称", 30);
                    if (string.IsNullOrEmpty(name))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                    else
                    {
                        if (SavedWeapons.ContainsKey("vmenu_string_saved_weapon_loadout_" + name))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                        }
                        else
                        {
                            if (SaveWeaponLoadout("vmenu_string_saved_weapon_loadout_" + name))
                            {
                                Log("saveweapons called from menu select (save loadout button)");
                                Notify.Success($"您的武器已保存为 ~g~<C>{name}</C>~s~。");
                            }
                            else
                            {
                                Notify.Error(CommonErrors.UnknownError);
                            }
                        }
                    }
                }
            };

            // 管理装备、重命名、删除等
            ManageLoadoutMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (SavedWeapons.ContainsKey(SelectedSavedLoadoutName))
                {
                    var weapons = SavedWeapons[SelectedSavedLoadoutName];

                    if (item == spawnLoadout) // 装备
                    {
                        await SpawnWeaponLoadoutAsync(SelectedSavedLoadoutName, false, true, false);
                    }
                    else if (item == renameLoadout || item == cloneLoadout) // 重命名或克隆
                    {
                        var newName = await GetUserInput("输入保存名称", SelectedSavedLoadoutName.Replace("vmenu_string_saved_weapon_loadout_", ""), 30);
                        if (string.IsNullOrEmpty(newName))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }
                        else
                        {
                            if (SavedWeapons.ContainsKey("vmenu_string_saved_weapon_loadout_" + newName))
                            {
                                Notify.Error(CommonErrors.SaveNameAlreadyExists);
                            }
                            else
                            {
                                SetResourceKvp("vmenu_string_saved_weapon_loadout_" + newName, JsonConvert.SerializeObject(weapons));
                                Notify.Success($"您的武器载具已被 {(item == renameLoadout ? "重命名" : "克隆")} 为 ~g~<C>{newName}</C>~s~。");

                                if (item == renameLoadout)
                                {
                                    DeleteResourceKvp(SelectedSavedLoadoutName);
                                }

                                ManageLoadoutMenu.GoBack();
                            }
                        }
                    }
                    else if (item == setDefaultLoadout) // 设置为默认
                    {
                        SetResourceKvp("vmenu_string_default_loadout", SelectedSavedLoadoutName);
                        Notify.Success("这现在是您的武器载具。");
                        item.LeftIcon = MenuItem.Icon.TICK;
                    }
                    else if (item == replaceLoadout) // 替换
                    {
                        if (replaceLoadout.Label == "您确定吗？")
                        {
                            replaceLoadout.Label = "";
                            SaveWeaponLoadout(SelectedSavedLoadoutName);
                            Log("save weapons called from replace loadout");
                            Notify.Success("您的已保存武器已被当前武器替换。");
                        }
                        else
                        {
                            replaceLoadout.Label = "您确定吗？";
                        }
                    }
                    else if (item == deleteLoadout) // 删除
                    {
                        if (deleteLoadout.Label == "您确定吗？")
                        {
                            deleteLoadout.Label = "";
                            DeleteResourceKvp(SelectedSavedLoadoutName);
                            ManageLoadoutMenu.GoBack();
                            Notify.Success("您的已保存武器已被删除。");
                        }
                        else
                        {
                            deleteLoadout.Label = "您确定吗？";
                        }
                    }
                }
            };

            // 重置‘您确定吗’状态。
            ManageLoadoutMenu.OnMenuClose += (sender) =>
            {
                deleteLoadout.Label = "";
                renameLoadout.Label = "";
            };
            // 重置‘您确定吗’状态。
            ManageLoadoutMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                deleteLoadout.Label = "";
                renameLoadout.Label = "";
            };

            // 每当打开此菜单时刷新已生成的武器菜单。
            SavedLoadoutsMenu.OnMenuOpen += (sender) =>
            {
                RefreshSavedWeaponsMenu();
            };

            // 每当选择一个载具时设置当前已保存的载具。
            SavedLoadoutsMenu.OnItemSelect += (sender, item, index) =>
            {
                if (SavedWeapons.ContainsKey("vmenu_string_saved_weapon_loadout_" + item.Text))
                {
                    SelectedSavedLoadoutName = "vmenu_string_saved_weapon_loadout_" + item.Text;
                }
                else // 不应该发生，但以防万一
                {
                    ManageLoadoutMenu.GoBack();
                }
            };

            // 每当打开 ManageLoadout 菜单时重置索引。只是为了防止自动选择删除选项。
            ManageLoadoutMenu.OnMenuOpen += (sender) =>
            {
                ManageLoadoutMenu.RefreshIndex();
                var kvp = GetResourceKvpString("vmenu_string_default_loadout");
                if (string.IsNullOrEmpty(kvp) || kvp != SelectedSavedLoadoutName)
                {
                    setDefaultLoadout.LeftIcon = MenuItem.Icon.NONE;
                }
                else
                {
                    setDefaultLoadout.LeftIcon = MenuItem.Icon.TICK;
                }

            };

            // 刷新已保存的武器菜单。
            RefreshSavedWeaponsMenu();
        }

        /// <summary>
        /// 获取菜单。
        /// </summary>
        /// <returns></returns>
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
