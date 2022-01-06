using MhCore.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyServerForLinux.Model
{
    /// <summary>
    /// 牌肯遊戲.
    /// </summary>
    public abstract class FightGame : GameRoom
    {
        public FightGame(int no, String name) : base(no, name) { }

        /// <summary>
        /// 是否遊戲中.
        /// </summary>
        /// <returns></returns>
        public abstract Boolean IsPlaying();


        /// <summary>
        /// 湊桌入房.
        /// </summary>
        /// <param name="players">玩家列表</param>
        public abstract void DeskmateToRoom(Player[] players);
    }
}
