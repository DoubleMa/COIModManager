using COILib.General;
using COILib.INI;
using COILib.INI.Lang;
using Mafi.Collections;

namespace COIModManager {

    public class LangManager : ALangManager<LangManager, LangManager.Keys> {

        public enum Keys {
            tr_add_btn,
            tr_refresh_btn,
            tr_enable_btn,
            tr_disable_btn,
            tr_download_btn,
            tr_update_btn,
            tr_update_all_btn,
            tr_confirm_btn,
            tr_cancel_btn,
            tr_description_title,
            tr_recently_updated_title,
            tr_last_month_title,
            tr_old_title,
            tr_url_dialog_title,
            tr_delete_title,
            tr_loading_text,
            tr_delete_text,
            tr_enter_url_placeholder,
        }

        public LangManager() : base(Static.GetCallingAssemblyLocation("Lang.ini")) {
        }

        protected override Dict<Keys, IniKeyData<string>> GetKeyValues() {
            var tag = "General";
            return new Dict<Keys, IniKeyData<string>> {
                { Keys.tr_add_btn, new IniKeyData<string>(loader, LangSection, "add_btn", "add", "title of a button that add a mod", tag) },
                { Keys.tr_refresh_btn, new IniKeyData<string>(loader, LangSection, "refresh_btn", "refresh", "title of a button to refresh the mod list", tag) },
                { Keys.tr_enable_btn, new IniKeyData<string>(loader, LangSection, "enable_btn", "enable", "title of a button that enable a mod", tag) },
                { Keys.tr_disable_btn, new IniKeyData<string>(loader, LangSection, "disable_btn", "disable", "title of a button that disable a mod", tag) },
                { Keys.tr_download_btn, new IniKeyData<string>(loader, LangSection, "download_btn", "download", "title of a button that download a mod", tag) },
                { Keys.tr_update_btn, new IniKeyData<string>(loader, LangSection, "update_btn", "update", "title of a button that update a mod", tag) },
                { Keys.tr_update_all_btn, new IniKeyData<string>(loader, LangSection, "update_all_btn", "update all", "title of a button that update all downloaded mods", tag) },
                { Keys.tr_confirm_btn, new IniKeyData<string>(loader, LangSection, "confirm_btn", "confirm", "title of a button that will confirm the entry of a dialog.", tag) },
                { Keys.tr_cancel_btn, new IniKeyData<string>(loader, LangSection, "cancel_btn", "cancel", "title of a button that will close a dialog.", tag) },
                { Keys.tr_description_title, new IniKeyData<string>(loader, LangSection, "description_title", "description", "title for the description of a mod", tag) },
                { Keys.tr_recently_updated_title, new IniKeyData<string>(loader, LangSection, "recently_updated_title", "recently updated", "title for recently updated mods", tag) },
                { Keys.tr_last_month_title, new IniKeyData<string>(loader, LangSection, "last_month_title", "last month", "title for mods updated last month", tag) },
                { Keys.tr_old_title, new IniKeyData<string>(loader, LangSection, "old_title", "old", "title for mods updated more than one month ago", tag) },
                { Keys.tr_url_dialog_title, new IniKeyData<string>(loader, LangSection, "url_dialog_title", "enter url", "title for the dialog to enter a url", tag) },
                { Keys.tr_delete_title, new IniKeyData<string>(loader, LangSection, "delete_title", "delete", "title for deleting an item", tag) },
                { Keys.tr_loading_text, new IniKeyData<string>(loader, LangSection, "loading_text", "loading", "text to show when the mod list is still loading", tag)  },
                { Keys.tr_delete_text, new IniKeyData<string>(loader, LangSection, "delete_text", "are you sure you want do delete {0}?", "text to confirm if you want to delete {1}", tag)  },
                { Keys.tr_enter_url_placeholder, new IniKeyData<string>(loader, LangSection, "enter_url_placeholder", "enter a github url...", "placeholder for the textfield in the url dialog", tag)  }
            };
        }
    }
}