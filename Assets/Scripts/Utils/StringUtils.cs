using UnityEditor;

namespace BeachHero
{
    public class StringUtils
    {
        public const string LEVELNUMBER = "LevelNumber";

        //Whirlpool
        public const string WHIRLPOOL_DISTANCE = "_WP_Distance";
        public const string WHIRLPOOL_POSITION = "_WP_Position";
        public const string WHIRLPOOL_ENABLE = "_WP_Enable";

        //Trail Renderer
        public const string TRAIL_SPEED = "_Speed";

        //Tags
        public const string PLAYER_TAG = "Player";
        public const string OBSTACLE_TAG = "Obstacle";
        public const string CHARACTER_TAG = "Character";
        public const string GROUND_TAG = "Ground";


        //Animations
        public const string SINKING_ANIM = "Sinking";
        public const string IDLE_ANIM = "Idle";
        public const string VICTORY_ANIM = "Victory";

        //Powerups
        public const string MAGNET_UNLOCKED = "MagnetUnlockLevel";
        public const string SPEEDBOOST_UNLOCKED = "SpeedBoostUnlockLevel";
        public const string MAGNET_BALANCE = "MagnetBalance";
        public const string SPEEDBOOST_BALANCE = "SpeedBoostBalance";

        //Boat Skins
        public const string CURRENT_BOAT_INDEX = "BoatSelectionIndex";
        public const string CURRENT_BOAT_COLOR_INDEX = "CurrentBoatColorIndex_";
        public const string BOAT_SKIN_UNLOCKED = "BoatSkin_";
        public const string BOAT_SKIN_COLOR_UNLOCK = "BoatSkinColor_";

        //Audio
        public const string GAME_MUSIC_VOLUME = "GameMusicVolume";

        //Product Purchase
        public const string PRODUCT_PURCHASED_SUCCESS = "Purchase successful!";
        public const string PRODUCT_PURCHASE_FAILED = "Purchase failed. Please try again later.";

        //Game Currency
        public const string GAME_CURRENCY_BALANCE = "GameCurrencyBalance";

        //ADS
        public const string NO_ADS_PURCHASED = "NoAdsPurchased";

        ///Scenes
        public const string MAP_SCENE = "BeachMetaMap";
        public const string GAME_SCENE = "BeachHeroGame";
    }

    public class IntUtils
    {
        //Powerup
        public const int MAGNET_UNLOCK_LEVEL = 2;
        public const int SPEEDBOOST_UNLOCK_LEVEL = 3;
        public const int DEFAULT_MAGNET_BALANCE = 2;
        public const int DEFAULT_SPEEDBOOST_BALANCE = 2;

        //Game Currency
        public const int DEFAULT_GAME_CURRENCY_BALANCE = 100;

        // Level 
        public const int DEFAULT_LEVEL = 1;

        //Scene
        public const int MAP_SCENE_LOAD_DELAY = 500;
    }

}
