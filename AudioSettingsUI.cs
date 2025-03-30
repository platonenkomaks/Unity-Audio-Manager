using System.Collections.Generic;
using UnityEngine;

public class AudioSettingsUI : MonoBehaviour
{
    [System.Serializable]
    public class CategorySlider
    {
        public SoundCategory category;
        public UnityEngine.UI.Slider slider;
    }
    
    public CategorySlider[] volumeSliders;
    public UnityEngine.UI.Toggle muteToggle;
    
    private readonly Dictionary<SoundCategory, float> _savedVolumes = new Dictionary<SoundCategory, float>();
    
    private void Start()
    {
        // Инициализируем слайдеры текущими значениями громкости
        foreach (CategorySlider cs in volumeSliders)
        {
            if (cs.slider != null)
            {
                cs.slider.value = G.AudioManager.GetCategoryVolume(cs.category);
                
                // Добавляем обработчик события изменения слайдера
                cs.slider.onValueChanged.AddListener((value) => {
                    G.AudioManager.SetCategoryVolume(cs.category, value);
                });
                
                // Сохраняем текущую громкость
                _savedVolumes[cs.category] = cs.slider.value;
            }
        }
        
        // Обработчик для кнопки Mute
        if (muteToggle != null)
        {
            muteToggle.onValueChanged.AddListener(ToggleMute);
        }
        
        // Загружаем сохраненные настройки, если они есть
        LoadSettings();
    }
    
    // Включение/выключение звука
    public void ToggleMute(bool isMuted)
    {
        if (isMuted)
        {
            // Сохраняем текущие значения и устанавливаем 0
            foreach (SoundCategory category in System.Enum.GetValues(typeof(SoundCategory)))
            {
                _savedVolumes[category] = G.AudioManager.GetCategoryVolume(category);
                G.AudioManager.SetCategoryVolume(category, 0f);
            }
            
            // Обновляем слайдеры, но без вызова событий
            foreach (CategorySlider cs in volumeSliders)
            {
                if (cs.slider != null)
                {
                    cs.slider.SetValueWithoutNotify(0f);
                }
            }
        }
        else
        {
            // Восстанавливаем сохраненные значения
            foreach (SoundCategory category in System.Enum.GetValues(typeof(SoundCategory)))
            {
                if (_savedVolumes.ContainsKey(category))
                {
                    G.AudioManager.SetCategoryVolume(category, _savedVolumes[category]);
                }
            }
            
            // Обновляем слайдеры, но без вызова событий
            foreach (CategorySlider cs in volumeSliders)
            {
                if (cs.slider != null && _savedVolumes.ContainsKey(cs.category))
                {
                    cs.slider.SetValueWithoutNotify(_savedVolumes[cs.category]);
                }
            }
        }
    }
    
    // Сохранение настроек
    public void SaveSettings()
    {
        foreach (SoundCategory category in System.Enum.GetValues(typeof(SoundCategory)))
        {
            PlayerPrefs.SetFloat("Audio_" + category.ToString(), G.AudioManager.GetCategoryVolume(category));
        }
        
        PlayerPrefs.SetInt("Audio_Muted", muteToggle != null && muteToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    // Загрузка настроек
    public void LoadSettings()
    {
        bool hasSavedSettings = false;
        
        foreach (SoundCategory category in System.Enum.GetValues(typeof(SoundCategory)))
        {
            string key = "Audio_" + category.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                float volume = PlayerPrefs.GetFloat(key);
                G.AudioManager.SetCategoryVolume(category, volume);
                _savedVolumes[category] = volume;
                hasSavedSettings = true;
            }
        }
        
        // Обновляем слайдеры, если есть сохраненные настройки
        if (hasSavedSettings)
        {
            foreach (CategorySlider cs in volumeSliders)
            {
                if (cs.slider != null && _savedVolumes.ContainsKey(cs.category))
                {
                    cs.slider.SetValueWithoutNotify(_savedVolumes[cs.category]);
                }
            }
        }
        
        // Проверяем состояние Mute
        if (PlayerPrefs.HasKey("Audio_Muted") && muteToggle != null)
        {
            bool isMuted = PlayerPrefs.GetInt("Audio_Muted") == 1;
            muteToggle.SetIsOnWithoutNotify(isMuted);
            
            if (isMuted)
            {
                ToggleMute(true);
            }
        }
    }
}