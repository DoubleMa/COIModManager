using COILib.Extensions;
using COILib.General;
using COILib.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace COIModManager.ModManager {

    internal class ModsManager : InstanceObject<ModsManager> {
        private static readonly string AssemblyLocation = Static.GetCallingAssemblyLocation();
        public static readonly string Base_Dir = AssemblyLocation.Substring(0, AssemblyLocation.LastIndexOf("\\"));
        public static readonly string Download_Folder = Path.Combine(Base_Dir, ".downloaded");
        public static string Mods_Json_Path = Path.Combine(Download_Folder, "mods.json");

        public static string GetDownloadedModFolder(ModData mod) => Path.Combine(Download_Folder, mod.Name);

        public static string GetModFolder(ModData mod) => Path.Combine(Base_Dir, mod.Name);

        public static string GetModConfigFolder(ModData mod) => Path.Combine(GetDownloadedModFolder(mod), "ModManager");

        public static bool CheckModDownloaded(ModData mod) => File.Exists(Path.Combine(GetDownloadedModFolder(mod), mod.Name + ".dll"));

        public static bool CheckModEnabled(ModData mod) => File.Exists(Path.Combine(GetModFolder(mod), mod.Name + ".dll"));

        public static ModConfig GetModConfig(ModData mod) => ModConfig.Deserialize(Path.Combine(GetModConfigFolder(mod), "config.json"), new ModConfig() { DisplayName = mod.Name, Authors = mod.Authors, Description = mod.Description });

        public static string GetModImagePath(ModData mod) => Path.Combine(GetModConfigFolder(mod), "icon.png");

        private readonly int LimitRefreshRatePerMinute = 10;
        private DateTime lastRefresh;
        private RepoModDataWarpper savedData;
        private readonly ActionVariable<bool> isLoading = new(false);
        public Action<RepoModDataWarpper> DataChangedEvent;

        public RepoModDataWarpper GetData() => savedData;

        public bool IsLoading() => isLoading.Value;

        public void AddLoadingEvent(Action<bool> action) => isLoading.Event += action;

        public void RemoveLoadingEvent(Action<bool> action) => isLoading.Event -= action;

        protected override void OnInit() {
            Refresh();
        }

        private RepoData FindRepo(string url) => savedData.ReposData.FirstOrDefault(x => x.RepoUrl == url);

        private ModData FindMod(string name) => savedData.ModsData.FirstOrDefault(x => x.Name == name);

        //ToDo: Maybe I should just save it in the mod
        public RepoData FindModRepo(ModData modData) => savedData.ReposData.FirstOrDefault(r => r.Assets.FindIndex(a => a.Mods != null && a.Mods.ToList().IndexOf(modData.Name) > 0) > 0);

        private void HandleDataChanged(bool save = true, bool trigger = true, bool sort = true) {
            if (sort) {
                savedData.ReposData.Sort((a, b) => b.LastUpdated.CompareTo(a.LastUpdated));
                savedData.ModsData.Sort((a, b) => b.LastUpdated.CompareTo(a.LastUpdated));
            }
            if (save) Static.TryRun(() => savedData.Json(Mods_Json_Path));
            if (trigger) DataChangedEvent?.Invoke(savedData);
        }

        public async void Refresh() {
            if (!Lock()) return;
            savedData = RepoModDataWarpper.Deserialize(Mods_Json_Path, null);
            string[] repoUrls;
            if (savedData == null) {
                savedData = new RepoModDataWarpper();
                repoUrls = [.. (await FetchModMangerFromUrl()).GitHubRepo];
            }
            else repoUrls = savedData.ReposData.Select(e => e.RepoUrl).ToArray();
            if (lastRefresh == null || lastRefresh < DateTime.Now.AddMinutes(-LimitRefreshRatePerMinute)) {
                lastRefresh = DateTime.Now;
                foreach (var url in repoUrls) await UpdateRepoData(url);
            }
            else ExtLog.Info("Limited");
            foreach (var mod in savedData.ModsData) mod.InitConfig();
            UnLockAndSave();
        }

        private async Task<ModManagerData> FetchModMangerFromUrl() {
            using HttpClient client = new();
            return ModManagerData.DeserializeNew(await client.GetStringAsync("https://demo.doubleoutsource.com/COI/ModManager.json"));
        }

        public async void AddRepo(string repoUrl) {
            if (!Lock()) return;
            if (!GithubManager.Instance.IsValidRepoUrl(repoUrl) || FindRepo(repoUrl) != null) return;
            await UpdateRepoData(repoUrl);
            UnLockAndSave();
        }

        private async Task UpdateRepoData(string repoUrl) {
            await Static.TryRunTask(() => {
                var repoData = GithubManager.Instance.GetRepoData(repoUrl);
                var savedRepo = FindRepo(repoUrl);
                if (savedRepo != null) {
                    if (savedRepo.Downloaded())
                        repoData.CurrentDownloaded = savedRepo.CurrentDownloaded;
                    foreach (var asset in savedRepo.Assets) {
                        if (asset.Mods != null)
                            foreach (var mod in asset.Mods) {
                                var savedMod = FindMod(mod);
                                if (savedMod != null) savedMod.LastUpdated = repoData.LastUpdated;
                            }
                    }
                    savedRepo.Clone(repoData);
                }
                else savedData.ReposData.Add(repoData);
            });
        }

        public async void DownloadRepo(RepoData repo) {
            if (!Lock()) return;
            await Static.TryRunTask(() => {
                GithubManager.Instance.DownloadRepo(repo);
                List<RepoAsset> assets = repo.Assets;
                if (assets != null && assets.Count > 0) {
                    foreach (var asset in assets) {
                        if (asset.Mods == null) continue;
                        foreach (var name in asset.Mods) {
                            var mod = FindMod(name);
                            if (mod == null) {
                                mod = new ModData() {
                                    Name = name,
                                    Authors = repo.Authors,
                                    Description = repo.Description,
                                    LastUpdated = asset.LastUpdated,
                                    CurrentDownloaded = asset.LastUpdated,
                                    ImagePath = repo.ImagePath
                                };
                                mod.InitConfig();
                                savedData.ModsData.Add(mod);
                            }
                            else mod.LastUpdated = repo.LastUpdated;
                        }
                    }
                }
            });
            UnLockAndSave();
        }

        public void UpdateAll() {
        }

        public void ToggelMod(ModData mod) {
            Static.TryRun(() => {
                if (mod.Enabled()) DisableMod(mod);
                else EnableMod(mod);
            });
        }

        public void DisableMod(ModData mod) {
            if (!mod.Enabled() || !Lock()) return;
            Static.DeleteFilesAsync(GetModFolder(mod), [.. mod.IgnoreFiles]);
            UnLockAndSave();
        }

        public void EnableMod(ModData mod) {
            if (mod.Enabled() || !Lock()) return;
            ExtLog.Info(mod.IgnoreFiles.ToArray().ToPrint());
            Static.CopyFilesAsync(GetDownloadedModFolder(mod), GetModFolder(mod), [.. mod.IgnoreFiles]);
            UnLockAndSave();
        }

        public void DeleteMod(ModData mod, bool innerCall = false) {
            if (mod == null || (!innerCall && !Lock())) return;
            if (mod.Enabled()) Static.DeleteFilesAsync(GetModFolder(mod), [.. mod.IgnoreFiles]);
            if (mod.Downloaded()) Static.DeleteFilesAsync(GetDownloadedModFolder(mod));
            savedData.ModsData.Remove(mod);
            if (!innerCall) UnLockAndSave();
        }

        public void DeleteRepo(RepoData repo) {
            if (!Lock()) return;
            foreach (var asset in repo.Assets) if (asset.Mods != null) foreach (var mod in asset.Mods) DeleteMod(FindMod(mod), true);
            savedData.ReposData.Remove(repo);
            UnLockAndSave();
        }

        private bool Lock() {
            if (isLoading) return false;
            return isLoading.Value = true;
        }

        private void UnLockAndSave() {
            isLoading.Value = false;
            HandleDataChanged();
        }
    }
}