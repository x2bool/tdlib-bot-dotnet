using System;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Configuration;
using TdLib;

namespace TDBot
{
	class Program
	{
		private static IConfigurationRoot _config;
		private static Agent _agent;
		private static FileLoader _fileLoader;
		private static AuthManager _authManager;
		private static BotLogic _botLogic;

		static void Main(string[] args)
		{
			_config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();

			_agent = new Agent();
			_fileLoader = new FileLoader(_agent);
			_authManager = new AuthManager(_agent, _config);
			_botLogic = new BotLogic(_agent, _fileLoader);

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
