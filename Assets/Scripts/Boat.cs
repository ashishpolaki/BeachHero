using UnityEngine;

namespace BeachHero
{
    public class Boat : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] boatMeshRenderer;
        [SerializeField] private Transform boostPosition;
        [SerializeField] private Animator characterAnimator;

        private Material boatMaterial;
        private int VICTORY_HASH = Animator.StringToHash(StringUtils.VICTORY_ANIM);
        private int IDLE_HASH = Animator.StringToHash(StringUtils.IDLE_ANIM);

        public void PlayVictoryAnimation()
        {
            characterAnimator.SetTrigger(VICTORY_HASH);
        }
        private void PlayIdleAnimation()
        {
            characterAnimator.Play(IDLE_HASH, -1, Random.Range(0f, 1f));
        }

        public void SetBoatInit(int currentBoatIndex, int currentColorIndex)
        {
            for (int m = 0; m < boatMeshRenderer.Length; m++)
            {
                boatMaterial = boatMeshRenderer[m].material;
                boatMaterial.SetFloat(Shader.PropertyToID(StringUtils.BOAT_REPLACEABLE_COLORS_KEY), 1f);
                Color[] colors = GameController.GetInstance.SkinController.GetBoatPartColors(currentBoatIndex, currentColorIndex);
                for (int i = 0; i < colors.Length; i++)
                {
                    int index = i;
                    boatMaterial.SetColor(Shader.PropertyToID($"{StringUtils.BOAT_REPLACECOLOR_PREFIX}{index}"), colors[index]);
                }
                PlayIdleAnimation();
            }
        }
    }
}
