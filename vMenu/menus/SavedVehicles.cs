using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;

namespace vMenuClient.menus
{
    public class SavedVehicles
    {
        // 变量
        private Menu classMenu;
        private Menu savedVehicleTypeMenu;
        private readonly Menu vehicleCategoryMenu = new("类别", "管理已保存的车辆");
        private readonly Menu savedVehiclesCategoryMenu = new("类别", "在运行时更新！");
        private readonly Menu selectedVehicleMenu = new("管理车辆", "管理此已保存的车辆。");
        private readonly Menu unavailableVehiclesMenu = new("缺失的车辆", "不可用的已保存车辆");
        private Dictionary<string, VehicleInfo> savedVehicles = new();
        private readonly List<Menu> subMenus = new();
        private Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>> svMenuItems = new();
        private KeyValuePair<string, VehicleInfo> currentlySelectedVehicle = new();
        private int deleteButtonPressedCount = 0;
        private int replaceButtonPressedCount = 0;
        private SavedVehicleCategory currentCategory;

        // Need to be editable from other functions
       private readonly MenuListItem setCategoryBtn = new("设置车辆类别", new List<string> { }, 0, "设置此车辆的类别。选择以保存。");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateClassMenu()
        {
             var menuTitle = "已保存车辆";
            #region 创建菜单和子菜单
            // 创建菜单
            classMenu = new Menu(menuTitle, "管理已保存的车辆");

            for (var i = 0; i < 23; i++)
            {
                var categoryMenu = new Menu("已保存的车辆", GetLabelText($"VEH_CLASS_{i}"));

                var vehClassButton = new MenuItem(GetLabelText($"VEH_CLASS_{i}"), $"来自{GetLabelText($"VEH_CLASS_{i}")}类别的所有已保存车辆。");
                subMenus.Add(categoryMenu);
                MenuController.AddSubmenu(classMenu, categoryMenu);
                classMenu.AddMenuItem(vehClassButton);
                vehClassButton.Label = "→→→";
                MenuController.BindMenuItem(classMenu, categoryMenu, vehClassButton);

                categoryMenu.OnMenuClose += (sender) =>
                {
                    UpdateMenuAvailableCategories();
                };

                categoryMenu.OnItemSelect += (sender, item, index) =>
                {
                    UpdateSelectedVehicleMenu(item, sender);
                };
            }

            var unavailableModels = new MenuItem("缺失的已保存车辆", "这些车辆当前不可用，因为模型在游戏中不存在。这些车辆很可能没有从服务器上加载。")
            {
                Label = "→→→"
            };


            classMenu.AddMenuItem(unavailableModels);
            MenuController.BindMenuItem(classMenu, unavailableVehiclesMenu, unavailableModels);
            MenuController.AddSubmenu(classMenu, unavailableVehiclesMenu);


            MenuController.AddMenu(savedVehicleTypeMenu);
            MenuController.AddMenu(savedVehiclesCategoryMenu);
            MenuController.AddMenu(selectedVehicleMenu);

            // Load selected category
            vehicleCategoryMenu.OnItemSelect += async (sender, item, index) =>
            {
                // Create new category
                if (item.ItemData is not SavedVehicleCategory)
                {
                    var name = await GetUserInput(windowTitle: "输入类别名称。", maxInputLength: 30);
                    if (string.IsNullOrEmpty(name) || name.ToLower() == "未分类" || name.ToLower() == "创建新类别")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "输入类别描述（可选）。", maxInputLength: 120);
                        var newCategory = new SavedVehicleCategory
                        {
                            Name = name,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("saved_veh_category_" + name, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"您的类别 (~g~<C>{name}</C>~s~) 已保存。");
                            Log($"已保存类别 {name}。");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            savedVehiclesCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                        }
                        else
                        {
                            Notify.Error($"保存失败，很可能是因为此名称 (~y~<C>{name}</C>~s~) 已被使用。");
                            return;
                        }
                    }
                }
                // Select an old category
                else
                {
                    currentCategory = item.ItemData;
                }

                bool isUncategorized = currentCategory.Name == "未分类";

                savedVehiclesCategoryMenu.MenuTitle = currentCategory.Name;
                savedVehiclesCategoryMenu.MenuSubtitle = $"~s~类别: ~y~{currentCategory.Name}";
                savedVehiclesCategoryMenu.ClearMenuItems();
                var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();

                string ChangeCallback(MenuDynamicListItem item, bool left)
                {
                    int currentIndex = iconNames.IndexOf(item.CurrentItem);
                    int newIndex = left ? currentIndex - 1 : currentIndex + 1;

                    // If going past the start or end of the list
                    if (iconNames.ElementAtOrDefault(newIndex) == default)
                    {
                        if (left)
                        {
                            newIndex = iconNames.Count - 1;
                        }
                        else
                        {
                            newIndex = 0;
                        }
                    }

                    item.RightIcon = (MenuItem.Icon)newIndex;

                    return iconNames[newIndex];
                }

               var renameBtn = new MenuItem("重命名类别", "重命名此类别。")
                {
                    Enabled = !isUncategorized
                };
                var descriptionBtn = new MenuItem("更改类别描述", "更改此类别的描述。")
                {
                    Enabled = !isUncategorized
                };
                var iconBtn = new MenuDynamicListItem("更改类别图标", iconNames[(int)currentCategory.Icon], new MenuDynamicListItem.ChangeItemCallback(ChangeCallback), "更改此类别的图标。选择以保存。")
                {
                    Enabled = !isUncategorized,
                    RightIcon = currentCategory.Icon
                };
                var deleteBtn = new MenuItem("删除类别", "删除此类别。这无法撤销！")
                {
                    RightIcon = MenuItem.Icon.WARNING,
                    Enabled = !isUncategorized
                };
                var deleteCharsBtn = new MenuCheckboxItem("删除所有车辆", "如果选中，当按下 \"删除类别\" 时，此类别中的所有已保存车辆也将被删除。如果不选中，已保存的车辆将被移动到 \"未分类\"。")
                {
                    Enabled = !isUncategorized
                };

                savedVehiclesCategoryMenu.AddMenuItem(renameBtn);
                savedVehiclesCategoryMenu.AddMenuItem(descriptionBtn);
                savedVehiclesCategoryMenu.AddMenuItem(iconBtn);
                savedVehiclesCategoryMenu.AddMenuItem(deleteBtn);
                savedVehiclesCategoryMenu.AddMenuItem(deleteCharsBtn);

                var spacer = GetSpacerMenuItem("↓ 车辆 ↓");
                savedVehiclesCategoryMenu.AddMenuItem(spacer);

                if (savedVehicles.Count > 0)
                {
                    foreach (var kvp in savedVehicles)
                    {
                        string name = kvp.Key;
                        VehicleInfo vehicle = kvp.Value;

                        if (string.IsNullOrEmpty(vehicle.Category))
                        {
                            if (!isUncategorized)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (vehicle.Category != currentCategory.Name)
                            {
                                continue;
                            }
                        }

                        bool canUse = IsModelInCdimage(vehicle.model);

                        var btn = new MenuItem(name.Substring(4), canUse ? "Manage this saved vehicle." : "This model could not be found in the game files. Most likely because this is an addon vehicle and it's currently not streamed by the server.")
                        {
                            Label = $"({vehicle.name}) →→→",
                            Enabled = canUse,
                            LeftIcon = canUse ? MenuItem.Icon.NONE : MenuItem.Icon.LOCK,
                            ItemData = kvp,
                        };

                        savedVehiclesCategoryMenu.AddMenuItem(btn);
                    }
                }
            };

            savedVehiclesCategoryMenu.OnItemSelect += async (sender, item, index) =>
            {
                switch (index)
                {
                    // Rename Category
                    case 0:
                        var name = await GetUserInput(windowTitle: "Enter a new category name", defaultText: currentCategory.Name, maxInputLength: 30);

                        if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        else if (GetAllCategoryNames().Contains(name) || !string.IsNullOrEmpty(GetResourceKvpString("saved_veh_category_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                            return;
                        }

                        string oldName = currentCategory.Name;

                        currentCategory.Name = name;

                        if (StorageManager.SaveJsonData("saved_veh_category_" + name, JsonConvert.SerializeObject(currentCategory), false))
                        {
                            StorageManager.DeleteSavedStorageItem("saved_veh_category_" + oldName);

                            int totalCount = 0;
                            int updatedCount = 0;

                            if (savedVehicles.Count > 0)
                            {
                                foreach (var kvp in savedVehicles)
                                {
                                    string saveName = kvp.Key;
                                    VehicleInfo vehicle = kvp.Value;

                                    if (string.IsNullOrEmpty(vehicle.Category))
                                    {
                                        continue;
                                    }

                                    if (vehicle.Category != oldName)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    vehicle.Category = name;

                                    if (StorageManager.SaveVehicleInfo(saveName, vehicle, true))
                                    {
                                        updatedCount++;
                                        Log($"更新了 \"{saveName}\" 的类别");
                                    }
                                    else
                                    {
                                        Log($"更新 \"{saveName}\" 的类别时出错");
                                    }
                                }
                            }

                            Notify.Success($"您的类别已重命名为 ~g~<C>{name}</C>~s~。 {updatedCount}/{totalCount} 车辆已更新。");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("重命名类别时出错，您的旧类别不会被删除。");
                        }
                        break;

                    // Change Category Description
                    case 1:
                        var description = await GetUserInput(windowTitle: "Enter a new category description", defaultText: currentCategory.Description, maxInputLength: 120);

                        currentCategory.Description = description;

                        if (StorageManager.SaveJsonData("saved_veh_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                        {
                            Notify.Success("您的类别描述已更改。");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("保存类别描述时出错。");
                        }
                        break;

                    // Delete Category
                    case 3:
                        if (item.Label == "你确定吗?")
                        {
                            bool deleteVehicles = (sender.GetMenuItems().ElementAt(4) as MenuCheckboxItem).Checked;

                            item.Label = "";
                            DeleteResourceKvp("saved_veh_category_" + currentCategory.Name);

                            int totalCount = 0;
                            int updatedCount = 0;

                            if (savedVehicles.Count > 0)
                            {
                                foreach (var kvp in savedVehicles)
                                {
                                    string saveName = kvp.Key;
                                    VehicleInfo vehicle = kvp.Value;

                                    if (string.IsNullOrEmpty(vehicle.Category))
                                    {
                                        continue;
                                    }

                                    if (vehicle.Category != currentCategory.Name)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    if (deleteVehicles)
                                    {
                                        updatedCount++;

                                        DeleteResourceKvp(saveName);
                                    }
                                    else
                                    {
                                        vehicle.Category = "未分类";

                                        if (StorageManager.SaveVehicleInfo(saveName, vehicle, true))
                                        {
                                            updatedCount++;
                                            Log($"更新了 \"{saveName}\" 的分类");
                                        }
                                        else
                                        {
                                            Log($"更新 \"{saveName}\" 分类时出错");
                                        }
                                    }

                                }
                            }

                            Notify.Success($"您的保存的分类已被删除。{updatedCount}/{totalCount} 辆车辆 {(deleteVehicles ? "已删除" : "已更新")}。");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();

                        }
                        else
                        {
                            item.Label = "你确定吗?";
                        }
                        break;

                    // Load saved vehicle menu
                    default:
                        List<string> categoryNames = GetAllCategoryNames();
                        List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
                        int nameIndex = categoryNames.IndexOf(currentCategory.Name);

                        setCategoryBtn.ItemData = categoryIcons;
                        setCategoryBtn.ListItems = categoryNames;
                        setCategoryBtn.ListIndex = nameIndex == 1 ? 0 : nameIndex;
                        setCategoryBtn.RightIcon = categoryIcons[setCategoryBtn.ListIndex];

                        var vehInfo = item.ItemData;
                        selectedVehicleMenu.MenuSubtitle = $"{vehInfo.Key.Substring(4)} ({vehInfo.Value.name})";
                        currentlySelectedVehicle = vehInfo;
                        MenuController.CloseAllMenus();
                        selectedVehicleMenu.OpenMenu();
                        MenuController.AddSubmenu(savedVehiclesCategoryMenu, selectedVehicleMenu);
                        break;
                }
            };

            // Change Category Icon
            savedVehiclesCategoryMenu.OnDynamicListItemSelect += (_, _, currentItem) =>
            {
            var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();
            int iconIndex = iconNames.IndexOf(currentItem);

            currentCategory.Icon = (MenuItem.Icon)iconIndex;

            if (StorageManager.SaveJsonData("saved_veh_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
            {
                Notify.Success($"您的分类图标已更改为 ~g~<C>{iconNames[iconIndex]}</C>~s~。");
                UpdateSavedVehicleCategoriesMenu();
            }
            else
            {
                Notify.Error("更改分类图标时出错。");
            }
        };


            var spawnVehicle = new MenuItem("生成车辆", "生成此保存的车辆。");
            var renameVehicle = new MenuItem("重命名车辆", "重命名您的保存车辆。");
            var replaceVehicle = new MenuItem("~r~替换车辆", "您的保存车辆将被您当前坐在的车辆替换。~r~警告：这无法撤销！");
            var deleteVehicle = new MenuItem("~r~删除车辆", "~r~这将删除您的保存车辆。警告：这无法撤销！");
            selectedVehicleMenu.AddMenuItem(spawnVehicle);
            selectedVehicleMenu.AddMenuItem(renameVehicle);
            selectedVehicleMenu.AddMenuItem(setCategoryBtn);
            selectedVehicleMenu.AddMenuItem(replaceVehicle);
            selectedVehicleMenu.AddMenuItem(deleteVehicle);

            selectedVehicleMenu.OnMenuOpen += (sender) =>
            {
                spawnVehicle.Label = "(" + GetDisplayNameFromVehicleModel(currentlySelectedVehicle.Value.model).ToLower() + ")";
            };

            selectedVehicleMenu.OnMenuClose += (sender) =>
            {
                selectedVehicleMenu.RefreshIndex();
                deleteButtonPressedCount = 0;
                deleteVehicle.Label = "";
                replaceButtonPressedCount = 0;
                replaceVehicle.Label = "";
            };

            selectedVehicleMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnVehicle)
                {
                    if (MainMenu.VehicleSpawnerMenu != null)
                    {
                        await SpawnVehicle(currentlySelectedVehicle.Value.model, MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                    else
                    {
                        await SpawnVehicle(currentlySelectedVehicle.Value.model, true, true, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                }
                else if (item == renameVehicle)
                {
                    var newName = await GetUserInput(windowTitle: "请输入此车辆的新名称。", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                    else
                    {
                        if (StorageManager.SaveVehicleInfo("veh_" + newName, currentlySelectedVehicle.Value, false))
                        {
                            DeleteResourceKvp(currentlySelectedVehicle.Key);
                            while (!selectedVehicleMenu.Visible)
                            {
                                await BaseScript.Delay(0);
                            }
                            Notify.Success("您的车辆已成功重命名。");
                            UpdateMenuAvailableCategories();
                            selectedVehicleMenu.GoBack();
                            currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>(); // 清除旧信息
                        }
                        else
                        {
                            Notify.Error("此名称已被使用或发生未知错误。如果您认为有问题，请联系服务器管理员。");
                        }
                    }
                }
                else if (item == replaceVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        if (replaceButtonPressedCount == 0)
                        {
                            replaceButtonPressedCount = 1;
                            item.Label = "再次按下以确认。";
                            Notify.Alert("您确定要替换此车辆吗？再次按下按钮以确认。");
                        }
                        else
                        {
                            replaceButtonPressedCount = 0;
                            item.Label = "";
                            SaveVehicle(currentlySelectedVehicle.Key.Substring(4), currentlySelectedVehicle.Value.Category);
                            selectedVehicleMenu.GoBack();
                            Notify.Success("您的保存车辆已被当前车辆替换。");
                        }
                    }
                    else
                    {
                        Notify.Error("您需要在车辆中才能替换旧车辆。");
                    }

                }
                else if (item == deleteVehicle)
                {
                    if (deleteButtonPressedCount == 0)
                    {
                        deleteButtonPressedCount = 1;
                        item.Label = "再次按下以确认。";
                        Notify.Alert("您确定要删除此车辆吗？再次按下按钮以确认。");
                    }
                    else
                    {
                        deleteButtonPressedCount = 0;
                        item.Label = "";
                        DeleteResourceKvp(currentlySelectedVehicle.Key);
                        UpdateMenuAvailableCategories();
                        selectedVehicleMenu.GoBack();
                        Notify.Success("您的保存车辆已被删除。");
                    }
                }

                if (item != deleteVehicle) // if any other button is pressed, restore the delete vehicle button pressed count.
                {
                    deleteButtonPressedCount = 0;
                    deleteVehicle.Label = "";
                }
                if (item != replaceVehicle)
                {
                    replaceButtonPressedCount = 0;
                    replaceVehicle.Label = "";
                }
            };

            // Update category preview icon
            selectedVehicleMenu.OnListIndexChange += (_, listItem, _, newSelectionIndex, _) => listItem.RightIcon = listItem.ItemData[newSelectionIndex];

            // Update vehicle's category
            selectedVehicleMenu.OnListItemSelect += async (_, listItem, listIndex, _) =>
            {
                string name = listItem.ListItems[listIndex];

                if (name == "Create New")
                {
                    var newName = await GetUserInput(windowTitle: "请输入分类名称。", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName) || newName.ToLower() == "未分类" || newName.ToLower() == "创建新分类")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "请输入分类描述（可选）。", maxInputLength: 120);
                        var newCategory = new SavedVehicleCategory
                        {
                            Name = newName,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("saved_veh_category_" + newName, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"您的分类 (~g~<C>{newName}</C>~s~) 已保存。");
                            Log($"保存了分类 {newName}。");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            savedVehiclesCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                            name = newName;
                        }
                        else
                        {
                            Notify.Error($"保存失败，可能是因为此名称 (~y~<C>{newName}</C>~s~) 已被使用。");
                            return;
                        }

                    }
                }

                VehicleInfo vehicle = currentlySelectedVehicle.Value;

                vehicle.Category = name;

                if (StorageManager.SaveVehicleInfo(currentlySelectedVehicle.Key, vehicle, true))
                {
                    Notify.Success("您的车辆已成功保存。");
                }
                else
                {
                    Notify.Error("您的车辆无法保存。原因未知。 :(");
                }

                MenuController.CloseAllMenus();
                UpdateSavedVehicleCategoriesMenu();
                vehicleCategoryMenu.OpenMenu();
            };

            unavailableVehiclesMenu.InstructionalButtons.Add(Control.FrontendDelete, "删除车辆！！！");

            unavailableVehiclesMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.FrontendDelete, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>((m, c) =>
            {
                if (m.Size > 0)
                {
                    var index = m.CurrentIndex;
                    if (index < m.Size)
                    {
                        var item = m.GetMenuItems().Find(i => i.Index == index);
                        if (item != null && item.ItemData is KeyValuePair<string, VehicleInfo> sd)
                        {
                           if (item.Label == "~r~确定吗？")
                            {
                                Log("不可用的保存车辆已删除，数据: " + JsonConvert.SerializeObject(sd));
                                DeleteResourceKvp(sd.Key);
                                unavailableVehiclesMenu.GoBack();
                                UpdateMenuAvailableCategories();
                            }
                            else
                            {
                                item.Label = "~r~确定吗？";
                            }
                            }
                            else
                            {
                                Notify.Error("这个车辆怎么找不到了。");
                            }
                            }
                            else
                            {
                                Notify.Error("您不小心触发了删除一个不存在的菜单项，这怎么可能...？");
                            }
                            }
                            else
                            {
                                Notify.Error("目前没有不可用的车辆可以删除！");
                            }

            }), true));

            void ResetAreYouSure()
            {
                foreach (var i in unavailableVehiclesMenu.GetMenuItems())
                {
                    if (i.ItemData is KeyValuePair<string, VehicleInfo> vd)
                    {
                        i.Label = $"({vd.Value.name})";
                    }
                }
            }
            unavailableVehiclesMenu.OnMenuClose += (sender) => ResetAreYouSure();
            unavailableVehiclesMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) => ResetAreYouSure();

            #endregion
        }

        private void CreateTypeMenu()
        {
            savedVehicleTypeMenu = new("保存的车辆", "从类别或自定义分类中选择");

            var saveVehicle = new MenuItem("保存当前车辆", "保存您当前坐在的车辆。")
            {
                LeftIcon = MenuItem.Icon.CAR
            };
            var classButton = new MenuItem("车辆类别", "按车辆类别选择保存的车辆。")
            {
                Label = "→→→"
            };
            var categoryButton = new MenuItem("车辆分类", "按自定义分类选择保存的车辆。")
            {
                Label = "→→→"
            };

            savedVehicleTypeMenu.AddMenuItem(saveVehicle);
            savedVehicleTypeMenu.AddMenuItem(classButton);
            savedVehicleTypeMenu.AddMenuItem(categoryButton);

            savedVehicleTypeMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == saveVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        SaveVehicle();
                    }
                    else
                    {
                        Notify.Error("您当前不在任何车辆中。请先进入车辆后再尝试保存。");
                    }

                }
                else if (item == classButton)
                {
                    UpdateMenuAvailableCategories();
                }
                else if (item == categoryButton)
                {
                    UpdateSavedVehicleCategoriesMenu();
                }
            };

            MenuController.BindMenuItem(savedVehicleTypeMenu, GetClassMenu(), classButton);
            MenuController.BindMenuItem(savedVehicleTypeMenu, vehicleCategoryMenu, categoryButton);
        }

        /// <summary>
        /// Updates the selected vehicle.
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns>A bool, true if successfull, false if unsuccessfull</returns>
        private bool UpdateSelectedVehicleMenu(MenuItem selectedItem, Menu parentMenu = null)
        {
            if (!svMenuItems.ContainsKey(selectedItem))
            {
                Notify.Error("以某种非常奇怪的方式，您选择了一个在此列表中不存在的按钮。因此，您的车辆无法加载。 :( 也许您的保存文件已损坏？");
                return false;
            }
            var vehInfo = svMenuItems[selectedItem];
            selectedVehicleMenu.MenuSubtitle = $"{vehInfo.Key.Substring(4)} ({vehInfo.Value.name})";
            currentlySelectedVehicle = vehInfo;
            MenuController.CloseAllMenus();
            selectedVehicleMenu.OpenMenu();
            if (parentMenu != null)
            {
                MenuController.AddSubmenu(parentMenu, selectedVehicleMenu);
            }
            return true;
        }


        /// <summary>
        /// Updates the available vehicle category list.
        /// </summary>
        public void UpdateMenuAvailableCategories()
        {
            savedVehicles = GetSavedVehicles();
            svMenuItems = new Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>>();

            for (var i = 0; i < GetClassMenu().Size - 1; i++)
            {
                if (savedVehicles.Any(a => GetVehicleClassFromName(a.Value.model) == i && IsModelInCdimage(a.Value.model)))
                {
                    GetClassMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.NONE;
                    GetClassMenu().GetMenuItems()[i].Label = "→→→";
                    GetClassMenu().GetMenuItems()[i].Enabled = true;
                    GetClassMenu().GetMenuItems()[i].Description = $"来自 {GetClassMenu().GetMenuItems()[i].Text} 类别的所有保存车辆。";
                }
                else
                {
                    GetClassMenu().GetMenuItems()[i].Label = "";
                    GetClassMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.LOCK;
                    GetClassMenu().GetMenuItems()[i].Enabled = false;
                    GetClassMenu().GetMenuItems()[i].Description = $"您没有任何保存的车辆属于 {GetClassMenu().GetMenuItems()[i].Text} 类别。";
                }
            }

            // Check if the items count will be changed. If there are less cars than there were before, one probably got deleted
            // so in that case we need to refresh the index of that menu just to be safe. If not, keep the index where it is for improved
            // usability of the menu.
            foreach (var m in subMenus)
            {
                var size = m.Size;
                var vclass = subMenus.IndexOf(m);

                var count = savedVehicles.Count(a => GetVehicleClassFromName(a.Value.model) == vclass);
                if (count < size)
                {
                    m.RefreshIndex();
                }
            }

            foreach (var m in subMenus)
            {
                // Clear items but don't reset the index because we can guarantee that the index won't be out of bounds.
                // this is the case because of the loop above where we reset the index if the items count changes.
                m.ClearMenuItems(true);
            }

            // Always clear this index because it's useless anyway and it's safer.
            unavailableVehiclesMenu.ClearMenuItems();

            foreach (var sv in savedVehicles)
            {
                if (IsModelInCdimage(sv.Value.model))
                {
                    var vclass = GetVehicleClassFromName(sv.Value.model);
                    var menu = subMenus[vclass];

                    var savedVehicleBtn = new MenuItem(sv.Key.Substring(4), $"管理此保存车辆。")
                    {
                        Label = $"({sv.Value.name}) →→→"
                    };
                    menu.AddMenuItem(savedVehicleBtn);

                    svMenuItems.Add(savedVehicleBtn, sv);
                }
                else
                {
                    var missingVehItem = new MenuItem(sv.Key.Substring(4), "此模型在游戏文件中找不到。很可能是因为这是一个附加车辆，服务器当前没有加载它。")
                    {
                        Label = "(" + sv.Value.name + ")",
                        Enabled = false,
                        LeftIcon = MenuItem.Icon.LOCK,
                        ItemData = sv
                    };
                    //SetResourceKvp(sv.Key + "_tmp_dupe", JsonConvert.SerializeObject(sv.Value));
                    unavailableVehiclesMenu.AddMenuItem(missingVehItem);
                }
            }
            foreach (var m in subMenus)
            {
                m.SortMenuItems((A, B) =>
                {
                    return A.Text.ToLower().CompareTo(B.Text.ToLower());
                });
            }
        }

        /// <summary>
        /// Updates the saved vehicle categories menu.
        /// </summary>
        private void UpdateSavedVehicleCategoriesMenu()
        {
            savedVehicles = GetSavedVehicles();

            var categories = GetAllCategoryNames();

            vehicleCategoryMenu.ClearMenuItems();

            var createCategoryBtn = new MenuItem("创建分类", "创建一个新的车辆分类。")
            {
                Label = "→→→"
            };
            vehicleCategoryMenu.AddMenuItem(createCategoryBtn);

            var spacer = GetSpacerMenuItem("↓ 车辆分类 ↓");
            vehicleCategoryMenu.AddMenuItem(spacer);

            var uncategorized = new SavedVehicleCategory
            {
                Name = "未分类",
                Description = "所有未分配到任何分类的保存车辆。"
            };
            var uncategorizedBtn = new MenuItem(uncategorized.Name, uncategorized.Description)
            {
                Label = "→→→",
                ItemData = uncategorized
            };
            vehicleCategoryMenu.AddMenuItem(uncategorizedBtn);
            MenuController.BindMenuItem(vehicleCategoryMenu, savedVehiclesCategoryMenu, uncategorizedBtn);

            // Remove "Create New" and "Uncategorized"
            categories.RemoveRange(0, 2);

            if (categories.Count > 0)
            {
                categories.Sort((a, b) => a.ToLower().CompareTo(b.ToLower()));
                foreach (var item in categories)
                {
                    SavedVehicleCategory category = StorageManager.GetSavedVehicleCategoryData("saved_veh_category_" + item);

                    var btn = new MenuItem(category.Name, category.Description)
                    {
                        Label = "→→→",
                        LeftIcon = category.Icon,
                        ItemData = category
                    };
                    vehicleCategoryMenu.AddMenuItem(btn);
                    MenuController.BindMenuItem(vehicleCategoryMenu, savedVehiclesCategoryMenu, btn);
                }
            }

            vehicleCategoryMenu.RefreshIndex();
        }

        private List<string> GetAllCategoryNames()
        {
            var categories = new List<string>();
            var handle = StartFindKvp("saved_veh_category_");
            while (true)
            {
                var foundCategory = FindKvp(handle);
                if (string.IsNullOrEmpty(foundCategory))
                {
                    break;
                }
                else
                {
                    categories.Add(foundCategory.Substring(19));
                }
            }
            EndFindKvp(handle);

            categories.Insert(0, "创建新分类");
            categories.Insert(1, "未分类");

            return categories;
        }

        private List<MenuItem.Icon> GetCategoryIcons(List<string> categoryNames)
        {
            List<MenuItem.Icon> icons = new List<MenuItem.Icon> { };

            foreach (var name in categoryNames)
            {
                icons.Add(StorageManager.GetSavedVehicleCategoryData("saved_veh_category_" + name).Icon);
            }

            return icons;
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetClassMenu()
        {
            if (classMenu == null)
            {
                CreateClassMenu();
            }
            return classMenu;
        }

        public Menu GetTypeMenu()
        {
            if (savedVehicleTypeMenu == null)
            {
                CreateTypeMenu();
            }
            return savedVehicleTypeMenu;
        }

        public struct SavedVehicleCategory
        {
            public string Name;
            public string Description;
            public MenuItem.Icon Icon;
        }
    }
}
