using System;

namespace Chat
{
	public class WorkersPool
	{
		private const int workersCount = 10;

		public WorkersPool ()
		{
		}

		public void ThreadPoolCallback(Object threadContext)
		{
			int threadIndex = (int)threadContext;
			Console.WriteLine("thread {0} started: ", threadIndex);
			Process (threadIndex);
			Console.WriteLine("thread {0} stopped: ", threadIndex);
		}

		private void Process(int id)
		{
			ClientWorker[] workers = new ClientWorker[workersCount];
			for (int i = 0; i < workersCount; i++)
			{
				workers [i] = new ClientWorker ();
			}

			while (true)
			{
				foreach (var w in workers)
				{
					foreach (var x in w.Process(id)) {
					}
				}
			}
		}
	}
}
