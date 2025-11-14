using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FOW
{
    public class CustomHideRenderers : HiderBehavior
    {
        [Header("Manual / Auto Collect")]
        [SerializeField] private Renderer[] ObjectsToHide;
        [Tooltip("If true, will automatically collect MeshRenderer and SkinnedMeshRenderer from children")]
        [SerializeField] private bool autoCollectChildRenderers = true;
        [Tooltip("Include inactive children when auto-collecting")]
        [SerializeField] private bool includeInactive = true;
        [Tooltip("Include a Renderer on the same GameObject (if present) when auto-collecting")]
        [SerializeField] private bool includeSelfRenderer = true;

        protected override void Awake()
        {
            base.Awake();
            if (autoCollectChildRenderers)
                CollectChildRenderers();
        }

        private void OnValidate()
        {
            // keep inspector-friendly behaviour: re-collect in editor when toggled
            if (autoCollectChildRenderers)
                CollectChildRenderers();
        }

        private void CollectChildRenderers()
        {
            var meshRenderers = GetComponentsInChildren<MeshRenderer>(includeInactive).Cast<Renderer>();
            var skinned = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive).Cast<Renderer>();

            var list = new List<Renderer>();

            if (includeSelfRenderer) {
                var self = GetComponent<Renderer>();
                if (self != null)
                    list.Add(self);
            }

            list.AddRange(meshRenderers);
            list.AddRange(skinned);

            // remove nulls and duplicates
            ObjectsToHide = list.Where(r => r != null).Distinct().ToArray();
        }

        // Call this at runtime to force a rescan (useful after changing hierarchy)
        public void RefreshRenderers()
        {
            CollectChildRenderers();
            // apply current hidden/reveal state immediately
            if (enabled && !IsEnabled)
                OnHide();
            else
                OnReveal();
        }

        protected override void OnHide()
        {
            if (ObjectsToHide == null || ObjectsToHide.Length == 0)
                return;

            foreach (Renderer renderer in ObjectsToHide) {
                if (renderer != null)
                    renderer.enabled = false;
            }
        }

        protected override void OnReveal()
        {
            if (ObjectsToHide == null || ObjectsToHide.Length == 0)
                return;

            foreach (Renderer renderer in ObjectsToHide) {
                if (renderer != null)
                    renderer.enabled = true;
            }
        }

        public void ModifyHiddenRenderers(Renderer[] newObjectsToHide)
        {
            // stop auto-collecting if caller explicitly provides a set
            autoCollectChildRenderers = false;

            OnReveal();
            ObjectsToHide = newObjectsToHide;
            if (!enabled)
                return;

            if (!IsEnabled)
                OnHide();
            else
                OnReveal();
        }
    }
}