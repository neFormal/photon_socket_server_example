using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Chat;
using System.Linq;
using Photon.SocketServer.Security;
using System.Security.Cryptography;

namespace Chat
{
	public class ClientWorker : IPhotonPeerListener, IChatClientListener
	{
		private static readonly Random random = new Random ();

		private enum State
		{
			connect,
			auth,
			join_chat,
			chatting
		}

		private string username = "";
		private string token = "";

		private State state = State.connect;
		private PhotonPeer peer;
		private ChatClient chatClient;
		private string chatChannel;

		private const double connectDeltaSeconds = 2;
		private DateTime lastLoginTry = DateTime.Now;

		private const double chatSendDeltaSeconds = 1;
		private DateTime lastChatSend = DateTime.Now;

		// TODO: read from config
		private const string appId = "f0e057cd-4f5b-4229-8ff9-e716863121f3";
		private const string appVersion = "1.0";
		private const int historyLength = 10;
		private readonly string[] text = new string[]{ "hello", "my name is boris", "i eat an egg for breakfast", "the London is a capital of Great Britain"};
		private int currentText = 0;

		public ClientWorker ()
		{
		}

		public IEnumerable<int> Process (int id)
		{
			switch (state)
			{
			case State.connect:
				if (peer == null)
				{
					Console.WriteLine ("try to connect");
					peer = new PhotonPeer (this, ConnectionProtocol.Tcp);
					peer.Connect ("localhost:4000", "ChatServer");
				}
				break;
			case State.auth:
				var now = DateTime.Now;
				if (now > lastLoginTry)
				{
					lastLoginTry = now.AddSeconds (connectDeltaSeconds);
					Console.WriteLine ("try to auth: {0} - {1}", now, now.AddSeconds (connectDeltaSeconds));
					var parameters = new Dictionary<byte, object> { { 1, random.Next (Int32.MaxValue) } };
					//var login = new LoginOperation{ Id = random.Next (Int32.MaxValue)};
					peer.OpCustom ((byte)OpCode.login, parameters, true);
				}
				break;
			case State.join_chat:
				if (chatClient == null) {
					chatClient = new ChatClient (this);
					chatClient.CustomAuthenticationValues = new AuthenticationValues ();
					chatClient.CustomAuthenticationValues.AuthType = CustomAuthenticationType.Custom;
					chatClient.CustomAuthenticationValues.AuthParameters = "token=" + token;

					bool connected = chatClient.Connect (appId, appVersion, username, chatClient.CustomAuthenticationValues);
					if (!connected) {
						Console.WriteLine ("can not connect to chat");
						yield break;
						//return;
					}
				} else {
					chatClient.Service ();
				}
				break;
			case State.chatting:
				if (DateTime.Now > lastChatSend)
				{
					lastChatSend = DateTime.Now.AddSeconds (chatSendDeltaSeconds);

					string msg = text [currentText];
					currentText++;
					if (currentText >= text.Length)
						currentText = 0;

					chatClient.PublishMessage (this.chatChannel, msg);
				}
				chatClient.Service ();
				break;
			}

			peer.Service ();
			yield return 0;
		}

		#region IPhotonPeerListener implementation

		public void OnEvent (EventData eventData)
		{
			throw new NotImplementedException ();
		}

		public void OnMessage (object messages)
		{
			throw new NotImplementedException ();
		}

		public void DebugReturn (DebugLevel level, string message)
		{
			Console.WriteLine (level + ":" + message);
		}

		public void OnOperationResponse (OperationResponse operationResponse)
		{
			switch ((OpCode)operationResponse.OperationCode)
			{
			case OpCode.login:
				username = operationResponse.Parameters [1] as string;
				token = operationResponse.Parameters [2] as string;
				chatChannel = operationResponse.Parameters [3] as string;
				Console.WriteLine ("chat: {0}, {1}, {2}", username, token, chatChannel);

				state = State.join_chat;
				break;
			default:
				Console.WriteLine ("unsupported operation response: " + operationResponse.ToStringFull ());
				break;
			}
		}

		public void OnStatusChanged (StatusCode statusCode)
		{
			if (statusCode == StatusCode.Connect)
			{
				state = State.auth;
			}
			else
			{
				Console.WriteLine ("unexpected status change: " + statusCode);
			}
		}

		#endregion

		#region IChatClientListener implementation

		public void OnConnected ()
		{
			Console.WriteLine ("connected");
			this.chatClient.Subscribe (new string[] {chatChannel}, historyLength);
		}

		public void OnDisconnected ()
		{
			Console.WriteLine ("disconnected");
		}

		public void OnChatStateChange (ChatState state)
		{
			Console.WriteLine ("chat state changed: " + state);
		}

		public void OnGetMessages (string channelName, string[] senders, object[] messages)
		{
			var sendersToMessages = senders.Zip (messages, (s, m) => new {sender = s, message = m}).ToList ();
//			Console.WriteLine ("got msgs from {0}", channelName);
			foreach (var msg in sendersToMessages)
			{
				Console.WriteLine ("{0}: {1}", msg.sender, msg.message);
			}

			foreach (var channel in chatClient.PublicChannels.Values)
			{
				channel.ClearMessages ();
			}
		}

		public void OnPrivateMessage (string sender, object message, string channelName)
		{
			throw new NotImplementedException ();
		}

		public void OnSubscribed (string[] channels, bool[] results)
		{
			Console.WriteLine ("subscribed to: {0}", channels.Zip (results, (c, r) => new {channel=c, subscribed=r} ).ToArray() );
			state = State.chatting;
		}

		public void OnUnsubscribed (string[] channels)
		{
			throw new NotImplementedException ();
		}

		public void OnStatusUpdate (string user, int status, bool gotMessage, object message)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
