using System;

namespace AuthorizationLab.Models
{
    public class Album
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Publisher { get; set; }
    }
}
