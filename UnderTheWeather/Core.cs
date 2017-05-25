using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnderTheWeather
{
    class Core
    {
        public class Weather
        {
            public string temp { get; set; }
            public string min { get; set; }
            public string max { get; set; }
            public string humidity { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
            public async Task<Weather> GetTemp(string locality, string countryCode)
            {
                Weather weather = new Weather();

                try
                {
                    string queryString = "http://api.openweathermap.org/data/2.5/weather?q=" + locality + ","  + countryCode + "&appid=" + Configuration.openWeatherAppId +  "&units=metric";

                    dynamic results = await DataService.getDataFromService(queryString).ConfigureAwait(false);

                    var responseObject = JObject.Parse(results.ToString());

                    JObject mainObject = (JObject)(responseObject["main"]);
                    JArray weatherObject = (JArray)(responseObject["weather"]);

                    weather.min = mainObject["temp_min"].ToString();
                    weather.max = mainObject["temp_max"].ToString();
                    weather.temp = mainObject["temp"].ToString();
                    weather.humidity = mainObject["humidity"].ToString();
                    weather.description = weatherObject[0]["main"].ToString();
                    weather.icon = "http://openweathermap.org/img/w/" + weatherObject[0]["icon"].ToString() + ".png";

                }
                catch (Exception ex)
                {
                    string exception = ex.ToString();
                }

                return weather;
            }
        }

        public class DataService
        {
            public static async Task<dynamic> getDataFromService(string queryString)
            {
                dynamic data = null;

                try
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Configuration.openWeatherKey);

                    var response = await client.GetAsync(queryString);
                    
                    if (response != null)
                    {
                        string json = response.Content.ReadAsStringAsync().Result;
                        data = JsonConvert.DeserializeObject(json);
                    }
                }
                catch (Exception ex) {
                    string exception = ex.ToString();
                }

                return data;
            }
        }
    }
}