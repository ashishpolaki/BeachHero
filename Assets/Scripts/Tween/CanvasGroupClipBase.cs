using LitMotion;
using System;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public abstract class CanvasGroupClipBase : TweenClipBase
    {
        public CanvasGroup target;
        public float fromAlpha = 1f;
        public float toAlpha = 0f;

        public CanvasGroupClipBase()
        {
            clipType = TweenClipType.CanvasGroup;
        }
    }

   [Serializable]
   public class CanvasGroupFadeClip : CanvasGroupClipBase
   {
       protected override MotionHandle CreateTweenCore()
       {
           if (target == null)
           {
               DebugUtils.LogError("Target CanvasGroup is null.");
           }
          return TweenManager.Fade(target,fromAlpha,toAlpha,duration,ease).Handle;
       }

       public override void ApplyFromState()
       {
           if (target != null)
           {
               target.alpha = fromAlpha;
           }
       }

       public override void ApplyToState()
       {
           if (target != null)
           {
               target.alpha = toAlpha;
           }
       }
   }
}
