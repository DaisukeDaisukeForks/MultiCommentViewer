﻿using Newtonsoft.Json;

namespace NicoSitePlugin.Metadata
{
    class MessageServer : IMetaMessage
    {
        public string ViewUri { get; }
        public string VposBaseTime { get; }
        public string HashedUserId { get; }
        public string Raw { get; }
        private MessageServer(string raw, string viewUri, string vposBaseTime, string hashedUserId)
        {
            Raw = raw;
            ViewUri = viewUri;
            VposBaseTime = vposBaseTime;
            HashedUserId = hashedUserId;
        }

        public static MessageServer CreateMessage(string raw)
        {
            dynamic? d = JsonConvert.DeserializeObject(raw);
            var viewUri = (string)d.data.viewUri;
            var vposBaseTime = (string)d.data.vposBaseTime;
            var hashedUserId = (string)d.data.hashedUserId;
            return new MessageServer(raw, viewUri, vposBaseTime, hashedUserId);
        }
    }
    class Room : IMetaMessage
    {
        public static bool IsLoggedIn(Room room)
        {
            return room.YourPostKey != null;
        }
        ////{"type":"room","data":{"name":"アリーナ","messageServer":{"uri":"wss://msgd.live2.nicovideo.jp/websocket","type":"niwavided"},"threadId":"M.F6z6aDgDo9ZTeIT7TjyEfA","yourPostKey":"T.Gxu4P9bDRWJZsQAecKlQ5EWcA-lKxVSVPef1pW7MP1fGb3btJP6xNwf_","isFirst":true,"waybackkey":"1611205291.N9TT1vQOpe73RBtnEx8hyK1unEg"}}
        public Room(string json)
        {
            dynamic d = JsonConvert.DeserializeObject(json);
            Name = (string)d.data.name;
            MessageServerUrl = (string)d.data.messageServer.uri;
            ThreadId = (string)d.data.threadId;
            if (d.data.ContainsKey("yourPostKey"))
            {
                YourPostKey = d.data.yourPostKey;
            }
            else
            {
                YourPostKey = null;
            }
            Waybackkey = (string)d.data.waybackkey;
            Raw = json;
        }
        public string Name { get; }
        public string MessageServerUrl { get; }
        public string ThreadId { get; }
        public string YourPostKey { get; }
        public string Waybackkey { get; }
        public string Raw { get; }
    }
}