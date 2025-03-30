# Unity Audio System - Руководство пользователя

## Содержание
1. [Обзор системы](#обзор-системы)
2. [Настройка аудио миксера](#настройка-аудио-миксера)
3. [Настройка AudioManager](#настройка-audiomanager)
4. [Воспроизведение звуков](#воспроизведение-звуков)
5. [Работа с музыкой](#работа-с-музыкой)
6. [Воспроизведение шагов персонажа](#воспроизведение-шагов-персонажа)
7. [Управление громкостью](#управление-громкостью)
8. [UI для аудио настроек](#ui-для-аудио-настроек)
9. [Расширенное использование](#расширенное-использование)
10. [Примеры использования](#примеры-использования)

## Обзор системы

Эта аудио система предоставляет комплексное решение для управления звуком в Unity с поддержкой категорий, миксеров, случайных звуков и звуков шагов персонажа. Система включает:

- **AudioManager**: Основной контроллер для воспроизведения всех звуков в игре
- **RandomSoundPlayer**: Компонент для воспроизведения случайных звуков из набора
- **CollisionSoundPlayer**: Воспроизведение звуков при столкновениях объектов
- **FootstepController**: Управление звуками шагов на различных поверхностях
- **AudioSettingsUI**: Пользовательский интерфейс для управления настройками звука

## Настройка аудио миксера

### Шаг 1: Создание миксера
1. Выберите **Window → Audio → Audio Mixer**
2. Нажмите **+** для создания нового миксера
3. Назовите миксер (например, "MainMixer")

### Шаг 2: Создание групп
1. Щелкните правой кнопкой мыши на группе Master
2. Выберите **Add Child Group**
3. Создайте пять групп со следующими именами:
   - Music
   - SFX
   - Ambient
   - Voice
   - UI

### Шаг 3: Настройка параметров
Для каждой группы:
1. Выберите группу
2. Найдите регулятор Volume в инспекторе
3. Щелкните правой кнопкой мыши и выберите **Expose Parameter**
4. Назовите параметр (важно использовать точные имена):
   - MusicVolume
   - SFXVolume
   - AmbientVolume
   - VoiceVolume
   - UIVolume

### Шаг 4: Добавление эффектов (опционально)
1. Выберите группу
2. Нажмите **Add Effect** в инспекторе
3. Настройте эффекты по вашему усмотрению

## Настройка AudioManager

### Шаг 1: Добавление компонента
1. Создайте пустой GameObject с именем "AudioManager"
2. Добавьте скрипт AudioManager
3. Отметьте объект как DontDestroyOnLoad, если хотите сохранять звуки между сценами

### Шаг 2: Назначение миксера
1. Перетащите созданный аудио миксер в поле **Audio Mixer** в инспекторе AudioManager

### Шаг 3: Добавление звуков
1. Разверните секцию **Звуковые настройки**
2. Укажите размер массива звуков
3. Для каждого звука заполните:
   - name: Уникальное имя звука
   - clip: Аудио файл
   - category: Категория звука (Music, SFX, Ambient, Voice, UI)
   - volume: Громкость (0-1)
   - pitch: Высота звука (0.1-3)
   - loop: Зацикливание
   - spatialBlend: 0 для 2D-звуков, 1 для 3D-звуков

## Воспроизведение звуков

### Базовое воспроизведение
```csharp
// Воспроизведение по имени
AudioManager.Instance.Play("JumpSound");

// Остановка звука
AudioManager.Instance.Stop("JumpSound");

// Пауза и продолжение
AudioManager.Instance.Pause("JumpSound");
AudioManager.Instance.Resume("JumpSound");
```

### Воспроизведение 3D-звуков
```csharp
// Воспроизведение звука в определенной позиции
AudioManager.Instance.PlayAtPosition("ExplosionSound", explosionPosition);
```

### Динамическое воспроизведение
```csharp
// Воспроизведение звука без предварительной настройки
AudioClip newSound = Resources.Load<AudioClip>("Sounds/NewSound");
AudioManager.Instance.PlayOneShot(newSound, SoundCategory.SFX, 0.8f, 1.1f);
```

## Работа с музыкой

```csharp
// Воспроизведение музыки с плавным переходом
AudioManager.Instance.PlayMusic("MainTheme", 2.0f);

// Переключение на другую музыку с кроссфейдом
AudioManager.Instance.PlayMusic("BattleTheme", 1.5f);
```

## Воспроизведение шагов персонажа

### Шаг 1: Добавление FootstepController
Создайте новый скрипт FootstepController и добавьте его в ваш проект:

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SurfaceType
{
    public string surfaceName;
    public PhysicMaterial material;
    public int soundGroupIndex;
}

public class FootstepController : MonoBehaviour
{
    public RandomSoundPlayer soundPlayer;
    public SurfaceType[] surfaceTypes;
    public float stepDistance = 2.5f;
    public Transform footRayOrigin;
    
    private Vector3 lastStepPosition;
    private CharacterController characterController;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (soundPlayer == null)
            soundPlayer = GetComponent<RandomSoundPlayer>();
            
        if (footRayOrigin == null)
            footRayOrigin = transform;
            
        lastStepPosition = transform.position;
    }
    
    void Update()
    {
        // Автоматическое воспроизведение шагов на основе пройденного расстояния
        if (characterController != null && characterController.isGrounded)
        {
            Vector3 horizontalMovement = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 lastPosition = new Vector3(lastStepPosition.x, 0, lastStepPosition.z);
            
            float distanceMoved = Vector3.Distance(horizontalMovement, lastPosition);
            
            if (distanceMoved >= stepDistance && characterController.velocity.magnitude > 0.1f)
            {
                PlayFootstep();
                lastStepPosition = transform.position;
            }
        }
    }
    
    // Вызывается вручную или через Animation Event
    public void PlayFootstep()
    {
        if (soundPlayer == null) return;
        
        RaycastHit hit;
        int defaultSoundIndex = 0;
        
        if (Physics.Raycast(footRayOrigin.position, Vector3.down, out hit, 1.5f))
        {
            // Пытаемся определить поверхность по PhysicMaterial
            PhysicMaterial physicMaterial = hit.collider.sharedMaterial;
            
            if (physicMaterial != null)
            {
                foreach (SurfaceType surface in surfaceTypes)
                {
                    if (surface.material == physicMaterial)
                    {
                        soundPlayer.PlayRandomSound(surface.soundGroupIndex);
                        return;
                    }
                }
            }
            
            // Запасной вариант - определение по тегу
            string tag = hit.collider.tag;
            foreach (SurfaceType surface in surfaceTypes)
            {
                if (surface.surfaceName == tag)
                {
                    soundPlayer.PlayRandomSound(surface.soundGroupIndex);
                    return;
                }
            }
        }
        
        // Если поверхность не определена, воспроизводим звук по умолчанию
        soundPlayer.PlayRandomSound(defaultSoundIndex);
    }
}
```

### Шаг 2: Настройка RandomSoundPlayer для шагов
1. Добавьте компонент RandomSoundPlayer к объекту персонажа
2. Настройте группы звуков для разных поверхностей:
   - Группа 0: Бетон/Камень
   - Группа 1: Трава
   - Группа 2: Металл
   - Группа 3: Дерево
   - Группа 4: Вода
   - И так далее...

### Шаг 3: Настройка FootstepController
1. Добавьте компонент FootstepController к объекту персонажа
2. Перетащите компонент RandomSoundPlayer в поле soundPlayer
3. Настройте массив surfaceTypes, указав для каждого типа поверхности:
   - surfaceName: имя тега (например, "Grass")
   - material: PhysicMaterial поверхности
   - soundGroupIndex: индекс группы звуков в RandomSoundPlayer

### Шаг 4: Использование
Вы можете воспроизводить шаги двумя способами:
1. **Автоматически**: Система будет воспроизводить шаги на основе пройденного расстояния
2. **Через Animation Events**: Добавьте события в анимацию ходьбы/бега, вызывающие метод PlayFootstep()

## Управление громкостью

```csharp
// Установка громкости для категории
AudioManager.Instance.SetCategoryVolume(SoundCategory.Music, 0.8f);
AudioManager.Instance.SetCategoryVolume(SoundCategory.SFX, 0.5f);

// Получение текущей громкости
float musicVolume = AudioManager.Instance.GetCategoryVolume(SoundCategory.Music);
```

## UI для аудио настроек

### Шаг 1: Создание UI
1. Создайте Canvas с необходимыми UI элементами:
   - Слайдеры для каждой категории звука
   - Переключатель для mute

### Шаг 2: Настройка AudioSettingsUI
1. Добавьте скрипт AudioSettingsUI к объекту с UI
2. Настройте связи между слайдерами и категориями:
   - Добавьте элементы в массив volumeSliders
   - Для каждого элемента укажите категорию и соответствующий слайдер
   - Назначьте toggle для управления mute

### Шаг 3: Сохранение настроек
Вызовите метод SaveSettings() для сохранения настроек:
```csharp
audiSettingsUI.SaveSettings();
```

Настройки будут автоматически загружены при старте.

## Расширенное использование

### Снэпшоты миксера
Для переключения между предустановками:
1. Создайте снэпшоты в Audio Mixer
2. Добавьте метод в AudioManager:

```csharp
public void TransitionToSnapshot(string snapshotName, float transitionTime = 1.0f)
{
    if (audioMixer != null)
    {
        audioMixer.FindSnapshot(snapshotName).TransitionTo(transitionTime);
    }
}
```

### Динамические звуковые эффекты
Для создания звуковых эффектов на основе игрового процесса:

```csharp
// Например, изменение высоты звука в зависимости от скорости
float speed = playerRigidbody.velocity.magnitude;
soundSource.pitch = Mathf.Lerp(0.8f, 1.5f, speed / maxSpeed);
```

## Примеры использования

### Пример 1: Базовая настройка для игры
```csharp
void Start()
{
    // Воспроизведение фоновой музыки
    AudioManager.Instance.PlayMusic("MenuTheme");
    
    // Настройка громкости по умолчанию
    AudioManager.Instance.SetCategoryVolume(SoundCategory.Music, 0.7f);
    AudioManager.Instance.SetCategoryVolume(SoundCategory.SFX, 0.8f);
    AudioManager.Instance.SetCategoryVolume(SoundCategory.Ambient, 0.5f);
}
```

### Пример 2: Звуки для игрового интерфейса
```csharp
public void OnButtonClick()
{
    AudioManager.Instance.Play("ButtonClick");
}

public void OnMenuOpen()
{
    AudioManager.Instance.Play("MenuOpen");
}
```

### Пример 3: Звуки для оружия
```csharp
public void Fire()
{
    // Воспроизведение звука выстрела
    AudioManager.Instance.Play("GunShot");
    
    // Воспроизведение звука гильзы в 3D
    AudioManager.Instance.PlayAtPosition("ShellCasing", ejectionPort.position);
}
```

### Пример 4: Звуки окружения с случайными вариациями
```csharp
void Start()
{
    StartCoroutine(PlayRandomAmbientSounds());
}

IEnumerator PlayRandomAmbientSounds()
{
    RandomSoundPlayer ambientPlayer = GetComponent<RandomSoundPlayer>();
    
    while (true)
    {
        // Воспроизводим случайный звук из группы 0 (звуки окружения)
        ambientPlayer.PlayRandomSound(0);
        
        // Ждем случайное время перед следующим звуком
        yield return new WaitForSeconds(Random.Range(10f, 30f));
    }
}
```

### Пример 5: Шаги персонажа с Animation Events
```csharp
// Добавьте этот метод и вызовите его через Animation Events
public void FootstepLeft()
{
    GetComponent<FootstepController>().PlayFootstep();
}

public void FootstepRight()
{
    GetComponent<FootstepController>().PlayFootstep();
}
```

---
Эта система предоставляет мощные и гибкие инструменты для управления звуком в вашей игре. При правильной настройке она позволит создать богатый звуковой ландшафт, который повысит качество игрового опыта.
