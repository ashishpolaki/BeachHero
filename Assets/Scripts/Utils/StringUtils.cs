namespace BeachHero
{
    public class StringUtils
    {
        public const string LEVELNUMBER = "LevelNumber";

        //Tutorial
        public const string SHOW_WELCOME_MESSAGE = "IsWelcomeMessageShown";
        public const string TAP_AND_DRAG_TUTORIAL = "TAP & DRAG THE BOAT";
        public const string RESCUE_ALL_TUTORIAL = "RESCUE THEM ALL !";

        //Trail Renderer
        public const string TRAIL_SPEED = "_Speed";

        //ToonShader (ShaderGraph Properties)
        public const string TINT_COLOR = "_Tint";

        //RateUS
        public const string RATE_US_SHOWN = "RateUsShown";

        //Tags
        public const string PLAYER_TAG = "Player";
        public const string OBSTACLE_TAG = "Obstacle";
        public const string CHARACTER_TAG = "Character";
        public const string GROUND_TAG = "Ground";

        //Sorting Layers
        public const string SPRITES_ABOVE_UI_LAYER = "SpritesAboveUI";
        public const string SPRITES_BELOW_UI_LAYER = "SpritesBelowUI";
        public const string UI_LAYER = "UI";

        //Audio
        public const string MUSIC_ON = "Music";
        public const string SOUND_ON = "Sound";
        public const string SOUND_VOLUME = "SoundVolume";
        public const string MUSIC_VOLUME = "MusicVolume";

        //Medals
        public const string TOTAL_MEDALS = "TotalMedals";

        //Haptics
        public const string HAPTICS_ON = "Haptics";

        //Animations
        public const string SINKING_ANIM = "Sinking";
        public const string IDLE_ANIM = "Idle";
        public const string VICTORY_ANIM = "Victory";
        public const string DROWN_ANIM = "Drown";

        //Powerups
        public const string SPEEDBOOST_UNLOCKED = "SpeedBoostUnlockLevel";
        public const string SHIELD_UNLOCKED = "ShieldUnlockLevel";
        public const string SPEEDBOOST_BALANCE = "SpeedBoostBalance";
        public const string SHIELD_BALANCE = "ShieldBalance";

        //Boat Skins
        public const string CURRENT_BOAT_INDEX = "BoatSelectionIndex";
        public const string CURRENT_BOAT_COLOR_INDEX = "CurrentBoatColorIndex_";
        public const string BOAT_SKIN_UNLOCKED = "BoatSkin_";
        public const string BOAT_SKIN_COLOR_UNLOCK = "BoatSkinColor_";

        //Replace Boat Colors (ShaderGraph Properties)
        public const string BOAT_REPLACEABLE_COLORS_KEY = "_CanReplaceColors";
        public const string BOAT_TARGETCOLOR_PREFIX = "_TargetColor_";
        public const string BOAT_REPLACECOLOR_PREFIX = "_ReplaceColor_";

        //Product Purchase
        public const string PRODUCT_PURCHASED_SUCCESS = "Purchase successful!";
        public const string PRODUCT_PURCHASE_FAILED = "Purchase failed. Please try again later.";

        //Game Currency
        public const string GAME_CURRENCY_BALANCE = "GameCurrencyBalance";

        //Medals
        public const string MEDAL_EARNED_PREFIX = "MedalEarned_Level_";

        //ADS
        public const string NO_ADS_PURCHASED = "NoAdsPurchased";

        //Scenes
        public const string GAME_SCENE = "Game";
        public const string INIT_SCENE = "Init";

        //Tutorial Speech Messages
        public const string TUTORIAL_WELCOME_MESSAGE = "WELCOME, BRAVE HERO!\nBEACH IS IN TROUBLE,\nITS TIME TO DIVE IN\nAND SAVE THE DAY!";
        public const string SPEEDBOOST_POWERUP_TUTORIAL_MESSAGE = "HIT THE BOOST AND\nSPEED UP THE BOAT!";
        public const string SHIELD_POWERUP_TUTORIAL_MESSAGE = "ACTIVATE THE SHIELD\nAND BLOCK ONE HIT!";

        //Game Lose Message
        public const string CONSECUTIVE_LOSE_HINT = "SAVE THE PEOPLE BEFORE\nTHEY DROWN TO\nCOMPLETE THIS LEVEL.";
    }

    public class IntUtils
    {
        //Powerup
        public const int SPEEDBOOST_UNLOCK_LEVEL = 3;
        public const int SHIELD_UNLOCK_LEVEL = 4;
        public const int FREEZE_UNLOCK_LEVEL = 5;
        public const int STARFISH_MULTIPLIER_UNLOCK_LEVEL = 6;
        //Powerup Balances
        public const int DEFAULT_SPEEDBOOST_BALANCE = 2;
        public const int DEFAULT_SHIELD_BALANCE = 2;
        public const int DEFAULT_FREEZE_BALANCE = 2;
        public const int DEFAULT_STARFISHMULTIPLIER_BALANCE = 2;

        //Game Currency
        public const int DEFAULT_GAME_CURRENCY_BALANCE = 100;
        public const int BASE_GAME_CURRENCY_REWARD = 3;
        public const int MULTIPLIER_GAME_CURRENCY_REWARD = 2;

        // Level 
        public const int DEFAULT_LEVEL = 1;

        //Scene
        public const int MAP_SCENE_LOAD_DELAY = 500; //1000 milliseconds = 1 second
        public const int GAME_SCENE_LOAD_DELAY = 1000;

        //Boat
        public const int DEFAULT_BOAT_INDEX = 0;
        public const int DEFAULT_BOAT_COLOR_INDEX = 0;

        //Sorting Layers
        public const int TUTORIAL_CANVAS_LAYER = 2;

        //Rate Us
        public const int RATE_US_TRIGGER_LEVEL = 5;
        public const int RATE_US_MIN_RATING_FOR_STORE = 3;

        //Ads
        public const int INTERSTITIAL_AD_INTERVAL = 3;
        public const int ADS_START_LEVEL = 1;
    }

}
