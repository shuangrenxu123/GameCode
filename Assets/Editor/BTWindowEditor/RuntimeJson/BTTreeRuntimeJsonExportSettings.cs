using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BT.Editor.RuntimeJson
{
    [FilePath("ProjectSettings/BTTreeRuntimeJsonExportSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    class BTTreeRuntimeJsonExportSettings : ScriptableSingleton<BTTreeRuntimeJsonExportSettings>
    {
        [Serializable]
        class Entry
        {
            public string graphGuid;
            public string exportAssetPath;
        }

        [SerializeField] List<Entry> entries = new();

        public string GetExportAssetPath(string graphGuid)
        {
            if (string.IsNullOrEmpty(graphGuid))
                return null;

            for (var i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e != null && string.Equals(e.graphGuid, graphGuid, StringComparison.OrdinalIgnoreCase))
                    return e.exportAssetPath;
            }

            return null;
        }

        public void SetExportAssetPath(string graphGuid, string exportAssetPath)
        {
            if (string.IsNullOrEmpty(graphGuid))
                return;

            exportAssetPath = (exportAssetPath ?? string.Empty).Replace('\\', '/');

            for (var i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null)
                    continue;

                if (!string.Equals(e.graphGuid, graphGuid, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.IsNullOrEmpty(exportAssetPath))
                {
                    entries.RemoveAt(i);
                }
                else
                {
                    e.exportAssetPath = exportAssetPath;
                }

                Save(true);
                return;
            }

            if (string.IsNullOrEmpty(exportAssetPath))
                return;

            entries.Add(new Entry
            {
                graphGuid = graphGuid,
                exportAssetPath = exportAssetPath,
            });
            Save(true);
        }
    }
}
