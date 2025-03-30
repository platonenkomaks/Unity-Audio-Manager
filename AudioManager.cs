using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Перечисление для категорий звуков
public enum SoundCategory
{
    Music,
    SFX,
    Ambient,
    Voice,
    UI
}

// Класс для хранения информации о звуковом клипе
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public SoundCategory category;

    [Range(0f, 1f)] public float volume = 1f;

    [Range(0.1f, 3f)] public float pitch = 1f;

    public bool loop = false;

    [Range(0f, 1f)] public float spatialBlend = 0f; // 0 = 2D, 1 = 3D

    [HideInInspector] public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    [Header("Звуковые настройки")] public Sound[] sounds;

    [Header("Аудио миксер")] public AudioMixer audioMixer;

    // Словарь для быстрого поиска звуков по имени
    private Dictionary<string, Sound> _soundDictionary = new Dictionary<string, Sound>();

    // Текущая играющая музыка
    private Sound _currentMusic;

    // Громкости для категорий
    private Dictionary<SoundCategory, float> _categoryVolumes = new Dictionary<SoundCategory, float>();

    private void Awake()
    {
        G.AudioManager = this;

        // Инициализируем громкости категорий
        foreach (SoundCategory category in System.Enum.GetValues(typeof(SoundCategory)))
        {
            _categoryVolumes[category] = 1f;
        }

        // Создаем источники звука для каждого звука
        InitializeSounds();
    }

    private void InitializeSounds()
    {
        // Создаем AudioSource компоненты для каждого звука
        foreach (var sound in sounds)
        {
            var source = gameObject.AddComponent<AudioSource>();
            sound.source = source;

            source.clip = sound.clip;
            source.volume = sound.volume * _categoryVolumes[sound.category];
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.spatialBlend = sound.spatialBlend;

            // Назначаем группу в миксере в зависимости от категории
            if (audioMixer != null)
            {
                switch (sound.category)
                {
                    case SoundCategory.Music:
                        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
                        break;
                    case SoundCategory.SFX:
                        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                        break;
                    case SoundCategory.Ambient:
                        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Ambient")[0];
                        break;
                    case SoundCategory.Voice:
                        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Voice")[0];
                        break;
                    case SoundCategory.UI:
                        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("UI")[0];
                        break;
                }
            }

            // Добавляем звук в словарь для быстрого доступа
            _soundDictionary[sound.name] = sound;
        }
    }

    // Проигрывание звука по имени
    public void Play(string soundName)
    {
        if (_soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning("Sound " + soundName + " not found!");
        }
    }

    // Проигрывание музыки с плавным переходом
    public void PlayMusic(string soundName, float fadeDuration = 1.0f)
    {
        if (_soundDictionary.TryGetValue(soundName, out Sound newMusic))
        {
            if (newMusic.category != SoundCategory.Music)
            {
                Debug.LogWarning("Sound " + soundName + " is not in Music category!");
                return;
            }

            // Если уже играет музыка, делаем затухание
            if (_currentMusic != null && _currentMusic.source.isPlaying)
            {
                StartCoroutine(CrossfadeMusic(_currentMusic, newMusic, fadeDuration));
            }
            else
            {
                // Просто запускаем новую музыку
                newMusic.source.Play();
                _currentMusic = newMusic;
            }
        }
        else
        {
            Debug.LogWarning("Music " + soundName + " not found!");
        }
    }

    // затухание между треками
    private IEnumerator CrossfadeMusic(Sound oldMusic, Sound newMusic, float duration)
    {
        float startTime = Time.time;
        float endTime = startTime + duration;

        newMusic.source.volume = 0f;
        newMusic.source.Play();

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            oldMusic.source.volume = Mathf.Lerp(oldMusic.source.volume, 0f, t);
            newMusic.source.volume = Mathf.Lerp(0f, newMusic.volume * _categoryVolumes[SoundCategory.Music], t);
            yield return null;
        }

        oldMusic.source.Stop();
        oldMusic.source.volume = oldMusic.volume * _categoryVolumes[SoundCategory.Music];
        newMusic.source.volume = newMusic.volume * _categoryVolumes[SoundCategory.Music];

        _currentMusic = newMusic;
    }

    // Остановка звука
    public void Stop(string soundName)
    {
        if (_soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sound.source.Stop();
        }
        else
        {
            Debug.LogWarning("Sound " + soundName + " not found!");
        }
    }

    // Пауза звука
    public void Pause(string soundName)
    {
        if (_soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sound.source.Pause();
        }
        else
        {
            Debug.LogWarning("Sound " + soundName + " not found!");
        }
    }

    // Возобновление звука
    
    public void Resume(string soundName)
    {
        if (_soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sound.source.UnPause();
        }
        else
        {
            Debug.LogWarning("Sound " + soundName + " not found!");
        }
    }

    // Проигрывание звука в определенном месте (для 3D звуков)
    public void PlayAtPosition(string soundName, Vector3 position)
    {
        if (_soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * _categoryVolumes[sound.category]);
        }
        else
        {
            Debug.LogWarning("Sound " + soundName + " not found!");
        }
    }

    // Изменение громкости категории звуков
    public void SetCategoryVolume(SoundCategory category, float volume)
    {
        _categoryVolumes[category] = Mathf.Clamp01(volume);

        // Обновляем громкость всех источников этой категории
        foreach (Sound sound in sounds)
        {
            if (sound.category == category)
            {
                sound.source.volume = sound.volume * _categoryVolumes[category];
            }
        }

        // Обновляем громкость в миксере
        if (audioMixer == null) return;
        // Преобразуем линейную громкость в логарифмическую для миксера
        float dbVolume = volume > 0.001f ? 20f * Mathf.Log10(volume) : -80f;

        switch (category)
        {
            case SoundCategory.Music:
                audioMixer.SetFloat("MusicVolume", dbVolume);
                break;
            case SoundCategory.SFX:
                audioMixer.SetFloat("SFXVolume", dbVolume);
                break;
            case SoundCategory.Ambient:
                audioMixer.SetFloat("AmbientVolume", dbVolume);
                break;
            case SoundCategory.Voice:
                audioMixer.SetFloat("VoiceVolume", dbVolume);
                break;
            case SoundCategory.UI:
                audioMixer.SetFloat("UIVolume", dbVolume);
                break;
        }
    }

    // Получение громкости категории
    public float GetCategoryVolume(SoundCategory category)
    {
        return _categoryVolumes[category];
    }

    // Метод для быстрого воспроизведения звуков без предварительной настройки
    public AudioSource PlayOneShot(AudioClip clip, SoundCategory category, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return null;

        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource source = tempAudio.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume * _categoryVolumes[category];
        source.pitch = pitch;

        // Назначаем группу в миксере
        if (audioMixer != null)
        {
            switch (category)
            {
                case SoundCategory.Music:
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
                    break;
                case SoundCategory.SFX:
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                    break;
                case SoundCategory.Ambient:
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Ambient")[0];
                    break;
                case SoundCategory.Voice:
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Voice")[0];
                    break;
                case SoundCategory.UI:
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("UI")[0];
                    break;
            }
        }

        source.Play();

        // Уничтожаем объект после окончания звука
        Destroy(tempAudio, clip.length / pitch);

        return source;
    }
}

