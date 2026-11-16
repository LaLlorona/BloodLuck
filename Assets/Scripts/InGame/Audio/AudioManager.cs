using UnityEngine;

namespace BloodLuck
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip[] swings;
        [SerializeField] private AudioClip[] hits;
        [SerializeField] private AudioClip[] footsteps;
        [SerializeField] private AudioClip death;
        [SerializeField] private AudioClip[] shopHovers;
        [SerializeField] private AudioClip shopPurchase;
        [Header("SFX Volumes")]
        [SerializeField, Range(0f, 1f)] private float footstepVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float swingVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float impactVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float shopHoverVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float shopPurchaseVolume = 1f;
        [Header("Optional music clips")]
        [SerializeField] private AudioClip combatMusic;
        [SerializeField] private AudioClip shopMusic;
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip finalSlotMachineMusic;

        private void Awake()
        {
            Instance = this;
            musicSource.loop = true;
            sfxSource.loop = false;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();
        public void PlayCombatMusic() => PlayMusic(combatMusic);
        public void PlayShopMusic() => PlayMusic(shopMusic);
        public void PlayMainMenuMusic() => PlayMusic(mainMenuMusic);
        public void PlayFinalSlotMachineMusic() => PlayMusic(finalSlotMachineMusic);

        private void PlayRandom(AudioClip[] clips, float volumeScale)
        {
            if (clips == null || clips.Length == 0) return;
            var clip = clips[Random.Range(0, clips.Length)];
            if (clip != null) sfxSource.PlayOneShot(clip, volumeScale);
        }

        public void PlaySwing() => PlayRandom(swings, swingVolume);
        public void PlayHit() => PlayRandom(hits, impactVolume);
        public void PlayFootstep() => PlayRandom(footsteps, footstepVolume);
        public void PlayHover() => PlayRandom(shopHovers, shopHoverVolume);
        public void PlayPurchase()
        {
            if (shopPurchase != null) sfxSource.PlayOneShot(shopPurchase, shopPurchaseVolume);
        }
        public void PlayDeath()
        {
            if (death != null) sfxSource.PlayOneShot(death, deathVolume);
        }
    }
}
