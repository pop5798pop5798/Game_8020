using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyServerForLinux.Model
{
    /// <summary>
    /// 遊戲命令.
    /// </summary>
    public class GameCommand
    {
        public const string SetUserBlack = "SetUserBlack";          // 在線玩家新增黑名單
        public const string SwitchServerFunc = "SwitchServerFunc";          // 指定牌型
        public const string SystemCheck = "SystemCheck";                // 系統檢測.
        public const string KickUser = "KickUser";                      // 踢離玩家
        public const string KickAll = "KickAll";                	// 全踢-指定等級
        public const string MaintainSwitch = "MaintainSwitch";          // 維護開關
        public const string ServerGetPing = "ServerGetPing";            // Server要求連線品質檢查.
        public const string ClientGetPing = "ClientGetPing";            // Client要求連線品質檢查
        public const string CheckGameRun = "CheckGameRun";              // 偵測遊戲運行服務
        public const string Announcement = "Announcement";              // 公告通知公告

        public const string UserLogin = "LoginServer";                  // 用戶端登入. 
        public const string GetLobbyInfo = "GetLobbyInfo";              // 取得大廳資訊. 
        public const string GetLobbyToken = "GetLobbyToken";        // 取得大廳網址
        public const string WaitGameGroup = "WaitGameGroup";            // 玩家請求遊戲.(等侯湊桌)
        public const string CancelWait = "CancelWait";                  // 取消等侯湊桌.
        public const string ReturnLobby = "ReturnLobby";                // 返回大廳.
        public const string SystemDisconnect = "SystemDisconnect";      // 系統離線訊息.
        public const string SystemClose = "PrepareShutdown";                // 系統準備關閉
        public const string CheckUser = "CheckUser";            	// 檢測玩家狀態

        public const string GameAnnounce = "GameAnnounce";              // 遊戲公告
        public const string Game_JoinRoom = "GameJoinRoom";             // 有玩家進入(及入座)遊戲.
        public const string Game_LeaveRoom = "GameLeaveRoom";           // 玩家離開遊戲.
        public const string WriteMemberReport = "WriteMemberReport";    // 玩家回覆意見
        public const string GetHistoryRecord = "GetHistoryRecord";      // 玩家歷史紀錄.

        //=====---  牌肯  ---============
        public const string GameOver = "GameOver";                                  // 結束
        public const string TableInfo = "GameTableInfo";           // 復原桌資訊..
        public const string GamePlayCard = "GamePlayCard";   //玩家出牌
        public const string GameCardFlod = "GameCardFlod";                 // 定時出牌.
        public const string PaiKen_PK = "GamePlayPK";          // PK場景.

        public const string PaiKen_Banker = "GsBanker";          // Banker.

        public const string PaiKen_GS0 = "GameGs0";           // 準備場景.
        public const string PaiKen_GS01 = "GameGs1";          // 開局場景.
        public const string PaiKen_GS02 = "GameGs2";          // 發牌場景.
        public const string PaiKen_GS03 = "GameGs3";          // 玩家思考場景.
        public const string PaiKen_GS04 = "GameGs4";          // Delay場景.
        public const string PaiKen_GS05 = "GameGs5";          // 結算場景.
        public const string PaiKen_GS06 = "GameGs6";          // 該局結束場景.

        public const string SetAct = "SetAct";                      // 設定活動開關
        public const string SetBank = "SetBank";                    // 活動重置Bank
        public const string SetBonus = "SetBonus";                  // 指定中獎
    }

}
