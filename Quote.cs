using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Meebey.SmartIrc4net;

namespace Bot
{
	public class Quote : IBottable
	{
		private IrcClient _irc;
		private QuoteStore _store;
		private string _storeFileName = "banusquote.xml";
		private IList<QuoteData> _quotes;
		private Random _rng;
		private bool _multilineOut = true;
		private static int _maxMultilineOut = 10;
		private int _deleteRequestPeriodInSecs = 10;
		private IDictionary<int, DeleteRequest> _deleteRequests = new Dictionary<int, DeleteRequest>();
		private IList<int> _last20QuoteIds;

		//todo
		//1. search by index - DONE
		//2. add - DONE
		//3. random - DONE
		//4. multiline display - DONE
		//5. search by text - DONE
		//6. delete - DONE
		//7. help - DONE
		//8. popularity - DONE
		//9. multiline submission
		//10. browse by text (random quote with search term in it) - DONE
		//11. multiple person deletion - DONE

		public Quote(IrcClient irc)
		{
			_irc = irc;
			_store = new QuoteStore(_storeFileName);
			_quotes = _store.Load();
			_rng = new Random((int)DateTime.Now.Ticks);
			_last20QuoteIds = new List<int>();
		}

		public bool OnReadLine(IrcMessageData data)
		{
			if (data.Type != ReceiveType.ChannelMessage)
				return true;

			if (data.MessageArray[0] == "!add")
				AddQuote(data);
				
			if ((data.MessageArray[0] == "!delete") || (data.MessageArray[0] == "!del"))
				DeleteQuote(data);
            
			if ((data.MessageArray[0] == "!quote") || (data.MessageArray[0] == "!q"))
				DisplayQuote(data);

			if ((data.MessageArray[0] == "!popularity") || (data.MessageArray[0] == "!popular"))
				DisplayPopularity(data);

			if (data.MessageArray[0] == "!search")
				SearchQuote(data, false);

			if (data.MessageArray[0] == "!random")
				SearchQuote(data, true);

			if (data.MessageArray[0] == "!help")
				DisplayHelp(data);
            
			return true;
		}

		public void OnConsoleLine(string[] args)
		{
		}

		private void AddQuote(IrcMessageData data)
		{
			if ((data.MessageArray.Length < 2) || (String.IsNullOrEmpty(data.MessageArray[1])) || (String.IsNullOrEmpty(data.MessageArray[1].Trim())))
				return;

			int index = (_quotes.OrderBy(x => x.Id).LastOrDefault() ?? new QuoteData()).Id + 1;
			QuoteData qd = new QuoteData();
			qd.Date = DateTime.Now;
			qd.Id = index;
			qd.Channel = data.Channel;
			qd.Submitter = data.Nick;
			qd.Quote = data.Message.Substring(5);
			_quotes.Add(qd);
			_store.Flush(_quotes);
			_irc.SendMessage(SendType.Message, data.Channel, String.Format("Quote 04{0} added.", index));
		}

		private void DeleteQuote(IrcMessageData data)
		{
			if (_quotes.Count == 0)
				return;

			//requested index
			int index = 0;
			QuoteData qd;
			if ((data.MessageArray.Length == 2) && (int.TryParse(data.MessageArray[1], out index))) {
				//find
				qd = _quotes.FirstOrDefault(x => x.Id == index);
				//not found
				if (null == qd) {
					_irc.SendMessage(SendType.Message, data.Channel, String.Format("Quote 04{0} does not exist.", index));
					return;
				}
				//no access
				if ((qd.Submitter.ToLowerInvariant() != data.Nick.ToLowerInvariant()) && (!CanDelete(qd.Id, data.Host))) {
					//_irc.SendMessage(SendType.Message, data.Channel, String.Format("Quote 04{0} can only be deleted by 04{1}.", index, qd.Submitter.ToLowerInvariant()));
					return;
				}
				//delete
				_quotes.Remove(qd);
				_store.Flush(_quotes);
				if (_deleteRequests.ContainsKey(qd.Id))
					_deleteRequests.Remove(qd.Id);
				_irc.SendMessage(SendType.Message, data.Channel, String.Format("Quote 04{0} deleted.", index));
			}	
		}

		private struct DeleteRequest
		{
			public string Host;
			public DateTime Date;
		}

		private bool CanDelete(int id, string host)
		{
			if ((!_deleteRequests.ContainsKey(id)) || 
				(_deleteRequests[id].Host == host) ||
				(_deleteRequests[id].Date < DateTime.Now.AddSeconds(-1 * _deleteRequestPeriodInSecs))
				) {
				_deleteRequests[id] = new DeleteRequest {Host = host, Date = DateTime.Now};
				return false;
			}
			_deleteRequests.Remove(id);
			return true;
		}

		private void DisplayQuote(IrcMessageData data)
		{
			if (_quotes.Count == 0)
				return;

			//requested index
			int index = 0;
			QuoteData qd;
			if ((data.MessageArray.Length == 2) && (int.TryParse(data.MessageArray[1], out index))) {
				//find
				qd = _quotes.FirstOrDefault(x => x.Id == index);
				//not found
				if (null == qd)
				{
					_irc.SendMessage(SendType.Message, data.Channel, String.Format("Quote 04{0} does not exist.", index));
					return;
				}
				//up the popularity and persist
				qd.Popularity++;
				_store.Flush(_quotes);
			}
			//true random
			else if ((data.MessageArray.Length == 1))
				qd = GetRandomQuote();

			//random containing search term
			else {
				SearchQuote(data, true);
				return;
			}

			DisplayQuote(data.Channel, qd, null);
		}
		
		//out of all
		private QuoteData GetRandomQuote()
		{
			return GetRandomQuote(null);
		}

		//out of a list
		private QuoteData GetRandomQuote(IEnumerable<int> outOfThese)
		{
			if (null == outOfThese)
				outOfThese = _quotes.Select(x => x.Id);

			IList<int> possibilities = outOfThese.Except(_last20QuoteIds).ToList();
			if ((null == possibilities) || (possibilities.Count == 0)) {
				_last20QuoteIds = _last20QuoteIds.Except(outOfThese).ToList();
				possibilities = outOfThese.ToList();
			}

			int randId = possibilities[_rng.Next(possibilities.Count)];

			AddIdToLast20(randId);
			return _quotes.FirstOrDefault(x => x.Id == randId);
		}

		private void AddIdToLast20(int id)
		{
			if (_last20QuoteIds.Contains(id))
				return;

			_last20QuoteIds.Insert(0, id);
			while (_last20QuoteIds.Count > 20)
				_last20QuoteIds.RemoveAt(20);
		}

		private void DisplayQuote(string target, QuoteData quote, string header)
		{
			IList<string> display;
			//multiline
			if (_multilineOut)
				display = SplitQuote(quote.Quote);
			else
				display = new List<string> { quote.Quote };

			_irc.SendMessage(SendType.Message, target, String.Format(header ?? "Quote 04{0}: {1}", quote.Id, display[0]));
			for (int i = 1; i < display.Count; i++)
				_irc.SendMessage(SendType.Message, target, display[i]);
		}

		private void DisplayPopularity(IrcMessageData data)
		{
			_irc.SendMessage(SendType.Message, data.Channel, "13~<3~ Most Popular Quotes 13~<3~");
			int loop = 1;
			foreach (QuoteData qd in _quotes.OrderByDescending(x => x.Popularity).ThenBy(x => x.Id).Take(5)) {
				_irc.SendMessage(SendType.Message, data.Channel, String.Format("#{0} with {1} views: !quote {2}", loop, qd.Popularity, qd.Id ));
				loop++;
			}
		}

		private void SearchQuote(IrcMessageData data, bool random)
		{
			if (data.MessageArray.Length < 2)
				return;

			IList<QuoteData> results = _quotes;
			IEnumerable<string> searchWords = data.MessageArray.Skip(1);
			
			results = results.Where(qd => searchWords.All(s => qd.Quote.ToLower().Contains(s.ToLower()))).ToList();
			
			if ((results.Count > 10) && (!random)) {
				_irc.SendMessage(SendType.Message, data.Nick, String.Format("More than 10 results returned for 04{0}. Please be more specific.", data.Message.Split(null, 2)[1]));
				return;
			}
			if (results.Count == 0) {
				_irc.SendMessage(SendType.Message, random ? data.Channel : data.Nick, String.Format("No quotes found containing 04{0}.", data.Message.Split(null, 2)[1]));
				return;
			}

			if (random) {
				QuoteData qd = GetRandomQuote(results.Select(x => x.Id));
				DisplayQuote(data.Channel, qd, null);
			}
			else
				foreach (QuoteData qd in results)
					DisplayQuote(data.Nick, qd, "04!quote {0} {1}");
		}
        
		private void DisplayHelp(IrcMessageData data)
		{
			_irc.SendMessage(SendType.Message, data.Nick, "04!quote or 04!q for a random quote.");
			_irc.SendMessage(SendType.Message, data.Nick, "04!quote # or 04!q # for quote #.");
			_irc.SendMessage(SendType.Message, data.Nick, "04!quote <text> or 04!q <text> for a random quote containing <text>.");
			_irc.SendMessage(SendType.Message, data.Nick, "04!search <text> for a list of quotes containing <text>.");
			_irc.SendMessage(SendType.Message, data.Nick, "04!add <text> to add a quote.");
			_irc.SendMessage(SendType.Message, data.Nick, "04!delete # or 04!del # to delete a quote. you must be the submitter, or have one other person also request deletion within " + _deleteRequestPeriodInSecs + "secs.");
			_irc.SendMessage(SendType.Message, data.Nick, "04!popularity or 04!popular for the popularity rankings.");
		}

		public static IList<string> SplitQuote(string raw)
		{
			IList<string> temp = new List<string>();
			if (String.IsNullOrEmpty(raw))
				return temp;
			
			string[] lessThanSplit = raw.Split('<');
            
			foreach (string s in lessThanSplit) {
				string[] spaceSplit = s.Split(' ');
				//is not new line
				if (!spaceSplit[0].Contains(">")) {
					//opening line starts without a name
					if (temp.Count == 0)
						temp.Add(s);
					//a < was found within a line
					else
						temp[temp.Count - 1] = temp.LastOrDefault() + "<" + s;
				}
				//is new line, starting with name
				else
					temp.Add("<" + s);
			}
			//clean up
			for (int i = 0; i < temp.Count; i++) {
				temp[i] = temp[i].Trim();
				if (String.IsNullOrEmpty(temp[i])) {
					temp.RemoveAt(i);
					i--;
				}
			}
			//crunch the last excessive lines into one line
			while (temp.Count > _maxMultilineOut) {
				temp[_maxMultilineOut - 1] = temp[_maxMultilineOut - 1] + " " + temp[_maxMultilineOut];
				temp.RemoveAt(_maxMultilineOut);
			}

			return temp;
		}
        
		private class QuoteStore
		{
			private string _storeFileName;

			public QuoteStore(string storeFileName)
			{
				_storeFileName = storeFileName;
			}

			public void Flush(IList<QuoteData> quotes)
			{
				if (null == quotes)
					quotes = new List<QuoteData>();

				if (!File.Exists(_storeFileName))
					File.Create(_storeFileName);

				using (XmlWriter writer = new XmlTextWriter(new StreamWriter(_storeFileName))) {
					XmlSerializer xml = new XmlSerializer(typeof (List<QuoteData>));
					xml.Serialize(writer, quotes);
				}

			}

			public IList<QuoteData> Load()
			{
				if ((!File.Exists(_storeFileName)) || (new FileInfo(_storeFileName).Length == 0))
					Flush(null);

				using (XmlReader reader = new XmlTextReader(new StreamReader(_storeFileName))) {
					XmlSerializer xml = new XmlSerializer(typeof (List<QuoteData>));
					return (IList<QuoteData>) xml.Deserialize(reader);
				}
			}

		}

		[Serializable]
		public class QuoteData
		{
			public int Id;
			public string Submitter;
			public DateTime Date;
			public string Channel;
			public string Quote;
			public int Popularity;
		}

	}
}
