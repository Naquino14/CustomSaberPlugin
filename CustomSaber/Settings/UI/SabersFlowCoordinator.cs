using BeatSaberMarkupLanguage;
using HMUI;
using System;

namespace CustomSaber.Settings.UI
{
    internal class SabersFlowCoordinator : FlowCoordinator
    {
        private SaberListViewController saberListView;
        private SaberSettingsViewController saberSettingsView;

        public void Awake()
        {

            if (!saberSettingsView)
            {
                saberSettingsView = BeatSaberUI.CreateViewController<SaberSettingsViewController>();
            }

            if (!saberListView)
            {
                saberListView = BeatSaberUI.CreateViewController<SaberListViewController>();
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                if (firstActivation)
                {
                    SetTitle("Custom Sabers");
                    showBackButton = true;
                }
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            // Dismiss ourselves
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
