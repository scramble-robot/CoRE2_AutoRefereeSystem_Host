using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoRE2_AutoRefereeSystem_Host
{
    public class CoreClass
    {
        public String GameTime { get; set; } = "00:00";
        //public String SettingTime { get; set; } = "00:00";
        public int GameSystem { get; set; } = 0;    //0:なし，1:予選，2:準決，3:決勝
        //public int GameStatus { get; set; } = 0;    //0:なし，1:セッティング，2:試合前，3:試合中，4:試合終了
        public int RedDeathCnt { get; set; } = 0;   //赤が死んだ回数
        public int BlueDeathCnt { get; set; } = 0;  //青が死んだ回数
        public int RedReceivedDamage { get; set; } = 0;     //赤が受けたダメージ(青が与えたダメージ)
        public int BlueReceivedDamage { get; set; } = 0;    //青が受けたダメージ(赤が与えたダメージ)

        // スポット
        public int[] RedSpot { get; set; } = { 0, 0, 0 };    //[0],[2]:攻撃力バフ，[1]:回復
        public int[] BlueSpot { get; set; } = { 0, 0, 0 };

        //陣地
        public int RedArea { get; set; } = 5;      //0～4:赤，5：中立，6～10：青
        public int BlueArea { get; set; } = 5;
        public int CenterArea { get; set; } = 5;

        // 試合結果
        public uint RedWin { get; set; } = 0;
        public uint BlueWin { get; set; } = 0;
        public uint Winner { get; set; } = 0;  //1:赤, 2:青, 3:引き分け

        public RobotClass[] Robot { get; set; } = { new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass(),
                                                    new RobotClass() };
    }

    public class RobotClass
    {
        public int TeamID { get; set; } = 0;
        public String TeamColor { get; set; } = "";
        public int HP { get; set; } = 100;
        public int MaxHP { get; set; } = 100;
        public int DeathFlag { get; set; } = 0;     //0:Alive, 1:Dead(Respawn), 2:Dead
        public String RespawnTime { get; set; } = "00:00";


    }

    public class WindowClass
    {
        public string[] TeamName = {   "",
                                "[FRCI]FRENTE-Cielo",
                                "[FRRO]FRENTE-Rosa",
                                "[FRSE]FRENTE-Selva",
                                "[FRBR]Front Line Breakers",
                                "[GRGA]GIRASOLE-Gauss",
                                "[GRVO]GIRASOLE-Volta",
                                "[KMOK]KmoKHS-CoRE",
                                "[KTTM]KT-tokitama",
                                "[SHND]SCHWARZ-HANEDA",
                                "[VXGA]VERTEX-Gamma",
                                "[VXZE]VERTEX-Zeta",
                                "[HYLI]東山Lightning"};

        public string[] Status = { "", "Setting Time", "Stand-by", "Game", "Game Set" };
        // 各ロボットの情報
        public RobotClass[] Red { get; set; } = { new RobotClass(), new RobotClass(), new RobotClass(), new RobotClass(), new RobotClass(), new RobotClass() };
        public RobotClass[] Blue { get; set; } = { new RobotClass(), new RobotClass(), new RobotClass(), new RobotClass(), new RobotClass(), new RobotClass() };


        // タイマー
        public String GameTime { get; set; } = "00:00";
        //public String SettingTime { get; set; } = "00:00";
        public int GameSystem { get; set; } = 0;    //0：なし，1:予選，2：準決，3:決勝
        //public int GameStatus { get; set; } = 0;    //0：なし，1：セッティング，2：試合前，3：試合中，4：試合終了
        public int RedDeathCnt { get; set; } = 0;
        public int BlueDeathCnt { get; set; } = 0;
        public int RedReceivedDamage { get; set; } = 0;
        public int BlueReceivedDamage { get; set; } = 0;

        public uint ShowFlag { get; set; } = 0;     // 表示切り替えフラグ

        // 試合結果
        public uint RedWin { get; set; } = 0;
        public uint BlueWin { get; set; } = 0;
        public uint Winner { get; set; } = 0;  //1:赤, 2:青, 3:引き分け


        // スポット
        public int[] RedSpot { get; set; } = { 0, 0, 0 };    //[0],[2]:攻撃力バフ(バフ中のみ)，[1]:回復
        public int[] BlueSpot { get; set; } = { 0, 0, 0 };

        //陣地
        public int RedArea { get; set; } = 5;      //0～4:赤，5：中立，6～10：青
        public int BlueArea { get; set; } = 5;
        public int CenterArea { get; set; } = 5;

    }
    public class PlayerClass
    {
        //画面を操作するプレーヤー
        public bool PlayerTeamColor { get; set; } = false;  //True:Red False:Blue
        public uint PlayerNo { get; set; } = 0;
        public uint EnablePlayerGUI { get; set; } = 0;    // チーム情報の表示選択
        public uint EnableTeamGUI { get; set; } = 0;    // チーム情報の表示選択
        public uint SettingChengeFlag { get; set; } = 0;     // 表示切り替えフラグ
    }
}
