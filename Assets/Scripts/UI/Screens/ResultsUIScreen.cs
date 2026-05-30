namespace BeachHero
{
    public class ResultsUIScreen : BaseScreen
    {
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            AdController.GetInstance.HideBanner();
        }
    }
}
