﻿using CustomSaber.Data;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPA.Utilities;
using Zenject;

namespace CustomSaber.Utilities
{
    /// <summary>
    /// Grabbing the default sabers, so we can display them in their full glory in the settings (Doesn't work. Game hangs on WaitUntil())
    /// Also fails in regular GameCore without forcing scene change... Something about null, but I gave up
    /// </summary>
    public class DefaultSaberGrabber : MonoBehaviour
    {
        public static bool isCompleted { get; private set; } = false;

        public static GameObject defaultLeftSaber = null;
        public static GameObject defaultRightSaber = null;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            if (!isCompleted)
            {
                StartCoroutine(PreloadDefaultSabers());
            }
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene current, Scene next)
        {
            if (next.name == "PCInit")
            {
                if (defaultLeftSaber != null)
                {
                    Destroy(defaultLeftSaber);
                }
                if (defaultRightSaber != null)
                {
                    Destroy(defaultRightSaber);
                }
                isCompleted = false;
            }
        }

        private IEnumerator PreloadDefaultSabers()
        {
            bool isSceneLoaded = false;
            string sceneName;

            try
            {
                sceneName = "StandardGameplay";
                Logger.log.Debug($"Loading {sceneName} scene");
                var loadScene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                while (!loadScene.isDone) yield return null;

                sceneName = "Online";
                Logger.log.Debug($"Loading {sceneName} scene");
                loadScene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                while (!loadScene.isDone) yield return null;

                sceneName = "GameCore";
                Logger.log.Debug($"Loading {sceneName} scene");
                loadScene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                while (!loadScene.isDone) yield return null;

                isSceneLoaded = true;
                Logger.log.Debug("Loaded!");

                yield return new WaitForSecondsRealtime(0.1f);

                SaberModelController saber = Resources.FindObjectsOfTypeAll<SaberModelController>().FirstOrDefault();                

                Logger.log.Debug("Got sabers!");

                Logger.log.Debug($"Saber: {saber.name}, GameObj: {saber.gameObject.name}, {saber.ToString()}");

                // Left Saber
                defaultLeftSaber = Instantiate(saber).gameObject;
                DestroyImmediate(defaultLeftSaber.GetComponent<SaberModelController>());
                DestroyImmediate(defaultLeftSaber.GetComponentInChildren<ConditionalMaterialSwitcher>());
                foreach (var c in defaultLeftSaber.GetComponentsInChildren<SetSaberGlowColor>())
                    DestroyImmediate(c);

                DontDestroyOnLoad(defaultLeftSaber);
                defaultLeftSaber.transform.SetParent(this.transform);
                defaultLeftSaber.gameObject.name = "LeftSaber";
                defaultLeftSaber.transform.localPosition = Vector3.zero;
                defaultLeftSaber.transform.localRotation = Quaternion.identity;
                defaultLeftSaber.AddComponent<DummySaber>();

                // Right Saber
                defaultRightSaber = Instantiate(saber).gameObject;
                DestroyImmediate(defaultRightSaber.GetComponent<SaberModelController>());
                DestroyImmediate(defaultRightSaber.GetComponentInChildren<ConditionalMaterialSwitcher>());
                foreach (var c in defaultRightSaber.GetComponentsInChildren<SetSaberGlowColor>())
                    DestroyImmediate(c);

                DontDestroyOnLoad(defaultRightSaber);
                defaultRightSaber.transform.SetParent(this.transform);
                defaultRightSaber.gameObject.name = "RightSaber";
                defaultRightSaber.transform.localPosition = Vector3.zero;
                defaultRightSaber.transform.localRotation = Quaternion.identity;
                defaultRightSaber.AddComponent<DummySaber>();

                Logger.log.Debug("Finished! Got default sabers! Setting active state");

                if (defaultLeftSaber)
                {
                    Logger.log.Debug("Found default left saber");
                    defaultLeftSaber.SetActive(false);
                }

                if (defaultRightSaber)
                {
                    Logger.log.Debug("Found default right saber");
                    defaultRightSaber.SetActive(false);
                }

                if (defaultLeftSaber && defaultRightSaber)
                {
                    // Add them as the first Object in the list, replacing the empty version.
                    CustomSaberData defaultSabers = new CustomSaberData(defaultLeftSaber.gameObject, defaultRightSaber.gameObject);
                    SaberAssetLoader.CustomSabers[0] = defaultSabers;
                    isCompleted = true;
                }
            }
            finally
            {
                if (isSceneLoaded)
                {
                    sceneName = "StandardGameplay";
                    Logger.log.Debug($"Unloading {sceneName}");
                    SceneManager.UnloadSceneAsync(sceneName);

                    sceneName = "GameCore";
                    Logger.log.Debug($"Unloading {sceneName}");
                    SceneManager.UnloadSceneAsync(sceneName);
                }
            }
        }
    }
}
