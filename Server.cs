using ComputerUtils.Discord;
using ComputerUtils.Logging;
using ComputerUtils.Webserver;
using ModUploadSite;
using ModUploadSite.Users;
using System.Net;
using System.Text.Json;

namespace ChordSite
{
	internal class Server
	{
		public HttpServer server;
		public static Config config { get { return Env.config; } }
		public Dictionary<string, string> replace = new Dictionary<string, string>
		{
			{"{meta}", "<meta name=\"theme-color\" content=\"#63fac3\">\n<meta property=\"og:site_name\" content=\"Chord site\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" }
		};

		public string GetToken(ServerRequest request, bool send403 = true)
		{
			string token = request.context.Request.Headers["token"];
			if (token == null)
			{
				token = request.queryString["token"];
				if (token == null)
				{
					token = request.cookies["token"] == null ? "" : request.cookies["token"].Value;
					if (token == null)
					{
						if (send403) request.Send403();
						return "";
					}
				}
			}
			return token;
		}

		public bool IsUserAdmin(ServerRequest request, bool send403 = true)
		{
			return GetToken(request, send403) == config.masterToken;
		}

		public bool IsUserAdmin(string token, bool send403 = true)
		{
			return token == config.masterToken;
		}

		public static void SendMasterWebhookMessage(string title, string message, int color)
		{
			if (config.masterWebhookUrl == "") return;
			try
			{
				Logger.Log("Sending master webhook");
				DiscordWebhook webhook = new DiscordWebhook(config.masterWebhookUrl);
				webhook.SendEmbed(title, message, "master " + DateTime.UtcNow, "Master Server", "https://computerelite.github.io/assets/CE_512px.png", "", "https://computerelite.github.io/assets/CE_512px.png", "", color);
			}
			catch (Exception ex)
			{
				Logger.Log("Exception while sending webhook" + ex.ToString(), LoggingType.Warning);
			}
		}

		public void HandleGenericResponse(ServerRequest request, GenericRequestResponse response)
		{
			request.SendString(response.message, response.contentType, response.statusCode);
		}

		public void StartServer()
		{
			server = new HttpServer();
			Func<ServerRequest, bool> accessCheck = new Func<ServerRequest, bool>(request => IsUserAdmin(request, false));
			string frontend = "frontend" + Path.DirectorySeparatorChar;
			MongoDBInteractor.Initiate();
			UserSystem.Initialize();
			server.DefaultCacheValidityInSeconds = 0;

			// Songs
			server.AddRoute("GET", "/api/v1/song/", new Func<ServerRequest, bool>(request =>
			{
				request.SendString(JsonSerializer.Serialize(MongoDBInteractor.GetSong(request.pathDiff, GetToken(request))));
				return true;
			}), true);
			server.AddRoute("GET", "/api/v1/search/", new Func<ServerRequest, bool>(request =>
			{
				request.SendString(JsonSerializer.Serialize(MongoDBInteractor.SearchSongs(request.pathDiff, GetToken(request))));
				return true;
			}), true);
			server.AddRoute("POST", "/api/v1/updatesong", new Func<ServerRequest, bool>(request =>
			{
				Song s = JsonSerializer.Deserialize<Song>(request.bodyString);
				HandleGenericResponse(request, MongoDBInteractor.UpdateSong(s, GetToken(request)));
				return true;
			}));
			server.AddRoute("DELETE", "/api/v1/deletesong", new Func<ServerRequest, bool>(request =>
			{
				Song s = JsonSerializer.Deserialize<Song>(request.bodyString);
				HandleGenericResponse(request, MongoDBInteractor.DeleteSong(s, GetToken(request)));
				return true;
			}));

			// Users
			server.AddRoute("GET", "/api/v1/me", new Func<ServerRequest, bool>(request =>
			{
				HandleGenericResponse(request, UserSystem.GetUserByToken(GetToken(request)));
				return true;
			}));
			server.AddRoute("GET", "/api/v1/amiloggedin", new Func<ServerRequest, bool>(request =>
			{
				HandleGenericResponse(request, UserSystem.GetLoggedInUser(GetToken(request)));
				return true;
			}));
			server.AddRoute("POST", "/api/v1/login", new Func<ServerRequest, bool>(request =>
			{
				HandleGenericResponse(request, UserSystem.Login(request.bodyString));
				return true;
			}));
			server.AddRoute("POST", "/api/v1/register", new Func<ServerRequest, bool>(request =>
			{
				HandleGenericResponse(request, UserSystem.Register(request.bodyString));
				return true;
			}));
			server.AddRoute("POST", "/api/v1/requestpasswordreset", new Func<ServerRequest, bool>(request =>
			{
				HandleGenericResponse(request, UserSystem.RequestPasswordReset(request.bodyString));
				return true;
			}));
			server.AddRoute("POST", "/api/v1/confirmpasswordreset", new Func<ServerRequest, bool>(request =>
			{
				HandleGenericResponse(request, UserSystem.ResetPasswordConfirmed(request.bodyString));
				return true;
			}));

			server.AddRouteFile("/login", frontend + "login.html", replace, true, true, true, 0);
			server.AddRouteFile("/register", frontend + "register.html", replace, true, true, true, 0);
			server.AddRouteFile("/requestpasswordreset", frontend + "requestpasswordreset.html", replace, true, true, true, 0);
			server.AddRouteFile("/confirmpasswordreset", frontend + "confirmpasswordreset.html", replace, true, true, true, 0);


			server.AddRouteFile("/chord", frontend + "chord.html", replace, true, true, true, 0);
			server.AddRouteFile("/song", frontend + "song.html", replace, true, true, true, 0);
			server.AddRouteFile("/edit", frontend + "edit.html", replace, true, true, true, 0);
			server.AddRouteFile("/", frontend + "index.html", replace, true, true, true, 0);
			server.AddRouteFile("/style.css", frontend + "style.css", replace, true, true, true, 0);
			server.AddRouteFile("/script.js", frontend + "script.js", replace, true, true, true, 0);


			server.StartServer(config.port);
		}
		}
}