using UnityEngine;

namespace VariantPersist
{
    /// <summary>
    /// Runs in the editor. Sets default variants when editor starts up. Remembers any
    /// default-variant choices that the player makes.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class VariantPersistBehavior : MonoBehaviour
    {
        public void Awake()
        {
            Logging.Log("Registering events");
            GameEvents.onEditorDefaultVariantChanged.Add(OnEditorDefaultVariantChanged);
        }

        public void OnDestroy()
        {
            Logging.Log("Unregistering events");
            GameEvents.onEditorDefaultVariantChanged.Remove(OnEditorDefaultVariantChanged);
        }

        /// <summary>
        /// Here when the player changes the default variant for a part.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="variant"></param>
        private void OnEditorDefaultVariantChanged(AvailablePart part, PartVariant variant)
        {
            VariantPersistScenario.SelectDefaultVariant(part);
        }
    }
}
