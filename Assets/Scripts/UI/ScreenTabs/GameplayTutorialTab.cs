using UnityEngine;
using Febucci.UI;
using System.Threading.Tasks;

namespace BeachHero
{
    public class GameplayTutorialTab : BaseScreenTab
    {
        [SerializeField] private TextAnimatorPlayer instructionText;

        private Camera cam;
        private TutorialType currentTutorialType;

        public override async void Open()
        {
            base.Open();
            if (TutorialController.GetInstance != null)
            {
                TutorialController.GetInstance.OnPathDrawnAction += OnPathDrawn;
            }
            currentTutorialType = TutorialController.GetInstance.TutorialType;
            cam = GameController.GetInstance.LevelController.Cam;
            await Task.Delay(100); // Wait for a frame to ensure the camera is set up
            InitializeTutorial();
        }

        public override void Close()
        {
            base.Close();
            if (TutorialController.GetInstance != null)
            {
                TutorialController.GetInstance.OnPathDrawnAction -= OnPathDrawn;
            }
            TutorialController.GetInstance.TutorialHand.Hide();
        }

        private void InitializeTutorial()
        {
            DisplayInstructionText();
            HandAnimation();
        }

        private void DisplayInstructionText()
        {
            string text = currentTutorialType switch
            {
                TutorialType.TapAndDrag => StringUtils.TAP_AND_DRAG_TUTORIAL,
                TutorialType.RescueAll => StringUtils.RESCUE_ALL_TUTORIAL,
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(text))
                instructionText.ShowText(text);
        }

        private void HideInstructionText() => instructionText.StartDisappearingText();

        private void OnPathDrawn()
        {
            HideInstructionText();
            Close();
        }

        private void HandAnimation()
        {
            var canvas = UIController.GetInstance.Canvas;
            var level = GameController.GetInstance.LevelController;

            Vector3 playerLocalPos = WorldToCanvasLocalPosition(cam, canvas, level.PlayerTransform.position);
            int charIndex = currentTutorialType == TutorialType.TapAndDrag ? 0 : 1;

            Vector3 characterLocalPos = WorldToCanvasLocalPosition(
                cam, canvas, level.GetDrowningCharacter(charIndex).position);
            TutorialController.GetInstance.TutorialHand.PlayPunchThenMoveLoop(playerLocalPos, characterLocalPos);
            TutorialController.GetInstance.TutorialHand.SetHandSortingLayer(StringUtils.SPRITES_BELOW_UI_LAYER, 5);
        }

        private Vector3 WorldToCanvasLocalPosition(Camera camera, Canvas canvas, Vector3 worldPosition)
        {
            if (camera == null || canvas == null)
            {
                DebugUtils.LogError("WorldToCanvasLocalPosition: Missing camera or canvas reference.");
                return Vector3.zero;
            }

            // Convert world - screen position
            Vector3 screenPos = camera.WorldToScreenPoint(worldPosition);

            // Convert screen - world position on canvas plane
            Vector3 worldOnCanvas = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, canvas.planeDistance));

            // Convert to local position on canvas RectTransform
            RectTransform canvasRect = canvas.transform as RectTransform;
            Vector3 localPos = canvasRect.InverseTransformPoint(worldOnCanvas);
            localPos.z = 0f; // Keep flat on 2D plane

            return localPos;
        }
    }
}
