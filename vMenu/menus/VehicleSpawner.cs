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
    public class VehicleSpawner
    {
        // 变量
        private Menu menu;
        public static Dictionary<string, uint> AddonVehicles;

        public bool SpawnInVehicle { get; private set; } = UserDefaults.VehicleSpawnerSpawnInside;
        public bool ReplaceVehicle { get; private set; } = UserDefaults.VehicleSpawnerReplacePrevious;
        public static List<bool> allowedCategories;

        private void CreateMenu()
        {
            #region 初始设置
            // 创建菜单
            menu = new Menu(Game.Player.Name, "车辆生成器");

            // 创建按钮和复选框
            var spawnByName = new MenuItem("按模型名称生成车辆", "输入一个车辆名称以生成。");
            var spawnInVeh = new MenuCheckboxItem("生成车辆时进入车内", "这将在生成车辆时将您传送到车内。", SpawnInVehicle);
            var replacePrev = new MenuCheckboxItem("替换之前的车辆", "这将在生成新车辆时自动删除您之前生成的车辆。", ReplaceVehicle);

            // 将项添加到菜单中
            if (IsAllowed(Permission.VSSpawnByName))
            {
                menu.AddMenuItem(spawnByName);
            }
            menu.AddMenuItem(spawnInVeh);
            menu.AddMenuItem(replacePrev);
            #endregion

            #region 附加车辆菜单
            // 附加车辆列表
            var addonCarsMenu = new Menu("附加车辆", "生成附加车辆");
            var addonCarsBtn = new MenuItem("附加车辆", "此服务器上可用的附加车辆列表。") { Label = "→→→" };

            menu.AddMenuItem(addonCarsBtn);

            if (IsAllowed(Permission.VSAddon))
            {
                if (AddonVehicles != null)
                {
                    if (AddonVehicles.Count > 0)
                    {
                        MenuController.BindMenuItem(menu, addonCarsMenu, addonCarsBtn);
                        MenuController.AddSubmenu(menu, addonCarsMenu);
                        var unavailableCars = new Menu("附加生成器", "不可用的车辆");
                        var unavailableCarsBtn = new MenuItem("不可用的车辆", "这些附加车辆当前无法生成，因为它们没有被正确地加载。") { Label = "→→→" };
                        MenuController.AddSubmenu(addonCarsMenu, unavailableCars);

                        for (var cat = 0; cat < 23; cat++)
                        {
                            var categoryMenu = new Menu("附加生成器", GetLabelText($"VEH_CLASS_{cat}"));
                            var categoryBtn = new MenuItem(GetLabelText($"VEH_CLASS_{cat}"), $"从 {GetLabelText($"VEH_CLASS_{cat}")} 类中生成附加车辆。") { Label = "→→→" };

                            addonCarsMenu.AddMenuItem(categoryBtn);

                            if (!allowedCategories[cat])
                            {
                                categoryBtn.Description = "此车辆类别已被服务器禁用。";
                                categoryBtn.Enabled = false;
                                categoryBtn.LeftIcon = MenuItem.Icon.LOCK;
                                categoryBtn.Label = "";
                                continue;
                            }

                            // 遍历此类别中的所有附加车辆
                            foreach (var veh in AddonVehicles.Where(v => GetVehicleClassFromName(v.Value) == cat))
                            {
                                var localizedName = GetLabelText(GetDisplayNameFromVehicleModel(veh.Value));

                                var name = localizedName != "NULL" ? localizedName : GetDisplayNameFromVehicleModel(veh.Value);
                                name = name != "CARNOTFOUND" ? name : veh.Key;

                                var carBtn = new MenuItem(name, $"点击生成 {name}.")
                                {
                                    Label = $"({veh.Key})",
                                    ItemData = veh.Key // 将模型名称存储在按钮数据中。
                                };

                                // 这应该是不可能为假，但我们还是检查一下。
                                if (IsModelInCdimage(veh.Value))
                                {
                                    categoryMenu.AddMenuItem(carBtn);
                                }
                                else
                                {
                                    carBtn.Enabled = false;
                                    carBtn.Description = "此车辆不可用。请询问服务器所有者检查车辆是否正确加载。";
                                    carBtn.LeftIcon = MenuItem.Icon.LOCK;
                                    unavailableCars.AddMenuItem(carBtn);
                                }
                            }

                            //if (AddonVehicles.Count(av => GetVehicleClassFromName(av.Value) == cat && IsModelInCdimage(av.Value)) > 0)
                            if (categoryMenu.Size > 0)
                            {
                                MenuController.AddSubmenu(addonCarsMenu, categoryMenu);
                                MenuController.BindMenuItem(addonCarsMenu, categoryMenu, categoryBtn);

                                categoryMenu.OnItemSelect += (sender, item, index) =>
                                {
                                    SpawnVehicle(item.ItemData.ToString(), SpawnInVehicle, ReplaceVehicle);
                                };
                            }
                            else
                            {
                                categoryBtn.Description = "此类别中没有附加车辆。";
                                categoryBtn.Enabled = false;
                                categoryBtn.LeftIcon = MenuItem.Icon.LOCK;
                                categoryBtn.Label = "";
                            }
                        }

                        if (unavailableCars.Size > 0)
                        {
                            addonCarsMenu.AddMenuItem(unavailableCarsBtn);
                            MenuController.BindMenuItem(addonCarsMenu, unavailableCars, unavailableCarsBtn);
                        }
                    }
                    else
                    {
                        addonCarsBtn.Enabled = false;
                        addonCarsBtn.LeftIcon = MenuItem.Icon.LOCK;
                        addonCarsBtn.Description = "此服务器上没有可用的附加车辆。";
                    }
                }
                else
                {
                    addonCarsBtn.Enabled = false;
                    addonCarsBtn.LeftIcon = MenuItem.Icon.LOCK;
                    addonCarsBtn.Description = "包含所有附加车辆的列表无法加载，请检查是否配置正确。";
                }
            }
            else
            {
                addonCarsBtn.Enabled = false;
                addonCarsBtn.LeftIcon = MenuItem.Icon.LOCK;
                addonCarsBtn.Description = "服务器所有者已限制访问此列表。";
            }
            #endregion

            // 这些是每个车辆类别的最大速度、加速度、刹车和牵引力值。
            var speedValues = new float[23]
            {
                44.9374657f,
                50.0000038f,
                48.862133f,
                48.1321335f,
                50.7077942f,
                51.3333359f,
                52.3922348f,
                53.86687f,
                52.03867f,
                49.2241631f,
                39.6176529f,
                37.5559425f,
                42.72843f,
                21.0f,
                45.0f,
                65.1952744f,
                109.764259f,
                42.72843f,
                56.5962219f,
                57.5398865f,
                43.3140678f,
                26.66667f,
                53.0537224f
            };
            var accelerationValues = new float[23]
            {
                0.34f,
                0.29f,
                0.335f,
                0.28f,
                0.395f,
                0.39f,
                0.66f,
                0.42f,
                0.425f,
                0.475f,
                0.21f,
                0.3f,
                0.32f,
                0.17f,
                18.0f,
                5.88f,
                21.0700016f,
                0.33f,
                14.0f,
                6.86f,
                0.32f,
                0.2f,
                0.76f
            };
            var brakingValues = new float[23]
            {
                0.72f,
                0.95f,
                0.85f,
                0.9f,
                1.0f,
                1.0f,
                1.3f,
                1.25f,
                1.52f,
                1.1f,
                0.6f,
                0.7f,
                0.8f,
                3.0f,
                0.4f,
                3.5920403f,
                20.58f,
                0.9f,
                2.93960738f,
                3.9472363f,
                0.85f,
                5.0f,
                1.3f
            };
            var tractionValues = new float[23]
            {
                2.3f,
                2.55f,
                2.3f,
                2.6f,
                2.625f,
                2.65f,
                2.8f,
                2.782f,
                2.9f,
                2.95f,
                2.0f,
                3.3f,
                2.175f,
                2.05f,
                0.0f,
                1.6f,
                2.15f,
                2.55f,
                2.57f,
                3.7f,
                2.05f,
                2.5f,
                3.2925f
            };

            #region 车辆类别子菜单
            // 遍历所有车辆类别
            for (var vehClass = 0; vehClass < 23; vehClass++)
            {
                // 获取类别名称
                var className = GetLabelText($"VEH_CLASS_{vehClass}");

                // 创建一个按钮和一个菜单，将菜单添加到菜单池中，并将按钮绑定到菜单
                var btn = new MenuItem(className, $"从 ~o~{className} ~s~类别中生成一辆车。")
                {
                    Label = "→→→"
                };

                var vehicleClassMenu = new Menu("车辆生成器", className);

                MenuController.AddSubmenu(menu, vehicleClassMenu);
                menu.AddMenuItem(btn);

                if (allowedCategories[vehClass])
                {
                    MenuController.BindMenuItem(menu, vehicleClassMenu, btn);
                }
                else
                {
                    btn.LeftIcon = MenuItem.Icon.LOCK;
                    btn.Description = "此类别已被服务器所有者禁用。";
                    btn.Enabled = false;
                }

                // 为重复的车辆名称（在此车辆类别中）创建一个字典
                var duplicateVehNames = new Dictionary<string, int>();

                #region 为类别添加车辆
                // 遍历车辆类别中的所有车辆
                foreach (var veh in VehicleData.Vehicles.VehicleClasses[className])
                {
                    // 将模型名称转换为首字母大写，其余字符小写。
                    var properCasedModelName = veh[0].ToString().ToUpper() + veh.ToLower().Substring(1);

                    // 获取本地化的车辆名称，如果为 "NULL"（未找到标签），则使用上面创建的 "properCasedModelName"
                    var vehName = GetVehDisplayNameFromModel(veh) != "NULL" ? GetVehDisplayNameFromModel(veh) : properCasedModelName;
                    var vehModelName = veh;
                    var model = (uint)GetHashKey(vehModelName);

                    var topSpeed = Map(GetVehicleModelEstimatedMaxSpeed(model), 0f, speedValues[vehClass], 0f, 1f);
                    var acceleration = Map(GetVehicleModelAcceleration(model), 0f, accelerationValues[vehClass], 0f, 1f);
                    var maxBraking = Map(GetVehicleModelMaxBraking(model), 0f, brakingValues[vehClass], 0f, 1f);
                    var maxTraction = Map(GetVehicleModelMaxTraction(model), 0f, tractionValues[vehClass], 0f, 1f);

                    // 遍历所有菜单项，检查每个项的标题/文本是否与当前车辆（显示）名称匹配
                    var duplicate = false;
                    for (var itemIndex = 0; itemIndex < vehicleClassMenu.Size; itemIndex++)
                    {
                        // 如果匹配...
                        if (vehicleClassMenu.GetMenuItems()[itemIndex].Text.ToString() == vehName)
                        {

                            // 检查模型是否之前已标记为重复
                            if (duplicateVehNames.Keys.Contains(vehName))
                            {
                                // 如果是，则将此模型名称的重复计数器加 1
                                duplicateVehNames[vehName]++;
                            }

                            // 如果这是第一个重复，则将其设置为 2
                            else
                            {
                                duplicateVehNames[vehName] = 2;
                            }

                            // 模型名称是重复的，所以获取模型名称并将重复数量添加到车辆名称的末尾
                            vehName += $" ({duplicateVehNames[vehName]})";

                            // 然后创建并添加一个新按钮

                            if (DoesModelExist(veh))
                            {
                                var vehBtn = new MenuItem(vehName)
                                {
                                    Enabled = true,
                                    Label = $"({vehModelName.ToLower()})",
                                    ItemData = new float[4] { topSpeed, acceleration, maxBraking, maxTraction }
                                };
                                vehicleClassMenu.AddMenuItem(vehBtn);
                            }
                            else
                            {
                                var vehBtn = new MenuItem(vehName, "由于无法在游戏文件中找到模型，此车辆不可用。如果这是一个 DLC 车辆，请确保服务器正在加载它。")
                                {
                                    Enabled = false,
                                    Label = $"({vehModelName.ToLower()})",
                                    ItemData = new float[4] { 0f, 0f, 0f, 0f }
                                };
                                vehicleClassMenu.AddMenuItem(vehBtn);
                                vehBtn.RightIcon = MenuItem.Icon.LOCK;
                            }

                            // 将重复标记为 true 并退出循环，因为我们已经找到了重复项
                            duplicate = true;
                            break;
                        }
                    }

                    // 如果不是重复项，则添加模型名称
                    if (!duplicate)
                    {
                        if (DoesModelExist(veh))
                        {
                            var vehBtn = new MenuItem(vehName)
                            {
                                Enabled = true,
                                Label = $"({vehModelName.ToLower()})",
                                ItemData = new float[4] { topSpeed, acceleration, maxBraking, maxTraction }
                            };
                            vehicleClassMenu.AddMenuItem(vehBtn);
                        }
                        else
                        {
                            var vehBtn = new MenuItem(vehName, "由于无法在游戏文件中找到模型，此车辆不可用。如果这是一个 DLC 车辆，请确保服务器正在加载它。")
                            {
                                Enabled = false,
                                Label = $"({vehModelName.ToLower()})",
                                ItemData = new float[4] { 0f, 0f, 0f, 0f }
                            };
                            vehicleClassMenu.AddMenuItem(vehBtn);
                            vehBtn.RightIcon = MenuItem.Icon.LOCK;
                        }
                    }
                }
                #endregion

                vehicleClassMenu.ShowVehicleStatsPanel = true;

                // 处理按钮按下事件
                vehicleClassMenu.OnItemSelect += async (sender2, item2, index2) =>
                {
                    await SpawnVehicle(VehicleData.Vehicles.VehicleClasses[className][index2], SpawnInVehicle, ReplaceVehicle);
                };

                static void HandleStatsPanel(Menu openedMenu, MenuItem currentItem)
                {
                    if (currentItem != null)
                    {
                        if (currentItem.ItemData is float[] data)
                        {
                            openedMenu.ShowVehicleStatsPanel = true;
                            openedMenu.SetVehicleStats(data[0], data[1], data[2], data[3]);
                            openedMenu.SetVehicleUpgradeStats(0f, 0f, 0f, 0f);
                        }
                        else
                        {
                            openedMenu.ShowVehicleStatsPanel = false;
                        }
                    }
                }

                vehicleClassMenu.OnMenuOpen += (m) =>
                {
                    HandleStatsPanel(m, m.GetCurrentMenuItem());
                };

                vehicleClassMenu.OnIndexChange += (m, oldItem, newItem, oldIndex, newIndex) =>
                {
                    HandleStatsPanel(m, newItem);
                };
            }
            #endregion

            #region 处理事件
            // 处理按钮按下事件
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnByName)
                {
                    // 传递 "custom" 作为车辆名称，将要求用户输入
                    await SpawnVehicle("custom", SpawnInVehicle, ReplaceVehicle);
                }
            };

            // 处理复选框更改
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == spawnInVeh)
                {
                    SpawnInVehicle = _checked;
                }
                else if (item == replacePrev)
                {
                    ReplaceVehicle = _checked;
                }
            };
            #endregion
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
