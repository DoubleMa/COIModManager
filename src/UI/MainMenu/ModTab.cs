using COILib.General;
using COILib.UI.Component;
using COIModManager.ModManager;
using Mafi;
using Mafi.Collections;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using System.Linq;

namespace COIModManager.UI.MainMenu {

    internal class ModsTab : Row, ITab {
        private ScrollColumn _modDetailsScroller;
        private Row _modsContainer;
        private IndeterminateProgressBar _refreshProgressBar;
        private ModButton _selected;
        private List<KeyButtonText> _keyBtnTexts;
        private bool _updating;
        private Dict<IBaseData, ModButton> _btnsMap;
        private readonly UiComponent _parent;
        private Column _overlay;

        public ModsTab(UiComponent parent) {
            _parent = parent;
            AddListeners();
            InitComponents();
        }

        void ITab.Activate() {
        }

        void ITab.Deactivate() {
        }

        private void AddListeners() {
            ModsManager.Instance.AddLoadingEvent(UpdateRefreshProgress);
            ModsManager.Instance.DataChangedEvent += UpdateModList;
        }

        private void InitComponents() {
            _keyBtnTexts = [];
            _btnsMap = [];
            this.AlignItemsStretch().MinHeight(Percent.Hundred);
            var modsPanel = new Column().AlignItemsStretch().Width(380.px());
            var modsTitlePanel = new Row().AlignItemsStart().JustifyItemsSpaceBetween();
            var modsBtnsPanel = new Row().AlignItemsEnd().Gap(3.px()).PaddingRight(20.px());
            modsBtnsPanel.Add(new KeyButtonText(LangManager.Keys.tr_update_all_btn, HandleUpdateAll).Group(_keyBtnTexts).SetEnabled(UpdateAllEnabled));
            modsBtnsPanel.Add(new KeyButtonText(LangManager.Keys.tr_refresh_btn, HandleRefresh).Group(_keyBtnTexts).SetEnabled(RefreshEnabled));
            modsBtnsPanel.Add(new KeyButtonText(LangManager.Keys.tr_add_btn, HandleAddRepo).Group(_keyBtnTexts).SetEnabled(AddEnabled));
            modsTitlePanel.Add(new Title(Tr.ConfigureMods_Action));
            modsTitlePanel.Add(modsBtnsPanel);
            modsPanel.Add(modsTitlePanel);
            modsPanel.Add(new ScrollColumn { (_modsContainer = new Row().AlignItemsStretch().Wrap()) }.PaddingTop(10.px()).Size(Percent.Hundred).ScrollerAlwaysVisible());
            var detailsPanel = new Column().AlignItemsStretch().Fill();
            var detailsTitlePanel = new Row().AlignItemsStart().JustifyItemsSpaceBetween();
            var detailsBtnsPanel = new Row().AlignItemsEnd().Gap(3.px());
            detailsBtnsPanel.PaddingRight(20.px());
            detailsBtnsPanel.Add(new KeyButtonText(LangManager.Keys.tr_download_btn, HandleDownloadOrUpdateMod).Group(_keyBtnTexts).SetEnabled(DownloadlEnabled)
                .SetAlText(LangManager.Keys.tr_update_btn, () => _selected != null && _selected.Data.Downloaded() && !_selected.Data.Updated())
                .SetAltColor(ColorRgba.Green, ColorRgba.Gold));
            detailsBtnsPanel.Add(new KeyButtonText(Tr.BlueprintDelete__Action, HandleDeleteMod).Group(_keyBtnTexts).SetEnabled(DeleteEnabled).SetColor(ColorRgba.Red));
            detailsBtnsPanel.Add(new KeyButtonText(LangManager.Keys.tr_disable_btn, HandleEnableOrDisableMod).Group(_keyBtnTexts).SetEnabled(DisableEnabled)
                .SetAlText(LangManager.Keys.tr_enable_btn, () => _selected != null && !_selected.Data.Enabled())
                .SetAltColor(ColorRgba.Red, ColorRgba.Green));
            detailsTitlePanel.Add(new Title(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_description_title)));
            detailsTitlePanel.Add(detailsBtnsPanel);
            detailsPanel.Add(detailsTitlePanel);
            detailsPanel.Add(_modDetailsScroller = new ScrollColumn().PaddingTop(10.px()).PaddingLeftRight(5.px()).AlignItemsStretch());
            _overlay = new Column().AbsolutePosition(0.Percent()).Size(Percent.Hundred);
            _overlay.GetBackgroundDecorator().SetBackground(ColorRgba.Black.SetA(150));
            Add(new Column { (_refreshProgressBar = new IndeterminateProgressBar().Width(Percent.Hundred)).MarginBottom(10.px()), new Row { modsPanel.Height(Percent.Hundred), detailsPanel.Height(Percent.Hundred) }.Size(Percent.Hundred), _overlay }.Size(Percent.Hundred));
            _refreshProgressBar.SetVisible(false);
            _overlay.SetVisible(false);
            UpdateModList(ModsManager.Instance.GetData());
        }

        private bool AddEnabled() => !ModsManager.Instance.IsLoading();

        private bool RefreshEnabled() => true;

        private bool UpdateAllEnabled() => !ModsManager.Instance.IsLoading() && ModsManager.Instance.GetData().ModsData.FindIndex(e => !e.Updated()) >= 0;

        private bool DownloadlEnabled() => _selected != null && !ModsManager.Instance.IsLoading() && (_selected.Data is RepoData) && (!_selected.Data.Downloaded() || !_selected.Data.Updated());

        private bool DeleteEnabled() => _selected != null && !ModsManager.Instance.IsLoading() && (_selected.Data is RepoData || _selected.Data.Downloaded());

        private bool DisableEnabled() => _selected != null && !ModsManager.Instance.IsLoading() && _selected.Data.Downloaded() && _selected.Data is not RepoData;

        private void HandleRefresh() => ModsManager.Instance.Refresh();

        private void HandleAddRepo() {
            new TextFieldDialog(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_url_dialog_title))
                .SetPlaceHolder(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_enter_url_placeholder, COILib.INI.Lang.StringType.Capitalize))
                .OnConfirm((text) => ModsManager.Instance.AddRepo(text))
                .SetConfirmText(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_confirm_btn))
                .SetCancelText(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_cancel_btn))
                .Show(_parent);
        }

        private void HandleUpdateAll() => ModsManager.Instance.UpdateAll();

        private void HandleDownloadOrUpdateMod() {
            if (_selected == null) return;
            if (_selected.Data is RepoData data) ModsManager.Instance.DownloadRepo(data);
        }

        private void HandleDeleteMod() {
            if (_selected == null) return;
            new ConfirmDialog(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_delete_title))
               .SetText(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_delete_text, COILib.INI.Lang.StringType.Capitalize, _selected.Data.Name))
               .SetConfirmText(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_confirm_btn))
               .SetCancelText(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_cancel_btn))
               .OnConfirm(() => {
                   if (_selected.Data is ModData data) ModsManager.Instance.DeleteMod(data);
                   else ModsManager.Instance.DeleteRepo((RepoData)_selected.Data);
               }).Show(_parent);
        }

        private void HandleEnableOrDisableMod() {
            if (_selected == null || _selected.Data is RepoData) return;
            ModsManager.Instance.ToggelMod((ModData)_selected.Data);
        }

        private void UpdateRefreshProgress(bool isLoading) {
            _refreshProgressBar.SetVisible(isLoading);
            //_overlay.SetVisible(isLoading);
            CheckAllBtns();
        }

        private void UpdateModList(RepoModDataWarpper dataWarpper) {
            if (_updating) return;
            CheckAllBtns();
            if (ModsManager.Instance.IsLoading()) return;
            UpdateRefreshProgress(true);
            _updating = true;
            _modsContainer.Clear();
            var recently = new List<IBaseData>();
            var lastMonth = new List<IBaseData>();
            var old = new List<IBaseData>();
            foreach (var mod in dataWarpper.ModsData) {
                if (mod.LastUpdated >= DateTime.Now.AddMonths(-1)) recently.Add(mod);
                else if (mod.LastUpdated >= DateTime.Now.AddMonths(-2)) lastMonth.Add(mod);
                else old.Add(mod);
            }
            _modsContainer.Add(CreateTitledModButtonPanel(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_recently_updated_title), recently, SelectMod));
            _modsContainer.Add(CreateTitledModButtonPanel(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_last_month_title), lastMonth, SelectMod));
            _modsContainer.Add(CreateTitledModButtonPanel(LangManager.Instance.GetAsLoc(LangManager.Keys.tr_old_title), old, SelectMod));
            _modsContainer.Add(CreateTitledModButtonPanel("GitHub".AsLoc(), new List<IBaseData>(dataWarpper.ReposData), SelectMod));
            if (_btnsMap.Count > 0) SelectMod(_btnsMap.First().Value);
            _updating = false;
            UpdateRefreshProgress(false);
        }

        private void SelectMod(ModButton button = null) {
            _selected?.Selected(false);
            _selected = button;
            _selected?.Selected(true);
            UpdateDetails();
            CheckAllBtns();
        }

        private void UpdateDetails() {
            _modDetailsScroller.Clear();
            if (_selected == null) return;
            _modDetailsScroller.Add(new Label(_selected.Data.Name.AsLoc()).UpperCase(false));
            _modDetailsScroller.Add(new Label(_selected.Data.Description.AsLoc()).UpperCase(false));
        }

        private void CheckAllBtns() {
            foreach (var btn in _keyBtnTexts) btn.Check();
        }

        private TitledModButtonPanel CreateTitledModButtonPanel(LocStrFormatted title, List<IBaseData> mods, Action<ModButton> onClick) {
            List<ModButton> modsbtns = [];
            _btnsMap.Clear();
            foreach (var mod in mods) {
                var btn = new ModButton(onClick, mod);
                _btnsMap.Add(mod, btn);
                modsbtns.Add(btn);
            }
            return new TitledModButtonPanel(title, modsbtns);
        }
    }

    public class TitledModButtonPanel : TitledPanel {
        private readonly Row _row;

        public TitledModButtonPanel(LocStrFormatted title, List<IBaseData> mods, Action<ModButton> onClick) : base(title, new Row()) {
            _row = ((Row)AllChildren.Last()).AlignItemsStretch().Wrap().Width(Percent.Hundred);
            foreach (var mod in mods) _row.Add(new ModButton(onClick, mod));
            if (mods.Count == 0) SetVisible(false);
        }

        public TitledModButtonPanel(LocStrFormatted title, List<ModButton> modsbtns) : base(title, new Row()) {
            _row = ((Row)AllChildren.Last()).AlignItemsStretch().Wrap().Width(Percent.Hundred);
            _row.Add(modsbtns);
            if (modsbtns.Count == 0) SetVisible(false);
        }

        public ModButton GetFirst() => _row.AllChildren.OfType<ModButton>().FirstOrDefault();
    }

    public class TitledPanel : Column {

        public TitledPanel(LocStrFormatted title, UiComponent component) : base() {
            this.Width(Percent.Hundred);
            this.MarginTopBottom(10.px());
            this.Gap(10.px());
            Add(new Title(title));
            Add(component);
        }
    }

    public class ModButton : ButtonColumn {
        public IBaseData Data { get; }

        public ModButton(Action<ModButton> onClick, IBaseData data) : base() {
            Data = data;
            this.Variant<ButtonColumn>(ButtonVariant.Area);
            this.Padding<ButtonColumn>(8.px());
            Add(COILib.UI.Utils.GetImage(data.ImagePath ?? Static.GetCallingAssemblyLocation("Resources", "Images", "github.png"), 100.px(), 100.px()));
            Add(new Label((data is ModData modData ? modData.DisplayName : data.Name).AsLoc()).UpperCase(false).TextOverflow(TextOverflow.Ellipsis).OverflowHidden().AlignTextCenter().Width(100.px()).MarginTop(5.px()));
            this.OnClick(() => onClick(this));
        }
    }

    public class KeyButtonText : ButtonText {
        private readonly LocStrFormatted loc;
        private Func<bool> enabledGetter;
        private Func<bool> altChecker;
        private LocStrFormatted altLoc;
        private ColorRgba? color;
        private ColorRgba? altColor;

        public KeyButtonText(LangManager.Keys key, Action onClick) : this(LangManager.Instance.GetAsLoc(key), onClick) {
        }

        public KeyButtonText(LocStrFormatted loc, Action onClick) : base(loc, onClick) {
            this.loc = loc;
            SetEnabledInternal(false);
        }

        public KeyButtonText SetAlText(LangManager.Keys altKey, Func<bool> altChecker) => SetAltText(LangManager.Instance.GetAsLoc(altKey), altChecker);

        public KeyButtonText SetAltText(LocStrFormatted altLoc, Func<bool> altChecker) {
            this.altLoc = altLoc;
            this.altChecker = altChecker;
            return this;
        }

        public KeyButtonText SetColor(ColorRgba color) {
            this.color = color;
            SetColorInternal(color);
            return this;
        }

        public KeyButtonText SetAltColor(ColorRgba color, ColorRgba altColor) {
            this.color = color;
            this.altColor = altColor;
            return this;
        }

        public KeyButtonText SetEnabled(Func<bool> enabledGetter) {
            this.enabledGetter = enabledGetter;
            return this;
        }

        public KeyButtonText Check() {
            bool enabled = enabledGetter == null || enabledGetter.Invoke();
            SetEnabledInternal(enabled);
            ColorRgba? tempColor = color ?? ColorRgba.White;
            tempColor = !enabled ? ColorRgba.Gray : altChecker != null && altChecker.Invoke() ? altColor : tempColor;
            SetColorInternal(tempColor);
            this.Text(altChecker != null && altLoc != null && altChecker.Invoke() ? altLoc : loc);
            return this;
        }

        public KeyButtonText Group(List<KeyButtonText> list) {
            list.Add(this);
            return this;
        }
    }
}