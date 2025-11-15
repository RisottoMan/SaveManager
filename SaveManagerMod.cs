using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BBModMenu;
using MelonLoader;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaveManager
{
    public class SaveManagerMod : MelonMod
    {
        public static GameModeManager gameModeManager;
        
        private Vector3 lastPosition;
        private Quaternion lastRotation;

        private bool keyPressed = false;
        private string saveKey;
        private string loadKey;
        private string currentMap;
        
        private GameObject startObject;
        private GameObject endObject;

        public override void OnLateInitializeMelon()
        {
            MelonLogger.Msg("Initializing SaveManager");
            
            GameObject gameUIObject = GameObject.Find("GameUI");
            GameUI gameUI = gameUIObject.GetComponent<GameUI>();
            List<UIScreen> screens = typeof(GameUI)
                ?.GetField("screens", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(gameUI) as List<UIScreen>;
            
            ModMenu modMenu = screens?.FirstOrDefault(screen => screen is ModMenu) as ModMenu;
            if (modMenu is null)
            {
                MelonLogger.Msg("ModMenu is not found");
                return;
            }

            // Create category in settings
            string categoryName = "SaveManager";
            VisualElement saveSettings = modMenu.AddSetting(categoryName);

            // Create save position hotkey
            var saveHotKey = modMenu.CreateHotKey(categoryName, "Save position", KeyCode.F1);
            saveKey = saveHotKey.Value;
            saveHotKey.OnChanged += newKey =>
            {
                MelonLogger.Msg($"Save key has been changed to {newKey} key.");
                saveKey = newKey;
            };
            
            // Create load position hotkey
            var loadHotKey = modMenu.CreateHotKey(categoryName, "Load position", KeyCode.F2);
            loadKey = loadHotKey.Value;
            loadHotKey.OnChanged += newKey =>
            {
                MelonLogger.Msg($"Load key has been changed to {newKey} key.");
                loadKey = newKey;
            };
            
            // Create group in settings
            var group = modMenu.CreateGroup("SaveManager");
            var wrapper = modMenu.CreateWrapper();
            wrapper.Add(modMenu.CreateLabel("Save button"));
            wrapper.Add(saveHotKey.Root);
            wrapper.Add(modMenu.CreateLabel("Load button"));
            wrapper.Add(loadHotKey.Root);
            group.Add(wrapper);
            saveSettings.Add(group);
            
            base.OnLateInitializeMelon();
        }
        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            gameModeManager = GameObject.Find("Managers").GetComponent<GameModeManager>();
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnUpdate()
        {
            // While the player is in the menu
            if (GameModeManager.Instance.IsGameModeActive<MenuGameMode>())
                return;
            
            CheckCurrentMap();
            CheckPlayerPosition();
        }

        private void CheckPlayerPosition()
        {
            PlayerController player =  gameModeManager.player;

            // Player is in Scouting mode
            if (player.IsFlying)
                return;
            
            // Save the player position on the ground
            if (Utils.IsHotkeyPressed(saveKey) 
                && player.isGrounded)
            {
                keyPressed = true;
                lastPosition = player.transform.position;
                lastRotation = player.cam.transform.rotation;

                SpawnStartBlock(lastPosition);
            }
            
            // Teleport the player to the last position
            if (Utils.IsHotkeyPressed(loadKey) && keyPressed)
            {
                SpawnFinishBlock(player.transform.position);
                
                player.transform.position = lastPosition;
                player.SetCamRotation(0.0f, lastRotation.eulerAngles.y);
            }
        }

        private void CheckCurrentMap()
        {
            // Check custom map
            string map = MapEditor.Instance.mapDirPath.Split('\\', '/').Last();
            if (map == "")
            {
                // Check original game maps
                switch (PlayerStats.Instance.CurrentStatsMode)
                {
                    case PlayerStats.StatsMode.Main:
                        map = "Beton Brutal";
                        break;
                    
                    case PlayerStats.StatsMode.DLC1:
                        map = "Beton Bath";
                        break;
                    
                    case PlayerStats.StatsMode.Birthday:
                        map = "Beton Birthday";
                        break;
                }
            }
            
            if (currentMap != map)
            {
                keyPressed = false;
                currentMap = map;
            }
        }

        private void SpawnStartBlock(Vector3 position)
        {
            if (startObject is null)
            {
                startObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                startObject.GetComponent<MeshRenderer>().material.color = new Color(0, 50, 0, 0.05f);
                startObject.GetComponent<CapsuleCollider>().enabled = false;
                startObject.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            }
            
            startObject.transform.position = position;
        }

        private void SpawnFinishBlock(Vector3 position)
        {
            if (endObject is null)
            {
                endObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                endObject.GetComponent<MeshRenderer>().material.color = new Color(50, 0, 0, 0.05f);
                endObject.GetComponent<CapsuleCollider>().enabled = false;
                endObject.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            }
            
            endObject.transform.position = position;
        }
    }
}