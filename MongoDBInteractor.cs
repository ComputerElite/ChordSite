using ComputerUtils.Encryption;
using ComputerUtils.Logging;
using ModUploadSite;
using ModUploadSite.Users;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordSite
{
	public class MongoDBInteractor
	{
		public static MongoClient mongoClient = null;
		public static IMongoDatabase database = null;
		public static IMongoCollection<Song> songs = null;
		public static IMongoCollection<User> userCollection = null;
		public static IMongoCollection<SongSet> songSetCollection = null;
		public static void Initiate()
		{
			mongoClient = new MongoClient(Env.config.mongoDBURL);
			database = mongoClient.GetDatabase(Env.config.mongoDBName);
			songs = database.GetCollection<Song>("Songs");
			userCollection = database.GetCollection<User>("users");
			songSetCollection = database.GetCollection<SongSet>("Sets");
		}

		public static Song GetSong(string id, string token)
		{
			User u = GetUserByToken(token);
			Song s = songs.Find(x => x.songId == id).FirstOrDefault();
			if(s != null && u != null) s.isOwn = u._id.ToString() == s.uploaderID;
			return s;
		}

		public static SongSet GetSongSet(string id, string token, bool populateSongs = true)
		{
			User u = GetUserByToken(token);
			SongSet s = songSetCollection.Find(x => x.setId == id).FirstOrDefault();
			if (s != null && u != null) s.isOwn = u._id.ToString() == s.uploaderID;
			if (populateSongs && s != null)
			{
				foreach (string songId in s.songIds)
				{
					Song song = GetSong(songId, token);
					if (song != null) s.songs.Add(song);
				}
			}
			return s;
		}

		public static User GetUser(User u)
		{
			string sha = Hasher.GetSHA256OfString(u.password);
			return userCollection.Find(x => x.username == u.username && x.password == sha).FirstOrDefault();
		}

		public static User GetUserByUsernameAndEmail(User u)
		{
			return userCollection.Find(x => x.username == u.username && x.email.ToLower() == u.email.ToLower()).FirstOrDefault();
		}

		public static User GetUserByUsername(string username)
		{
			return userCollection.Find(x => x.username == username).FirstOrDefault();
		}

		public static void AddUser(User u)
		{
			userCollection.InsertOne(u);
		}

		public static User GetUserByToken(string token)
		{
			return userCollection.Find(x => x.currentToken == token).FirstOrDefault();
		}
		public static void UpdateUser(User u)
		{
			string sha = Hasher.GetSHA256OfString(u.password);
			userCollection.ReplaceOne(x => x.username == u.username, u);
		}

		public static List<Song> SearchSongs(string query, string token)
		{
			User u = GetUserByToken(token);
			BsonRegularExpression regex = new BsonRegularExpression("/.*" + query.Replace(" ", ".*") + ".*/i");
			FilterDefinition<Song> q = Builders<Song>.Filter.Or(new FilterDefinition<Song>[]
			{
				Builders<Song>.Filter.Regex(x => x.uploaderName, regex),
				Builders<Song>.Filter.Regex(x => x.name, regex),
				Builders<Song>.Filter.Regex(x => x.artist, regex)
			});
			List<Song> s = songs.Find(q).ToList();
			if(u != null) s.ForEach(x => x.isOwn = u._id.ToString() == x.uploaderID);
			return s;
		}


		public static List<SongSet> SearchSets(string query, string token)
		{
			User u = GetUserByToken(token);
			BsonRegularExpression regex = new BsonRegularExpression("/.*" + query.Replace(" ", ".*") + ".*/i");
			FilterDefinition<SongSet> q = Builders<SongSet>.Filter.Or(new FilterDefinition<SongSet>[]
			{
				Builders<SongSet>.Filter.Regex(x => x.uploaderName, regex),
				Builders<SongSet>.Filter.Regex(x => x.name, regex)
			});
			List<SongSet> s = songSetCollection.Find(q).ToList();
			if (u != null) s.ForEach(x => x.isOwn = u._id.ToString() == x.uploaderID);
			return s;
		}

		public static GenericRequestResponse UpdateSong(Song s, string token)
		{
			User u = GetUserByToken(token);
			if(u == null) return new GenericRequestResponse(403, "Log in you piece of shit");
			Song old = GetSong(s.songId, "");
			if (old != null && old.uploaderID != u._id.ToString()) return new GenericRequestResponse(403, "Not your song");
			s.uploaderID = u._id.ToString();
			s.uploaderName = u.username;
			if (s.songId == "") s.songId = DateTime.Now.Ticks.ToString();
			songs.DeleteOne(x => x.songId == s.songId);
			songs.InsertOne(s);
			return new GenericRequestResponse(200, s.songId.ToString());
		}

		public static GenericRequestResponse UpdateSet(SongSet s, string token)
		{
			User u = GetUserByToken(token);
			if (u == null) return new GenericRequestResponse(403, "Log in you piece of shit");
			SongSet old = GetSongSet(s.setId, "", false);
			if (old != null && old.uploaderID != u._id.ToString()) return new GenericRequestResponse(403, "Not your set");
			s.uploaderID = u._id.ToString();
			s.uploaderName = u.username;
			if (s.setId == "") s.setId = DateTime.Now.Ticks.ToString();
			songSetCollection.DeleteOne(x => x.setId == s.setId);
			songSetCollection.InsertOne(s);
			return new GenericRequestResponse(200, s.setId.ToString());
		}

		public static GenericRequestResponse DeleteSong(Song s, string token)
		{
			User u = GetUserByToken(token);
			Song old = GetSong(s.songId, "");
			if (old != null && old.uploaderID != u._id.ToString()) return new GenericRequestResponse(403, "Not your song");
			songs.DeleteOne(x => x.songId == s.songId);
			return new GenericRequestResponse(200, "Song Deleted");
		}

		public static GenericRequestResponse DeleteSet(SongSet s, string token)
		{
			User u = GetUserByToken(token);
			SongSet old = GetSongSet(s.setId, "", false);
			if (old != null && old.uploaderID != u._id.ToString()) return new GenericRequestResponse(403, "Not your set");
			songSetCollection.DeleteOne(x => x.setId == s.setId);
			return new GenericRequestResponse(200, "Set Deleted");
		}
	}
}
