using System;

namespace Chat
{
	using Photon.SocketServer;
	using Photon.SocketServer.Rpc;

	public enum OpCode {
		login = 1
	}

	public class LoginOperation : Operation
	{
		public LoginOperation (IRpcProtocol protocol, OperationRequest request) : base(protocol, request)
		{
		}
		public LoginOperation ()
		{
		}

		[DataMember(Code = 1, IsOptional = false)]
		public int Id { get; set; }
	}
}

