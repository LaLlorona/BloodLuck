using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BloodLuck
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource transitionMusicSource;
        [SerializeField] private AudioSource sfxSource;
        [Header("SFX Clips")]
        [SerializeField] private AudioClip[] swings;
        [SerializeField] private AudioClip[] hits;
        [SerializeField] private AudioClip[] footsteps;
        [SerializeField] private AudioClip[] dashes;
        [SerializeField] private AudioClip[] playerDamages;
        [SerializeField] private AudioClip[] shopHovers;
        [SerializeField] private AudioClip shopPurchase;
        [SerializeField] private AudioClip diceRoll;
        [SerializeField] private AudioClip perkSelection;
        [SerializeField] private AudioClip uiSelect;
        [Header("SFX Volumes")]
        [SerializeField, Range(0f, 1f)] private float footstepVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float swingVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float impactVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float dashVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float playerDamageVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float shopHoverVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float shopPurchaseVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float diceRollVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float perkSelectionVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float uiSelectVolume = 1f;
        [Header("Music Clips")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip gameplayIntro;
        [SerializeField] private AudioClip gameplayLoop;
        [SerializeField] private AudioClip shopMusic;
        [SerializeField] private AudioClip defeatIntro;
        [SerializeField] private AudioClip defeatLoop;
        [SerializeField] private AudioClip winLoop;
        [Header("Music Settings")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
        [SerializeField, Min(0.1f)] private float crossfadeDuration = 1.25f;

        private AudioSource activeMusicSource;
        private Coroutine musicRoutine;
        private Coroutine introLoopRoutine;
        private AudioSource scheduledGameplayLoopSource;
        private double scheduledGameplayLoopDspTime;
        private AudioSource pausedGameplaySource;
        private int pausedGameplayTimeSamples;
        private bool pausedGameplayWasIntro;

        private void Awake()
        {
            Instance = this;
            activeMusicSource = musicSource;
            ConfigureMusicSource(musicSource);
            ConfigureMusicSource(transitionMusicSource);
            if (sfxSource != null) sfxSource.loop = false;
        }

        private void Start()
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "IntroScene":
                    PlayMainMenuMusic();
                    break;
                case "InGameScene":
                    PlayGameplayIntro();
                    break;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void PlayMainMenuMusic()
        {
            DiscardPausedGameplay();
            PlayLoopingMusic(mainMenuMusic, false);
        }

        public void PlayGameplayIntro()
        {
            DiscardPausedGameplay();
            PlayGameplayIntroThenLoop();
        }

        public void PlayGameplayLoop()
        {
            if (pausedGameplaySource != null)
            {
                ResumePausedGameplay();
                return;
            }

            PlayLoopingMusic(gameplayLoop, true);
        }

        public void PlayShopMusic()
        {
            if (pausedGameplaySource != null || shopMusic == null) return;
            if (activeMusicSource == null || !activeMusicSource.isPlaying || !IsGameplayMusic(activeMusicSource.clip))
            {
                PlayLoopingMusic(shopMusic, false);
                return;
            }

            StopMusicRoutine();
            pausedGameplaySource = activeMusicSource;
            pausedGameplayTimeSamples = pausedGameplaySource.timeSamples;
            pausedGameplayWasIntro = pausedGameplaySource.clip == gameplayIntro;

            var shopSource = OtherMusicSource(pausedGameplaySource);
            if (shopSource == null)
            {
                pausedGameplaySource.Pause();
                return;
            }

            StopSource(shopSource);
            PlayOnSource(shopSource, shopMusic, true, 0f);
            activeMusicSource = shopSource;
            musicRoutine = StartCoroutine(CrossfadeToShop(pausedGameplaySource, shopSource));
        }

        public void PlayWinMusic()
        {
            DiscardPausedGameplay();
            PlayLoopingMusic(winLoop, false);
        }

        public void PlayDefeatMusic()
        {
            DiscardPausedGameplay();
            PlayIntroThenLoop(defeatIntro, defeatLoop);
        }

        public void StopMusic()
        {
            StopMusicRoutine();
            DiscardPausedGameplay();
            StopSource(musicSource);
            StopSource(transitionMusicSource);
        }

        private void PlayIntroThenLoop(AudioClip intro, AudioClip loop)
        {
            if (intro == null)
            {
                PlayLoopingMusic(loop, true);
                return;
            }

            StopMusicRoutine();
            StopSource(musicSource);
            StopSource(transitionMusicSource);
            activeMusicSource = musicSource;
            PlayOnSource(activeMusicSource, intro, false, musicVolume);
            introLoopRoutine = StartCoroutine(WaitForIntroThenLoop(activeMusicSource, intro, loop));
        }

        private void PlayGameplayIntroThenLoop()
        {
            if (gameplayIntro == null)
            {
                PlayLoopingMusic(gameplayLoop, true);
                return;
            }

            StopMusicRoutine();
            StopSource(musicSource);
            StopSource(transitionMusicSource);
            activeMusicSource = musicSource;
            PlayOnSource(activeMusicSource, gameplayIntro, false, musicVolume);
            ScheduleGameplayLoop(activeMusicSource);
        }

        private void ScheduleGameplayLoop(AudioSource introSource)
        {
            if (introSource == null || gameplayLoop == null || introSource.clip != gameplayIntro)
            {
                return;
            }

            CancelScheduledGameplayLoop();
            var loopSource = OtherMusicSource(introSource);
            if (loopSource == null)
            {
                introLoopRoutine = StartCoroutine(WaitForIntroThenLoop(introSource, gameplayIntro, gameplayLoop));
                return;
            }

            StopSource(loopSource);
            loopSource.clip = gameplayLoop;
            loopSource.loop = true;
            loopSource.volume = musicVolume;
            var remainingSamples = Mathf.Max(0, gameplayIntro.samples - introSource.timeSamples);
            scheduledGameplayLoopDspTime = AudioSettings.dspTime + remainingSamples / (double)gameplayIntro.frequency;
            scheduledGameplayLoopSource = loopSource;
            loopSource.PlayScheduled(scheduledGameplayLoopDspTime);
            introLoopRoutine = StartCoroutine(ActivateScheduledGameplayLoop(loopSource, scheduledGameplayLoopDspTime));
        }

        private IEnumerator ActivateScheduledGameplayLoop(AudioSource loopSource, double dspTime)
        {
            yield return new WaitUntil(() => AudioSettings.dspTime >= dspTime);
            if (scheduledGameplayLoopSource == loopSource)
            {
                activeMusicSource = loopSource;
                scheduledGameplayLoopSource = null;
            }
            introLoopRoutine = null;
        }

        private IEnumerator WaitForIntroThenLoop(AudioSource source, AudioClip intro, AudioClip loop)
        {
            yield return new WaitWhile(() => source != null && source.isPlaying && source.clip == intro);
            if (loop != null && source != null)
            {
                PlayOnSource(source, loop, true, musicVolume);
            }
            introLoopRoutine = null;
        }

        private void PlayLoopingMusic(AudioClip clip, bool restart)
        {
            if (clip == null) return;
            if (!restart && activeMusicSource != null && activeMusicSource.isPlaying && activeMusicSource.clip == clip)
            {
                return;
            }

            StopMusicRoutine();
            var outgoing = activeMusicSource;
            var incoming = outgoing == musicSource ? transitionMusicSource : musicSource;
            if (incoming == null)
            {
                PlayOnSource(outgoing, clip, true, musicVolume);
                return;
            }

            StopSource(incoming);
            PlayOnSource(incoming, clip, true, outgoing != null && outgoing.isPlaying ? 0f : musicVolume);
            if (outgoing == null || !outgoing.isPlaying)
            {
                activeMusicSource = incoming;
                return;
            }

            musicRoutine = StartCoroutine(Crossfade(outgoing, incoming));
        }

        private IEnumerator Crossfade(AudioSource outgoing, AudioSource incoming)
        {
            float elapsed = 0f;
            float outgoingStartVolume = outgoing.volume;
            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / crossfadeDuration);
                outgoing.volume = Mathf.Lerp(outgoingStartVolume, 0f, t);
                incoming.volume = Mathf.Lerp(0f, musicVolume, t);
                yield return null;
            }

            StopSource(outgoing);
            incoming.volume = musicVolume;
            activeMusicSource = incoming;
            musicRoutine = null;
        }

        private IEnumerator CrossfadeToShop(AudioSource gameplaySource, AudioSource shopSource)
        {
            float elapsed = 0f;
            float gameplayStartVolume = gameplaySource.volume;
            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / crossfadeDuration);
                gameplaySource.volume = Mathf.Lerp(gameplayStartVolume, 0f, t);
                shopSource.volume = Mathf.Lerp(0f, musicVolume, t);
                yield return null;
            }

            RestorePlaybackPosition(gameplaySource, pausedGameplayTimeSamples);
            gameplaySource.Pause();
            shopSource.volume = musicVolume;
            musicRoutine = null;
        }

        private void ResumePausedGameplay()
        {
            StopMusicRoutine();
            var gameplaySource = pausedGameplaySource;
            var shopSource = activeMusicSource;
            RestorePlaybackPosition(gameplaySource, pausedGameplayTimeSamples);
            gameplaySource.volume = 0f;
            gameplaySource.UnPause();
            float fadeDuration = crossfadeDuration;
            if (pausedGameplayWasIntro)
            {
                fadeDuration = Mathf.Min(crossfadeDuration, Mathf.Max(0f, RemainingIntroSeconds(gameplaySource) - 0.1f));
            }

            musicRoutine = StartCoroutine(CrossfadeFromShop(shopSource, gameplaySource, fadeDuration, pausedGameplayWasIntro));
        }

        private IEnumerator CrossfadeFromShop(AudioSource shopSource, AudioSource gameplaySource, float duration, bool scheduleGameplayLoop)
        {
            float elapsed = 0f;
            float shopStartVolume = shopSource != null ? shopSource.volume : 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (shopSource != null) shopSource.volume = Mathf.Lerp(shopStartVolume, 0f, t);
                gameplaySource.volume = Mathf.Lerp(0f, musicVolume, t);
                yield return null;
            }

            StopSource(shopSource);
            gameplaySource.volume = musicVolume;
            activeMusicSource = gameplaySource;
            pausedGameplaySource = null;
            pausedGameplayWasIntro = false;
            if (scheduleGameplayLoop) ScheduleGameplayLoop(gameplaySource);
            musicRoutine = null;
        }

        private bool IsGameplayMusic(AudioClip clip) => clip == gameplayIntro || clip == gameplayLoop;

        private AudioSource OtherMusicSource(AudioSource source) =>
            source == musicSource ? transitionMusicSource : musicSource;

        private static void RestorePlaybackPosition(AudioSource source, int timeSamples)
        {
            if (source == null || source.clip == null) return;
            source.timeSamples = Mathf.Clamp(timeSamples, 0, Mathf.Max(0, source.clip.samples - 1));
        }

        private float RemainingIntroSeconds(AudioSource source)
        {
            if (source == null || source.clip != gameplayIntro || gameplayIntro.frequency == 0) return 0f;
            return Mathf.Max(0f, gameplayIntro.samples - source.timeSamples) / (float)gameplayIntro.frequency;
        }

        private void DiscardPausedGameplay()
        {
            if (pausedGameplaySource == null) return;
            StopSource(pausedGameplaySource);
            pausedGameplaySource = null;
            pausedGameplayWasIntro = false;
        }

        private void StopMusicRoutine()
        {
            if (musicRoutine != null)
            {
                StopCoroutine(musicRoutine);
                musicRoutine = null;
            }

            if (introLoopRoutine != null)
            {
                StopCoroutine(introLoopRoutine);
                introLoopRoutine = null;
            }

            CancelScheduledGameplayLoop();
        }

        private void CancelScheduledGameplayLoop()
        {
            if (scheduledGameplayLoopSource == null) return;
            StopSource(scheduledGameplayLoopSource);
            scheduledGameplayLoopSource = null;
        }

        private static void StopSource(AudioSource source)
        {
            if (source != null) source.Stop();
        }

        private static void ConfigureMusicSource(AudioSource source)
        {
            if (source == null) return;
            source.playOnAwake = false;
            source.loop = false;
        }

        private static void PlayOnSource(AudioSource source, AudioClip clip, bool loop, float volume)
        {
            if (source == null || clip == null) return;
            source.Stop();
            source.clip = clip;
            source.loop = loop;
            source.volume = volume;
            source.Play();
        }

        private void PlayRandom(AudioClip[] clips, float volumeScale)
        {
            if (clips == null || clips.Length == 0 || sfxSource == null) return;
            var clip = clips[Random.Range(0, clips.Length)];
            if (clip != null) sfxSource.PlayOneShot(clip, volumeScale);
        }

        public void PlaySwing() => PlayRandom(swings, swingVolume);
        public void PlayHit() => PlayRandom(hits, impactVolume);
        public void PlayFootstep() => PlayRandom(footsteps, footstepVolume);
        public void PlayDash() => PlayRandom(dashes, dashVolume);
        public void PlayPlayerDamage() => PlayRandom(playerDamages, playerDamageVolume);
        public void PlayHover() => PlayRandom(shopHovers, shopHoverVolume);
        public void PlayPurchase()
        {
            if (shopPurchase != null && sfxSource != null) sfxSource.PlayOneShot(shopPurchase, shopPurchaseVolume);
        }

        public void PlayPerkSelection()
        {
            if (perkSelection != null && sfxSource != null) sfxSource.PlayOneShot(perkSelection, perkSelectionVolume);
        }

        public void PlayDiceRoll()
        {
            if (diceRoll != null && sfxSource != null) sfxSource.PlayOneShot(diceRoll, diceRollVolume);
        }

        public void PlayUiSelect()
        {
            if (uiSelect != null && sfxSource != null) sfxSource.PlayOneShot(uiSelect, uiSelectVolume);
        }

        public void PlayDeath()
        {
            PlayDefeatMusic();
        }
    }
}
