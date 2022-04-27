using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace TankEngine
{
    public class TankEngine : ModBehaviour
    {
        public static TankEngine Instance;
        private static readonly Shader standardShader = Shader.Find("Standard");
        
        private GameObject _thomasPrefab;
        private AudioSource _audioSource;
        private float _audioVolume;

        private bool _loaded = false;

        private void Start()
        {
            Instance = this;

            var bundle = ModHelper.Assets.LoadBundle("thomas-the-tank-engine");

            _thomasPrefab = LoadPrefab(bundle, "Assets/Prefabs/thomas.prefab");

            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        private void OnDestroy()
        {
            LoadManager.OnCompleteSceneLoad -= OnCompleteSceneLoad;
        }

        public override void Configure(IModConfig config)
        {
            base.Configure(config);
            _audioVolume = config.GetSettingsValue<float>("Audio volume");
            if(_loaded && _audioSource != null)
            {
                if (_audioVolume == 0 && _audioSource.isPlaying) _audioSource.Stop();
                if (_audioVolume > 0 &&  !_audioSource.isPlaying) _audioSource.Play();
                _audioSource.volume = _audioVolume;
            }
        }

        private void OnCompleteSceneLoad(OWScene _, OWScene currentScene)
        {
            if (currentScene != OWScene.SolarSystem) 
            {
                _loaded = false;
                return;
            }

            _loaded = true;

            Log("Creating Thomas");

            var ship = GameObject.Find("Ship_Body").gameObject;
            var thomas = GameObject.Instantiate(_thomasPrefab, ship.transform);
            thomas.transform.localPosition = new Vector3(0, -0.5f, 0f);
            thomas.transform.localScale = Vector3.one * 0.3f;
            _audioSource = thomas.GetComponent<AudioSource>();
            if (_audioVolume > 0) _audioSource.Play();
            else _audioSource.Stop();
            _audioSource.volume = _audioVolume;

            thomas.SetActive(true);

            var toDelete = new string[]
            {
                "Ship_Body/Module_Supplies/Geo_Supplies/Supplies_Geometry/",
                "Ship_Body/Module_Cabin/Geo_Cabin/Cabin_Tech/Cabin_Tech_Exterior/Tanks",
                "Ship_Body/Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Exterior",
                "Ship_Body/Module_Supplies/Geo_Supplies/Supplies_Tech",
                "Ship_Body/Module_Supplies/Systems_Supplies/ExpeditionGear/EquipmentGeo",
                "Ship_Body/Module_LandingGear/LandingGear_Right/Geo_LandingGear_Right",
                "Ship_Body/Module_LandingGear/LandingGear_Left/Geo_LandingGear_Left",
                "Ship_Body/Module_LandingGear/LandingGear_Front/Geo_LandingGear_Front",
                "Ship_Body/Module_LandingGear/LandingGear_Front/LandingGear_Front_Tech/LandingCamPivot",
                "Ship_Body/Module_Supplies/Geo_Supplies/ShadowCaster_Supplies",
                "Ship_Body/Module_Engine/Geo_Engine/ShadowCaster_Engine",
                "Ship_Body/Module_Cabin/Geo_Cabin/Shadowcaster_Cabin",
                "Ship_Body/Module_Cabin/Geo_Cabin/Cabin_Tech/Cabin_Tech_Interior",
                "Ship_Body/Module_Cabin/Geo_Cabin/Cabin_Tech/Cabin_Tech_Exterior",
                "Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogSplashScreen",
            };
            foreach(var s in toDelete)
            {
                var obj = GameObject.Find(s);
                if (obj == null) Log($"Couldn't find {s}");
                else obj.gameObject.SetActive(false);
            };

            var toDeleteChildMeshes = new string[]
            {
                "Ship_Body/Module_Cockpit/Geo_Cockpit/Cockpit_Geometry/",
                "Ship_Body/Module_Cabin/Geo_Cabin/Cabin_Geometry/",
                "Ship_Body/Module_Cabin/Geo_Cabin/Cabin_Tech/Cabin_Tech_Exterior/",
                "Ship_Body/Module_Engine/Geo_Engine/Engine_Geometry/Engine_Exterior",
                "Ship_Body/Module_Engine/Geo_Engine/Engine_Geometry/Engine_Interior",
                "Ship_Body/Module_Supplies/Geo_Supplies/Supplies_Geometry/Supplies_Exterior",
                "Ship_Body/Module_Supplies/Geo_Supplies/Supplies_Geometry/Supplies_Interior",
                "Ship_Body/Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Exterior"
            };
            foreach (var s in toDeleteChildMeshes)
            {
                foreach(var mesh in GameObject.Find(s).gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    if (mesh == null) Log($"Couldn't find {s}");
                    else mesh.gameObject.SetActive(false);
                }
            }

            // Add smoke
            var smoke = GameObject.Find("GabbroIsland_Body/Sector_GabbroIsland/Interactables_GabbroIsland/Prefab_HEA_Campfire/Effects/Effects_HEA_SmokeColumn/");
            var thomasSmoke = GameObject.Instantiate(smoke, thomas.transform);
            thomasSmoke.transform.localPosition = new Vector3(0f, 24f, 21f);
            thomasSmoke.transform.localScale = Vector3.one * 3f;
            thomasSmoke.SetActive(true);
        }

        private static GameObject LoadPrefab(AssetBundle bundle, string path)
        {
            var prefab = bundle.LoadAsset<GameObject>(path);

            // Repair materials             
            foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = standardShader;
                    mat.renderQueue = 2000;
                }
            }
            foreach (Transform child in prefab.transform)
            {
                foreach (var renderer in child.GetComponentsInChildren<MeshRenderer>())
                {
                    foreach (var mat in renderer.materials)
                    {
                        mat.shader = standardShader;
                        mat.renderQueue = 2000;
                    }
                }
            }

            prefab.SetActive(false);

            return prefab;
        }

        private static void Log(string message)
        {
            Instance.ModHelper.Console.WriteLine(message, MessageType.Info);
        }
    }
}
