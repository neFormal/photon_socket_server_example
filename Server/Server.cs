using System;
using System.IO;
using Photon.SocketServer;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net.Config;

namespace Chat
{
	public class Server : ApplicationBase
	{
		protected static readonly ILogger log = LogManager.GetCurrentClassLogger ();

		public Server ()
		{
			log.Warn ("========== Ctor!!1");
		}

		protected override PeerBase CreatePeer(InitRequest initRequest)
		{
			Console.WriteLine ("peer console created");
			log.Info ("peer created");
			return new Peer (initRequest);
		}

		protected override void Setup()
		{
			log4net.GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(this.ApplicationRootPath, "log");
			var configFileInfo = new FileInfo(Path.Combine(this.BinaryPath, "log4net.config"));
			if (configFileInfo.Exists)
			{
				LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
				XmlConfigurator.ConfigureAndWatch(configFileInfo);
			}

//			if (log.IsDebugEnabled)
//			{
//				log.Debug ("test failed");
//			}

			log.Info ("========== Setup!!1");
		}

		protected override void TearDown()
		{
		}
	}
}
