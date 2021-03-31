using UnityEngine;

/// <summary>
/// Class to provides references to sound effects.
/// </summary>
public class SoundsManager : MonoBehaviour
{
    /// <summary>
    /// Current instance of <seealso cref="SoundsManager"/>.
    /// </summary>
    public static SoundsManager Manager { get; private set; }

    public AudioSource WinSound => transform.GetChild(0).GetComponent<AudioSource>();
    public AudioSource EnterTargetSound => transform.GetChild(1).GetComponent<AudioSource>();
    public AudioSource ExitTargetSound => transform.GetChild(2).GetComponent<AudioSource>();
    public AudioSource ClickSound => transform.GetChild(3).GetComponent<AudioSource>();

    private void Awake() => Manager = this;
}
