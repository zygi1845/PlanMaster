using System;

namespace Frontend.Models
{
    public class HourPlanRow
    {
        public string Hour { get; set; }
        private string hour
        {
            get => hour;
            set
            {
                hour = value;
                Console.WriteLine($"Ustawiono godzinę: {value}");
            }
        }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string Saturday { get; set; }
        public string Sunday { get; set; }
    }
}
