using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneDetails : MonoBehaviour {
    [SerializeField] List<SceneDetails> connectedScenes;
    public bool IsLoaded { get; private set; }
    List<SavableEntity> savableEntities;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Player") {
            Debug.Log($"Entered {gameObject.name}");

            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            foreach(var scene in connectedScenes) {
                scene.LoadScene();
            }

            // unload scenes
            var previousScene = GameController.Instance.PreviousScene;
            if (previousScene != null) {
                var previousLoadedScenes = GameController.Instance.PreviousScene.connectedScenes;
                foreach (var scene in previousLoadedScenes) {
                    if (!connectedScenes.Contains(scene) && scene != this) {
                        scene.UnloadScene();
                    }
                }
            
                if (!connectedScenes.Contains(previousScene)) {
                    previousScene.UnloadScene();
                } 
                
            }
        }
    }

    public void LoadScene() {
        if (!IsLoaded) {
            var sceneLoader = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            sceneLoader.completed += (AsyncOperation op) => {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnloadScene() {
        if (IsLoaded) {
            SavingSystem.i.CaptureEntityStates(savableEntities);
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    public List<SavableEntity> GetSavableEntitiesInScene() {
            var currentScene = SceneManager.GetSceneByName(gameObject.name);
            return FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currentScene).ToList();      
    }
}
