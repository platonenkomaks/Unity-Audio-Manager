using UnityEngine;

public class CollisionSoundPlayer : MonoBehaviour
{
    public AudioClip[] impactSounds;
    public SoundCategory category = SoundCategory.SFX;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitchMin = 0.9f;
    [Range(0.1f, 3f)] public float pitchMax = 1.1f;
    public float minImpactForce = 0.1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (impactSounds.Length == 0) return;

        // Проверяем силу столкновения
        var impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < minImpactForce) return;

        // Выбираем случайный звук
        AudioClip randomSound = impactSounds[Random.Range(0, impactSounds.Length)];
        if (randomSound == null) return;

        // Регулируем громкость в зависимости от силы столкновения
        var dynamicVolume = Mathf.Clamp01(volume * (impactForce / 10f));

        // Выбираем случайную высоту звука
        var randomPitch = Random.Range(pitchMin, pitchMax);

        // Воспроизводим звук в точке столкновения
        G.AudioManager.PlayOneShot(randomSound, category, dynamicVolume, randomPitch);
    }
}