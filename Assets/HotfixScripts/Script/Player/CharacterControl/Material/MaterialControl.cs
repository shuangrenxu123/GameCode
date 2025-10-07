using System;
using UnityEngine;

namespace CharacterController
{
    public class MaterialControl : MonoBehaviour
    {
        CharacterActor characterActor;
        [SerializeField]
        MaterialsProperties MaterialsProperties;

        #region Event
        public event Action<Volume> OnVolumeEnter;
        public event Action<Volume> OnVolumeExit;
        public event Action<Surface> OnSurfaceEnter;
        public event Action<Surface> OnSurfaceExit;
        #endregion

        public Volume CurrentVolume { get; private set; }
        public Surface CurrentSurface { get; private set; }

        private void Awake()
        {
            characterActor = GetComponentInParent<CharacterActor>();
            if (characterActor == null)
            {
                Debug.LogError("Character is Null");
                return;
            }
            GetSurfaceData();
        }
        private void FixedUpdate()
        {
            GetSurfaceData();
            GetVolumeData();
        }
        void GetSurfaceData()
        {
            if (!characterActor.IsGrounded)
            {
                SetCurrentSurfaceData(MaterialsProperties.DefaultSurface);
            }
            else
            {
                var ground = characterActor.GroundObject;
                if (ground != null)
                {
                    bool validSurface = MaterialsProperties.GetSurface(ground, out Surface surface);
                    if (validSurface)
                    {
                        SetCurrentSurfaceData(surface);
                    }
                    else
                    {
                        if (ground.CompareTag("Untagged"))
                        {
                            SetCurrentSurfaceData(MaterialsProperties.DefaultSurface);
                        }
                    }
                }
            }
        }
        void SetCurrentSurfaceData(Surface surface)
        {
            if (surface != CurrentSurface)
            {
                OnSurfaceExit?.Invoke(CurrentSurface);
                CurrentSurface = surface;
                OnSurfaceEnter?.Invoke(CurrentSurface);
            }
        }
        void GetVolumeData()
        {

            if (!characterActor.IsGrounded)
            {
                SetCurrentVolumeDate(MaterialsProperties.DefaultVolume);
            }
            else
            {
                var ground = characterActor.GroundObject;
                if (ground != null)
                {
                    bool validSurface = MaterialsProperties.GetVolume(ground, out Volume volume);
                    if (validSurface)
                    {
                        SetCurrentVolumeDate(volume);
                    }
                    else
                    {
                        if (ground.CompareTag("Untagged"))
                        {
                            SetCurrentVolumeDate(MaterialsProperties.DefaultVolume);
                        }
                    }
                }
            }
        }
        void SetCurrentVolumeDate(Volume volume)
        {
            if (volume != CurrentVolume)
            {
                OnVolumeExit?.Invoke(volume);
                CurrentVolume = volume;
                OnVolumeEnter?.Invoke(volume);
            }
        }
    }
}