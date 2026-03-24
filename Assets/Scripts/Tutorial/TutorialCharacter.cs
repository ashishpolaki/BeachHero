using UnityEngine;
using LitMotion;
using System;

namespace BeachHero
{
    public enum TutorialCharacterType
    {
        Idle,
        WaveHand,
        Cry,
        Talk
    }
    public class TutorialCharacter : MonoBehaviour
    {
        [SerializeField] private GameObject characterRoot;
        [SerializeField] private Animator animator;
        [SerializeField] private Ease moveEase;
        [SerializeField] private Vector3 moveOffset;
        [SerializeField] private float moveDuration;

        private int idleAnim = Animator.StringToHash("Idle");
        private int waveHandAnim = Animator.StringToHash("Wavehand");
        private int cryAnim = Animator.StringToHash("Cry");
        private int talkAnim = Animator.StringToHash("Talk");
        private TweenHandle moveTween;

        public void SkipAnimation()
        {
            animator.StopPlayback();
            characterRoot.SetActive(false);
            moveTween.Cancel();
        }

        public void PlayAnimation(TutorialCharacterType tutorialCharacterType, Vector3 pos, Action OnComplete = null)
        {
            characterRoot.transform.localPosition = pos + moveOffset;
            moveTween.Cancel();
            moveTween = TweenManager.Move(characterRoot.transform, characterRoot.transform.localPosition, pos, moveDuration, 0, LoopType.Restart, TransformSpace.Local, moveEase,OnComplete);
            characterRoot.SetActive(true);
            switch (tutorialCharacterType)
            {
                case TutorialCharacterType.WaveHand:
                    animator.Play(waveHandAnim, 0, 0);
                    break;
                case TutorialCharacterType.Cry:
                    animator.Play(cryAnim, 0, 0);
                    break;
                case TutorialCharacterType.Talk:
                    animator.Play(talkAnim, 0, 0);
                    break;
                case TutorialCharacterType.Idle:
                    animator.Play(idleAnim, 0, 0);
                    break;
                default:
                    break;
            }
        }
    }
}
