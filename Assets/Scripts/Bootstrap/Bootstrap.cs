using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Bootstrap scene - load new levels and unload previous
public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string _sceneName;
    [SerializeField] private GameObject _loadingScreen;
    private string _currentSceneName;
    

    private void Start()
    {
        if (!LoadScene(_sceneName))
            Debug.Log("No starting scene to load.");
    }

    public bool LoadScene(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        StartCoroutine(LoadNextScene(name));
        return true;
    }

    // Update is called once per frame
    private IEnumerator LoadNextScene(string name)
    {  
        if (!string.IsNullOrEmpty(_currentSceneName))
        {
            yield return StartCoroutine(UnloadPreviousScene());
            yield return StartCoroutine(UnloadUnusedAssets());
        }

        yield return StartCoroutine(AddScene(name));
    }

    private IEnumerator AddScene(string name)
    {
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

        while (!loadSceneAsync.isDone)
            yield return null;

        _currentSceneName = name;
        _loadingScreen.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_currentSceneName));

    }

    private IEnumerator UnloadPreviousScene()
    {
         AsyncOperation unloadSceneAsync = SceneManager.UnloadSceneAsync(_sceneName);

         while (!unloadSceneAsync.isDone)
            yield return null;
    }

    private IEnumerator UnloadUnusedAssets()
    {
         AsyncOperation unloadAssetsAsync = Resources.UnloadUnusedAssets();

         while (!unloadAssetsAsync.isDone)
            yield return null;
    }
}
