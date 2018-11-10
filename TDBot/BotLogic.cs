using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using TdLib;

namespace TDBot
{
	public class BotLogic
	{
		private readonly Agent _agent;
		private readonly FileLoader _fileLoader;

		public BotLogic(Agent agent, FileLoader fileLoader)
		{
			_agent = agent;
			_fileLoader = fileLoader;
		}

		public IObservable<Unit> HandleMessage(
			TdApi.Update.UpdateNewMessage update)
		{
			Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

			var message = update.Message;

			return _agent.Execute(new TdApi.GetMe())
				.SelectMany(me =>
				{
					if (message.SenderUserId != me.Id)
					{
						switch (message.Content)
						{
							case TdApi.MessageContent.MessageText textContent:
								return ReplyToTextMessage(message, textContent);

							case TdApi.MessageContent.MessageDocument documentContent:
								return ReplyToDocumentMessage(message, documentContent);
						}
					}
					
					return Observable.Empty<TdApi.InputMessageContent>();
				})
				.Select(replyContent => _agent.Execute(new TdApi.SendMessage
				{
					ChatId = message.ChatId,
					ReplyToMessageId = message.Id,
					InputMessageContent = replyContent
				}))
				.Select(_ => Unit.Default);
		}

		private IObservable<TdApi.InputMessageContent> ReplyToTextMessage(
			TdApi.Message message,
			TdApi.MessageContent.MessageText messageContent)
		{
			var content = messageContent.Text.Text;

			var reply = new TdApi.InputMessageContent.InputMessageText
			{
				Text = new TdApi.FormattedText
				{
					Text = $"You said: {content}"
				}
			};

			return Observable.Return(reply);
		}

		private IObservable<TdApi.InputMessageContent> ReplyToDocumentMessage(
			TdApi.Message message,
			TdApi.MessageContent.MessageDocument messageContent)
		{
			var file = messageContent.Document.Document_;
			
			return _fileLoader.LoadFile(file)
				.FirstAsync(f => f.Local != null && f.Local.IsDownloadingCompleted)
				.SelectMany(f =>
				{
					var localFile = f.Local?.Path;

					if (localFile != null && File.Exists(localFile))
					{
						return File.ReadAllTextAsync(localFile)
							.ToObservable()
							.Select(content => new TdApi.InputMessageContent.InputMessageText
							{
								Text = new TdApi.FormattedText
								{
									Text = $"You said: {content}"
								}
							});
					}
					
					return Observable.Empty<TdApi.InputMessageContent>();
				});

		}
	}
}
