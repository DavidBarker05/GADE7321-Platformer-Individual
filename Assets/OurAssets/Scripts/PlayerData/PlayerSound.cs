using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField]
    PlayerMovement m_PlayerMovement;

    // David - Due to how blend trees work this isn't 100% effective, sometimes
    // the step sound doesn't play when they step and that's because the weight
    // isn't enough when the event happens, but if I lower the weight check then
    // other sounds play when they shouldn't. This is the closest I could reasonably
    // get
    public void PlaySound(AnimationEvent sender)
    {
        // David - Since in blend tree the animations all play at the same time
        // check the weight of the animation from the event to see if it is actually
        // playing so that don't play footsteps when idle or double footsteps
        // when walking or running
        if (sender.animatorClipInfo.weight <= 0.05f + Mathf.Epsilon * 8f) return; // David - 8f * Mathf.Epsilon is what Unity uses for Mathf.Approximately()
        if (sender.stringParameter == "step")
        {
            string groundTag = m_PlayerMovement.GroundTag;
            if (!string.IsNullOrWhiteSpace(groundTag)) SFXManager.Instance.PlayAudio(groundTag, SFXManager.Instance.DefaultAudioSource);
        }
    }

    public void PlaySound(string sound)
    {
        if (sound == "land")
        {
            string groundTag = m_PlayerMovement.GroundTag;
            if (!string.IsNullOrWhiteSpace(groundTag)) SFXManager.Instance.PlayAudio(groundTag, SFXManager.Instance.DefaultAudioSource);
            SFXManager.Instance.PlayAudio("Grunt", SFXManager.Instance.DefaultAudioSource);
        }
        else if (sound == "jump") SFXManager.Instance.PlayAudio("Grunt", SFXManager.Instance.DefaultAudioSource);
    }
}
