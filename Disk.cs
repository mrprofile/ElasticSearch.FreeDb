using System.Collections.Generic;
using Nest;

namespace XmcdParser
{
    public class Disk
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = false)]
        public int DiskLength { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed)]
        public string Genre { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed)]
        public int Year { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = false)]
        public List<string> DiskIds { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = false)]
        public List<int> TrackFramesOffsets { get; set; }
        public List<string> Tracks { get; set; }
        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = false)]
        public Dictionary<string, string> Attributes { get; set; }
        public Disk()
        {
            TrackFramesOffsets = new List<int>();
            Tracks = new List<string>();
            DiskIds = new List<string>();
            Attributes = new Dictionary<string, string>();
        }
    }
}