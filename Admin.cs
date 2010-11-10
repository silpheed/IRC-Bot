using System;
using System.Web.Security;
using Meebey.SmartIrc4net;

namespace Bot
{
	public class Admin : IBottable
	{
		private IrcClient _irc;
		//Change these to meet your own requirements.
		private string _server = "irc.server.com";
		private int _port = 6667;
		internal static string _channel = "#channel";
		private string _botname = "Mr_Bot";
		private string _botemail = "Mr@Bot.com";
		//Admin user's nick + password, SHA1 hashed. See IsAuth() to make your own.
		private string _saltedhash = "HASH_HERE";
	
		private bool _autojoin = false;

		public Admin(IrcClient irc)
		{
			_irc = irc;
			_irc.OnConnected += OnConnected;
		}

        public bool OnReadLine(IrcMessageData data)
		{
			if ((data.Type != ReceiveType.QueryMessage) || (!IsAuth(data.Nick, data.MessageArray[0])))
				return true;
            
			if (data.MessageArray[1].ToLower() == "join") {
				if (data.MessageArray.Length == 2)
					Join(_channel);
				else
					Join(data.MessageArray[2]);
			}

			if (data.MessageArray[1].ToLower() == "part") {
				if (data.MessageArray.Length == 1)
					Part(_channel);
				else
					Part(data.MessageArray[2]);
			}

			if ((data.MessageArray[1].ToLower() == "quit") || (data.MessageArray[1].ToLower() == "exit"))
				Quit();

			return false;
		}

		private bool IsAuth(string name, string password)
		{
			return FormsAuthentication.HashPasswordForStoringInConfigFile(name + password, "SHA1") == _saltedhash;
		}

		public void OnConsoleLine(string[] args)
		{
			if ((null == args) || (args.Length == 0)) {
				_autojoin = true;
				Connect(_server, _port);
			}
			else if (args.Length == 1)
				Connect(args[0], _port);
			else
				Connect(args[0], Convert.ToInt32(args[1]));
		}

		private void Connect(string server, int port)
		{
			_irc.Connect(server, port);
		}

		private void OnConnected(object sender, EventArgs e)
		{
			_irc.Login(_botname, _botname, 0, _botemail, String.Empty);
			if (_autojoin)
				Join(_channel);
			_autojoin = false;

			_irc.Listen();
		}

		private void Join(string channel)
		{
			_irc.RfcJoin(channel ?? _channel);
		}

		private void Part(string channel)
		{
			_irc.RfcPart(channel ?? _channel, "outta here");
		}

		private void Quit()
		{
			_irc.RfcQuit("outta here");
		}
	}
}
