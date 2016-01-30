using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace druggedcode
{
    public class SceneUtil
    {
        string _currentSceneName;
        List<string> _sceneStack;
        int _changedCount;

        public SceneUtil()
        {
			_currentSceneName = SceneManager.GetActiveScene().name;

            _sceneStack = new List<string>();
            _sceneStack.Add(_currentSceneName);
            _changedCount = 0;
        }

        public string GetLastSceneName(int step = 1)
        {
            if (1 >= _sceneStack.Count)
                return null;

            int index = _sceneStack.Count - (1 + step);

            return _sceneStack[index];
        }

        public string CurrentSceneName
        {
            get { return _currentSceneName; }
        }

        public int ChangedCount
        {
            get { return _changedCount; }
        }

        public bool isFirst
        {
            get { return _changedCount == 0; }
        }

        public void LoadLevel(string sceneName)
        {
            if (_currentSceneName == sceneName)
            {
                return;
            }

            Debug.Log("[SceneUtil]: " + _currentSceneName + " > " + sceneName);
            _currentSceneName = sceneName;
            _sceneStack.Add(sceneName);
            _changedCount++;

			SceneManager.LoadScene( _currentSceneName );
        }

        public IEnumerator LoadLevelAsync(string sceneName)
        {
            if (_currentSceneName == sceneName) yield break;

            Debug.Log("[SceneUtil]: '" + _currentSceneName + "' > '" + sceneName + "'  async");


            _currentSceneName = sceneName;
            _sceneStack.Add(sceneName);
            _changedCount++;

			AsyncOperation async = SceneManager.LoadSceneAsync( sceneName );
            yield return async;
            
            Debug.Log("[SceneUtil] '" + sceneName + "' SceneLoadComplete.");

            _currentSceneName = sceneName;
            _sceneStack.Add(sceneName);
            _changedCount++;
        }
    }
}
