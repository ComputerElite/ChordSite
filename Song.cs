using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordSite
{
	[BsonIgnoreExtraElements]
	public class Song
	{
		public string songId { get; set; } = "";

		public string name { get; set; } = "";
		public string artist { get; set; } = "";
		public string text { get; set; } = "";
		public string youtubeLink { get; set; } = "";
		public string uploaderID { get; set; } = "";
		public string uploaderName { get; set; } = "";
		[BsonIgnore]
		public bool isOwn { get; set; } = false;
	}
}
