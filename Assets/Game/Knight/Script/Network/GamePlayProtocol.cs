﻿//======================================================================
//        Copyright (C) 2015-2020 Winddy He. All rights reserved
//        Email: hgplan@126.com
//======================================================================
using UnityEngine;
using System.Collections.Generic;
using Core;
using Core.WindJson;
using Framework;

namespace Game.Knight
{
    /// <summary>
    /// 游戏的协议接收发送处理
    /// </summary>
    public class GamePlayProtocol : TSingleton<GamePlayProtocol>
    {
        private GamePlayProtocol() { }
        
        /// <summary>
        /// 查询Gate进行登陆，返回Connector的服务器
        /// </summary>
        public void DoClientQueryGateEntryRequest(string rAccount)
        {
            JsonNode rGateMsg = new JsonClass();
            rGateMsg.Add("userName", new JsonData(rAccount));
            NetworkClient.Instance.ClientRequest("gate.gateHandler.queryEntry", rGateMsg, OnClientQueryGateEntryResponse);
        }

        /// <summary>
        /// Gate登陆响应
        /// </summary>
        private void OnClientQueryGateEntryResponse(JsonNode rRequestMsg, JsonNode rProtocolMsg)
        {
            var rMsgCode = NetworkClient.Instance.GetMessageCode(rProtocolMsg);
            string rURL = rProtocolMsg["host"] != null ? rProtocolMsg["host"].AsString : "";
            int rPort = rProtocolMsg["port"] != null ? rProtocolMsg["port"].AsInt : 0;
            Login.Instance.LoginConnectorRequest(rMsgCode, rURL, rPort);
        }

        /// <summary>
        /// 角色登录请求
        /// </summary>
        public void DoClientLoginRequest(string rToken)
        {
            JsonNode rLoginMsg = new JsonClass();
            rLoginMsg.Add("token", new JsonData(rToken));
            Debug.Log(rLoginMsg.ToString());
            NetworkClient.Instance.ClientRequest("connector.entryHandler.ClientLoginRequest", rLoginMsg, OnClientLoginResponse);
        }

        /// <summary>
        /// 角色登录的响应
        /// </summary>
        private void OnClientLoginResponse(JsonNode rRequestMsg, JsonNode rProtocolMsg)
        {
            var rMsgCode = NetworkClient.Instance.GetMessageCode(rProtocolMsg);
            JsonNode rActorsNode = rProtocolMsg["actors"];
            List<NetActor> rNetActors = new List<NetActor>();
            if (rActorsNode != null)
            {
                for (int i = 0; i < rActorsNode.Count; i++)
                {
                    JsonNode rActorNode = rActorsNode[i];
                    NetActor rActor = new NetActor() {
                        ActorID = rActorNode["actorID"].AsLong,
                        ActorName = rActorNode["actorName"].AsString,
                        Level = rActorNode["level"].AsInt,
                        ServerID = rActorNode["serverID"].AsInt
                    };
                    rActor.Professional = GameConfig.Instance.GetActorProfessional(rActorNode["professionalID"].AsInt);
                    rNetActors.Add(rActor);
                }
            }
            long rAccountID = rProtocolMsg["uid"].AsLong;
            Login.Instance.OnClientLoginResponse(rMsgCode, rAccountID, rNetActors);
        }

        /// <summary>
        /// 创建角色的请求
        /// </summary>
        public void DoClientCreatePlayerRequest(long rAccountID, string rActorName, int rProfessionalID, int rServerID)
        {
            JsonNode rMsg = new JsonClass();
            rMsg.Add("accountID", new JsonData(rAccountID));
            rMsg.Add("playerName", new JsonData(rActorName));
            rMsg.Add("professionalID", new JsonData(rProfessionalID));
            rMsg.Add("serverID", new JsonData(rServerID));
            NetworkClient.Instance.ClientRequest("connector.entryHandler.ClientCreatePlayerRequest", rMsg, OnPlayerCreateResponse);
        }

        /// <summary>
        /// 创建角色的响应
        /// </summary>
        private void OnPlayerCreateResponse(JsonNode rRequestMsg, JsonNode rProtocolMsg)
        {
            var rMsgCode = NetworkClient.Instance.GetMessageCode(rProtocolMsg);
            CreatePlayer.Instance.OnPlayerCreateResponse(rMsgCode, rRequestMsg["playerName"].AsString, 
                                                         rRequestMsg["professionalID"].AsInt, rProtocolMsg["actorID"].AsLong);
        }
    }
}