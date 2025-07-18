using Haptics;

namespace BeachHero
{
    public class HapticsManager : SingleTon<HapticsManager>
    {
        public Most_HapticFeedback.CustomHapticPattern CustomHapticPatternA;
        public Most_HapticFeedback.CustomHapticPattern CustomHapticPatternB;

        public void Init()
        {
            bool hapticsEnabled = SaveSystem.LoadBool(StringUtils.HAPTICS_ON, true);
            ToggleHaptics(hapticsEnabled);
        }

        public void GenerateBasicHaptic(Most_HapticFeedback.HapticTypes type)
        {
            Most_HapticFeedback.Generate(type);
        }

        public void GenerateBasicHapticWithCoolDown(Most_HapticFeedback.HapticTypes type, float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(type, cooldown);
        }

        public void GenerateCustomHapticA()
        {
            StartCoroutine(Most_HapticFeedback.GeneratePattern(CustomHapticPatternA));
        }

        public void GenerateCustomHapticB()
        {
            StartCoroutine(Most_HapticFeedback.GeneratePattern(CustomHapticPatternB));
        }

        #region Basic
        // __________________________________ Basic Haptics __________________________________
        public void SelectionHaptic()
        {
            Most_HapticFeedback.Generate(Most_HapticFeedback.HapticTypes.Selection);
        }

        public void SuccessHaptic()
        {
            Most_HapticFeedback.Generate(Most_HapticFeedback.HapticTypes.Success);
        }

        public void WarningHaptic()
        {
            Most_HapticFeedback.Generate(Most_HapticFeedback.HapticTypes.Warning);
        }

        public void FailureHaptic()
        {
            Most_HapticFeedback.Generate(Most_HapticFeedback.HapticTypes.Failure);
        }

        public void LightImpactHaptic()
        {
            Most_HapticFeedback.Generate(Most_HapticFeedback.HapticTypes.LightImpact);
        }

        public void MediumImpactHaptic()
        {
            Most_HapticFeedback.Generate(Most_HapticFeedback.HapticTypes.MediumImpact);
        }

        public void HeavyImpactHaptic()
        {
            Most_HapticFeedback.Generate(Most_HapticFeedback.HapticTypes.HeavyImpact);
        }

        public void RigidImpactHaptic()
        {
            Most_HapticFeedback.Generate(Most_HapticFeedback.HapticTypes.RigidImpact);
        }

        public void SoftImpactHaptic()
        {
            Most_HapticFeedback.Generate(Most_HapticFeedback.HapticTypes.SoftImpact);
        }
        #endregion

        #region Basic With Cooldown
        // __________________________________ Basic Haptics with Cooldown __________________________________ 
        public void SelectionHapticWithCooldown(float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(Most_HapticFeedback.HapticTypes.Selection, cooldown);
        }

        public void SuccessHapticWithCooldown(float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(Most_HapticFeedback.HapticTypes.Success, cooldown);
        }

        public void WarningHapticWithCooldown(float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(Most_HapticFeedback.HapticTypes.Warning, cooldown);
        }

        public void FailureHapticWithCooldown(float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(Most_HapticFeedback.HapticTypes.Failure, cooldown);
        }

        public void LightImpactHapticWithCooldown(float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(Most_HapticFeedback.HapticTypes.LightImpact, cooldown);
        }

        public void MediumImpactHapticWithCooldown(float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(Most_HapticFeedback.HapticTypes.MediumImpact, cooldown);
        }

        public void HeavyImpactHapticWithCooldown(float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(Most_HapticFeedback.HapticTypes.HeavyImpact, cooldown);
        }

        public void RigidImpactHapticWithCooldown(float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(Most_HapticFeedback.HapticTypes.RigidImpact, cooldown);
        }

        public void SoftImpactHapticWithCooldown(float cooldown)
        {
            Most_HapticFeedback.GenerateWithCooldown(Most_HapticFeedback.HapticTypes.SoftImpact, cooldown);
        }
        #endregion

        // ___________________ Enable / Disable Haptic Feedback ___________________  
        public void ToggleHaptics(bool enabled)
        {
            Most_HapticFeedback.HapticsEnabled = enabled;
        }
    }
}
