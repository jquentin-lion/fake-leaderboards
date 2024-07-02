using UnityEngine;
using UnityEditor;

namespace LionStudios.Suite.Leaderboards.Fake.Editor
{

    public static class MenuItemsEditor
    {
        private const string LION_STUDIO_GAME_OBJECT_MENU_PATH = "GameObject/LionStudios/FakeLeaderboards/";
        
        private const string LEAGUES_MANAGER_NAME = "Leagues Manager";
        private const string LEAGUES_BUTTON_NAME = "Leagues Button";

        [MenuItem(LION_STUDIO_GAME_OBJECT_MENU_PATH + LEAGUES_MANAGER_NAME, false, priority = 0)]
        private static void CreateLeaguesManager(MenuCommand menuCommand)
        {
            
            GameObject itemToSpawn = AssetDatabase.LoadAssetAtPath("Packages/com.lionstudios.release.fakeleaderboards/Prefabs/Leagues/LeaguesManager.prefab", typeof(GameObject)) as GameObject;
            GameObject parent = menuCommand.context as GameObject;
            Spawn(itemToSpawn, parent);
        }
        [MenuItem(LION_STUDIO_GAME_OBJECT_MENU_PATH + LEAGUES_BUTTON_NAME, false, priority = 0)]
        private static void CreateLeaguesButton(MenuCommand menuCommand)
        {
            
            GameObject itemToSpawn = AssetDatabase.LoadAssetAtPath("Packages/com.lionstudios.release.fakeleaderboards/Prefabs/Leagues/LeagueShowButton.prefab", typeof(GameObject)) as GameObject;
            GameObject parent = menuCommand.context as GameObject;
            Spawn(itemToSpawn, parent);
        }
        
        private static void Spawn(GameObject itemToSpawn, GameObject parent)
        {
            GameObject selectedObject = null;
            
            if (parent != null)
            {
                selectedObject = PrefabUtility.InstantiatePrefab(itemToSpawn, parent.transform) as GameObject;
            }
            else
            {
                selectedObject = PrefabUtility.InstantiatePrefab(itemToSpawn) as GameObject;
            }
            
            selectedObject.name = selectedObject.name.Replace("(Clone)", "");

            Selection.activeGameObject = selectedObject;
            
            Undo.RegisterCreatedObjectUndo (selectedObject, $"Created {selectedObject.name}");
        }

    }

}
