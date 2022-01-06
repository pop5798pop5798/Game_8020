using System.Collections.Generic;

namespace LobbyServerForLinux.Model
{
    //牌肯 場景索引.
    public struct PaiKenScene
    {
        public const int Wait = -1;             // 空房.

        public const int Ready = 0;             // 準備遊戲.
        public const int StartRound = 1;        // 新局開始.
        public const int Deal = 2;              // 發牌. 
        public const int Banker = 3;              // 發牌. 

        public const int Think = 4;             // 玩家思考.
        public const int PK = 5;                // 玩家思考.

        public const int Delay = 6;             //丟牌後時間延遲
        public const int Result = 7;            // 結算.       

        public const int EndRound = 8;          // 該局結束.
        public const int ForceKickUser = 9;     // 強制踢人
    }

    // 牌肯 場景運作時間.
    public struct PaiKenSceneTime
    {
        public const int Wait = 0;             
        public const int Ready = 2; 

        public const int StartRound = 1;
        public const double Deal = 3;
        public const double Banker = 0.1;
        public const double Think = 4;
        public const double PK = 1;
        public const double OverThink = 3.3;
        public const int Fight = 3;
        public const int Result = 2;

        public const int EndRound = 1;
        public const int KickRound = 3;

        public const double PlayCard = 1; //出牌
        public const double Draw = 1.7; //抽牌
    }

    // 座位狀態.
    public struct SeatStatus
    {
        public const int None = 0;      // 無.(未遊戲)
        public const int Lose = 1;      // 輸家.
        public const int Winner = 3;    // 最後贏家.
        public const int Tie = 4;      // 平局.
        public const int Play = 5;      // 遊戲中.
    }
  

    //玩家動作
    public enum ActionType
    {
        Draw,    //補牌
        Follow,  //跟牌
        Throw,   //丟牌        
        PK,     //比牌
        Error,  //失敗
    }

    public enum Result
    {
        Success, //成功
        Error,//失敗        
    }

    public struct OverType
    {
        public const int DeckOver = 1;      // 牌堆 0 over
        public const int HCardOver = 2;      // 手牌 0 over
        public const int SupOver = 3;       //特牌 over.     
        public const int PKOver = 4;       //一般比牌 over.  
    }

    public struct ReturnLobbyType 
    {
        public const int Error = -1;      // 失敗
        public const int Lobby = 1;      // 可返Lobby.
        public const int Over = 2;    //結束.

    }

    public struct PokerType
    {
        public const int Three = 1;      // 三條
        public const int Straight = 2;      // 順子.
        public const int Flush = 3;    // 同花.
        public const int Full = 4;      // 葫蘆.
        public const int Four = 5;      // 四條.
        public const int StraightF = 6;      // 同花順.
        public const int Royal = 7;      // 皇家同花順.

    }

}
