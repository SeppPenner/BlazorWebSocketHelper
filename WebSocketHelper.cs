﻿using BlazorWebSocketHelper.Classes;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorWebSocketHelper.Classes.BwsEnums;

namespace BlazorWebSocketHelper
{
    public class WebSocketHelper : IDisposable
    {

        public BwsState bwsState = BwsState.Undefined;


        public BwsTransportType bwsTransportType { get; private set; } = BwsTransportType.String;

        public bool IsDisposed = false;

        public Action<short> OnStateChange { get; set; }

        public Action<object> OnMessage { get; set; }

        public Action<string> OnError { get; set; }


        public List<BwsMessage> Log = new List<BwsMessage>();
        public bool DoLog { get; set; } = true;
        public int LogMaxCount { get; set; } = 100;

        private string _id = BwsFunctions.Cmd_Get_UniqueID();

        private string _url = string.Empty;

        public List<BwsError> BwsError = new List<BwsError>();

        private byte[] buffer;

        public async Task<string> get_WsStatus()
        {
            
            short a = await BwsJsInterop.WsGetStatus(_id);

            return BwsFunctions.ConvertStatus(a).ToString();
            
        }

        public WebSocketHelper(string Par_URL, BwsTransportType Par_TransportType)
        {
            _initialize(Par_URL,Par_TransportType);
        }

        public void Connect(string Par_URL, BwsTransportType Par_TransportType)
        {
            _initialize(Par_URL, Par_TransportType);
        }



        private void _initialize(string Par_URL, BwsTransportType Par_TransportType)
        {
            if (!string.IsNullOrEmpty(Par_URL))
            {
                StaticClass.webSocketHelper = this;
                _url = Par_URL;
                bwsTransportType = Par_TransportType;
                _connect();
            }
            else
            {
                BwsError.Add(new BwsError { Message = "Url is not provided!", Description = string.Empty });
            }
        }

        private void _connect()
        {
            BwsJsInterop.WsAdd(_id, _url, bwsTransportType.ToString(), new DotNetObjectRef(this));
        }


        private int GetNewIDFromLog()
        {

            if (Log.Any())
            {
                return Log.Max(x => x.ID) + 1;
            }
            else
            {
                return 1;
            }
        }

        public void send(string Par_Message)
        {
            if (!string.IsNullOrEmpty(Par_Message))
            {


                BwsJsInterop.WsSend(_id, Par_Message);


                if (DoLog)
                {
                    
                    Log.Add(new BwsMessage { ID = GetNewIDFromLog(), Date = DateTime.Now, Message = Par_Message, MessageType = BwsMessageType.send});
                    if (Log.Count > LogMaxCount)
                    {
                        Log.RemoveAt(0);
                    }
                }
              
            }
        }


        public void send(byte[] Par_Message)
        {
            if (Par_Message.Length>0)
            {


                BwsJsInterop.WsSend(_id, Par_Message);


                if (DoLog)
                {

                    Log.Add(new BwsMessage { ID = GetNewIDFromLog(), Date = DateTime.Now, MessageBinary = Par_Message, MessageType = BwsMessageType.send });
                    if (Log.Count > LogMaxCount)
                    {
                        Log.RemoveAt(0);
                    }
                }

            }
        }

        [JSInvokable]
        public void InvokeStateChanged(short par_state)
        {
            bwsState = BwsFunctions.ConvertStatus(par_state);
            OnStateChange?.Invoke(par_state);
        }


        [JSInvokable]
        public void InvokeOnError(string par_error)
        {
            OnError?.Invoke(par_error);
        }


        [JSInvokable]
        public void InvokeOnMessage(string par_message)
        {

            if (DoLog)
            {
                
                Log.Add(new BwsMessage { ID = GetNewIDFromLog(), Date = DateTime.Now, Message = par_message, MessageType = BwsMessageType.received });
                
                if (Log.Count > LogMaxCount)
                {
                    Log.RemoveAt(0);
                }
            }

            
            OnMessage?.Invoke(par_message);
        }


        public void InvokeOnMessageBinary(byte[] par_message)
        {

            if (DoLog)
            {

                Log.Add(new BwsMessage { ID = GetNewIDFromLog(), Date = DateTime.Now, MessageBinary = par_message, MessageType = BwsMessageType.received });

                if (Log.Count > LogMaxCount)
                {
                    Log.RemoveAt(0);
                }
            }


            OnMessage?.Invoke(par_message);
        }


        public void Close()
        {
            if (DoLog)
            {
                Log = new List<BwsMessage>();
            }
            BwsJsInterop.WsClose(_id);
        }

        public void Dispose()
        {
            if (DoLog)
            {
                Log = new List<BwsMessage>();
            }
            BwsJsInterop.WsRemove(_id);
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }




    }
}
