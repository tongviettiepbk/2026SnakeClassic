using System;
using UnityEngine;


public class ConstantValues
{
    public const string SCENE_ROOT = "Root";
    public const string SCENE_LOGIN = "Login";
    public const string SCENE_LOBBY = "Lobby";
    public const string SCENE_GAME = "Game";
    public const string SCENE_GAME_PROTOTYPE = "GamePrototype";

    public const string TAG_TEAM_A = "TeamA";
    public const string TAG_TEAM_B = "TeamB";

    public const string LAYER_TOWER = "Tower";
    public const string LAYER_WALL = "Wall";
    public const string LAYER_HITBOX = "Hitbox";

    public const string SORTING_LAYER_GROUND = "Ground";
    public const string SORTING_LAYER_UNIT = "Unit";
    public const string SORTING_LAYER_PROJECTILE = "Projectile";
    public const string SORTING_LAYER_FX = "Fx";
    public const string SORTING_LAYER_UI = "UI";
    public const string SORTING_LAYER_OVERLAY = "PopupOverlay";

    public const float MAP_CELL_SIZE = 0.42f;
    public const float DEFAULT_KNOCK_BACK_DURATION = 0.2f;
    public const float DEFAULT_KNOCK_BACK_RANGE_BY_CELLS = 0.2f;
    public const int TOWER_MAX_UPGRADE_LEVEL = 5;
    public const float EXP_PER_NORMAL_ENEMY = 10f;

    public const float CAMPAIGN_TIME_FINAL_WAVE_IN_SECONDS = 180;
    public const float GAME_SPEED_PAUSE = 0f;
    public const float GAME_SPEED_SLOW_MOTION = 0.05f;
    public const float GAME_SPEED_X1 = 1f;
#if UNITY_EDITOR
    public const float GAME_SPEED_X2 = 2f;
#else
    public const float GAME_SPEED_X2 = 1.5f;
#endif
    public const float GAME_SPEED_X3 = 2.5f;

    public const int TOWERS_IN_DECK = 5;
    public const int GEAR_IN_DECK = 4;
    public const int HERO_IN_DECK = 1;
    public const int MAX_WAREHOUSE_GEARS = 200;

    public static readonly DateTime defaultDate = new DateTime(2024, 1, 1);
    public static double defaultDateMiliseconds = defaultDate.ToMiliseconds();
}
