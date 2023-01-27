using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordSite
{
	[BsonIgnoreExtraElements]
	public class SongSet
	{
		public string setId { get; set; } = "";

		public string name { get; set; } = "";
		public string description { get; set; } = "";
		public string uploaderID { get; set; } = "";
		public string uploaderName { get; set; } = "";
		[BsonIgnore]
		public bool isOwn { get; set; } = false;
		[BsonIgnore]
		public List<Song> songs { get; set; } = new List<Song>();
		public List<string> songIds { get; set; } = new List<string>();
	}
}
