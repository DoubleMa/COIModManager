using COILib.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace COIModManager.ModManager {

    [Serializable]
    public class RepoModDataWarpper : SerializableObject<RepoModDataWarpper> {
        public List<ModData> ModsData = new List<ModData>();
        public List<RepoData> ReposData = new List<RepoData>();

        public RepoModDataWarpper() : this(null, null) {
        }

        public RepoModDataWarpper(List<RepoData> ReposData, List<ModData> ModsData) {
            this.ModsData = ModsData ?? new List<ModData>();
            this.ReposData = ReposData ?? new List<RepoData>();
        }
    }

    public interface IBaseData {
        string Name { get; set; }
        string Description { get; set; }
        List<string> Authors { get; set; }
        DateTime LastUpdated { get; set; }
        DateTime CurrentDownloaded { get; set; }
        string ImagePath { get; set; }

        bool Downloaded();

        bool Enabled();

        bool Updated();
    }

    [Serializable]
    public class RepoData : SerializableObject<RepoData>, IBaseData {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Authors { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CurrentDownloaded { get; set; }
        public string ImagePath { get; set; }
        public string RepoUrl;
        public string DefaultBranch;
        public List<RepoAsset> Assets;

        public bool Updated() => CurrentDownloaded == LastUpdated;

        public bool Downloaded() {
            bool isDownloaded = true;
            Assets.ForEach(a => {
                if (a.Mods != null) a.Mods.ToList().ForEach(m => isDownloaded = isDownloaded && File.Exists(Path.Combine(ModsManager.Download_Folder, m)));
                else isDownloaded = false;
            });
            return isDownloaded;
        }

        public bool Enabled() => true;

        public void Clone(RepoData repo) {
            if (repo == null) return;
            Name = repo.Name;
            Description = repo.Description;
            Authors = new List<string>(repo.Authors);
            LastUpdated = repo.LastUpdated;
            CurrentDownloaded = repo.CurrentDownloaded;
            ImagePath = repo.ImagePath;
            RepoUrl = repo.RepoUrl;
            DefaultBranch = repo.DefaultBranch;
            Assets = repo.Assets;
        }
    }

    [Serializable]
    public class ModData : SerializableObject<ModData>, IBaseData {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<string> Authors { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CurrentDownloaded { get; set; }
        public string ImagePath { get; set; }
        public HashSet<string> IgnoreFiles { get; set; }

        public bool Updated() => CurrentDownloaded == LastUpdated;

        public bool Downloaded() => ModsManager.CheckModDownloaded(this);

        public bool Enabled() => ModsManager.CheckModEnabled(this);

        public void InitConfig() {
            ModConfig modConfig = ModsManager.GetModConfig(this);
            DisplayName = modConfig.DisplayName;
            Authors = modConfig.Authors;
            Description = modConfig.Description;
            IgnoreFiles = modConfig.IgnoreFiles ?? new HashSet<string>();
            if (!IgnoreFiles.Contains("ModManager")) IgnoreFiles.Add("ModManager");
            var imagePath = ModsManager.GetModImagePath(this);
            if (File.Exists(imagePath)) ImagePath = imagePath;
        }

        public void Clone(ModData mod) {
            if (mod == null) return;
            Name = mod.Name;
            Description = mod.Description;
            Authors = new List<string>(mod.Authors);
            LastUpdated = mod.LastUpdated;
            CurrentDownloaded = mod.CurrentDownloaded;
            ImagePath = mod.ImagePath;
            IgnoreFiles = new HashSet<string>(mod.IgnoreFiles);
        }
    }

    [Serializable]
    public class RepoAsset {
        public string Name { get; set; }
        public string BrowserDownloadUrl { get; set; }
        public HashSet<string> Mods { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    [Serializable]
    public class ModConfig : SerializableObject<ModConfig> {
        public string DisplayName { get; set; }
        public List<string> Authors { get; set; }
        public string Description { get; set; }
        public HashSet<string> IgnoreFiles { get; set; }
    }

    [Serializable]
    public class ModManagerData : SerializableObject<ModManagerData> {
        public List<string> GitHubRepo { get; set; } = new List<string>();
    }
}