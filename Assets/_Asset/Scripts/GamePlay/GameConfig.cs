
using UnityEngine;

public class GameConfig
{
    public static bool isHackInapp = false;
    public static bool isHackAds = false;

    public static bool isLowGraphic = false;
    public static bool isShowOpenAdsFisrt = false;  

    public static int levelMinShowInter = 5;
    public static int INTER_COOLDOWN = 75;
    public static float timeWaitPopupShowInter = 1.5f;

    public static string placementInterQuitGame = "QuitGame";
    public static string placementInterEndGame = "EndGame";
    public static string placementInterEndGameWin = "EndGameWin";

    public static float posYBanner = 0;
    public static bool isAdsOn = true;
    public static bool isRemoveAds = false;

    public static float timelimitplay = 15f;
    public static bool isEnableLog = false;
    public static float sizeCell = 1f;
    public static int LiveWhenStartGame = 3;

    public static int goldNewUser = 150;

    public static float priceRefillTime = 100;
    public static int timeRefillTime = 60;
    public static float priceRevival = 100;
    public static float speed = 25f;

    public static float dotSpeed = 0.25f;

    public const int HEART_MAX_RECOVERY = 5;

    public const string SCENE_ROOT = "00Root";
    public const string SCENE_LOGIN = "Login";
    public const string SCENE_LOBBY = "02Lobby";
    public const string SCENE_GAME = "01GamePlay";
    public const string SCENE_LOADING = "03Loading";
    public const string SCENE_GD = "99TestGd";
    public const string SCENE_GAME_PROTOTYPE = "GamePrototype";

    public static bool MOVE_ZIGZAG = false;
    public static bool MOVE_OUT_ZIGZAG = true;
    public static bool OUT_ZIGZAG = true;
    public static bool SMOOTH_CORNER = true;

    public static float RATIO = 1.7f;
    public static bool IS_TABLET = false;

    public static string sceneNext = "";



    //tutorial
    public const int lvlTutMove = 1;
    public const int lvlTutZoomOutIn = 6;
    public const int lvlTutShowLineGecko = 7;
    //Level Unlock Booster
    public const int lvlUnlockEscapseGecko = 4;
    public const int lvlUnlockFindGeckoCanMove = 9;
    public const int lvlUnlockStopTime = 8;
    public const int lvlUnlockDestroyIceHamer = 14;
    public const int lvlUnlockDestroyBarrier = 43;

    //Level Unlock Tut
    public const int lvlTutFeatureBrick = 11;
    public const int lvlTutFeatureHoleExit = 21;
    public const int lvlTutFeatureStop = 31;
    public const int lvlTutFeatureBarrier = 41;

    // Key event, placement
    public const string ADS_PLACEMENT_WINX2 = "WIN_X2";
    public const string ADS_PLACEMENT_TIMEOUT = "TIMEOUT";
    public const string ADS_PLACEMENT_BUY_BOOSTER = "BUY_BOOSTER";
    public const string ADS_PLACEMENT_REFRESH = "REFRESH_INTER";

    // Notice String
    public const string NOTICE_NO_INTERNET = "PLEASE CHECK YOUR NETWORK!";
    public const string WATCH_VIDEOADS_UNSUCCESS = "PLEASE CHECK YOUR NETWORK!";
    public const string WATCH_VIDEOADS_NEED_FULL = "You need to watch the full ad to receive rewards!";
    public const string NotFindGeckoEscapse = "No have snake can move.";


    // Tool mkt request 
    public static TypeDev typeDev = TypeDev.none; // nếu != none sẽ lấy confing theo cái này, dùng cho mkt
    public static int lvMkt = 0;
    public static int lvMktProductPA = -1;
    public static string keyDataMkt = "levelGD";
    public static int lvMktOpenNeedVideo = -1;
    public static int indexBgMkt = -1;

    //------------------------------------------------------
}

public enum ColorGecko
{
    GREEN = 0,
    YELLOW = 1,
    ORANGE = 2,
    RED = 3,
    BLUE = 4,
    PURPLE = 5,
    PINK = 6,
}

public enum TypeNodeValueInMap
{
    EMPTY = 0,
    NODE_GECKO = 2,
    OBSTACLE_BRICK = 3,
    OBSTALCE_EXIT_HOLE = 4,
    OBSTALCE_STOP = 5,
    OBSTALCE_BARRIER = 6,
}

public enum DirectionMove
{
    UP = 0,
    DOWN = 1,
    LEFT = 2,
    RIGHT = 3
}

public enum ItemType
{
    NONE = 0,
    EXP_ACCOUNT = 1,
    GOLD = 2,
    HEART_1H = 3,
    HEART_3H = 4,
    HEART_6H = 5,
    HEART_12H = 6,
    HEART_24H = 7,
    HEART = 8, // tuong tu stamina

    //booster
    ESCAPE_GECKO = 9,
    BRICK_BROKEN = 10,
    TIME_STOP_ICE = 11,
    BARRIER = 12,
    FIND_GECKO_MOVE = 13,
    SHOW_LINE = 99, // KO SHOW TRONG SHOP OR INVENT
}

public enum ShopPackageId
{
    NONE = 0,
    REMOVE_ADS = 1,
    MINI_PACK = 2,
    MEDIUM_PACK = 3,
    SUPER_PACK = 4,
    CHAMPION_PACK = 5,
    GOD_PACK = 6,
    REMOVE_ADS_2 = 7,

    BUNDLE_01 = 8,
    BUNDLE_02 = 9,
    BUNDLE_03 = 10,
}