namespace Noble.DungeonCrawler
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering.PostProcessing;

    public class OutlineCamera : MonoBehaviour
    {
        public Camera leftOutlineWrapCamera;
        public Camera rightOutlineWrapCamera;
        Camera centerCamera;
        public PostProcessVolume outlineEffectVolume;

        Wrap wrapSettings;

        float oldScreenWidth, oldScreenHeight;

        private void Start()
        {
            centerCamera = GetComponent<Camera>();
            outlineEffectVolume.profile.TryGetSettings(out wrapSettings);
            OnScreenResize();
        }

        void OnScreenResize()
        {
            // Recreate the render texture at the proper resolution
            centerCamera.targetTexture.Release();
            centerCamera.targetTexture.width = Screen.width;
            centerCamera.targetTexture.height = Screen.height;
            centerCamera.targetTexture.Create();

            // Reset the camera viewports to match the new aspect ratio
            centerCamera.ResetAspect();
            leftOutlineWrapCamera.ResetAspect();
            rightOutlineWrapCamera.ResetAspect();

            // Keep track of dimensions so we know when they change
            oldScreenHeight = Screen.height;
            oldScreenWidth = Screen.width;
        }

        void Update()
        {
            wrapSettings.enabled.value = leftOutlineWrapCamera.enabled || rightOutlineWrapCamera.enabled;

            if (Screen.height != oldScreenHeight || Screen.width != oldScreenWidth)
            {
                OnScreenResize();
            }
        }
    }
}