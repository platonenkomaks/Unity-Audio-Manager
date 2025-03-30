using UnityEngine;


public class RandomSoundPlayer : MonoBehaviour
{
    [System.Serializable]
    public class RandomSound
    {
        public AudioClip[] clips;
        public SoundCategory category = SoundCategory.SFX;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitchMin = 0.9f;
        [Range(0.1f, 3f)]
        public float pitchMax = 1.1f;
    }
    
    public RandomSound[] randomSounds;
    
    // Воспроизвести случайный звук из группы
    public void PlayRandomSound(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= randomSounds.Length)
        {
            Debug.LogWarning("Invalid sound group index: " + groupIndex);
            return;
        }
        
        RandomSound soundGroup = randomSounds[groupIndex];
        
        if (soundGroup.clips.Length == 0)
        {
            Debug.LogWarning("No clips in sound group: " + groupIndex);
            return;
        }
        
        // Выбираем случайный клип
        AudioClip randomClip = soundGroup.clips[Random.Range(0, soundGroup.clips.Length)];
        if (randomClip == null) return;
        
        // Выбираем случайную высоту звука
        float randomPitch = Random.Range(soundGroup.pitchMin, soundGroup.pitchMax);
        
        // Воспроизводим
        G.AudioManager.PlayOneShot(randomClip, soundGroup.category, soundGroup.volume, randomPitch);
    }
}