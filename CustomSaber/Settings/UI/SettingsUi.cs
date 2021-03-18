﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;

namespace CustomSaber.Settings.UI
{
    internal class SettingsUI
    {
        public static SabersFlowCoordinator sabersFlowCoordinator;
        public static bool created = false;

        public static void CreateMenu()
        {
            if (!created)
            {
                MenuButton menuButton = new MenuButton("Custom Saber", "Change your sabers and some other stuff!", SabersMenuButtonPressed, true);
                MenuButtons.instance.RegisterButton(menuButton);

                created = true;
            }
        }

        public static void ShowSaberFlow()
        {
            if (sabersFlowCoordinator == null)
            {
                sabersFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<SabersFlowCoordinator>();
            }

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(sabersFlowCoordinator);
        }

        private static void SabersMenuButtonPressed() => ShowSaberFlow();
    }
}
