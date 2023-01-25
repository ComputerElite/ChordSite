using ComputerUtils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChordSite
{
	public class Config
	{
		public string publicAddress { get; set; } = "";
		public int port { get; set; } = 20005;
		public string masterToken { get; set; } = "";
		public string masterWebhookUrl { get; set; } = "";
		public string mongoDBURL { get; set; } = "mongodb://localhost:27017";
		public string mongoDBName { get; set; } = "ChordDB";
		public string emailSender { get; set; } = "";
		public string emailPassword { get; set; } = "";
		public string emailServer { get; set; } = "";

		public static Config LoadConfig()
		{
			string configLocation = Env.workingDir + "data" + Path.DirectorySeparatorChar + "config.json";
			if (!File.Exists(configLocation)) File.WriteAllText(configLocation, JsonSerializer.Serialize(new Config()));
			return JsonSerializer.Deserialize<Config>(File.ReadAllText(configLocation));
		}

		public void Save()
		{
			try
			{
				File.WriteAllText(Env.workingDir + "data" + Path.DirectorySeparatorChar + "config.json", JsonSerializer.Serialize(this));
			}
			catch (Exception e)
			{
				Logger.Log("couldn't save config: " + e.ToString(), LoggingType.Warning);
			}
		}
	}
}
