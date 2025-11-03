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
        
        private bool keyPressed;
        private Vector3 lastPosition;
        private Quaternion lastRotation;

        private string saveKey;
        private string loadKey;

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
            keyPressed = false;
            gameModeManager = GameObject.Find("Managers").GetComponent<GameModeManager>();
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnLateUpdate()
        {
            PlayerController player =  gameModeManager.player;
            
            // Save the player position on the ground
            if (Utils.IsHotkeyPressed(saveKey) && player.isGrounded)
            {
                keyPressed = true;
                lastPosition = player.transform.position;
                lastRotation = player.transform.rotation;
            }

            // Teleport the player to the last position
            if (Utils.IsHotkeyPressed(loadKey) && keyPressed)
            {
                player.transform.position = lastPosition;
                player.transform.rotation = lastRotation;
            }
        }
    }
}