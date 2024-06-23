using Mafi;
using Mafi.Core;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using UnityEngine;

namespace COIModManager.UI.MainMenu {

    internal class ModManagerWindow : Window {
        private readonly TabContainer _tabContainer;

        private readonly SettingsTab _settingsTab;
        private readonly ModsTab _modsTab;

        public ModManagerWindow(bool darkMask = false) : base("COI Mod Manager".AsLoc(), darkMask) {
            this.Size(1400.px(), 1000.px()).Grow(1400f, 1000f);
            _tabContainer = new TabContainer();
            _settingsTab = new SettingsTab();
            _modsTab = new ModsTab(_tabContainer);
            _tabContainer.Add(c => c.RootPanel().Panel.Fill());
            _tabContainer.Add((LocStrFormatted)Tr.ConfigureMods_Action, Assets.Unity.UserInterface.General.Configure_svg, _modsTab, Scroll.No);
            _tabContainer.Add((LocStrFormatted)TrCore.NewGameWizard__Customization, Assets.Unity.UserInterface.General.Working128_png, _settingsTab, Scroll.No);
            _tabContainer.OnTabActivate(new Action(onTabActivate));
            Body.Add(_tabContainer);
        }

        private void onTabActivate() {
        }

        public override bool InputUpdate() {
            if (!IsVisible()) return false;
            if (Input.GetKeyDown(KeyCode.Escape)) {
                HandleClose();
                return true;
            }
            return false;
        }

        internal void CloseSelf() => HandleClose();

        protected override void HandleClose() {
            base.HandleClose();
        }
    }
}