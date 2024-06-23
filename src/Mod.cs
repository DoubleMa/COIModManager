using COILib.General;
using COILib.Logging;
using COIModManager.ModManager;
using COIModManager.Patcher;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using System;

namespace COIModManager {

    public sealed class Mod : IMod {
        public static Version ModVersion = new(1, 0, 0);
        public bool Initialized { get; private set; } = false;

        public string Name => "COIModManager";
        public int Version => 1;
        public bool IsUiOnly => false;
        public Option<IConfig> ModConfig { get; }

        public Mod() {
            Init();
        }

        public void Initialize(DependencyResolver resolver, bool gameWasLoaded) {
        }

        public void ChangeConfigs(Lyst<IConfig> configs) {
        }

        public void RegisterPrototypes(ProtoRegistrator registrator) {
        }

        public void RegisterDependencies(DependencyResolverBuilder depBuilder, ProtosDb protosDb, bool wasLoaded) {
        }

        public void EarlyInit(DependencyResolver resolver) {
        }

        private void Init() {
            if (Initialized) return;
            try {
                ExtLog.Info($"Current {Name} version v{ModVersion.ToString(3)}");
                AnEarlyAssemblyLoader.LoadAssemblies();
                MainMenuScreenPatcher.Instance.Init();
                LangManager.Instance.Init();
                ConfigManager.Instance.Init();
                ModsManager.Instance.Init();
            }
            catch (Exception ex) {
                ExtLog.Error(ex.Message);
            }
            Initialized = true;
        }
    }
}