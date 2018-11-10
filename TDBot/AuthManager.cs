using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TdLib;

namespace TDBot
{
	public class AuthManager
	{
		private readonly int _apiId;
		private readonly string _apiHash;
		private readonly string _botToken;

		private readonly Agent _agent;

		public AuthManager(Agent agent)
		{
			_agent = agent;
		}

		public IObservable<TdApi.Ok> SetupParameters(
			TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters state)
		{
			var filesDir = new DirectoryInfo(".").FullName;

			return _agent.Execute(new TdApi.SetTdlibParameters
			{
				Parameters = new TdApi.TdlibParameters
				{
					ApiId = _apiId,
					ApiHash = _apiHash,
					DatabaseDirectory = filesDir,
					FilesDirectory = filesDir,
					UseFileDatabase = true,
					UseChatInfoDatabase = true,
					UseMessageDatabase = true,
					EnableStorageOptimizer = true,
					SystemLanguageCode = "en",
					DeviceModel = "Mac",
					SystemVersion = "0.1",
					ApplicationVersion = "0.1"
				}
			});
		}

		public IObservable<TdApi.Ok> CheckEncryptionKey(
			TdApi.AuthorizationState.AuthorizationStateWaitEncryptionKey state)
		{
			return _agent.Execute(new TdApi.CheckDatabaseEncryptionKey());
		}

		public IObservable<TdApi.Ok> CheckBotToken(
			TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber state)
		{
			return _agent.Execute(new TdApi.CheckAuthenticationBotToken
			{
				Token = _botToken
			});
		}
	}
}
