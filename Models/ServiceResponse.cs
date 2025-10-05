using Newtonsoft.Json;
using System.ComponentModel;

namespace BlockHouse.Models
{
    public class ServiceResponse : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _description;
        private double? _price;
        private DateTime _createdAt;
        private DateTime _updatedAt;

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

        [JsonProperty("description")]
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        [JsonProperty("price")]
        public double? Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(nameof(Price)); }
        }

        [JsonProperty("created_at")]
        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(nameof(CreatedAt)); }
        }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set { _updatedAt = value; OnPropertyChanged(nameof(UpdatedAt)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ServiceListResponse : INotifyPropertyChanged
    {
        private List<ServiceResponse> _services;
        private int _page;
        private int _pageSize;
        private int _totalServices;

        [JsonProperty("services")]
        public List<ServiceResponse> Services
        {
            get => _services;
            set { _services = value; OnPropertyChanged(nameof(Services)); }
        }

        [JsonProperty("page")]
        public int Page
        {
            get => _page;
            set { _page = value; OnPropertyChanged(nameof(Page)); }
        }

        [JsonProperty("page_size")]
        public int PageSize
        {
            get => _pageSize;
            set { _pageSize = value; OnPropertyChanged(nameof(PageSize)); }
        }

        [JsonProperty("total_services")]
        public int TotalServices
        {
            get => _totalServices;
            set { _totalServices = value; OnPropertyChanged(nameof(TotalServices)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
