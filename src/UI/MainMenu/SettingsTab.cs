using Mafi;
using Mafi.Unity;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace COIModManager.UI.MainMenu {

    internal class SettingsTab : Column, ITab {

        public SettingsTab() {
            this.Gap(new Px?(2.pt())).MarginLeftRight(Px.Auto).AlignItemsStretch();
            this.Add(new Label(Tr.Menu__OpenSettings).UpperCase(false));
        }

        void ITab.Activate() {
        }

        void ITab.Deactivate() {
        }
    }
}