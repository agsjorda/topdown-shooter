using UnityEngine;

namespace FOW
{
    public class CustomHider : HiderBehavior
    {
        [SerializeField] private Renderer[] ObjectsToHide;
        private bool autoCollect;

        protected override void Awake()
        {
            autoCollect = ObjectsToHide == null || ObjectsToHide.Length == 0 || System.Array.TrueForAll(ObjectsToHide, r => r == null);
            base.Awake();
        }

        private void OnEnable()
        {
            IsEnabled = false;
            OnHide();
        }

        private Renderer[] GetRenderers()
        {
            if (autoCollect)
                return GetComponentsInChildren<Renderer>();
            return System.Array.FindAll(ObjectsToHide, r => r != null);
        }

        protected override void OnHide()
        {
            foreach (Renderer renderer in GetRenderers())
                renderer.enabled = false;
        }

        protected override void OnReveal()
        {
            foreach (Renderer renderer in GetRenderers())
                renderer.enabled = true;
        }

        private void OnTransformChildrenChanged()
        {
            if (!autoCollect) return;
            if (!IsEnabled)
                OnHide();
        }

        public void ModifyHiddenRenderers(Renderer[] newObjectsToHide)
        {
            OnReveal();
            ObjectsToHide = newObjectsToHide;
            autoCollect = false;
            if (!enabled)
                return;

            if (!IsEnabled)
                OnHide();
            else
                OnReveal();
        }
    }
}
