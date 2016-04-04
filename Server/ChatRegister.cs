using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Logging;

namespace Chat
{
	public static class ChatRegister
	{
		private static readonly ILogger log = LogManager.GetCurrentClassLogger ();

		private const int limit = 2;
		private static Dictionary<string, int> channels = new Dictionary<string, int> ();
		private static object accessLock = new object();

		public static string JoinChat ()
		{
			lock (accessLock)
			{
				string name = null;
				foreach (var pair in channels)
				{
					if (pair.Value < limit)
					{
						name = pair.Key;
						break;
					}
				}

				if (name == null)
				{
					name = "channel_" + DateTime.UtcNow.Ticks;
					log.Info ("channel: " + name);
					channels.Add (name, 1);
				}
				else
				{
					channels[name]++;
				}
				log.Info ("join channels: " + channels.Count);
				return name;
			}
		}

		public static void LeaveChat (string chatName)
		{
			lock (accessLock)
			{
				if (!channels.ContainsKey (chatName))
					return;

				if (channels [chatName] <= 1)
					channels.Remove (chatName);
				else
					channels[chatName]--;

				log.Info ("leave channels: " + channels.Count);
			}
		}
	}
}
