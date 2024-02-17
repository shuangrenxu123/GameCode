using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PlayerControl/Material Properties")]
public class MaterialsProperties : ScriptableObject
{
    [SerializeField]
    Surface defaultSurface = new Surface();

    [SerializeField]
    Volume defaultVolume = new Volume();

    [SerializeField]
    Surface[] surfaces = null;

    [SerializeField]
    Volume[] volumes = null;

    public Surface DefaultSurface => defaultSurface;
    public Volume DefaultVolume => defaultVolume;
    public Surface[] Surfaces => surfaces;
    public Volume[] Volumes => volumes;
    public bool GetSurface(GameObject gameObject, out Surface outputSurface)
    {
        outputSurface = null;

        for (int i = 0; i < surfaces.Length; i++)
        {
            var surface = surfaces[i];

            if (gameObject.CompareTag(surface.tagName))
            {
                outputSurface = surface;
                return true;
            }
        }

        return false;
    }

    public bool GetVolume(GameObject gameObject, out Volume outputVolume)
    {
        outputVolume = null;

        for (int i = 0; i < volumes.Length; i++)
        {
            var volume = volumes[i];

            if (gameObject.CompareTag(volume.tagName))
            {
                outputVolume = volume;
                return true;
            }
        }

        return false;
    }
}
