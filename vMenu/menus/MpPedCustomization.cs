using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuClient.MpPedDataManager;

namespace vMenuClient.menus
{
    public class MpPedCustomization
    {
        // Variables
        private Menu menu;
    public Menu createCharacterMenu = new("创建角色", "创建一个新角色");
    public Menu savedCharactersMenu = new("vMenu", "管理已保存的角色");
    public Menu savedCharactersCategoryMenu = new("分类", "我在运行时更新！");
    public Menu inheritanceMenu = new("vMenu", "角色继承选项");
    public Menu appearanceMenu = new("vMenu", "角色外观选项");
    public Menu faceShapeMenu = new("vMenu", "角色面部形状选项");
    public Menu tattoosMenu = new("vMenu", "角色纹身选项");
    public Menu clothesMenu = new("vMenu", "角色服装选项");
    public Menu propsMenu = new("vMenu", "角色道具选项");
    private readonly Menu manageSavedCharacterMenu = new("vMenu", "管理MP角色");

        // Need to be able to disable/enable these buttons from another class.
        internal MenuItem createMaleBtn = new("创建男性角色", "创建一个新的男性角色。") { Label = "→→→" };
        internal MenuItem createFemaleBtn = new("创建女性角色", "创建一个新的女性角色。") { Label = "→→→" };
        internal MenuItem editPedBtn = new("编辑已保存的角色", "这允许你编辑已保存角色的所有内容。保存按钮被按下后，变化将保存到该角色的保存文件条目中。");
        // Need to be editable from other functions
       private readonly MenuListItem setCategoryBtn = new("设置角色分类", new List<string> { }, 0, "设置此角色的分类。选择以保存。");
    private readonly MenuListItem categoryBtn = new("角色分类", new List<string> { }, 0, "设置此角色的分类。");

        public static bool DontCloseMenus { get { return MenuController.PreventExitingMenu; } set { MenuController.PreventExitingMenu = value; } }
        public static bool DisableBackButton { get { return MenuController.DisableBackButton; } set { MenuController.DisableBackButton = value; } }
        string selectedSavedCharacterManageName = "";
        private bool isEdidtingPed = false;
        private readonly List<string> facial_expressions = new() { "mood_Normal_1", "mood_Happy_1", "mood_Angry_1", "mood_Aiming_1", "mood_Injured_1", "mood_stressed_1", "mood_smug_1", "mood_sulk_1", };

        private MultiplayerPedData currentCharacter = new();
        private MpCharacterCategory currentCategory = new();



        /// <summary>
        /// Makes or updates the character creator menu. Also has an option to load data from the <see cref="currentCharacter"/> data, to allow for editing an existing ped.
        /// </summary>
        /// <param name="male"></param>
        /// <param name="editPed"></param>
        private void MakeCreateCharacterMenu(bool male, bool editPed = false)
        {
            isEdidtingPed = editPed;
            if (!editPed)
            {
                currentCharacter = new MultiplayerPedData();
                currentCharacter.DrawableVariations.clothes = new Dictionary<int, KeyValuePair<int, int>>();
                currentCharacter.PropVariations.props = new Dictionary<int, KeyValuePair<int, int>>();
                currentCharacter.PedHeadBlendData = Game.PlayerPed.GetHeadBlendData();
                currentCharacter.Version = 1;
                currentCharacter.ModelHash = male ? (uint)GetHashKey("mp_m_freemode_01") : (uint)GetHashKey("mp_f_freemode_01");
                currentCharacter.IsMale = male;

                SetPedComponentVariation(Game.PlayerPed.Handle, 3, 15, 0, 0);
                SetPedComponentVariation(Game.PlayerPed.Handle, 8, 15, 0, 0);
                SetPedComponentVariation(Game.PlayerPed.Handle, 11, 15, 0, 0);
            }
            currentCharacter.DrawableVariations.clothes ??= new Dictionary<int, KeyValuePair<int, int>>();
            currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();

            // Set the facial expression to default in case it doesn't exist yet, or keep the current one if it does.
            currentCharacter.FacialExpression ??= facial_expressions[0];

            // Set the facial expression on the ped itself.
            SetFacialIdleAnimOverride(Game.PlayerPed.Handle, currentCharacter.FacialExpression ?? facial_expressions[0], null);

            // Set the facial expression item list to the correct saved index.
            if (createCharacterMenu.GetMenuItems().ElementAt(6) is MenuListItem li)
            {
                var index = facial_expressions.IndexOf(currentCharacter.FacialExpression ?? facial_expressions[0]);
                if (index < 0)
                {
                    index = 0;
                }
                li.ListIndex = index;
            }

            appearanceMenu.ClearMenuItems();
            tattoosMenu.ClearMenuItems();
            clothesMenu.ClearMenuItems();
            propsMenu.ClearMenuItems();

            #region appearance menu.
            var opacity = new List<string>() { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };

            var overlayColorsList = new List<string>();
            for (var i = 0; i < GetNumHairColors(); i++)
            {
                overlayColorsList.Add($"颜色 #{i + 1}");
            }

            var maxHairStyles = GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2);
            //if (currentCharacter.ModelHash == (uint)PedHash.FreemodeFemale01)
            //{
            //    maxHairStyles /= 2;
            //}
            var hairStylesList = new List<string>();
             for (var i = 0; i < maxHairStyles; i++)
            {
                hairStylesList.Add($"发型 #{i + 1}");
            }
            hairStylesList.Add($"发型 #{maxHairStyles + 1}");

            var blemishesStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(0); i++)
            {
                blemishesStyleList.Add($"风格 #{i + 1}");
            }

            var beardStylesList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(1); i++)
            {
                beardStylesList.Add($"风格 #{i + 1}");
            }

            var eyebrowsStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(2); i++)
            {
                eyebrowsStyleList.Add($"风格 #{i + 1}");
            }

            var ageingStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(3); i++)
            {
                ageingStyleList.Add($"风格 #{i + 1}");
            }

            var makeupStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(4); i++)
            {
                makeupStyleList.Add($"风格 #{i + 1}");
            }

            var blushStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(5); i++)
            {
                blushStyleList.Add($"风格 #{i + 1}");
            }

            var complexionStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(6); i++)
            {
                complexionStyleList.Add($"风格 #{i + 1}");
            }

            var sunDamageStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(7); i++)
            {
                sunDamageStyleList.Add($"风格 #{i + 1}");
            }

            var lipstickStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(8); i++)
            {
                lipstickStyleList.Add($"风格 #{i + 1}");
            }

            var molesFrecklesStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(9); i++)
            {
                molesFrecklesStyleList.Add($"风格 #{i + 1}");
            }

            var chestHairStyleList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(10); i++)
            {
                chestHairStyleList.Add($"风格 #{i + 1}");
            }

            var bodyBlemishesList = new List<string>();
            for (var i = 0; i < GetNumHeadOverlayValues(11); i++)
            {
                bodyBlemishesList.Add($"风格 #{i + 1}");
            }

            var eyeColorList = new List<string>();
            for (var i = 0; i < 32; i++)
            {
                eyeColorList.Add($"眼睛颜色 #{i + 1}");
            }

            /*

            0               Blemishes             0 - 23,   255  
            1               Facial Hair           0 - 28,   255  
            2               Eyebrows              0 - 33,   255  
            3               Ageing                0 - 14,   255  
            4               Makeup                0 - 74,   255  
            5               Blush                 0 - 6,    255  
            6               Complexion            0 - 11,   255  
            7               Sun Damage            0 - 10,   255  
            8               Lipstick              0 - 9,    255  
            9               Moles/Freckles        0 - 17,   255  
            10              Chest Hair            0 - 16,   255  
            11              Body Blemishes        0 - 11,   255  
            12              Add Body Blemishes    0 - 1,    255  
            
            */


            // hair
            var currentHairStyle = editPed ? currentCharacter.PedAppearance.hairStyle : GetPedDrawableVariation(Game.PlayerPed.Handle, 2);
            var currentHairColor = editPed ? currentCharacter.PedAppearance.hairColor : 0;
            var currentHairHighlightColor = editPed ? currentCharacter.PedAppearance.hairHighlightColor : 0;

            // 0 blemishes
            var currentBlemishesStyle = editPed ? currentCharacter.PedAppearance.blemishesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 0) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 0) : 0;
            var currentBlemishesOpacity = editPed ? currentCharacter.PedAppearance.blemishesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, currentBlemishesStyle, currentBlemishesOpacity);

            // 1 beard
            var currentBeardStyle = editPed ? currentCharacter.PedAppearance.beardStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 1) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 1) : 0;
            var currentBeardOpacity = editPed ? currentCharacter.PedAppearance.beardOpacity : 0f;
            var currentBeardColor = editPed ? currentCharacter.PedAppearance.beardColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, currentBeardStyle, currentBeardOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, currentBeardColor, currentBeardColor);

            // 2 eyebrows
            var currentEyebrowStyle = editPed ? currentCharacter.PedAppearance.eyebrowsStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 2) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 2) : 0;
            var currentEyebrowOpacity = editPed ? currentCharacter.PedAppearance.eyebrowsOpacity : 0f;
            var currentEyebrowColor = editPed ? currentCharacter.PedAppearance.eyebrowsColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, currentEyebrowStyle, currentEyebrowOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, currentEyebrowColor, currentEyebrowColor);

            // 3 ageing
            var currentAgeingStyle = editPed ? currentCharacter.PedAppearance.ageingStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 3) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 3) : 0;
            var currentAgeingOpacity = editPed ? currentCharacter.PedAppearance.ageingOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, currentAgeingStyle, currentAgeingOpacity);

            // 4 makeup
            var currentMakeupStyle = editPed ? currentCharacter.PedAppearance.makeupStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 4) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 4) : 0;
            var currentMakeupOpacity = editPed ? currentCharacter.PedAppearance.makeupOpacity : 0f;
            var currentMakeupColor = editPed ? currentCharacter.PedAppearance.makeupColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, currentMakeupStyle, currentMakeupOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, currentMakeupColor, currentMakeupColor);

            // 5 blush
            var currentBlushStyle = editPed ? currentCharacter.PedAppearance.blushStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 5) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 5) : 0;
            var currentBlushOpacity = editPed ? currentCharacter.PedAppearance.blushOpacity : 0f;
            var currentBlushColor = editPed ? currentCharacter.PedAppearance.blushColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, currentBlushStyle, currentBlushOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, currentBlushColor, currentBlushColor);

            // 6 complexion
            var currentComplexionStyle = editPed ? currentCharacter.PedAppearance.complexionStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 6) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 6) : 0;
            var currentComplexionOpacity = editPed ? currentCharacter.PedAppearance.complexionOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, currentComplexionStyle, currentComplexionOpacity);

            // 7 sun damage
            var currentSunDamageStyle = editPed ? currentCharacter.PedAppearance.sunDamageStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 7) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 7) : 0;
            var currentSunDamageOpacity = editPed ? currentCharacter.PedAppearance.sunDamageOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, currentSunDamageStyle, currentSunDamageOpacity);

            // 8 lipstick
            var currentLipstickStyle = editPed ? currentCharacter.PedAppearance.lipstickStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 8) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 8) : 0;
            var currentLipstickOpacity = editPed ? currentCharacter.PedAppearance.lipstickOpacity : 0f;
            var currentLipstickColor = editPed ? currentCharacter.PedAppearance.lipstickColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, currentLipstickStyle, currentLipstickOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, currentLipstickColor, currentLipstickColor);

            // 9 moles/freckles
            var currentMolesFrecklesStyle = editPed ? currentCharacter.PedAppearance.molesFrecklesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 9) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 9) : 0;
            var currentMolesFrecklesOpacity = editPed ? currentCharacter.PedAppearance.molesFrecklesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, currentMolesFrecklesStyle, currentMolesFrecklesOpacity);

            // 10 chest hair
            var currentChesthairStyle = editPed ? currentCharacter.PedAppearance.chestHairStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 10) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 10) : 0;
            var currentChesthairOpacity = editPed ? currentCharacter.PedAppearance.chestHairOpacity : 0f;
            var currentChesthairColor = editPed ? currentCharacter.PedAppearance.chestHairColor : 0;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, currentChesthairStyle, currentChesthairOpacity);
            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, currentChesthairColor, currentChesthairColor);

            // 11 body blemishes
            var currentBodyBlemishesStyle = editPed ? currentCharacter.PedAppearance.bodyBlemishesStyle : GetPedHeadOverlayValue(Game.PlayerPed.Handle, 11) != 255 ? GetPedHeadOverlayValue(Game.PlayerPed.Handle, 11) : 0;
            var currentBodyBlemishesOpacity = editPed ? currentCharacter.PedAppearance.bodyBlemishesOpacity : 0f;
            SetPedHeadOverlay(Game.PlayerPed.Handle, 11, currentBodyBlemishesStyle, currentBodyBlemishesOpacity);

            var currentEyeColor = editPed ? currentCharacter.PedAppearance.eyeColor : 0;
            SetPedEyeColor(Game.PlayerPed.Handle, currentEyeColor);

            // 发型
            var hairStyles = new MenuListItem("发型", hairStylesList, currentHairStyle, "选择一个发型。");
            var hairColors = new MenuListItem("发色", overlayColorsList, currentHairColor, "选择一个发色。") 
            { 
                ShowColorPanel = true, 
                ColorPanelColorType = MenuListItem.ColorPanelType.Hair 
            };
            var hairHighlightColors = new MenuListItem("高光", overlayColorsList, currentHairHighlightColor, "选择一个高光。") 
            { 
                ShowColorPanel = true, 
                ColorPanelColorType = MenuListItem.ColorPanelType.Hair 
            };

            // 痘痕
            var blemishesStyle = new MenuListItem("痘痕样式", blemishesStyleList, currentBlemishesStyle, "选择一个痘痕样式。");
            var blemishesOpacity = new MenuListItem("痘痕透明度", opacity, (int)(currentBlemishesOpacity * 10f), "选择痘痕的透明度。") 
            { 
                ShowOpacityPanel = true 
            };

            // 胡须
            var beardStyles = new MenuListItem("胡须样式", beardStylesList, currentBeardStyle, "选择一个胡须/面部毛发样式。");
            var beardOpacity = new MenuListItem("胡须透明度", opacity, (int)(currentBeardOpacity * 10f), "选择胡须/面部毛发的透明度。") 
            { 
                ShowOpacityPanel = true 
            };
            var beardColor = new MenuListItem("胡须颜色", overlayColorsList, currentBeardColor, "选择一个胡须颜色。") 
            { 
                ShowColorPanel = true, 
                ColorPanelColorType = MenuListItem.ColorPanelType.Hair 
            };

            // 眉毛
            var eyebrowStyle = new MenuListItem("眉毛样式", eyebrowsStyleList, currentEyebrowStyle, "选择一个眉毛样式。");
            var eyebrowOpacity = new MenuListItem("眉毛透明度", opacity, (int)(currentEyebrowOpacity * 10f), "选择眉毛的透明度。") 
            { 
                ShowOpacityPanel = true 
            };
            var eyebrowColor = new MenuListItem("眉毛颜色", overlayColorsList, currentEyebrowColor, "选择一个眉毛颜色。") 
            { 
                ShowColorPanel = true, 
                ColorPanelColorType = MenuListItem.ColorPanelType.Hair 
            };

            // 老化
            var ageingStyle = new MenuListItem("老化样式", ageingStyleList, currentAgeingStyle, "选择一个老化样式。");
            var ageingOpacity = new MenuListItem("老化透明度", opacity, (int)(currentAgeingOpacity * 10f), "选择老化的透明度。") 
            { 
                ShowOpacityPanel = true 
            };

            // 化妆
            var makeupStyle = new MenuListItem("化妆样式", makeupStyleList, currentMakeupStyle, "选择一个化妆样式。");
            var makeupOpacity = new MenuListItem("化妆透明度", opacity, (int)(currentMakeupOpacity * 10f), "选择化妆的透明度。") 
            { 
                ShowOpacityPanel = true 
            };
            var makeupColor = new MenuListItem("化妆颜色", overlayColorsList, currentMakeupColor, "选择一个化妆颜色。") 
            { 
                ShowColorPanel = true, 
                ColorPanelColorType = MenuListItem.ColorPanelType.Makeup 
            };

            // 腮红
            var blushStyle = new MenuListItem("腮红样式", blushStyleList, currentBlushStyle, "选择一个腮红样式。");
            var blushOpacity = new MenuListItem("腮红透明度", opacity, (int)(currentBlushOpacity * 10f), "选择腮红的透明度。") 
            { 
                ShowOpacityPanel = true 
            };
            var blushColor = new MenuListItem("腮红颜色", overlayColorsList, currentBlushColor, "选择一个腮红颜色。") 
            { 
                ShowColorPanel = true, 
                ColorPanelColorType = MenuListItem.ColorPanelType.Makeup 
            };

            // 肤色
            var complexionStyle = new MenuListItem("肤色样式", complexionStyleList, currentComplexionStyle, "选择一个肤色样式。");
            var complexionOpacity = new MenuListItem("肤色透明度", opacity, (int)(currentComplexionOpacity * 10f), "选择肤色的透明度。") 
            { 
                ShowOpacityPanel = true 
            };

            // 日晒损伤
            var sunDamageStyle = new MenuListItem("日晒损伤样式", sunDamageStyleList, currentSunDamageStyle, "选择一个日晒损伤样式。");
            var sunDamageOpacity = new MenuListItem("日晒损伤透明度", opacity, (int)(currentSunDamageOpacity * 10f), "选择日晒损伤的透明度。") 
            { 
                ShowOpacityPanel = true 
            };

            // 唇膏
            var lipstickStyle = new MenuListItem("口红样式", lipstickStyleList, currentLipstickStyle, "选择一个口红样式。");
            var lipstickOpacity = new MenuListItem("口红透明度", opacity, (int)(currentLipstickOpacity * 10f), "选择口红的透明度。") 
            { 
                ShowOpacityPanel = true 
            };
            var lipstickColor = new MenuListItem("口红颜色", overlayColorsList, currentLipstickColor, "选择一个口红颜色。") 
            { 
                ShowColorPanel = true, 
                ColorPanelColorType = MenuListItem.ColorPanelType.Makeup 
            };

            // 痣和雀斑
            var molesFrecklesStyle = new MenuListItem("痣和雀斑样式", molesFrecklesStyleList, currentMolesFrecklesStyle, "选择一个痣和雀斑样式。");
            var molesFrecklesOpacity = new MenuListItem("痣和雀斑透明度", opacity, (int)(currentMolesFrecklesOpacity * 10f), "选择痣和雀斑的透明度。") 
            { 
                ShowOpacityPanel = true 
            };

            // 胸毛
            var chestHairStyle = new MenuListItem("胸毛样式", chestHairStyleList, currentChesthairStyle, "选择一个胸毛样式。");
            var chestHairOpacity = new MenuListItem("胸毛透明度", opacity, (int)(currentChesthairOpacity * 10f), "选择胸毛的透明度。") 
            { 
                ShowOpacityPanel = true 
            };
            var chestHairColor = new MenuListItem("胸毛颜色", overlayColorsList, currentChesthairColor, "选择一个胸毛颜色。") 
            { 
                ShowColorPanel = true, 
                ColorPanelColorType = MenuListItem.ColorPanelType.Hair 
            };

            // 身体瑕疵
            var bodyBlemishesStyle = new MenuListItem("身体瑕疵样式", bodyBlemishesList, currentBodyBlemishesStyle, "选择身体瑕疵样式。");
            var bodyBlemishesOpacity = new MenuListItem("身体瑕疵透明度", opacity, (int)(currentBodyBlemishesOpacity * 10f), "选择身体瑕疵的透明度。") 
            { 
                ShowOpacityPanel = true 
            };

            // 眼睛颜色
            var eyeColor = new MenuListItem("眼睛颜色", eyeColorList, currentEyeColor, "选择一个眼睛/隐形眼镜颜色。");

            appearanceMenu.AddMenuItem(hairStyles);
            appearanceMenu.AddMenuItem(hairColors);
            appearanceMenu.AddMenuItem(hairHighlightColors);

            appearanceMenu.AddMenuItem(blemishesStyle);
            appearanceMenu.AddMenuItem(blemishesOpacity);

            appearanceMenu.AddMenuItem(beardStyles);
            appearanceMenu.AddMenuItem(beardOpacity);
            appearanceMenu.AddMenuItem(beardColor);

            appearanceMenu.AddMenuItem(eyebrowStyle);
            appearanceMenu.AddMenuItem(eyebrowOpacity);
            appearanceMenu.AddMenuItem(eyebrowColor);

            appearanceMenu.AddMenuItem(ageingStyle);
            appearanceMenu.AddMenuItem(ageingOpacity);

            appearanceMenu.AddMenuItem(makeupStyle);
            appearanceMenu.AddMenuItem(makeupOpacity);
            appearanceMenu.AddMenuItem(makeupColor);

            appearanceMenu.AddMenuItem(blushStyle);
            appearanceMenu.AddMenuItem(blushOpacity);
            appearanceMenu.AddMenuItem(blushColor);

            appearanceMenu.AddMenuItem(complexionStyle);
            appearanceMenu.AddMenuItem(complexionOpacity);

            appearanceMenu.AddMenuItem(sunDamageStyle);
            appearanceMenu.AddMenuItem(sunDamageOpacity);

            appearanceMenu.AddMenuItem(lipstickStyle);
            appearanceMenu.AddMenuItem(lipstickOpacity);
            appearanceMenu.AddMenuItem(lipstickColor);

            appearanceMenu.AddMenuItem(molesFrecklesStyle);
            appearanceMenu.AddMenuItem(molesFrecklesOpacity);

            appearanceMenu.AddMenuItem(chestHairStyle);
            appearanceMenu.AddMenuItem(chestHairOpacity);
            appearanceMenu.AddMenuItem(chestHairColor);

            appearanceMenu.AddMenuItem(bodyBlemishesStyle);
            appearanceMenu.AddMenuItem(bodyBlemishesOpacity);

            appearanceMenu.AddMenuItem(eyeColor);

            if (male)
            {
                // There are weird people out there that wanted makeup for male characters
                // so yeah.... here you go I suppose... strange...

                /*
                makeupStyle.Enabled = false;
                makeupStyle.LeftIcon = MenuItem.Icon.LOCK;
                makeupStyle.Description = "This is not available for male characters.";

                makeupOpacity.Enabled = false;
                makeupOpacity.LeftIcon = MenuItem.Icon.LOCK;
                makeupOpacity.Description = "This is not available for male characters.";

                makeupColor.Enabled = false;
                makeupColor.LeftIcon = MenuItem.Icon.LOCK;
                makeupColor.Description = "This is not available for male characters.";


                blushStyle.Enabled = false;
                blushStyle.LeftIcon = MenuItem.Icon.LOCK;
                blushStyle.Description = "This is not available for male characters.";

                blushOpacity.Enabled = false;
                blushOpacity.LeftIcon = MenuItem.Icon.LOCK;
                blushOpacity.Description = "This is not available for male characters.";

                blushColor.Enabled = false;
                blushColor.LeftIcon = MenuItem.Icon.LOCK;
                blushColor.Description = "This is not available for male characters.";


                lipstickStyle.Enabled = false;
                lipstickStyle.LeftIcon = MenuItem.Icon.LOCK;
                lipstickStyle.Description = "This is not available for male characters.";

                lipstickOpacity.Enabled = false;
                lipstickOpacity.LeftIcon = MenuItem.Icon.LOCK;
                lipstickOpacity.Description = "This is not available for male characters.";

                lipstickColor.Enabled = false;
                lipstickColor.LeftIcon = MenuItem.Icon.LOCK;
                lipstickColor.Description = "This is not available for male characters.";
                */
            }
            else
            {
               // 禁用胡须样式
                beardStyles.Enabled = false;
                beardStyles.LeftIcon = MenuItem.Icon.LOCK;
                beardStyles.Description = "女性角色无法使用此选项。";

                // 禁用胡须透明度
                beardOpacity.Enabled = false;
                beardOpacity.LeftIcon = MenuItem.Icon.LOCK;
                beardOpacity.Description = "女性角色无法使用此选项。";

                // 禁用胡须颜色
                beardColor.Enabled = false;
                beardColor.LeftIcon = MenuItem.Icon.LOCK;
                beardColor.Description = "女性角色无法使用此选项。";

                // 禁用胸毛样式
                chestHairStyle.Enabled = false;
                chestHairStyle.LeftIcon = MenuItem.Icon.LOCK;
                chestHairStyle.Description = "女性角色无法使用此选项。";

                // 禁用胸毛透明度
                chestHairOpacity.Enabled = false;
                chestHairOpacity.LeftIcon = MenuItem.Icon.LOCK;
                chestHairOpacity.Description = "女性角色无法使用此选项。";

                // 禁用胸毛颜色
                chestHairColor.Enabled = false;
                chestHairColor.LeftIcon = MenuItem.Icon.LOCK;
                chestHairColor.Description = "女性角色无法使用此选项。";
            }

            #endregion

            #region clothing options menu
            var clothingCategoryNames = new string[12] { 
                "头部",           // Unused (head)
                "面具",                     // Masks
                "头发",           // Unused (hair)
                "上身服装",                 // Upper Body
                "下身服装",                 // Lower Body
                "背包与降落伞",             // Bags & Parachutes
                "鞋子",                     // Shoes
                "围巾与项链",               // Scarfs & Chains
                "衬衫与配件",               // Shirt & Accessory
                "护甲与配件2",             // Body Armor & Accessory 2
                "徽章与标志",               // Badges & Logos
                "衬衫覆盖物与夹克"          // Shirt Overlay & Jackets
                };
            for (var i = 0; i < 12; i++)
            {
                if (i is not 0 and not 2)
                {
                    var currentVariationIndex = editPed && currentCharacter.DrawableVariations.clothes.ContainsKey(i) ? currentCharacter.DrawableVariations.clothes[i].Key : GetPedDrawableVariation(Game.PlayerPed.Handle, i);
                    var currentVariationTextureIndex = editPed && currentCharacter.DrawableVariations.clothes.ContainsKey(i) ? currentCharacter.DrawableVariations.clothes[i].Value : GetPedTextureVariation(Game.PlayerPed.Handle, i);

                    var maxDrawables = GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, i);

                    var items = new List<string>();
                    for (var x = 0; x < maxDrawables; x++)
                    {
                        items.Add($"可选的 #{x} (of {maxDrawables})");
                    }

                    var maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, i, currentVariationIndex);

                    var listItem = new MenuListItem(clothingCategoryNames[i], items, currentVariationIndex, $"使用箭头键选择一个服装变体，然后按 ~o~回车~s~ 切换所有可用的纹理。当前选中的纹理：#{currentVariationTextureIndex + 1} (共 {maxTextures})。");
                    clothesMenu.AddMenuItem(listItem);
                }
            }
            #endregion

            #region props options menu
            var propNames = new string[5] {  "帽子和头盔", "眼镜", "杂项道具", "手表", "手链" };
            for (var x = 0; x < 5; x++)
            {
                var propId = x;
                if (x > 2)
                {
                    propId += 3;
                }

                var currentProp = editPed && currentCharacter.PropVariations.props.ContainsKey(propId) ? currentCharacter.PropVariations.props[propId].Key : GetPedPropIndex(Game.PlayerPed.Handle, propId);
                var currentPropTexture = editPed && currentCharacter.PropVariations.props.ContainsKey(propId) ? currentCharacter.PropVariations.props[propId].Value : GetPedPropTextureIndex(Game.PlayerPed.Handle, propId);

                var propsList = new List<string>();
                for (var i = 0; i < GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propId); i++)
                {
                    propsList.Add($"道具 #{i} (of {GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propId)})");
                }
                propsList.Add("无道具");


                if (GetPedPropIndex(Game.PlayerPed.Handle, propId) != -1)
                {
                    var maxPropTextures = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propId, currentProp);
                    var propListItem = new MenuListItem($"{propNames[x]}", propsList, currentProp, $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{currentPropTexture + 1} (of {maxPropTextures}).");
                    propsMenu.AddMenuItem(propListItem);
                }
                else
                {
                    var propListItem = new MenuListItem($"{propNames[x]}", propsList, currentProp, "Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.");
                    propsMenu.AddMenuItem(propListItem);
                }


            }
            #endregion

            #region face features menu
            foreach (MenuSliderItem item in faceShapeMenu.GetMenuItems())
            {
                if (editPed)
                {
                    if (currentCharacter.FaceShapeFeatures.features == null)
                    {
                        currentCharacter.FaceShapeFeatures.features = new Dictionary<int, float>();
                    }
                    else
                    {
                        if (currentCharacter.FaceShapeFeatures.features.ContainsKey(item.Index))
                        {
                            item.Position = (int)(currentCharacter.FaceShapeFeatures.features[item.Index] * 10f) + 10;
                            SetPedFaceFeature(Game.PlayerPed.Handle, item.Index, currentCharacter.FaceShapeFeatures.features[item.Index]);
                        }
                        else
                        {
                            item.Position = 10;
                            SetPedFaceFeature(Game.PlayerPed.Handle, item.Index, 0f);
                        }
                    }
                }
                else
                {
                    item.Position = 10;
                    SetPedFaceFeature(Game.PlayerPed.Handle, item.Index, 0f);
                }
            }
            #endregion

            #region Tattoos menu
            var headTattoosList = new List<string>();
            var torsoTattoosList = new List<string>();
            var leftArmTattoosList = new List<string>();
            var rightArmTattoosList = new List<string>();
            var leftLegTattoosList = new List<string>();
            var rightLegTattoosList = new List<string>();
            var badgeTattoosList = new List<string>();

            TattoosData.GenerateTattoosData();
            if (male)
            {
                var counter = 1;
                foreach (var tattoo in MaleTattoosCollection.HEAD)
                {
                    headTattoosList.Add($"纹身 #{counter} (共 {MaleTattoosCollection.HEAD.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.TORSO)
                {
                    torsoTattoosList.Add($"纹身 #{counter} (共 {MaleTattoosCollection.TORSO.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.LEFT_ARM)
                {
                    leftArmTattoosList.Add($"纹身 #{counter} (共 {MaleTattoosCollection.LEFT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.RIGHT_ARM)
                {
                    rightArmTattoosList.Add($"纹身 #{counter} (共 {MaleTattoosCollection.RIGHT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.LEFT_LEG)
                {
                    leftLegTattoosList.Add($"纹身 #{counter} (共 {MaleTattoosCollection.LEFT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.RIGHT_LEG)
                {
                    rightLegTattoosList.Add($"纹身 #{counter} (共 {MaleTattoosCollection.RIGHT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.BADGES)
                {
                    badgeTattoosList.Add($"徽章 #{counter} (共 {MaleTattoosCollection.BADGES.Count})");
                    counter++;
                }
            }
            else
            {
                var counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.HEAD)
                {
                    headTattoosList.Add($"纹身 #{counter} (共 {FemaleTattoosCollection.HEAD.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.TORSO)
                {
                    torsoTattoosList.Add($"纹身 #{counter} (共 {FemaleTattoosCollection.TORSO.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.LEFT_ARM)
                {
                    leftArmTattoosList.Add($"纹身 #{counter} (共 {FemaleTattoosCollection.LEFT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.RIGHT_ARM)
                {
                    rightArmTattoosList.Add($"纹身 #{counter} (共 {FemaleTattoosCollection.RIGHT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.LEFT_LEG)
                {
                    leftLegTattoosList.Add($"纹身 #{counter} (共 {FemaleTattoosCollection.LEFT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.RIGHT_LEG)
                {
                    rightLegTattoosList.Add($"纹身 #{counter} (共 {FemaleTattoosCollection.RIGHT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.BADGES)
                {
                    badgeTattoosList.Add($"徽章 #{counter} (共 {FemaleTattoosCollection.BADGES.Count})");
                    counter++;
                }
            }

            const string tatDesc = "浏览列表以预览纹身。如果你喜欢其中一个，按回车键选择它。选择纹身会将其添加到你的角色身上，如果你已经有了该纹身，则该纹身将被移除。";
            var headTatts = new MenuListItem("头部纹身", headTattoosList, 0, tatDesc);
            var torsoTatts = new MenuListItem("躯干纹身", torsoTattoosList, 0, tatDesc);
            var leftArmTatts = new MenuListItem("左臂纹身", leftArmTattoosList, 0, tatDesc);
            var rightArmTatts = new MenuListItem("右臂纹身", rightArmTattoosList, 0, tatDesc);
            var leftLegTatts = new MenuListItem("左腿纹身", leftLegTattoosList, 0, tatDesc);
            var rightLegTatts = new MenuListItem("右腿纹身", rightLegTattoosList, 0, tatDesc);
            var badgeTatts = new MenuListItem("徽章覆盖", badgeTattoosList, 0, tatDesc);

            tattoosMenu.AddMenuItem(headTatts);
            tattoosMenu.AddMenuItem(torsoTatts);
            tattoosMenu.AddMenuItem(leftArmTatts);
            tattoosMenu.AddMenuItem(rightArmTatts);
            tattoosMenu.AddMenuItem(leftLegTatts);
            tattoosMenu.AddMenuItem(rightLegTatts);
            tattoosMenu.AddMenuItem(badgeTatts);
            tattoosMenu.AddMenuItem(new MenuItem("移除所有纹身", "点击此处以移除所有纹身并重新开始。"));
            #endregion

            List<string> categoryNames = GetAllCategoryNames();

            categoryNames.RemoveAt(0);

            List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);

            categoryBtn.ItemData = new Tuple<List<string>, List<MenuItem.Icon>>(categoryNames, categoryIcons);
            categoryBtn.ListItems = categoryNames;
            categoryBtn.ListIndex = 0;
            categoryBtn.RightIcon = categoryIcons[categoryBtn.ListIndex];

            createCharacterMenu.RefreshIndex();
            appearanceMenu.RefreshIndex();
            inheritanceMenu.RefreshIndex();
            tattoosMenu.RefreshIndex();
        }

        /// <summary>
        /// Saves the mp character and quits the editor if successful.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SavePed()
        {
            currentCharacter.PedHeadBlendData = Game.PlayerPed.GetHeadBlendData();
            if (isEdidtingPed)
            {
                var json = JsonConvert.SerializeObject(currentCharacter);
                if (StorageManager.SaveJsonData(currentCharacter.SaveName, json, true))
                {
                    Notify.Success("您的角色已成功保存。");
                    return true;
                }
                else
                {
                    Notify.Error("您的角色无法保存。原因未知。:(");
                    return false;
                }
            }
            else
            {
                var name = await GetUserInput(windowTitle: "请输入保存名称。", maxInputLength: 30);
                if (string.IsNullOrEmpty(name))
                {
                    Notify.Error(CommonErrors.InvalidInput);
                    return false;
                }
                else
                {
                    currentCharacter.SaveName = "mp_ped_" + name;
                    var json = JsonConvert.SerializeObject(currentCharacter);

                    if (StorageManager.SaveJsonData("mp_ped_" + name, json, false))
                    {
                        Notify.Success($"您的角色 (~g~<C>{name}</C>~s~) 已被保存。");
                        Log($"保存角色 {name}. 数据: {json}");
                        return true;
                    }
                    else
                    {
                        Notify.Error($"保存失败，可能是因为该名称 (~y~<C>{name}</C>~s~) 已经被使用。");
                        return false;
                    }
                }
            }

        }

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
             menu = new Menu(Game.Player.Name, "MP 角色定制");

            var savedCharacters = new MenuItem("已保存角色", "生成、编辑或删除您现有的游戏角色。")
            {
                Label = "→→→"
            };

            MenuController.AddMenu(createCharacterMenu);
            MenuController.AddMenu(savedCharactersMenu);
            MenuController.AddMenu(savedCharactersCategoryMenu);
            MenuController.AddMenu(inheritanceMenu);
            MenuController.AddMenu(appearanceMenu);
            MenuController.AddMenu(faceShapeMenu);
            MenuController.AddMenu(tattoosMenu);
            MenuController.AddMenu(clothesMenu);
            MenuController.AddMenu(propsMenu);

            CreateSavedPedsMenu();

            menu.AddMenuItem(createMaleBtn);
            MenuController.BindMenuItem(menu, createCharacterMenu, createMaleBtn);
            menu.AddMenuItem(createFemaleBtn);
            MenuController.BindMenuItem(menu, createCharacterMenu, createFemaleBtn);
            menu.AddMenuItem(savedCharacters);
            MenuController.BindMenuItem(menu, savedCharactersMenu, savedCharacters);

            menu.RefreshIndex();

            createCharacterMenu.InstructionalButtons.Add(Control.MoveLeftRight, "旋转头部");
            inheritanceMenu.InstructionalButtons.Add(Control.MoveLeftRight, "旋转头部");
            appearanceMenu.InstructionalButtons.Add(Control.MoveLeftRight, "旋转头部");
            faceShapeMenu.InstructionalButtons.Add(Control.MoveLeftRight, "旋转头部");
            tattoosMenu.InstructionalButtons.Add(Control.MoveLeftRight, "旋转头部");
            clothesMenu.InstructionalButtons.Add(Control.MoveLeftRight, "旋转头部");
            propsMenu.InstructionalButtons.Add(Control.MoveLeftRight, "旋转头部");

            createCharacterMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "旋转角色");
            inheritanceMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "旋转角色");
            appearanceMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "旋转角色");
            faceShapeMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "旋转角色");
            tattoosMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "旋转角色");
            clothesMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "旋转角色");
            propsMenu.InstructionalButtons.Add(Control.PhoneExtraOption, "旋转角色");

            createCharacterMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "右转相机");
            inheritanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "右转相机");
            appearanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "右转相机");
            faceShapeMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "右转相机");
            tattoosMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "右转相机");
            clothesMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "右转相机");
            propsMenu.InstructionalButtons.Add(Control.ParachuteBrakeRight, "右转相机");

            createCharacterMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "左转相机");
            inheritanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "左转相机");
            appearanceMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "左转相机");
            faceShapeMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "左转相机");
            tattoosMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "左转相机");
            clothesMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "左转相机");
            propsMenu.InstructionalButtons.Add(Control.ParachuteBrakeLeft, "左转相机");

            var inheritanceButton = new MenuItem("角色继承", "角色继承选项。");
            var appearanceButton = new MenuItem("角色外观", "角色外观选项。");
            var faceButton = new MenuItem("角色面部形状选项", "角色面部形状选项。");
            var tattoosButton = new MenuItem("角色纹身选项", "角色纹身选项。");
            var clothesButton = new MenuItem("角色衣物", "角色衣物。");
            var propsButton = new MenuItem("角色道具", "角色道具。");
            var saveButton = new MenuItem("保存角色", "保存您的角色。");
            var exitNoSave = new MenuItem("退出不保存", "您确定吗？所有未保存的工作将会丢失。");
            var faceExpressionList = new MenuListItem("面部表情", new List<string> { "正常", "快乐", "生气", "瞄准", "受伤", "紧张", "得意", "闷闷不乐" }, 0, "设置一个面部表情，角色在闲置时将使用该表情。");

            inheritanceButton.Label = "→→→";
            appearanceButton.Label = "→→→";
            faceButton.Label = "→→→";
            tattoosButton.Label = "→→→";
            clothesButton.Label = "→→→";
            propsButton.Label = "→→→";

            createCharacterMenu.AddMenuItem(inheritanceButton);
            createCharacterMenu.AddMenuItem(appearanceButton);
            createCharacterMenu.AddMenuItem(faceButton);
            createCharacterMenu.AddMenuItem(tattoosButton);
            createCharacterMenu.AddMenuItem(clothesButton);
            createCharacterMenu.AddMenuItem(propsButton);
            createCharacterMenu.AddMenuItem(faceExpressionList);
            createCharacterMenu.AddMenuItem(categoryBtn);
            createCharacterMenu.AddMenuItem(saveButton);
            createCharacterMenu.AddMenuItem(exitNoSave);

            MenuController.BindMenuItem(createCharacterMenu, inheritanceMenu, inheritanceButton);
            MenuController.BindMenuItem(createCharacterMenu, appearanceMenu, appearanceButton);
            MenuController.BindMenuItem(createCharacterMenu, faceShapeMenu, faceButton);
            MenuController.BindMenuItem(createCharacterMenu, tattoosMenu, tattoosButton);
            MenuController.BindMenuItem(createCharacterMenu, clothesMenu, clothesButton);
            MenuController.BindMenuItem(createCharacterMenu, propsMenu, propsButton);

            #region inheritance
            var dads = new Dictionary<string, int>();
            var moms = new Dictionary<string, int>();

            void AddInheritance(Dictionary<string, int> dict, int listId, string textPrefix)
            {
                var baseIdx = dict.Count;
                var basePed = GetPedHeadBlendFirstIndex(listId);

                // list 0/2 are male, list 1/3 are female
                var suffix = $" ({(listId % 2 == 0 ? "男" : "女")})";

                for (var i = 0; i < GetNumParentPedsOfType(listId); i++)
                {
                    // get the actual parent name, or the index if none
                    var label = GetLabelText($"{textPrefix}{i}");
                    if (string.IsNullOrWhiteSpace(label) || label == "NULL")
                    {
                        label = $"{baseIdx + i}";
                    }

                    // append the gender of the list
                    label += suffix;
                    dict[label] = basePed + i;
                }
            }

            int GetInheritance(Dictionary<string, int> list, MenuListItem listItem)
            {
                if (listItem.ListIndex < listItem.ListItems.Count)
                {
                    if (list.TryGetValue(listItem.ListItems[listItem.ListIndex], out var idx))
                    {
                        return idx;
                    }
                }

                return 0;
            }

            var listIdx = 0;
            foreach (var list in new[] { dads, moms })
            {
                void AddDads()
                {
                    AddInheritance(list, 0, "Male_");
                    AddInheritance(list, 2, "Special_Male_");
                }

                void AddMoms()
                {
                    AddInheritance(list, 1, "Female_");
                    AddInheritance(list, 3, "Special_Female_");
                }

                if (listIdx == 0)
                {
                    AddDads();
                    AddMoms();
                }
                else
                {
                    AddMoms();
                    AddDads();
                }

                listIdx++;
            }

            var inheritanceDads = new MenuListItem("父亲", dads.Keys.ToList(), 0, "选择一个父亲。");
            var inheritanceMoms = new MenuListItem("母亲", moms.Keys.ToList(), 0, "选择一个母亲。");
            var mixValues = new List<float>() { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
            var inheritanceShapeMix = new MenuSliderItem("头部形状混合", "选择您头部形状中从父亲或母亲那里继承的比例。最左边是您的父亲，最右边是您的母亲。", 0, 10, 5, true) { SliderLeftIcon = MenuItem.Icon.MALE, SliderRightIcon = MenuItem.Icon.FEMALE };
            var inheritanceSkinMix = new MenuSliderItem("身体肤色混合", "选择您身体肤色中从父亲或母亲那里继承的比例。最左边是您的父亲，最右边是您的母亲。", 0, 10, 5, true) { SliderLeftIcon = MenuItem.Icon.MALE, SliderRightIcon = MenuItem.Icon.FEMALE };

            inheritanceMenu.AddMenuItem(inheritanceDads);
            inheritanceMenu.AddMenuItem(inheritanceMoms);
            inheritanceMenu.AddMenuItem(inheritanceShapeMix);
            inheritanceMenu.AddMenuItem(inheritanceSkinMix);

            // formula from maintransition.#sc
            float GetMinimum()
            {
                return currentCharacter.IsMale ? 0.05f : 0.3f;
            }

            float GetMaximum()
            {
                return currentCharacter.IsMale ? 0.7f : 0.95f;
            }

            float ClampMix(int value)
            {
                var sliderFraction = mixValues[value];
                var min = GetMinimum();
                var max = GetMaximum();

                return min + (sliderFraction * (max - min));
            }

            int UnclampMix(float value)
            {
                var min = GetMinimum();
                var max = GetMaximum();

                var origFraction = (value - min) / (max - min);
                return Math.Max(Math.Min((int)(origFraction * 10), 10), 0);
            }

            void SetHeadBlend()
            {
                SetPedHeadBlendData(Game.PlayerPed.Handle, GetInheritance(dads, inheritanceDads), GetInheritance(moms, inheritanceMoms), 0, GetInheritance(dads, inheritanceDads), GetInheritance(moms, inheritanceMoms), 0, ClampMix(inheritanceShapeMix.Position), ClampMix(inheritanceSkinMix.Position), 0f, true);
            }

            inheritanceMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, itemIndex) =>
            {
                SetHeadBlend();
            };

            inheritanceMenu.OnSliderPositionChange += (sender, item, oldPosition, newPosition, itemIndex) =>
            {
                SetHeadBlend();
            };
            #endregion

            #region appearance
            var hairOverlays = new Dictionary<int, KeyValuePair<string, string>>()
            {
                { 0, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
                { 1, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 2, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 3, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_003_a") },
                { 4, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 5, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 6, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 7, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 8, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_008_a") },
                { 9, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 10, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 11, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 12, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 13, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 14, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
                { 15, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
                { 16, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 17, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
                { 18, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_000_a") },
                { 19, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_001_a") },
                { 20, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_000_a") },
                { 21, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_001_a") },
                { 22, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
            };

            // manage the list changes for appearance items.
            appearanceMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, itemIndex) =>
            {
                if (itemIndex == 0) // hair style
                {
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    currentCharacter.PedAppearance.HairOverlay = new KeyValuePair<string, string>("", "");

                    if (newSelectionIndex >= GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2))
                    {
                        SetPedComponentVariation(Game.PlayerPed.Handle, 2, 0, 0, 0);
                        currentCharacter.PedAppearance.hairStyle = 0;
                    }
                    else
                    {
                        SetPedComponentVariation(Game.PlayerPed.Handle, 2, newSelectionIndex, 0, 0);
                        currentCharacter.PedAppearance.hairStyle = newSelectionIndex;
                        if (hairOverlays.ContainsKey(newSelectionIndex))
                        {
                            SetPedFacialDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(hairOverlays[newSelectionIndex].Key), (uint)GetHashKey(hairOverlays[newSelectionIndex].Value));
                            currentCharacter.PedAppearance.HairOverlay = new KeyValuePair<string, string>(hairOverlays[newSelectionIndex].Key, hairOverlays[newSelectionIndex].Value);
                        }
                    }
                }
                else if (itemIndex is 1 or 2) // hair colors
                {
                    var tmp = (MenuListItem)_menu.GetMenuItems()[1];
                    var hairColor = tmp.ListIndex;
                    tmp = (MenuListItem)_menu.GetMenuItems()[2];
                    var hairHighlightColor = tmp.ListIndex;

                    SetPedHairColor(Game.PlayerPed.Handle, hairColor, hairHighlightColor);

                    currentCharacter.PedAppearance.hairColor = hairColor;
                    currentCharacter.PedAppearance.hairHighlightColor = hairHighlightColor;
                }
                else if (itemIndex == 33) // eye color
                {
                    var selection = ((MenuListItem)_menu.GetMenuItems()[itemIndex]).ListIndex;
                    SetPedEyeColor(Game.PlayerPed.Handle, selection);
                    currentCharacter.PedAppearance.eyeColor = selection;
                }
                else
                {
                    var selection = ((MenuListItem)_menu.GetMenuItems()[itemIndex]).ListIndex;
                    var opacity = 0f;
                    if (_menu.GetMenuItems()[itemIndex + 1] is MenuListItem item2)
                    {
                        opacity = (((float)item2.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex - 1] is MenuListItem item1)
                    {
                        opacity = (((float)item1.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex] is MenuListItem item)
                    {
                        opacity = (((float)item.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else
                    {
                        opacity = 1f;
                    }

                    switch (itemIndex)
                    {
                        case 3: // blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, selection, opacity);
                            currentCharacter.PedAppearance.blemishesStyle = selection;
                            currentCharacter.PedAppearance.blemishesOpacity = opacity;
                            break;
                        case 5: // beards
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, selection, opacity);
                            currentCharacter.PedAppearance.beardStyle = selection;
                            currentCharacter.PedAppearance.beardOpacity = opacity;
                            break;
                        case 7: // beards color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, selection, selection);
                            currentCharacter.PedAppearance.beardColor = selection;
                            break;
                        case 8: // eyebrows
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, selection, opacity);
                            currentCharacter.PedAppearance.eyebrowsStyle = selection;
                            currentCharacter.PedAppearance.eyebrowsOpacity = opacity;
                            break;
                        case 10: // eyebrows color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, selection, selection);
                            currentCharacter.PedAppearance.eyebrowsColor = selection;
                            break;
                        case 11: // ageing
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, selection, opacity);
                            currentCharacter.PedAppearance.ageingStyle = selection;
                            currentCharacter.PedAppearance.ageingOpacity = opacity;
                            break;
                        case 13: // makeup
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, selection, opacity);
                            currentCharacter.PedAppearance.makeupStyle = selection;
                            currentCharacter.PedAppearance.makeupOpacity = opacity;
                            break;
                        case 15: // makeup color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, selection, selection);
                            currentCharacter.PedAppearance.makeupColor = selection;
                            break;
                        case 16: // blush style
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, selection, opacity);
                            currentCharacter.PedAppearance.blushStyle = selection;
                            currentCharacter.PedAppearance.blushOpacity = opacity;
                            break;
                        case 18: // blush color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, selection, selection);
                            currentCharacter.PedAppearance.blushColor = selection;
                            break;
                        case 19: // complexion
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, selection, opacity);
                            currentCharacter.PedAppearance.complexionStyle = selection;
                            currentCharacter.PedAppearance.complexionOpacity = opacity;
                            break;
                        case 21: // sun damage
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, selection, opacity);
                            currentCharacter.PedAppearance.sunDamageStyle = selection;
                            currentCharacter.PedAppearance.sunDamageOpacity = opacity;
                            break;
                        case 23: // lipstick
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, selection, opacity);
                            currentCharacter.PedAppearance.lipstickStyle = selection;
                            currentCharacter.PedAppearance.lipstickOpacity = opacity;
                            break;
                        case 25: // lipstick color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, selection, selection);
                            currentCharacter.PedAppearance.lipstickColor = selection;
                            break;
                        case 26: // moles and freckles
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, selection, opacity);
                            currentCharacter.PedAppearance.molesFrecklesStyle = selection;
                            currentCharacter.PedAppearance.molesFrecklesOpacity = opacity;
                            break;
                        case 28: // chest hair
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, selection, opacity);
                            currentCharacter.PedAppearance.chestHairStyle = selection;
                            currentCharacter.PedAppearance.chestHairOpacity = opacity;
                            break;
                        case 30: // chest hair color
                            SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, selection, selection);
                            currentCharacter.PedAppearance.chestHairColor = selection;
                            break;
                        case 31: // body blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 11, selection, opacity);
                            currentCharacter.PedAppearance.bodyBlemishesStyle = selection;
                            currentCharacter.PedAppearance.bodyBlemishesOpacity = opacity;
                            break;
                    }
                }
            };

            // manage the slider changes for opacity on the appearance items.
            appearanceMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, itemIndex) =>
            {
                if (itemIndex is > 2 and < 33)
                {

                    var selection = ((MenuListItem)_menu.GetMenuItems()[itemIndex - 1]).ListIndex;
                    var opacity = 0f;
                    if (_menu.GetMenuItems()[itemIndex] is MenuListItem item2)
                    {
                        opacity = (((float)item2.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex + 1] is MenuListItem item1)
                    {
                        opacity = (((float)item1.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else if (_menu.GetMenuItems()[itemIndex - 1] is MenuListItem item)
                    {
                        opacity = (((float)item.ListIndex + 1) / 10f) - 0.1f;
                    }
                    else
                    {
                        opacity = 1f;
                    }

                    switch (itemIndex)
                    {
                        case 4: // blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 0, selection, opacity);
                            currentCharacter.PedAppearance.blemishesStyle = selection;
                            currentCharacter.PedAppearance.blemishesOpacity = opacity;
                            break;
                        case 6: // beards
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 1, selection, opacity);
                            currentCharacter.PedAppearance.beardStyle = selection;
                            currentCharacter.PedAppearance.beardOpacity = opacity;
                            break;
                        case 9: // eyebrows
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 2, selection, opacity);
                            currentCharacter.PedAppearance.eyebrowsStyle = selection;
                            currentCharacter.PedAppearance.eyebrowsOpacity = opacity;
                            break;
                        case 12: // ageing
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 3, selection, opacity);
                            currentCharacter.PedAppearance.ageingStyle = selection;
                            currentCharacter.PedAppearance.ageingOpacity = opacity;
                            break;
                        case 14: // makeup
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 4, selection, opacity);
                            currentCharacter.PedAppearance.makeupStyle = selection;
                            currentCharacter.PedAppearance.makeupOpacity = opacity;
                            break;
                        case 17: // blush style
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 5, selection, opacity);
                            currentCharacter.PedAppearance.blushStyle = selection;
                            currentCharacter.PedAppearance.blushOpacity = opacity;
                            break;
                        case 20: // complexion
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 6, selection, opacity);
                            currentCharacter.PedAppearance.complexionStyle = selection;
                            currentCharacter.PedAppearance.complexionOpacity = opacity;
                            break;
                        case 22: // sun damage
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 7, selection, opacity);
                            currentCharacter.PedAppearance.sunDamageStyle = selection;
                            currentCharacter.PedAppearance.sunDamageOpacity = opacity;
                            break;
                        case 24: // lipstick
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 8, selection, opacity);
                            currentCharacter.PedAppearance.lipstickStyle = selection;
                            currentCharacter.PedAppearance.lipstickOpacity = opacity;
                            break;
                        case 27: // moles and freckles
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 9, selection, opacity);
                            currentCharacter.PedAppearance.molesFrecklesStyle = selection;
                            currentCharacter.PedAppearance.molesFrecklesOpacity = opacity;
                            break;
                        case 29: // chest hair
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 10, selection, opacity);
                            currentCharacter.PedAppearance.chestHairStyle = selection;
                            currentCharacter.PedAppearance.chestHairOpacity = opacity;
                            break;
                        case 32: // body blemishes
                            SetPedHeadOverlay(Game.PlayerPed.Handle, 11, selection, opacity);
                            currentCharacter.PedAppearance.bodyBlemishesStyle = selection;
                            currentCharacter.PedAppearance.bodyBlemishesOpacity = opacity;
                            break;
                    }
                }
            };
            #endregion

            #region clothes
            clothesMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, realIndex) =>
            {
                var componentIndex = realIndex + 1;
                if (realIndex > 0)
                {
                    componentIndex += 1;
                }

                var textureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, componentIndex);
                var newTextureIndex = 0;
                SetPedComponentVariation(Game.PlayerPed.Handle, componentIndex, newSelectionIndex, newTextureIndex, 0);
                currentCharacter.DrawableVariations.clothes ??= new Dictionary<int, KeyValuePair<int, int>>();

                var maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, newSelectionIndex);

                currentCharacter.DrawableVariations.clothes[componentIndex] = new KeyValuePair<int, int>(newSelectionIndex, newTextureIndex);
                listItem.Description = $"使用箭头键选择一个可绘制项，然后按 ~o~回车~s~ 以浏览所有可用的纹理。当前选中的纹理是：#{newTextureIndex + 1}（共 {maxTextures} 个）。";
            };

            clothesMenu.OnListItemSelect += (sender, listItem, listIndex, realIndex) =>
            {
                var componentIndex = realIndex + 1; // skip face options as that fucks up with inheritance faces
                if (realIndex > 0) // skip hair features as that is done in the appeareance menu
                {
                    componentIndex += 1;
                }

                var textureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, componentIndex);
                var newTextureIndex = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, listIndex) - 1 < textureIndex + 1 ? 0 : textureIndex + 1;
                SetPedComponentVariation(Game.PlayerPed.Handle, componentIndex, listIndex, newTextureIndex, 0);
                currentCharacter.DrawableVariations.clothes ??= new Dictionary<int, KeyValuePair<int, int>>();

                var maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, componentIndex, listIndex);

                currentCharacter.DrawableVariations.clothes[componentIndex] = new KeyValuePair<int, int>(listIndex, newTextureIndex);
                listItem.Description = $"使用箭头键选择一个可绘制项，然后按 ~o~回车~s~ 以循环浏览所有可用的纹理。当前选中的纹理是：#{newTextureIndex + 1}（共 {maxTextures} 个）。";
            };
            #endregion

            #region props
            propsMenu.OnListIndexChange += (_menu, listItem, oldSelectionIndex, newSelectionIndex, realIndex) =>
            {
                var propIndex = realIndex;
                if (realIndex == 3)
                {
                    propIndex = 6;
                }
                if (realIndex == 4)
                {
                    propIndex = 7;
                }

                var textureIndex = 0;
                if (newSelectionIndex >= GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Game.PlayerPed.Handle, propIndex);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    listItem.Description = $"使用箭头键选择一个道具，然后按 ~o~回车~s~ 以循环浏览所有可用的纹理。";
                }
                else
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, newSelectionIndex, textureIndex, true);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(newSelectionIndex, textureIndex);
                    if (GetPedPropIndex(Game.PlayerPed.Handle, propIndex) == -1)
                    {
                        listItem.Description = $"使用箭头键选择一个道具，然后按 ~o~回车~s~ 以循环浏览所有可用的纹理。";
                    }
                    else
                    {
                        var maxPropTextures = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, newSelectionIndex);
                        listItem.Description = $"使用箭头键选择一个道具，然后按 ~o~回车~s~ 以循环浏览所有可用的纹理。当前选中的纹理是：#{textureIndex + 1}（共 {maxPropTextures} 个）。";
                    }
                }
            };

            propsMenu.OnListItemSelect += (sender, listItem, listIndex, realIndex) =>
            {
                var propIndex = realIndex;
                if (realIndex == 3)
                {
                    propIndex = 6;
                }
                if (realIndex == 4)
                {
                    propIndex = 7;
                }

                var textureIndex = GetPedPropTextureIndex(Game.PlayerPed.Handle, propIndex);
                var newTextureIndex = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, listIndex) - 1 < textureIndex + 1 ? 0 : textureIndex + 1;
                if (textureIndex >= GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Game.PlayerPed.Handle, propIndex);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    listItem.Description = $"使用箭头键选择一个道具，然后按 ~o~回车~s~ 以循环浏览所有可用的纹理。";
                }
                else
                {
                    SetPedPropIndex(Game.PlayerPed.Handle, propIndex, listIndex, newTextureIndex, true);
                    currentCharacter.PropVariations.props ??= new Dictionary<int, KeyValuePair<int, int>>();
                    currentCharacter.PropVariations.props[propIndex] = new KeyValuePair<int, int>(listIndex, newTextureIndex);
                    if (GetPedPropIndex(Game.PlayerPed.Handle, propIndex) == -1)
                    {
                        listItem.Description = $"使用箭头键选择一个道具，然后按 ~o~回车~s~ 以循环浏览所有可用的纹理。";
                    }
                    else
                    {
                        var maxPropTextures = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, propIndex, listIndex);
                        listItem.Description = $"使用箭头键选择一个道具，然后按 ~o~回车~s~ 以循环浏览所有可用的纹理。当前选中的纹理是：#{newTextureIndex + 1}（共 {maxPropTextures} 个）。";
                    }
                }
                //propsMenu.UpdateScaleform();
            };
            #endregion

            #region face shape data
            /*
            Nose_Width  
            Nose_Peak_Hight  
            Nose_Peak_Lenght  
            Nose_Bone_High  
            Nose_Peak_Lowering  
            Nose_Bone_Twist  
            EyeBrown_High  
            EyeBrown_Forward  
            Cheeks_Bone_High  
            Cheeks_Bone_Width  
            Cheeks_Width  
            Eyes_Openning  
            Lips_Thickness  
            Jaw_Bone_Width 'Bone size to sides  
            Jaw_Bone_Back_Lenght 'Bone size to back  
            Chimp_Bone_Lowering 'Go Down  
            Chimp_Bone_Lenght 'Go forward  
            Chimp_Bone_Width  
            Chimp_Hole  
            Neck_Thikness  
            */

            var faceFeaturesValuesList = new List<float>()
            {
               -1.0f,    // 0
               -0.9f,    // 1
               -0.8f,    // 2
               -0.7f,    // 3
               -0.6f,    // 4
               -0.5f,    // 5
               -0.4f,    // 6
               -0.3f,    // 7
               -0.2f,    // 8
               -0.1f,    // 9
                0.0f,    // 10
                0.1f,    // 11
                0.2f,    // 12
                0.3f,    // 13
                0.4f,    // 14
                0.5f,    // 15
                0.6f,    // 16
                0.7f,    // 17
                0.8f,    // 18
                0.9f,    // 19
                1.0f     // 20
            };

            var faceFeaturesNamesList = new string[20]
            {
                "鼻子宽度",               // 0
                "鼻尖高度",               // 1
                "鼻尖长度",               // 2
                "鼻骨高度",               // 3
                "鼻尖下垂",               // 4
                "鼻骨扭曲",               // 5
                "眉毛高度",               // 6
                "眉毛深度",               // 7
                "颧骨高度",               // 8
                "颧骨宽度",               // 9
                "脸颊宽度",               // 10
                "眼睛开合",               // 11
                "嘴唇厚度",               // 12
                "下颚骨宽度",             // 13
                "下颚骨深度/长度",        // 14
                "下巴高度",               // 15
                "下巴深度/长度",          // 16
                "下巴宽度",               // 17
                "下巴凹槽大小",           // 18
                "脖子厚度"                // 19
            };

            for (var i = 0; i < 20; i++)
            {
                var faceFeature = new MenuSliderItem(faceFeaturesNamesList[i], $"设置 {faceFeaturesNamesList[i]} 面部特征", 0, 20, 10, true);
                faceShapeMenu.AddMenuItem(faceFeature);
            }

            faceShapeMenu.OnSliderPositionChange += (sender, sliderItem, oldPosition, newPosition, itemIndex) =>
            {
                currentCharacter.FaceShapeFeatures.features ??= new Dictionary<int, float>();
                var value = faceFeaturesValuesList[newPosition];
                currentCharacter.FaceShapeFeatures.features[itemIndex] = value;
                SetPedFaceFeature(Game.PlayerPed.Handle, itemIndex, value);
            };

            #endregion

            #region tattoos
            void CreateListsIfNull()
            {
                currentCharacter.PedTatttoos.HeadTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.TorsoTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.LeftArmTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.RightArmTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.LeftLegTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.RightLegTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.BadgeTattoos ??= new List<KeyValuePair<string, string>>();
            }

            void ApplySavedTattoos()
            {
                // remove all decorations, and then manually re-add them all. what a retarded way of doing this R*....
                ClearPedDecorations(Game.PlayerPed.Handle);

                foreach (var tattoo in currentCharacter.PedTatttoos.HeadTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.TorsoTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.LeftArmTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.RightArmTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.LeftLegTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.RightLegTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.BadgeTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }

                if (!string.IsNullOrEmpty(currentCharacter.PedAppearance.HairOverlay.Key) && !string.IsNullOrEmpty(currentCharacter.PedAppearance.HairOverlay.Value))
                {
                    // reset hair value
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(currentCharacter.PedAppearance.HairOverlay.Key), (uint)GetHashKey(currentCharacter.PedAppearance.HairOverlay.Value));
                }
            }

            tattoosMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                CreateListsIfNull();
                ApplySavedTattoos();
            };

            #region tattoos menu list select events
            tattoosMenu.OnListIndexChange += (sender, item, oldIndex, tattooIndex, menuIndex) =>
            {
                CreateListsIfNull();
                ApplySavedTattoos();
                if (menuIndex == 0) // head
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HEAD.ElementAt(tattooIndex) : FemaleTattoosCollection.HEAD.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.HeadTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 1) // torso
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.TORSO.ElementAt(tattooIndex) : FemaleTattoosCollection.TORSO.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.TorsoTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 2) // left arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.LeftArmTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 3) // right arm
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.RightArmTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 4) // left leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.LeftLegTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 5) // right leg
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.RightLegTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
                else if (menuIndex == 6) // badges
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.BADGES.ElementAt(tattooIndex) : FemaleTattoosCollection.BADGES.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (!currentCharacter.PedTatttoos.BadgeTattoos.Contains(tat))
                    {
                        SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                    }
                }
            };

            tattoosMenu.OnListItemSelect += (sender, item, tattooIndex, menuIndex) =>
            {
                CreateListsIfNull();

                if (menuIndex == 0) // head
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HEAD.ElementAt(tattooIndex) : FemaleTattoosCollection.HEAD.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.HeadTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~r~移除~s~。");
                        currentCharacter.PedTatttoos.HeadTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~g~添加~s~。");
                        currentCharacter.PedTatttoos.HeadTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 1) // 躯干
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.TORSO.ElementAt(tattooIndex) : FemaleTattoosCollection.TORSO.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.TorsoTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~r~移除~s~。");
                        currentCharacter.PedTatttoos.TorsoTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~g~添加~s~。");
                        currentCharacter.PedTatttoos.TorsoTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 2) // 左臂
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.LeftArmTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~r~移除~s~。");
                        currentCharacter.PedTatttoos.LeftArmTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~g~添加~s~。");
                        currentCharacter.PedTatttoos.LeftArmTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 3) // 右臂
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.RightArmTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~r~移除~s~。");
                        currentCharacter.PedTatttoos.RightArmTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~g~添加~s~。");
                        currentCharacter.PedTatttoos.RightArmTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 4) // 左腿
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.LeftLegTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~r~移除~s~。");
                        currentCharacter.PedTatttoos.LeftLegTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~g~添加~s~。");
                        currentCharacter.PedTatttoos.LeftLegTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 5) // 右腿
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.RightLegTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~r~移除~s~。");
                        currentCharacter.PedTatttoos.RightLegTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"纹身 #{tattooIndex + 1} 已被 ~g~添加~s~。");
                        currentCharacter.PedTatttoos.RightLegTattoos.Add(tat);
                    }
                }
                else if (menuIndex == 6) // 徽章
                {
                    var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.BADGES.ElementAt(tattooIndex) : FemaleTattoosCollection.BADGES.ElementAt(tattooIndex);
                    var tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                    if (currentCharacter.PedTatttoos.BadgeTattoos.Contains(tat))
                    {
                        Subtitle.Custom($"徽章 #{tattooIndex + 1} 已被 ~r~移除~s~。");
                        currentCharacter.PedTatttoos.BadgeTattoos.Remove(tat);
                    }
                    else
                    {
                        Subtitle.Custom($"徽章 #{tattooIndex + 1} 已被 ~g~添加~s~。");
                        currentCharacter.PedTatttoos.BadgeTattoos.Add(tat);
                    }
                }
                ApplySavedTattoos();

            };

            // eventhandler for when a tattoo is selected.
            tattoosMenu.OnItemSelect += (sender, item, index) =>
            {
                Notify.Success("所有纹身已被移除。");
                currentCharacter.PedTatttoos.HeadTattoos.Clear();
                currentCharacter.PedTatttoos.TorsoTattoos.Clear();
                currentCharacter.PedTatttoos.LeftArmTattoos.Clear();
                currentCharacter.PedTatttoos.RightArmTattoos.Clear();
                currentCharacter.PedTatttoos.LeftLegTattoos.Clear();
                currentCharacter.PedTatttoos.RightLegTattoos.Clear();
                currentCharacter.PedTatttoos.BadgeTattoos.Clear();
                ClearPedDecorations(Game.PlayerPed.Handle);
            };

            #endregion
            #endregion


            // handle list changes in the character creator menu.
            createCharacterMenu.OnListIndexChange += (sender, item, oldListIndex, newListIndex, itemIndex) =>
            {
                if (item == faceExpressionList)
                {
                    currentCharacter.FacialExpression = facial_expressions[newListIndex];
                    SetFacialIdleAnimOverride(Game.PlayerPed.Handle, currentCharacter.FacialExpression ?? facial_expressions[0], null);
                }
                else if (item == categoryBtn)
                {
                    List<string> categoryNames = categoryBtn.ItemData.Item1;
                    List<MenuItem.Icon> categoryIcons = categoryBtn.ItemData.Item2;
                    currentCharacter.Category = categoryNames[newListIndex];
                    categoryBtn.RightIcon = categoryIcons[newListIndex];
                }
            };

            // handle button presses for the createCharacter menu.
            createCharacterMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == saveButton) // save ped
                {
                    if (await SavePed())
                    {
                        while (!MenuController.IsAnyMenuOpen())
                        {
                            await BaseScript.Delay(0);
                        }

                        while (IsControlPressed(2, 201) || IsControlPressed(2, 217) || IsDisabledControlPressed(2, 201) || IsDisabledControlPressed(2, 217))
                        {
                            await BaseScript.Delay(0);
                        }

                        await BaseScript.Delay(100);

                        createCharacterMenu.GoBack();
                    }
                }
                else if (item == exitNoSave) // 退出不保存
                {
                    var confirm = false;
                    AddTextEntry("vmenu_warning_message_first_line", "您确定要退出角色创建器吗？");
                    AddTextEntry("vmenu_warning_message_second_line", "您将丢失所有（未保存的）自定义内容！");
                    createCharacterMenu.CloseMenu();

                    // 等待确认或取消输入。
                    while (true)
                    {
                        await BaseScript.Delay(0);
                        var unk = 1;
                        var unk2 = 1;
                        SetWarningMessage("vmenu_warning_message_first_line", 20, "vmenu_warning_message_second_line", true, 0, ref unk, ref unk2, true, 0);
                        if (IsControlJustPressed(2, 201) || IsControlJustPressed(2, 217)) // 继续/接受
                        {
                            confirm = true;
                            break;
                        }
                        else if (IsControlJustPressed(2, 202)) // 取消
                        {
                            break;
                        }
                    }

                    // if confirmed to discard changes quit the editor.
                    if (confirm)
                    {
                        while (IsControlPressed(2, 201) || IsControlPressed(2, 217) || IsDisabledControlPressed(2, 201) || IsDisabledControlPressed(2, 217))
                        {
                            await BaseScript.Delay(0);
                        }

                        await BaseScript.Delay(100);
                        menu.OpenMenu();
                    }
                    else // otherwise cancel and go back to the editor.
                    {
                        createCharacterMenu.OpenMenu();
                    }
                }
                else if (item == inheritanceButton) // update the inheritance menu anytime it's opened to prevent some weird glitch where old data is used.
                {
                    var data = Game.PlayerPed.GetHeadBlendData();
                    inheritanceDads.ListIndex = inheritanceDads.ListItems.IndexOf(dads.FirstOrDefault(entry => entry.Value == data.FirstFaceShape).Key);
                    inheritanceMoms.ListIndex = inheritanceMoms.ListItems.IndexOf(moms.FirstOrDefault(entry => entry.Value == data.SecondFaceShape).Key);
                    inheritanceShapeMix.Position = UnclampMix(data.ParentFaceShapePercent);
                    inheritanceSkinMix.Position = UnclampMix(data.ParentSkinTonePercent);
                    inheritanceMenu.RefreshIndex();
                }
            };

            // eventhandler for whenever a menu item is selected in the main mp characters menu.
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == createMaleBtn)
                {
                    var model = (uint)GetHashKey("mp_m_freemode_01");

                    if (!HasModelLoaded(model))
                    {
                        RequestModel(model);
                        while (!HasModelLoaded(model))
                        {
                            await BaseScript.Delay(0);
                        }
                    }

                    var maxHealth = Game.PlayerPed.MaxHealth;
                    var maxArmour = Game.Player.MaxArmor;
                    var health = Game.PlayerPed.Health;
                    var armour = Game.PlayerPed.Armor;

                    SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                    SetPlayerModel(Game.Player.Handle, model);
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, true);

                    Game.Player.MaxArmor = maxArmour;
                    Game.PlayerPed.MaxHealth = maxHealth;
                    Game.PlayerPed.Health = health;
                    Game.PlayerPed.Armor = armour;

                    ClearPedDecorations(Game.PlayerPed.Handle);
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                    SetPedHairColor(Game.PlayerPed.Handle, 0, 0);
                    SetPedEyeColor(Game.PlayerPed.Handle, 0);
                    ClearAllPedProps(Game.PlayerPed.Handle);

                    MakeCreateCharacterMenu(male: true);
                }
                else if (item == createFemaleBtn)
                {
                    var model = (uint)GetHashKey("mp_f_freemode_01");

                    if (!HasModelLoaded(model))
                    {
                        RequestModel(model);
                        while (!HasModelLoaded(model))
                        {
                            await BaseScript.Delay(0);
                        }
                    }

                    var maxHealth = Game.PlayerPed.MaxHealth;
                    var maxArmour = Game.Player.MaxArmor;
                    var health = Game.PlayerPed.Health;
                    var armour = Game.PlayerPed.Armor;

                    SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                    SetPlayerModel(Game.Player.Handle, model);
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, true);

                    Game.Player.MaxArmor = maxArmour;
                    Game.PlayerPed.MaxHealth = maxHealth;
                    Game.PlayerPed.Health = health;
                    Game.PlayerPed.Armor = armour;

                    ClearPedDecorations(Game.PlayerPed.Handle);
                    ClearPedFacialDecorations(Game.PlayerPed.Handle);
                    SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                    SetPedHairColor(Game.PlayerPed.Handle, 0, 0);
                    SetPedEyeColor(Game.PlayerPed.Handle, 0);
                    ClearAllPedProps(Game.PlayerPed.Handle);

                    MakeCreateCharacterMenu(male: false);
                }
                else if (item == savedCharacters)
                {
                    UpdateSavedPedsMenu();
                }
            };
        }

        /// <summary>
        /// Spawns this saved ped.
        /// </summary>
        /// <param name="name"></param>
        internal async Task SpawnThisCharacter(string name, bool restoreWeapons)
        {
            currentCharacter = StorageManager.GetSavedMpCharacterData(name);
            await SpawnSavedPed(restoreWeapons);
        }

        /// <summary>
        /// Spawns the ped from the data inside <see cref="currentCharacter"/>.
        /// Character data MUST be set BEFORE calling this function.
        /// </summary>
        /// <returns></returns>
        private async Task SpawnSavedPed(bool restoreWeapons)
        {
            if (currentCharacter.Version < 1)
            {
                return;
            }
            if (IsModelInCdimage(currentCharacter.ModelHash))
            {
                if (!HasModelLoaded(currentCharacter.ModelHash))
                {
                    RequestModel(currentCharacter.ModelHash);
                    while (!HasModelLoaded(currentCharacter.ModelHash))
                    {
                        await BaseScript.Delay(0);
                    }
                }
                var maxHealth = Game.PlayerPed.MaxHealth;
                var maxArmour = Game.Player.MaxArmor;
                var health = Game.PlayerPed.Health;
                var armour = Game.PlayerPed.Armor;

                SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn");
                SetPlayerModel(Game.Player.Handle, currentCharacter.ModelHash);
                await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", false, true, true);

                Game.Player.MaxArmor = maxArmour;
                Game.PlayerPed.MaxHealth = maxHealth;
                Game.PlayerPed.Health = health;
                Game.PlayerPed.Armor = armour;

                ClearPedDecorations(Game.PlayerPed.Handle);
                ClearPedFacialDecorations(Game.PlayerPed.Handle);
                SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                SetPedHairColor(Game.PlayerPed.Handle, 0, 0);
                SetPedEyeColor(Game.PlayerPed.Handle, 0);
                ClearAllPedProps(Game.PlayerPed.Handle);

                #region headblend
                var data = currentCharacter.PedHeadBlendData;
                SetPedHeadBlendData(Game.PlayerPed.Handle, data.FirstFaceShape, data.SecondFaceShape, data.ThirdFaceShape, data.FirstSkinTone, data.SecondSkinTone, data.ThirdSkinTone, data.ParentFaceShapePercent, data.ParentSkinTonePercent, 0f, data.IsParentInheritance);

                while (!HasPedHeadBlendFinished(Game.PlayerPed.Handle))
                {
                    await BaseScript.Delay(0);
                }
                #endregion

                #region appearance
                var appData = currentCharacter.PedAppearance;
                // hair
                SetPedComponentVariation(Game.PlayerPed.Handle, 2, appData.hairStyle, 0, 0);
                SetPedHairColor(Game.PlayerPed.Handle, appData.hairColor, appData.hairHighlightColor);
                if (!string.IsNullOrEmpty(appData.HairOverlay.Key) && !string.IsNullOrEmpty(appData.HairOverlay.Value))
                {
                    SetPedFacialDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(appData.HairOverlay.Key), (uint)GetHashKey(appData.HairOverlay.Value));
                }
                // blemishes
                SetPedHeadOverlay(Game.PlayerPed.Handle, 0, appData.blemishesStyle, appData.blemishesOpacity);
                // bread
                SetPedHeadOverlay(Game.PlayerPed.Handle, 1, appData.beardStyle, appData.beardOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, appData.beardColor, appData.beardColor);
                // eyebrows
                SetPedHeadOverlay(Game.PlayerPed.Handle, 2, appData.eyebrowsStyle, appData.eyebrowsOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, appData.eyebrowsColor, appData.eyebrowsColor);
                // ageing
                SetPedHeadOverlay(Game.PlayerPed.Handle, 3, appData.ageingStyle, appData.ageingOpacity);
                // makeup
                SetPedHeadOverlay(Game.PlayerPed.Handle, 4, appData.makeupStyle, appData.makeupOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, appData.makeupColor, appData.makeupColor);
                // blush
                SetPedHeadOverlay(Game.PlayerPed.Handle, 5, appData.blushStyle, appData.blushOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, appData.blushColor, appData.blushColor);
                // complexion
                SetPedHeadOverlay(Game.PlayerPed.Handle, 6, appData.complexionStyle, appData.complexionOpacity);
                // sundamage
                SetPedHeadOverlay(Game.PlayerPed.Handle, 7, appData.sunDamageStyle, appData.sunDamageOpacity);
                // lipstick
                SetPedHeadOverlay(Game.PlayerPed.Handle, 8, appData.lipstickStyle, appData.lipstickOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, appData.lipstickColor, appData.lipstickColor);
                // moles and freckles
                SetPedHeadOverlay(Game.PlayerPed.Handle, 9, appData.molesFrecklesStyle, appData.molesFrecklesOpacity);
                // chest hair 
                SetPedHeadOverlay(Game.PlayerPed.Handle, 10, appData.chestHairStyle, appData.chestHairOpacity);
                SetPedHeadOverlayColor(Game.PlayerPed.Handle, 10, 1, appData.chestHairColor, appData.chestHairColor);
                // body blemishes 
                SetPedHeadOverlay(Game.PlayerPed.Handle, 11, appData.bodyBlemishesStyle, appData.bodyBlemishesOpacity);
                // eyecolor
                SetPedEyeColor(Game.PlayerPed.Handle, appData.eyeColor);
                #endregion

                #region Face Shape Data
                for (var i = 0; i < 19; i++)
                {
                    SetPedFaceFeature(Game.PlayerPed.Handle, i, 0f);
                }

                if (currentCharacter.FaceShapeFeatures.features != null)
                {
                    foreach (var t in currentCharacter.FaceShapeFeatures.features)
                    {
                        SetPedFaceFeature(Game.PlayerPed.Handle, t.Key, t.Value);
                    }
                }
                else
                {
                    currentCharacter.FaceShapeFeatures.features = new Dictionary<int, float>();
                }

                #endregion

                #region Clothing Data
                if (currentCharacter.DrawableVariations.clothes != null && currentCharacter.DrawableVariations.clothes.Count > 0)
                {
                    foreach (var cd in currentCharacter.DrawableVariations.clothes)
                    {
                        SetPedComponentVariation(Game.PlayerPed.Handle, cd.Key, cd.Value.Key, cd.Value.Value, 0);
                    }
                }
                #endregion

                #region Props Data
                if (currentCharacter.PropVariations.props != null && currentCharacter.PropVariations.props.Count > 0)
                {
                    foreach (var cd in currentCharacter.PropVariations.props)
                    {
                        if (cd.Value.Key > -1)
                        {
                            SetPedPropIndex(Game.PlayerPed.Handle, cd.Key, cd.Value.Key, cd.Value.Value > -1 ? cd.Value.Value : 0, true);
                        }
                    }
                }
                #endregion

                #region Tattoos

                currentCharacter.PedTatttoos.HeadTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.TorsoTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.LeftArmTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.RightArmTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.LeftLegTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.RightLegTattoos ??= new List<KeyValuePair<string, string>>();
                currentCharacter.PedTatttoos.BadgeTattoos ??= new List<KeyValuePair<string, string>>();

                foreach (var tattoo in currentCharacter.PedTatttoos.HeadTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.TorsoTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.LeftArmTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.RightArmTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.LeftLegTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.RightLegTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                foreach (var tattoo in currentCharacter.PedTatttoos.BadgeTattoos)
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
                }
                #endregion
            }

            // Set the facial expression, or set it to 'normal' if it wasn't saved/set before.
            SetFacialIdleAnimOverride(Game.PlayerPed.Handle, currentCharacter.FacialExpression ?? facial_expressions[0], null);
        }

        /// <summary>
        /// Creates the saved mp characters menu.
        /// </summary>
        private void CreateSavedPedsMenu()
        {
            UpdateSavedPedsMenu();

            MenuController.AddMenu(manageSavedCharacterMenu);

            var spawnPed = new MenuItem("生成保存的角色", "生成所选的保存角色。");
            editPedBtn = new MenuItem("编辑保存的角色", "这允许您编辑保存角色的所有内容。更改将在您点击保存按钮后保存到该角色的保存文件中。");
            var clonePed = new MenuItem("克隆保存的角色", "这将克隆您的保存角色。系统会要求您为该角色提供一个名称。如果该名称已经被占用，则操作将被取消。");
            var setAsDefaultPed = new MenuItem("设置为默认角色", "如果您将此角色设置为默认角色，并在杂项设置菜单中启用“重生为默认MP角色”选项，那么每当您重生时，您将变为该角色。");
            var renameCharacter = new MenuItem("重命名保存的角色", "您可以重命名此保存的角色。如果名称已被占用，则操作将被取消。");
            var delPed = new MenuItem("删除保存的角色", "删除所选的保存角色。此操作无法撤销！")
            {
                LeftIcon = MenuItem.Icon.WARNING
            };
            manageSavedCharacterMenu.AddMenuItem(spawnPed);
            manageSavedCharacterMenu.AddMenuItem(editPedBtn);
            manageSavedCharacterMenu.AddMenuItem(clonePed);
            manageSavedCharacterMenu.AddMenuItem(setCategoryBtn);
            manageSavedCharacterMenu.AddMenuItem(setAsDefaultPed);
            manageSavedCharacterMenu.AddMenuItem(renameCharacter);
            manageSavedCharacterMenu.AddMenuItem(delPed);

            MenuController.BindMenuItem(manageSavedCharacterMenu, createCharacterMenu, editPedBtn);

            manageSavedCharacterMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == editPedBtn)
                {
                    currentCharacter = StorageManager.GetSavedMpCharacterData(selectedSavedCharacterManageName);

                    await SpawnSavedPed(true);

                    MakeCreateCharacterMenu(male: currentCharacter.IsMale, editPed: true);
                }
                else if (item == spawnPed)
                {
                    currentCharacter = StorageManager.GetSavedMpCharacterData(selectedSavedCharacterManageName);

                    await SpawnSavedPed(true);
                }
                else if (item == clonePed)
                {
                    var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);
                    var name = await GetUserInput(windowTitle: "输入克隆角色的名称", defaultText: tmpCharacter.SaveName.Substring(7), maxInputLength: 30);
                    if (string.IsNullOrEmpty(name))
                    {
                        Notify.Error(CommonErrors.InvalidSaveName);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GetResourceKvpString("mp_ped_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                        }
                        else
                        {
                            tmpCharacter.SaveName = "mp_ped_" + name;
                            if (StorageManager.SaveJsonData("mp_ped_" + name, JsonConvert.SerializeObject(tmpCharacter), false))
                            {
                                Notify.Success($"您的角色已被克隆。克隆角色的名称是: ~g~<C>{name}</C>~s~。");
                                MenuController.CloseAllMenus();
                                UpdateSavedPedsMenu();
                                savedCharactersMenu.OpenMenu();
                            }
                            else
                            {
                                Notify.Error("无法创建克隆，原因未知。是否已经存在该名称的角色？ :(");
                            }
                        }
                    }
                }
                else if (item == renameCharacter)
                {
                    var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);
                    var name = await GetUserInput(windowTitle: "输入新的角色名称", defaultText: tmpCharacter.SaveName.Substring(7), maxInputLength: 30);
                    if (string.IsNullOrEmpty(name))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GetResourceKvpString("mp_ped_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                        }
                        else
                        {
                            tmpCharacter.SaveName = "mp_ped_" + name;
                            if (StorageManager.SaveJsonData("mp_ped_" + name, JsonConvert.SerializeObject(tmpCharacter), false))
                            {
                                StorageManager.DeleteSavedStorageItem("mp_ped_" + selectedSavedCharacterManageName);
                                Notify.Success($"您的角色已被重命名为 ~g~<C>{name}</C>~s~.");
                                UpdateSavedPedsMenu();
                                while (!MenuController.IsAnyMenuOpen())
                                {
                                    await BaseScript.Delay(0);
                                }
                                manageSavedCharacterMenu.GoBack();
                            }
                            else
                            {
                                Notify.Error("重命名角色时出现问题，您的旧角色不会被删除。");
                            }
                        }
                    }
                }
                else if (item == delPed)
                {
                    if (delPed.Label == "Are you sure?")
                    {
                        delPed.Label = "";
                        DeleteResourceKvp("mp_ped_" + selectedSavedCharacterManageName);
                        Notify.Success("您的保存角色已被删除。");
                        manageSavedCharacterMenu.GoBack();
                        UpdateSavedPedsMenu();
                        manageSavedCharacterMenu.RefreshIndex();
                    }
                    else
                    {
                        delPed.Label = "Are you sure?";
                    }
                }
                else if (item == setAsDefaultPed)
                {
                    Notify.Success($"您的角色 <C>{selectedSavedCharacterManageName}</C> 将会被设置为默认角色，每当您（重新）生成时将使用此角色。");
                    SetResourceKvp("vmenu_default_character", "mp_ped_" + selectedSavedCharacterManageName);
                }

                if (item != delPed)
                {
                    if (delPed.Label == "Are you sure?")
                    {
                        delPed.Label = "";
                    }
                }
            };

            // Update category preview icon
            manageSavedCharacterMenu.OnListIndexChange += (_, listItem, _, newSelectionIndex, _) => listItem.RightIcon = listItem.ItemData[newSelectionIndex];

            // Update character's category
            manageSavedCharacterMenu.OnListItemSelect += async (_, listItem, listIndex, _) =>
            {
                var tmpCharacter = StorageManager.GetSavedMpCharacterData("mp_ped_" + selectedSavedCharacterManageName);

                string name = listItem.ListItems[listIndex];

                if (name == "Create New")
                {
                    var newName = await GetUserInput(windowTitle: "输入类别名称。", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName) || newName.ToLower() == "uncategorized" || newName.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "输入类别描述（可选）。", maxInputLength: 120);
                        var newCategory = new MpCharacterCategory
                        {
                            Name = newName,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("mp_character_category_" + newName, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"您的类别 (~g~<C>{newName}</C>~s~) 已被保存。");
                            Log($"保存了类别 {newName}。");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                            name = newName;
                        }
                        else
                        {
                            Notify.Error($"保存失败，可能是因为这个名称 (~y~<C>{newName}</C>~s~) 已被使用。");
                            return;
                        }
                    }
                }

                tmpCharacter.Category = name;

                var json = JsonConvert.SerializeObject(tmpCharacter);
                if (StorageManager.SaveJsonData(tmpCharacter.SaveName, json, true))
                {
                    Notify.Success("您的角色已成功保存。");
                }
                else
                {
                    Notify.Error("您的角色无法保存。原因未知。:(");
                }

                MenuController.CloseAllMenus();
                UpdateSavedPedsMenu();
                savedCharactersMenu.OpenMenu();
            };

            // reset the "are you sure" state.
            manageSavedCharacterMenu.OnMenuClose += (sender) =>
            {
                manageSavedCharacterMenu.GetMenuItems().Last().Label = "";
            };

            // Load selected category
            savedCharactersMenu.OnItemSelect += async (sender, item, index) =>
            {
                // Create new category
                if (item.ItemData is not MpCharacterCategory)
                {
                    var name = await GetUserInput(windowTitle: "输入类别名称。", maxInputLength: 30);
                    if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "输入类别描述（可选）。", maxInputLength: 120);
                        var newCategory = new MpCharacterCategory
                        {
                            Name = name,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("mp_character_category_" + name, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"您的类别 (~g~<C>{name}</C>~s~) 已被保存。");
                            Log($"保存了类别 {name}。");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                        }
                        else
                        {
                            Notify.Error($"保存失败，可能是因为这个名称 (~y~<C>{name}</C>~s~) 已被使用。");
                            return;
                        }
                    }
                }
                // Select an old category
                else
                {
                    currentCategory = item.ItemData;
                }

                bool isUncategorized = currentCategory.Name == "Uncategorized";

                savedCharactersCategoryMenu.MenuTitle = currentCategory.Name;
                savedCharactersCategoryMenu.MenuSubtitle = $"~s~类别: ~y~{currentCategory.Name}";
                savedCharactersCategoryMenu.ClearMenuItems();

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
                var deleteBtn = new MenuItem("删除类别", "删除此类别。这不能被撤销！")
                {
                    RightIcon = MenuItem.Icon.WARNING,
                    Enabled = !isUncategorized
                };
                var deleteCharsBtn = new MenuCheckboxItem("删除所有角色", "如果选中，当点击 \"删除类别\" 时，此类别中的所有保存角色也将被删除。如果未选中，保存角色将被移动到 \"未分类\"。")
                {
                    Enabled = !isUncategorized
                };

                savedCharactersCategoryMenu.AddMenuItem(renameBtn);
                savedCharactersCategoryMenu.AddMenuItem(descriptionBtn);
                savedCharactersCategoryMenu.AddMenuItem(iconBtn);
                savedCharactersCategoryMenu.AddMenuItem(deleteBtn);
                savedCharactersCategoryMenu.AddMenuItem(deleteCharsBtn);

                var spacer = GetSpacerMenuItem("↓ Characters ↓");
                savedCharactersCategoryMenu.AddMenuItem(spacer);

                List<string> names = GetAllMpCharacterNames();

                if (names.Count > 0)
                {
                    var defaultChar = GetResourceKvpString("vmenu_default_character") ?? "";

                    names.Sort((a, b) => a.ToLower().CompareTo(b.ToLower()));
                    foreach (var name in names)
                    {
                        var tmpData = StorageManager.GetSavedMpCharacterData("mp_ped_" + name);

                        if (string.IsNullOrEmpty(tmpData.Category))
                        {
                            if (!isUncategorized)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (tmpData.Category != currentCategory.Name)
                            {
                                continue;
                            }
                        }

                        var btn = new MenuItem(name, "点击以生成、编辑、克隆、重命名或删除此保存角色。")
                        {
                            Label = "→→→",
                            LeftIcon = tmpData.IsMale ? MenuItem.Icon.MALE : MenuItem.Icon.FEMALE,
                            ItemData = tmpData.IsMale
                        };
                        if (defaultChar == "mp_ped_" + name)
                        {
                            btn.LeftIcon = MenuItem.Icon.TICK;
                            btn.Description += " ~g~此角色当前设置为您的默认角色，将在您重生成时使用。";
                        }
                        savedCharactersCategoryMenu.AddMenuItem(btn);
                        MenuController.BindMenuItem(savedCharactersCategoryMenu, manageSavedCharacterMenu, btn);
                    }
                }
            };

            savedCharactersCategoryMenu.OnItemSelect += async (sender, item, index) =>
            {
                switch (index)
                {
                    // Rename Category
                    case 0:
                        var name = await GetUserInput(windowTitle: "输入新类别名称", defaultText: currentCategory.Name, maxInputLength: 30);

                        if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        else if (GetAllCategoryNames().Contains(name) || !string.IsNullOrEmpty(GetResourceKvpString("mp_character_category_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                            return;
                        }

                        string oldName = currentCategory.Name;

                        currentCategory.Name = name;

                        if (StorageManager.SaveJsonData("mp_character_category_" + name, JsonConvert.SerializeObject(currentCategory), false))
                        {
                            StorageManager.DeleteSavedStorageItem("mp_character_category_" + oldName);

                            int totalCount = 0;
                            int updatedCount = 0;
                            List<string> characterNames = GetAllMpCharacterNames();

                            if (characterNames.Count > 0)
                            {
                                foreach (var characterName in characterNames)
                                {
                                    var tmpData = StorageManager.GetSavedMpCharacterData("mp_ped_" + characterName);

                                    if (string.IsNullOrEmpty(tmpData.Category))
                                    {
                                        continue;
                                    }

                                    if (tmpData.Category != oldName)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    tmpData.Category = name;

                                    if (StorageManager.SaveJsonData(tmpData.SaveName, JsonConvert.SerializeObject(tmpData), true))
                                    {
                                        updatedCount++;
                                        Log($"更新了角色 \"{tmpData.SaveName}\" 的类别");
                                    }
                                    else
                                    {
                                        Log($"更新角色 \"{tmpData.SaveName}\" 的类别时出错");
                                    }
                                }
                            }

                           Notify.Success($"您的类别已重命名为 ~g~<C>{name}</C>~s~。 {updatedCount}/{totalCount} 角色已更新。");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("重命名类别时出现问题，您的旧类别不会被删除。");
                        }
                        break;

                    // Change Category Description
                    case 1:
                        var description = await GetUserInput(windowTitle: "输入新类别描述", defaultText: currentCategory.Description, maxInputLength: 120);

                        currentCategory.Description = description;

                        if (StorageManager.SaveJsonData("mp_character_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                        {
                           Notify.Success("您的类别描述已更改。");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("更改类别描述时出现问题。");
                        }
                        break;

                    // Delete Category
                    case 3:
                        if (item.Label == "Are you sure?")
                        {
                            bool deletePeds = (sender.GetMenuItems().ElementAt(4) as MenuCheckboxItem).Checked;

                            item.Label = "";
                            DeleteResourceKvp("mp_character_category_" + currentCategory.Name);

                            int totalCount = 0;
                            int updatedCount = 0;

                            List<string> characterNames = GetAllMpCharacterNames();

                            if (characterNames.Count > 0)
                            {
                                foreach (var characterName in characterNames)
                                {
                                    var tmpData = StorageManager.GetSavedMpCharacterData("mp_ped_" + characterName);

                                    if (string.IsNullOrEmpty(tmpData.Category))
                                    {
                                        continue;
                                    }

                                    if (tmpData.Category != currentCategory.Name)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    if (deletePeds)
                                    {
                                        updatedCount++;

                                        DeleteResourceKvp("mp_ped_" + tmpData.SaveName);
                                    }
                                    else
                                    {
                                        tmpData.Category = "Uncategorized";

                                        if (StorageManager.SaveJsonData(tmpData.SaveName, JsonConvert.SerializeObject(tmpData), true))
                                        {
                                            updatedCount++;
                                            Log($"更新了角色 \"{tmpData.SaveName}\" 的类别");
                                        }
                                        else
                                        {
                                            Log($"更新角色 \"{tmpData.SaveName}\" 的类别时出错");
                                        }
                                    }
                                }
                            }

                            Notify.Success($"您的保存类别已删除。 {updatedCount}/{totalCount} 角色 {(deletePeds ? "已删除" : "已更新")}。");
                            MenuController.CloseAllMenus();
                            UpdateSavedPedsMenu();
                            savedCharactersMenu.OpenMenu();
                        }
                        else
                        {
                            item.Label = "Are you sure?";
                        }
                        break;

                    // Load saved character menu
                    default:
                        List<string> categoryNames = GetAllCategoryNames();
                        List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
                        int nameIndex = categoryNames.IndexOf(currentCategory.Name);

                        setCategoryBtn.ItemData = categoryIcons;
                        setCategoryBtn.ListItems = categoryNames;
                        setCategoryBtn.ListIndex = nameIndex == 1 ? 0 : nameIndex;
                        setCategoryBtn.RightIcon = categoryIcons[setCategoryBtn.ListIndex];
                        selectedSavedCharacterManageName = item.Text;
                        manageSavedCharacterMenu.MenuSubtitle = item.Text;
                        manageSavedCharacterMenu.CounterPreText = $"{(item.LeftIcon == MenuItem.Icon.MALE ? "(Male)" : "(Female)")} ";
                        manageSavedCharacterMenu.RefreshIndex();
                        break;
                }
            };

            // Change Category Icon
            savedCharactersCategoryMenu.OnDynamicListItemSelect += (_, _, currentItem) =>
            {
                var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();
                int iconIndex = iconNames.IndexOf(currentItem);

                currentCategory.Icon = (MenuItem.Icon)iconIndex;

                if (StorageManager.SaveJsonData("mp_character_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                {
                    Notify.Success($"您的分类图标已更改为 ~g~<C>{iconNames[iconIndex]}</C>~s~.");
                    UpdateSavedPedsMenu();
                }
                else
                {
                    Notify.Error("更改分类图标时出现问题。");
                }
            };
        }

        /// <summary>
        /// Updates the saved peds menu.
        /// </summary>
        private void UpdateSavedPedsMenu()
        {
            var categories = GetAllCategoryNames();

            savedCharactersMenu.ClearMenuItems();

            var createCategoryBtn = new MenuItem("创建分类", "创建一个新的角色分类。")
            {
                Label = "→→→"
            };
            savedCharactersMenu.AddMenuItem(createCategoryBtn);

            var spacer = GetSpacerMenuItem("↓ 角色分类 ↓");
            savedCharactersMenu.AddMenuItem(spacer);

            var uncategorized = new MpCharacterCategory
            {
                Name = "Uncategorized",
                Description = "所有未分配到分类的保存MP角色。"
            };
            var uncategorizedBtn = new MenuItem(uncategorized.Name, uncategorized.Description)
            {
                Label = "→→→",
                ItemData = uncategorized
            };
            savedCharactersMenu.AddMenuItem(uncategorizedBtn);
            MenuController.BindMenuItem(savedCharactersMenu, savedCharactersCategoryMenu, uncategorizedBtn);

            // Remove "Create New" and "Uncategorized"
            categories.RemoveRange(0, 2);

            if (categories.Count > 0)
            {
                categories.Sort((a, b) => a.ToLower().CompareTo(b.ToLower()));
                foreach (var item in categories)
                {
                    MpCharacterCategory category = StorageManager.GetSavedMpCharacterCategoryData("mp_character_category_" + item);

                    var btn = new MenuItem(category.Name, category.Description)
                    {
                        Label = "→→→",
                        LeftIcon = category.Icon,
                        ItemData = category
                    };
                    savedCharactersMenu.AddMenuItem(btn);
                    MenuController.BindMenuItem(savedCharactersMenu, savedCharactersCategoryMenu, btn);
                }
            }

            savedCharactersMenu.RefreshIndex();
        }

        private List<string> GetAllCategoryNames()
        {
            var categories = new List<string>();
            var handle = StartFindKvp("mp_character_category_");
            while (true)
            {
                var foundCategory = FindKvp(handle);
                if (string.IsNullOrEmpty(foundCategory))
                {
                    break;
                }
                else
                {
                    categories.Add(foundCategory.Substring(22));
                }
            }
            EndFindKvp(handle);

            categories.Insert(0, "Create New");
            categories.Insert(1, "Uncategorized");

            return categories;
        }

        private List<MenuItem.Icon> GetCategoryIcons(List<string> categoryNames)
        {
            List<MenuItem.Icon> icons = new List<MenuItem.Icon> { };

            foreach (var name in categoryNames)
            {
                icons.Add(StorageManager.GetSavedMpCharacterCategoryData("mp_character_category_" + name).Icon);
            }

            return icons;
        }

        private List<string> GetAllMpCharacterNames()
        {
            var names = new List<string>();
            var handle = StartFindKvp("mp_ped_");
            while (true)
            {
                var foundName = FindKvp(handle);
                if (string.IsNullOrEmpty(foundName))
                {
                    break;
                }
                else
                {
                    names.Add(foundName.Substring(7));
                }
            }
            EndFindKvp(handle);

            return names;
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

        public struct MpCharacterCategory
        {
            public string Name;
            public string Description;
            public MenuItem.Icon Icon;
        }
    }
}
