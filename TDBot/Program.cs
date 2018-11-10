using System;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using TdLib;

namespace TDBot
{
	class Program
	{
		private static Agent _agent = new Agent();
		private static FileLoader _fileLoader = new FileLoader(_agent);
		private static AuthManager _authManager = new AuthManager(_agent);
		private static BotLogic _botLogic = new BotLogic(_agent, _fileLoader);

		static void Main(string[] args)
		{
			SetupAuth();
			SetupBot();

			_agent.Start();
		}

		private static void SetupAuth()
		{
			var authStateUpdates = _agent.Updates
				.OfType<TdApi.Update.UpdateAuthorizationState>()
				.Select(update => update.AuthorizationState);

			authStateUpdates
				.OfType<TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters>()
				.SelectMany(_authManager.SetupParameters)
				.Subscribe(OnNext, OnError);

			authStateUpdates
				.OfType<TdApi.AuthorizationState.AuthorizationStateWaitEncryptionKey>()
				.SelectMany(_authManager.CheckEncryptionKey)
				.Subscribe(OnNext, OnError);

			authStateUpdates
				.OfType<TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber>()
				.SelectMany(_authManager.CheckBotToken)
				.Subscribe(OnNext, OnError);
		}

		private static void SetupBot()
		{
			var messageUpdates = _agent.Updates
				.ObserveOn(TaskPoolScheduler.Default)
				.OfType<TdApi.Update.UpdateNewMessage>();

			messageUpdates
				.SelectMany(_botLogic.HandleMessage)
				.Subscribe(OnNext, OnError);
		}

		static void OnError(Exception e)
		{
			Console.Error.WriteLine(e);
		}

		static void OnNext(TdApi.Ok ok)
		{
		}

		static void OnNext(Unit unit)
		{
		}
	}
}
