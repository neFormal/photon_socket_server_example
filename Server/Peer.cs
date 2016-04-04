using System;
using System.Collections.Generic;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using ExitGames.Logging;
using System.Security.Cryptography;
using Photon.SocketServer.Security;

namespace Chat
{
	public class Peer : ClientPeer
	{
		protected static readonly ILogger log = LogManager.GetCurrentClassLogger ();
		private static readonly Random random = new Random ();

		private string chatName = null;

		public Peer (InitRequest initRequest)
			: base(initRequest)
		{
			log.Info ("new peer");
		}

		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
		{
			if (chatName != null)
				ChatRegister.LeaveChat (chatName);
		}

		protected override void OnOperationRequest(OperationRequest request, SendParameters parameters)
		{
			log.Info ("on operation");

			switch ((OpCode)request.OperationCode)
			{
			case OpCode.login:
				var operation = new LoginOperation (this.Protocol, request);
				if (!operation.IsValid) {
					var response = new OperationResponse {
						OperationCode = request.OperationCode,
						ReturnCode = 1,
						DebugMessage = "invalid operation",
					};
					this.SendOperationResponse (response, parameters);
					return;
				}

				chatName = ChatRegister.JoinChat ();
				log.InfoFormat ("login: {0}, {1}%n", operation.Id, chatName);

				string name = "user" + operation.Id + "_" + random.Next (Int32.MaxValue);
				string token = GetToken (name);
				var opResponse = new OperationResponse {
					OperationCode = request.OperationCode,
					ReturnCode = 0,
					Parameters = new Dictionary<byte, object> { {1, name}, {2, token}, {3, chatName} }
				};
				this.SendOperationResponse (opResponse, parameters);
				break;
			default:
				break;
			}
		}

		private static string GetToken (string name)
		{
			int timestamp = (int)(DateTime.UtcNow.AddSeconds (3) - new DateTime(1970, 1, 1)).TotalSeconds;
			string token = name + "_token_" + timestamp;

			var sharedKey = System.Text.Encoding.Default.GetBytes("cookie");
			byte[] shaHash;
			using (MD5 hashProvider = MD5.Create())
			{
				shaHash = hashProvider.ComputeHash(sharedKey);
			}
			var provider = new RijndaelCryptoProvider(shaHash, PaddingMode.Zeros);
			byte[] data = System.Text.Encoding.Default.GetBytes(token);

			string result = Convert.ToBase64String(provider.Encrypt (data));
			return result;
		}
	}
}
