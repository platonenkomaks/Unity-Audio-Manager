Unity Audio System - User Guide
Contents

System Overview
Audio Mixer Setup
AudioManager Setup
Playing Sounds
Working with Music
Character Footstep Playback
Volume Control
UI for Audio Settings
Advanced Usage
Usage Examples

System Overview
This audio system provides a comprehensive solution for sound management in Unity with support for categories, mixers, random sounds, and character footstep sounds. The system includes:

AudioManager: Main controller for playing all sounds in the game
RandomSoundPlayer: Component for playing random sounds from a set
CollisionSoundPlayer: Playing sounds during object collisions
FootstepController: Managing footstep sounds on various surfaces
AudioSettingsUI: User interface for managing sound settings

Audio Mixer Setup
Step 1: Creating a Mixer

Select Window → Audio → Audio Mixer
Click + to create a new mixer
Name the mixer (for example, "MainMixer")

Step 2: Creating Groups

Right-click on the Master group
Select Add Child Group
Create five groups with the following names:

Music
SFX
Ambient
Voice
UI



Step 3: Setting Up Parameters
For each group:

Select the group
Find the Volume control in the inspector
Right-click and select Expose Parameter
Name the parameter (it's important to use exact names):

MusicVolume
SFXVolume
AmbientVolume
VoiceVolume
UIVolume



Step 4: Adding Effects (optional)

Select a group
Click Add Effect in the inspector
Configure effects as you prefer

AudioManager Setup
Step 1: Adding the Component

Create an empty GameObject named "AudioManager"
Add the AudioManager script
Mark the object as DontDestroyOnLoad if you want to preserve sounds between scenes

Step 2: Assigning the Mixer

Drag the created audio mixer to the Audio Mixer field in the AudioManager inspector

Step 3: Adding Sounds

Expand the Sound Settings section
Specify the size of the sounds array
For each sound, fill in:

name: Unique sound name
clip: Audio file
category: Sound category (Music, SFX, Ambient, Voice, UI)
volume: Volume (0-1)
pitch: Sound pitch (0.1-3)
loop: Looping
spatialBlend: 0 for 2D sounds, 1 for 3D sounds



Playing Sounds
Basic Playback
csharpCopy// Playing by name
AudioManager.Instance.Play("JumpSound");

// Stopping a sound
AudioManager.Instance.Stop("JumpSound");

// Pause and resume
AudioManager.Instance.Pause("JumpSound");
AudioManager.Instance.Resume("JumpSound");
Playing 3D Sounds
csharpCopy// Playing a sound at a specific position
AudioManager.Instance.PlayAtPosition("ExplosionSound", explosionPosition);
Dynamic Playback
csharpCopy// Playing a sound without prior configuration
AudioClip newSound = Resources.Load<AudioClip>("Sounds/NewSound");
AudioManager.Instance.PlayOneShot(newSound, SoundCategory.SFX, 0.8f, 1.1f);
Working with Music
csharpCopy// Playing music with a smooth transition
AudioManager.Instance.PlayMusic("MainTheme", 2.0f);

// Switching to different music with crossfade
AudioManager.Instance.PlayMusic("BattleTheme", 1.5f);
Character Footstep Playback
Step 1: Adding FootstepController
Create a new FootstepController script and add it to your project:
csharpCopyusing System.Collections;
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
        // Automatic footstep playback based on distance traveled
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
    
    // Called manually or through Animation Event
    public void PlayFootstep()
    {
        if (soundPlayer == null) return;
        
        RaycastHit hit;
        int defaultSoundIndex = 0;
        
        if (Physics.Raycast(footRayOrigin.position, Vector3.down, out hit, 1.5f))
        {
            // Trying to determine the surface by PhysicMaterial
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
            
            // Fallback - identification by tag
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
        
        // If the surface is not identified, play default sound
        soundPlayer.PlayRandomSound(defaultSoundIndex);
    }
}
Step 2: Setting Up RandomSoundPlayer for Footsteps

Add the RandomSoundPlayer component to the character object
Configure sound groups for different surfaces:

Group 0: Concrete/Stone
Group 1: Grass
Group 2: Metal
Group 3: Wood
Group 4: Water
And so on...



Step 3: Setting Up FootstepController

Add the FootstepController component to the character object
Drag the RandomSoundPlayer component to the soundPlayer field
Configure the surfaceTypes array, specifying for each surface type:

surfaceName: tag name (for example, "Grass")
material: Surface PhysicMaterial
soundGroupIndex: sound group index in RandomSoundPlayer



Step 4: Usage
You can play footsteps in two ways:

Automatically: The system will play footsteps based on distance traveled
Through Animation Events: Add events to the walk/run animation that call the PlayFootstep() method

Volume Control
csharpCopy// Setting volume for a category
AudioManager.Instance.SetCategoryVolume(SoundCategory.Music, 0.8f);
AudioManager.Instance.SetCategoryVolume(SoundCategory.SFX, 0.5f);

// Getting current volume
float musicVolume = AudioManager.Instance.GetCategoryVolume(SoundCategory.Music);
UI for Audio Settings
Step 1: Creating UI

Create a Canvas with the necessary UI elements:

Sliders for each sound category
Toggle for mute



Step 2: Setting Up AudioSettingsUI

Add the AudioSettingsUI script to the UI object
Configure connections between sliders and categories:

Add elements to the volumeSliders array
For each element, specify the category and corresponding slider
Assign the toggle for mute control



Step 3: Saving Settings
Call the SaveSettings() method to save settings:
csharpCopyaudiSettingsUI.SaveSettings();
Settings will be automatically loaded at startup.
Advanced Usage
Mixer Snapshots
For switching between presets:

Create snapshots in the Audio Mixer
Add a method to AudioManager:

csharpCopypublic void TransitionToSnapshot(string snapshotName, float transitionTime = 1.0f)
{
    if (audioMixer != null)
    {
        audioMixer.FindSnapshot(snapshotName).TransitionTo(transitionTime);
    }
}
Dynamic Sound Effects
For creating sound effects based on gameplay:
csharpCopy// For example, changing sound pitch based on speed
float speed = playerRigidbody.velocity.magnitude;
soundSource.pitch = Mathf.Lerp(0.8f, 1.5f, speed / maxSpeed);
Usage Examples
Example 1: Basic Game Setup
csharpCopyvoid Start()
{
    // Playing background music
    AudioManager.Instance.PlayMusic("MenuTheme");
    
    // Setting default volume
    AudioManager.Instance.SetCategoryVolume(SoundCategory.Music, 0.7f);
    AudioManager.Instance.SetCategoryVolume(SoundCategory.SFX, 0.8f);
    AudioManager.Instance.SetCategoryVolume(SoundCategory.Ambient, 0.5f);
}
Example 2: Sounds for Game Interface
csharpCopypublic void OnButtonClick()
{
    AudioManager.Instance.Play("ButtonClick");
}

public void OnMenuOpen()
{
    AudioManager.Instance.Play("MenuOpen");
}
Example 3: Sounds for Weapons
csharpCopypublic void Fire()
{
    // Playing gunshot sound
    AudioManager.Instance.Play("GunShot");
    
    // Playing shell casing sound in 3D
    AudioManager.Instance.PlayAtPosition("ShellCasing", ejectionPort.position);
}
Example 4: Environment Sounds with Random Variations
csharpCopyvoid Start()
{
    StartCoroutine(PlayRandomAmbientSounds());
}

IEnumerator PlayRandomAmbientSounds()
{
    RandomSoundPlayer ambientPlayer = GetComponent<RandomSoundPlayer>();
    
    while (true)
    {
        // Playing a random sound from group 0 (ambient sounds)
        ambientPlayer.PlayRandomSound(0);
        
        // Waiting a random time before the next sound
        yield return new WaitForSeconds(Random.Range(10f, 30f));
    }
}
Example 5: Character Footsteps with Animation Events
csharpCopy// Add this method and call it through Animation Events
public void FootstepLeft()
{
    GetComponent<FootstepController>().PlayFootstep();
}

public void FootstepRight()
{
    GetComponent<FootstepController>().PlayFootstep();
}

This system provides powerful and flexible tools for sound management in your game. When properly configured, it will allow you to create a rich sound landscape that enhances the quality of the gaming experience.
