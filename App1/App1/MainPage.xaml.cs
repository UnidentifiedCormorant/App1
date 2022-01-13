using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App1
{
    public partial class MainPage : ContentPage
    {
        List<Teacher> result = new List<Teacher>();

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            giveName.Clicked += DoSomeMagic;
            giveNameByBackend.Clicked += DoSomeImpressiveMagic;
            giveNoticedTeachers.Clicked += ShowNotices;
        }

        //Вывести отмеченных преподавателей
        private async void ShowNotices(object sender, EventArgs e)
        {
            string url = $"{DataStorage.randomUrl}/api/THS/GiveNoticeTeachers";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Bypass-Tunnel-Reminder", "true");

            request.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            List<TeacherForDB> searchedTeachers = new List<TeacherForDB>();
            string textData;

            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                textData = streamReader.ReadToEnd();
                searchedTeachers = JsonConvert.DeserializeObject<List<TeacherForDB>>(textData);
            }

            List<Teacher> result = new List<Teacher>();
            foreach (TeacherForDB someTeacher in searchedTeachers)
            {
                result.Add(new Teacher { name = someTeacher.name, person_id = Convert.ToString(someTeacher.person_id) });
            }

            await Navigation.PushAsync(new Page1(result));
        }

        private async void DoSomeMagic(object sender, EventArgs e)
        {
            string teacherName = nameOfTeacher.Text;

            string url = $"{DataStorage.randomUrl}/api/THS/GiveTeachers/{teacherName}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Bypass-Tunnel-Reminder", "true");
            request.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string textData;
            List<Teacher> result = new List<Teacher>();

            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                textData = streamReader.ReadToEnd();
                result = JsonConvert.DeserializeObject<List<Teacher>>(textData);
            }

            await Navigation.PushAsync(new Page1(result));
        }

        // lt -p 60748 -l localhost - всё, что нужно для полного счастья
        private async void DoSomeImpressiveMagic(object sender, EventArgs e)
        {
            string teacherName = nameOfTeacher.Text;
            string url = $"{DataStorage.randomUrl}/api/THS/GiveTeachers/{teacherName}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Bypass-Tunnel-Reminder", "true");

            request.ServerCertificateValidationCallback = delegate { return true; };

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string textData;

            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                textData = streamReader.ReadToEnd();
                result = JsonConvert.DeserializeObject<List<Teacher>>(textData);
            }

            await DisplayAlert("УСПЕШНЫЙ УСПЕХ", $"{result[0].name}", "Зашибись");
        }
    }
}
