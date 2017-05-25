using Android.App;
using Android.Widget;
using Android.OS;
using static UnderTheWeather.Core;
using System;
using System.Threading.Tasks;
using Android.Locations;
using Android.Util;
using System.Collections.Generic;
using System.Linq;
using Android.Net;

namespace UnderTheWeather
{
    [Activity(MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.DeviceDefault.NoActionBar")]
    public class MainActivity : Activity, ILocationListener
    {
        private TextView tvMinTemp;
        private TextView tvMaxTemp;
        Location currentLocation;
        LocationManager locationManager;
        string locationProvider;
        TextView tvCity, tvCountry;
        TextView tvTodayDate;
        TextView tvHumidity;
        TextView tvDescriptionText;
        private string city = "";
        private string country = "";
        private string countryCode = "";
        NetworkInfo networkInfo = null;

        double lat = 0;
        double lng = 0;

        bool isOnline = false;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            

            tvMinTemp = FindViewById<TextView>(Resource.Id.tvMinTemp);
            tvMaxTemp = FindViewById<TextView>(Resource.Id.tvMaxTemp);
            tvCity = FindViewById<TextView>(Resource.Id.tvCity);
            tvCountry = FindViewById<TextView>(Resource.Id.tvCountry);
            tvTodayDate = FindViewById<TextView>(Resource.Id.tvTodayDate);
            tvHumidity = FindViewById<TextView>(Resource.Id.tvHumidity);
            tvDescriptionText = FindViewById<TextView>(Resource.Id.tvDescriptionText);


            InitializeLocationManager();
        }

        void InitializeLocationManager()
        {
            locationManager = (LocationManager)GetSystemService(LocationService);

            Criteria locationCriteria = new Criteria();

            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.Medium;

            locationProvider = locationManager.GetBestProvider(locationCriteria, true);

            if (locationProvider != null)
            {
                locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
            }
            else
            {
                Log.Info("", "No location providers available");
            }
            Log.Debug("", "Using " + locationProvider + ".");
        }

        public async void OnLocationChanged(Location location)
        {
            try
            {
                ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                networkInfo = connectivityManager.ActiveNetworkInfo;

                if (networkInfo != null)
                {
                    isOnline = networkInfo.IsConnected;
                    if (isOnline)
                    {
                        currentLocation = location;

                        if (currentLocation == null)
                        {
                            ClearValues("Unable to determine your current city.");
                        }
                        else
                        {
                            lat = currentLocation.Latitude;
                            lng = currentLocation.Longitude;
                            Address address = await ReverseGeocodeCurrentLocation();
                            city = address.Locality;
                            country = address.CountryName;
                            countryCode = address.CountryCode;

                            Weather weather = new Weather();
                            var asyncWeather = await weather.GetTemp(city, countryCode);

                            tvMinTemp.Text = asyncWeather.min + " \u2103";
                            tvMaxTemp.Text = asyncWeather.max + " \u2103";
                            tvTodayDate.Text = DateTime.Now.ToString("ddd, dd MMMM yyyy");
                            tvHumidity.Text = asyncWeather.humidity + " %";
                            tvDescriptionText.Text = asyncWeather.description;
                            tvCity.Text = city;
                            tvCountry.Text = country;
                        }
                    }
                }
                else
                {
                    ClearValues("Oops! No network access.");
                }
            }
            catch (Exception ex)
            {
                ClearValues("Eish!...failed to detect location!");
            }
        }

        public void OnProviderDisabled(string provider) {
            ClearValues("Please enable GPS setting.");
        }

        public void OnProviderEnabled(string provider) {
            locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras) { }

        protected override void OnResume()
        {
            base.OnResume();
            locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            locationManager.RemoveUpdates(this);
        }

        async Task<Address> ReverseGeocodeCurrentLocation()
        {
            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList = await geocoder.GetFromLocationAsync(currentLocation.Latitude, currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            return address;
        }

        private void ClearValues(string message) {
            tvCity.Text = "";
            tvMinTemp.Text = "0 \u2103";
            tvMaxTemp.Text = "0 \u2103";
            tvHumidity.Text = "0 %";
            tvDescriptionText.Text = "-";
            tvCountry.Text = message;
        }
    }
}

