using System.Collections.Generic;

namespace VariantPersist
{
    /// <summary>
    /// Stores persisted default-variant preferences in the .sfs file. This is where the settings
    /// are "remembered".
    /// </summary>
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.EDITOR)]
    class VariantPersistScenario : ScenarioModule
    {
        private static VariantPersistScenario instance = null;
        private static readonly IEnumerable<KeyValuePair<string, string>> emptyDefaults = new Dictionary<string, string>();

        private const string PART_PREFIX = "part:";

        // key = part name, value = preferred default variant
        private Dictionary<string, string> defaultVariants;

        private void Start()
        {
            instance = this;
        }

        /// <summary>
        /// Call this to remember a particular part's variant.
        /// </summary>
        /// <param name="part"></param>
        public static void SelectDefaultVariant(AvailablePart part)
        {
            if (instance == null) return;

            Logging.Log("Setting default variant: " + part.name + " = " + part.variant.Name);
            instance.defaultVariants[part.name] = part.variant.Name;
        }

        /// <summary>
        /// Here when the scenario is loading. It's called after the other stock
        /// info is loaded.
        /// </summary>
        /// <param name="node"></param>
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            defaultVariants = new Dictionary<string, string>();

            // Populate our default variants from the config node.
            for (int i = 0; i < node.values.Count; ++i)
            {
                ConfigNode.Value value = node.values[i];
                if (!value.name.StartsWith(PART_PREFIX)) continue;
                string tweakedPart = value.name.Substring(PART_PREFIX.Length);
                string selectedVariant = value.value;
                Logging.Log("Loading default variant: " + tweakedPart + " = " + selectedVariant);
                defaultVariants.Add(tweakedPart, selectedVariant);
            }

            // Set our preferences on the loaded parts.
            InitializeParts();
        }

        /// <summary>
        /// Here when the scenario is saving. It's called after the other stock info is saved.
        /// </summary>
        /// <param name="node"></param>
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            Logging.Log("Saving " + defaultVariants.Count + " default variants");
            foreach (KeyValuePair<string, string> pair in defaultVariants)
            {
                Logging.Log("Saving default variant: " + pair.Key + " = " + pair.Value);
                node.AddValue(PART_PREFIX + pair.Key, pair.Value);
            }
        }

        private void InitializeParts()
        {
            // Iterate through all our stored "default variant" selections. For each one,
            // set the default variant in the game.
            int setCount = 0;
            foreach (KeyValuePair<string, string> selection in defaultVariants)
            {
                // Find the referenced part.
                string partName = selection.Key;
                AvailablePart foundPart = PartLoader.LoadedPartsList.Find(part => part.name == partName);
                if (foundPart == null)
                {
                    // No such part exists. This could happen if, for example, a player previously
                    // had a mod installed, selected a variant for one of the parts in the mod,
                    // and then later uninstalled the mod. When this happens, just remove that
                    // selected variant from our stored list so that it won't bother us in the future.
                    Logging.Warn("No such part \"" + partName + "\" found. Removing default variant setting for it.");
                    RemoveDefaultVariant(partName);
                    continue;
                }

                // Find the referenced variant.
                string defaultVariant = selection.Value;
                PartVariant foundVariant = foundPart.Variants.Find(variant => variant.Name == defaultVariant);
                if (foundVariant == null)
                {
                    // No such variant exists. This could happen if, for example, a player updated
                    // a mod that used to have a variant with this name, such that the variant
                    // no longer exists. When this happens, just remove that seleted variant
                    // from our stored list so that it won't bother us in the future.
                    Logging.Warn("No such variant \"" + defaultVariant + "\" exists for part \""
                        + partName + "\". Removing default variant setting for it.");
                    RemoveDefaultVariant(partName);
                    continue;
                }

                // Set the preferred variant for the part.
                foundPart.variant = foundVariant;
                ++setCount;
            }
            Logging.Log("Finished setting default variants for " + setCount + " parts.");
        }

        private void RemoveDefaultVariant(string partName)
        {
            string defaultVariant;
            if (!defaultVariants.TryGetValue(partName, out defaultVariant)) return;
            Logging.Log("Unsetting default variant for part: " + partName + " (was \"" + defaultVariant + "\")");
            defaultVariants.Remove(partName);
        }

    }
}
