// using System;
// using LitMotion;
// using LitMotion.Extensions;
// using UnityEngine;
//
// namespace BeachHero.LitTween
// {
// // Tween.cs - Minimal version
//     public static class Tween
//     {
//         public static MotionHandle Move(Transform t, Vector3 to, float time) =>
//             LMotion.Create(t.position, to, time).BindToPosition(t).AddTo(t.gameObject).ToMotionHandle();
//     
//         public static MotionHandle Scale(Transform t, Vector3 to, float time) =>
//             LMotion.Create(t.localScale, to, time).BindToLocalScale(t).AddTo(t.gameObject).ToMotionHandle();
//     
//         public static MotionHandle Fade(CanvasGroup g, float to, float time) =>
//             LMotion.Create(g.alpha, to, time).Bind(x => g.alpha = x).ToMotionHandle();
//     
//         public static MotionHandle Delay(float time, Action action) =>
//             LMotion.Create(0f, 1f, time).WithOnComplete(action).RunWithoutBinding();
//     }
//
// // Usage: 
// // Tween.Move(myTransform, target, 1f);
// // Tween.Delay(2f, () => Destroy(gameObject));
// }