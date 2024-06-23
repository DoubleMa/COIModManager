using COILib.Extensions;
using COILib.General;
using COILib.Logging;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace COIModManager.ModManager {

    internal class GithubManager : InstanceObject<GithubManager> {

        protected override void OnInit() {
        }

        public bool IsValidRepoUrl(string repoUrl) {
            return Uri.TryCreate(repoUrl, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                && uriResult.Host == "github.com"
                && uriResult.Segments.Length >= 3;
        }

        public RepoData GetRepoData(string repoUrl) {
            var client = new GitHubClient(new ProductHeaderValue("GitHubManager"), new CredentialStore());
            var uri = new Uri(repoUrl);
            var segments = uri.Segments;
            var owner = segments[1].Trim('/');
            var repoName = segments[2].Trim('/');
            var repo = client.Repository.Get(owner, repoName).Result;
            var latest = client.Repository.Release.GetLatest(owner, repoName).Result;
            var Authors = new List<string>() { repo.Owner.Name };
            var Assets = new List<RepoAsset>();
            foreach (var asset in latest.Assets) Assets.Add(new RepoAsset { Name = asset.Name, BrowserDownloadUrl = asset.BrowserDownloadUrl, CreatedAt = asset.CreatedAt.Date, LastUpdated = asset.UpdatedAt.Date });
            return new RepoData {
                Name = repo.Name,
                Authors = Authors,
                Description = repo.Description,
                DefaultBranch = repo.DefaultBranch,
                LastUpdated = latest.CreatedAt.Date,
                CurrentDownloaded = latest.PublishedAt?.Date ?? latest.CreatedAt.Date,
                Assets = Assets,
                RepoUrl = repoUrl,
                ImagePath = repo.Owner.AvatarUrl
            };
        }

        public bool DownloadRepo(RepoData mod) {
            return DownloadAssets(mod.Assets);
        }

        public bool DownloadAssets(List<RepoAsset> Assets) {
            bool result = true;
            foreach (var asset in Assets) {
                bool temp = DownloadLatestRelease(asset);
                if (temp) ExtLog.Info($"Downloaded {asset.Name}.");
                else ExtLog.Error($"Failed to download {asset.Name}.");
                result = result && temp;
            }
            return result;
        }

        private bool DownloadLatestRelease(RepoAsset asset, string path = null) {
            return Static.TryRun(() => {
                var result = true;
                if (path is null) path = ModsManager.Download_Folder;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var zipFilePath = Path.Combine(path, asset.Name);
                using (var client = new WebClient()) client.DownloadFile(new Uri(asset.BrowserDownloadUrl), zipFilePath);
                using (var fileStream = new FileStream(zipFilePath, System.IO.FileMode.Open))
                using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read)) {
                    string[] folders = archive.GetFirstLevelDirectories();
                    if (folders.Length > 0) {
                        if (asset.Mods == null) asset.Mods = new HashSet<string>();
                        asset.Mods.UnionWith(folders);
                        archive.ExtractToDirectory(path, true);
                    }
                    else result = false;
                }
                File.Delete(zipFilePath);
                return result;
            });
        }
    }

    internal class CredentialStore : ICredentialStore {

        public Task<Credentials> GetCredentials() {
            //var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            var token = ConfigManager.Instance.Get<string>(ConfigManager.Keys.Settings_GitHub_Token);
            var credentials = string.IsNullOrEmpty(token) ? new Credentials(token) : null;
            return Task.FromResult(credentials);
        }
    }
}