using System.ComponentModel;
using Newtonsoft.Json;

namespace BlockHouse.Models.Responses
{
    public class EmployeeResponse : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _phone;
        private int _status;
        private decimal? _income;

        [JsonProperty("id")]
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        [JsonProperty("phone")]
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(nameof(Phone)); }
        }

        [JsonProperty("status")]
        public int Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        [JsonProperty("income")]
        public decimal? Income
        {
            get => _income;
            set { _income = value; OnPropertyChanged(nameof(Income)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}