using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace DeepSeaGame
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private bool _pauseAmbienceWhenTimeScaleIsZero = true;

        private EventInstance _ambienceEventInstance;
        private Bus _masterBus;
        private bool _lastAmbiencePausedState;

        private void Awake()
        {
            Instance = this;

            _masterBus = RuntimeManager.GetBus("bus:/");
        }

        private void Start()
        {
            // Debug.Log($"Amb started");
            _ambienceEventInstance = CreateInstance(FMODEvents.Instance.OceanAmbience);
            _ambienceEventInstance.start();
            SyncAmbiencePauseToTimeScale(force: true);
        }

        private void Update()
        {
            if (!_pauseAmbienceWhenTimeScaleIsZero)
            {
                return;
            }

            SyncAmbiencePauseToTimeScale(force: false);
        }

        public void OnDestroy()
        {
            if (_ambienceEventInstance.isValid())
            {
                _ambienceEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        // Play a sound one time at a specific world position
        public void PlayOneShot(EventReference sound, Vector3 worldPos)
        {
            RuntimeManager.PlayOneShot(sound, worldPos);
        }

        public void SetMasterVolume(float volume) // connected to pause menu volume slider
        {
            _masterBus.setVolume(volume);
        }

        public void StopCurrentAmbience()
        {
            _ambienceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public EventInstance CreateInstance(EventReference eventReference)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            return eventInstance;
        }

        private void SyncAmbiencePauseToTimeScale(bool force)
        {
            if (!_ambienceEventInstance.isValid())
            {
                return;
            }

            bool shouldPause = Time.timeScale <= 0f;
            if (!force && shouldPause == _lastAmbiencePausedState)
            {
                return;
            }

            _ambienceEventInstance.setPaused(shouldPause);
            _lastAmbiencePausedState = shouldPause;
        }
    }
}
