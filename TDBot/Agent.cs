using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using TdLib;

namespace TDBot
{
	public class Agent
	{
		private readonly Client _client;
		private readonly Hub _hub;
		private readonly Dialer _dialer;

		public Agent()
		{
			Client.Log.SetVerbosityLevel(0);
			_client = new Client();
			_hub = new Hub(_client);
			_dialer = new Dialer(_client, _hub);
		}

		public void Start()
		{
			_hub.Start();
		}

		public void Stop()
		{
			_hub.Stop();
		}

		public virtual IObservable<TdApi.Update> Updates
		{
			get
			{
				return Observable.FromEventPattern<TdApi.Object>(h => _hub.Received += h, h => _hub.Received -= h)
					.Select(a => a.EventArgs)
					.OfType<TdApi.Update>();
			}
		}

		public virtual IObservable<T> Execute<T>(TdApi.Function<T> function)
			where T : TdApi.Object
		{
			return _dialer.ExecuteAsync(function).ToObservable();
		}
	}
}
