using COILib.General;
using COILib.INI;
using COILib.INI.Config;
using Mafi.Collections;
using UnityEngine;

namespace COIModManager {

    internal class ConfigManager : AConfigManager<ConfigManager, ConfigManager.Keys> {

        public enum Keys {
            Settings_GitHub_Token,
            KeyCodes_open_menu
        }

        public IniKeyData<KeyCode> KeyCodes_openmenu { get; private set; }

        public ConfigManager() : base(Static.GetCallingAssemblyLocation("Config.ini")) {
        }

        protected override Dict<Keys, AIniKeyData> GetKeyValues() {
            IniSectionData Settings = new IniSectionData(loader, "Settings");
            IniSectionData KeyCodes = new IniSectionData(loader, "KeyCodes", GenerateAcceptedKeyCodesComment(AcceptedKeyCodes));
            return new Dict<Keys, AIniKeyData> {
              { Keys.Settings_GitHub_Token, new IniKeyData<string>(loader, Settings,  "github_token", "", "A github token to increase the rate limits.\nCheck https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api for more info.") },
              { Keys.KeyCodes_open_menu, new IniKeyData<KeyCode>(loader, KeyCodes,  "open_menu", fAcceptedKeyCodes(), KeyCode.F2, "KeyCode to open the dev menu.\nDefault: F2") },
            };
        }
    }
}