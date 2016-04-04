using System;
using System.Threading;
using System.Collections.Generic;

namespace Chat
{
	class Client
	{
		public static void Main (string[] args)
		{
			const int threadsCount = 2;
			WorkersPool[] pools = new WorkersPool[threadsCount];
			ManualResetEvent[] done = new ManualResetEvent[threadsCount];

			for (int i = 0; i < threadsCount; i++)
			{
				done [i] = new ManualResetEvent (false);
				var pool = new WorkersPool ();
				ThreadPool.QueueUserWorkItem(pool.ThreadPoolCallback, i);
			}

			WaitHandle.WaitAll (done);
			Console.WriteLine ("process complete");
		}
	}
}
