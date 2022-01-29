using System;
using System.Collections.Generic;


namespace LinkedData
{
    public class Top10kPlayer
    {
        public String id { get; set; }
        public String name { get; set; }
        public int rank { get; set; }
        public List<Top10kScore> top10kScore = new List<Top10kScore>();
    }
}