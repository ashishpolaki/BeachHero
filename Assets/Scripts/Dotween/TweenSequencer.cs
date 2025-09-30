using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class TweenSequencer : MonoBehaviour
{
    [SerializeReference] private TweenClipBase[] clips;
    public int loopCount = 0;              // 0 = no loop, -1 = infinite loop
    public LoopType loopType = LoopType.Restart;
    public float timelineDuration = 1f; 
    public TweenClipBase[] Clips => clips;

    public Sequence _sequence;

    public void BuildSequence()
    {
        // kill previous
        if (_sequence != null && _sequence.IsActive())
        {
            _sequence.Kill();
            _sequence = null;
        }

        _sequence = DOTween.Sequence().SetAutoKill(false).Pause();

        // apply loop settings
        _sequence.SetLoops(loopCount, loopType);

        if (clips == null)
        {
            return;
        }

        foreach (var clip in clips)
        {
            if (clip == null)
            {
                continue;
            }
            var tween = clip.GetTween();
            if (tween == null)
            {
                continue;
            }
            _sequence.Insert(clip.startTime, tween);
        }

    }
    public void Play()
    {
        if (_sequence == null || !_sequence.IsActive())
        {
            BuildSequence();
        }
        _sequence?.Restart(); // restart ensures it plays from beginning each time
    }
    public void Pause()
    {
        _sequence?.Pause();
    }
    public void Kill()
    {
        KillAllClips();
        if (_sequence != null && _sequence.IsActive())
        {
            _sequence.Kill();
            _sequence = null;
        }
    }
    private void KillAllClips()
    {
        if (clips == null) return;
        foreach (var c in clips) c?.KillTween();
    }
    public void ApplyAllFromStates()
    {
        if (clips == null) return;
        foreach (var c in clips)
        {
            c?.ApplyFromState();
        }
    }
}
