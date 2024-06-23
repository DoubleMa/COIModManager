using COILib.Extensions;
using COILib.Patcher;
using COIModManager.UI.MainMenu;
using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.MainMenu;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UserInterface;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace COIModManager.Patcher {

    internal class MainMenuScreenPatcher : APatcher<MainMenuScreenPatcher> {
        protected override bool DefaultState => true;
        protected override bool Enabled => true;
        private static ModManagerWindow _modManagerWindow;
        private static MainMenuScreen _instance;

        public MainMenuScreenPatcher() : base("MainMenuScreenPatcher") {
            MethodBase constructor = typeof(MainMenuScreen).ATGetConstructor([typeof(IMain), typeof(IFileSystemHelper), typeof(UiBuilder), typeof(PreInitModsAndProtos), typeof(DependencyResolver), typeof(Option<string>), typeof(Action)]);
            AddMethod(constructor, PrefixAllow, typeof(MainMenuScreenPatcher).GetHarmonyMethod(nameof(Postfix)));
        }

        public static void Postfix(MainMenuScreen __instance) {
            _instance = __instance;
            Panel panel = FindPanel(__instance);
            if (panel != null) ReorderAndInjectButton(panel);
        }

        private static void HandleOpenModManager() {
            if (_instance is null) return;
            if (_modManagerWindow is not null) {
                if (_modManagerWindow.IsVisible()) _modManagerWindow.CloseSelf();
                _modManagerWindow.RemoveFromHierarchy();
            }
            _modManagerWindow = [];
            _instance.RunWithBuilder(bld => bld.AddComponent(_modManagerWindow));
            _instance.GetField<Set<Window>>("m_childWindows").Add(_modManagerWindow);
            _modManagerWindow.SetVisible(true);
        }

        private static Panel FindPanel(UiComponent component) {
            foreach (var child in component.AllChildren) {
                if (child is Panel panel) return panel;
                else if (child is UiComponent or Column) {
                    var result = FindPanel(child);
                    if (result != null) return result;
                }
            }
            return null;
        }

        private static void ReorderAndInjectButton(Panel panel) {
            var buttons = new List<UiComponent>();
            foreach (var child in panel.AllChildren) if (child is ButtonBold || child is Button) buttons.Add(child);
            panel.Clear();
            var newButton = new ButtonBold(new LocStrFormatted("Mod Manager"), new Action(() => HandleOpenModManager()));
            int insertPosition = 3;

            if (insertPosition >= buttons.Count) panel.Add(newButton);
            else
                for (int i = 0; i < buttons.Count; i++) {
                    panel.Add(buttons[i]);
                    if (i == insertPosition) panel.Add(newButton);
                }
        }
    }
}