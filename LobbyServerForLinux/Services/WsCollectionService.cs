using LobbyServerForLinux.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyServerForLinux.Services
{
    /// <summary> WebSocket連線集合服務 </summary>
    public class WsCollectionService
    {
        private static List<WebsocketClient> _clients = new List<WebsocketClient>();

        /// <summary> 加入scoket </summary>
        /// <param name="client"> 用戶sokcet </param>
        public static void Add(WebsocketClient client)
        {
            _clients.Add(client);
        }
        /// <summary> 移除scoket </summary>
        /// <param name="client"> 用戶sokcet </param>
        public static void Remove(WebsocketClient client)
        {
            _clients.Remove(client);
        }

        /// <summary> 取得全部scoket用戶 </summary>
        /// <param name="clientId"> 用戶sokcet ID </param>
        public static List<WebsocketClient> GetAll()
        {
            List<WebsocketClient> client = _clients.ToList();

            return client;
        }
        /// <summary> 取得scoket用戶 </summary>
        /// <param name="clientId"> 用戶sokcet ID </param>
        public static WebsocketClient Get(string clientId)
        {
            var client = _clients.FirstOrDefault(c => c.Id == clientId);

            return client;
        }

    }
}
