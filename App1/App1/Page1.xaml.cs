using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Page1 : ContentPage
    {
        static int i = 0;
        public Page1(List<Teacher> searchedTeachers)
        {
            InitializeComponent();

            Button backButton = new Button { Text = "Назад" };
            Button showNotices = new Button { Text = "Показать избранных преподавателей" };

            backButton.Clicked += OnPrevPage;
            showNotices.Clicked += ShowNoticesTeachers;

            Label header = new Label
            {
                Text = "Результат поиска",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                HorizontalOptions = LayoutOptions.Center
            };

            ListView listView = new ListView
            {
                HasUnevenRows = true,
                // Определяем источник данных
                ItemsSource = searchedTeachers,

                // Определяем формат отображения данных
                ItemTemplate = new DataTemplate(() =>
                {
                    Label nameLabel = new Label { FontSize = 18 };
                    nameLabel.SetBinding(Label.TextProperty, "name");

                    Label idLabel = new Label { IsVisible = false };
                    idLabel.SetBinding(Label.TextProperty, "person_id");

                    Label likeLabel = new Label { Text = "Избранное", FontSize = 18 };
                    CheckBox noticeCheckBox = new CheckBox { IsChecked = TeacherInDBIncluding(searchedTeachers[i].person_id)};
                    if (i < searchedTeachers.Count - 1) i++;
                    StackLayout forLikeStack = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { likeLabel, noticeCheckBox } };

                    noticeCheckBox.CheckedChanged += NoticeTeacher;

                    // создаем объект ViewCell
                    return new ViewCell
                    {
                        View = new StackLayout
                        {
                            Padding = new Thickness(0, 5),
                            Orientation = StackOrientation.Vertical,
                            Children = { nameLabel, idLabel, forLikeStack }
                        }
                    };
                })
            };

            listView.ItemTapped += OnItemTapped;
            showNotices.Clicked += ShowNoticesTeachers;

            this.Content = new StackLayout { Children = { header, listView, showNotices, backButton } };
        }

        private async void ShowNoticesTeachers(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Page1(GetNoticedTeachers()));
        }

        private async void NoticeTeacher(object sender, CheckedChangedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            StackLayout listWiewItem = (StackLayout)checkBox.Parent.Parent;

            Label nameLabel = (Label)listWiewItem.Children[0];
            Label idLabel = (Label)listWiewItem.Children[1];

            string teacherName = nameLabel.Text;
            int teacherId = Convert.ToInt32(idLabel.Text);

            //Добавляем в БД, при постановке галочки в checkBox и удоляем при убирании галочки
            if (e.Value)
            {
                string url2 = $"{DataStorage.randomUrl}/api/THS/NoticeTeacher";
                TeacherForDB a = new TeacherForDB { id = 0, person_id = teacherId, name = teacherName };
                var json = JsonConvert.SerializeObject(a);

                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Bypass-Tunnel-Reminder", "true");

                var responce = await client.PostAsync(url2, stringContent);

                await DisplayAlert("УСПЕШНЫЙ УСПЕХ", $"{teacherName} добавлен в базу данных", "Зашибись");
            }
            else
            {
                var client = new HttpClient();
                using (client)
                {
                    client.DefaultRequestHeaders.Add("Bypass-Tunnel-Reminder", "true");
                    client.BaseAddress = new Uri($"{DataStorage.randomUrl}");

                    var response = client.DeleteAsync($"api/THS/UnNoticeTeacher/{teacherId}").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("УСПЕШНЫЙ УСПЕХ", $"{teacherName} удолён из базы данных", "Зашибись");
                    }
                    else await DisplayAlert("ПРОВАЛЬНЫЙ ПРОВАЛ", $"Ёбс тудэй...", "Плохо");
                }
            }
        }

        //Жмакаем на препода и нас кидает на страницу с его расписанием
        public async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            Teacher selectedTeacher = e.Item as Teacher;

            if (selectedTeacher != null)
            {
                string teacherID = selectedTeacher.person_id;
                DataStorage.teacherId = teacherID;
                DataStorage.teacherName = selectedTeacher.name;

                string url2 = $"{DataStorage.randomUrl}/api/THS/GiveTimetable/{teacherID}";

                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(url2);
                request2.ServerCertificateValidationCallback = delegate { return true; };
                request2.Headers.Add("Bypass-Tunnel-Reminder", "true");

                HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse();

                string textData2;
                List<Timetable> result2 = new List<Timetable>();

                using (StreamReader streamReader = new StreamReader(response2.GetResponseStream()))
                {
                    textData2 = streamReader.ReadToEnd();
                    result2 = JsonConvert.DeserializeObject<List<Timetable>>(textData2);
                }

                await Navigation.PushAsync(new Page2(result2));
            }
        }

        private async void OnPrevPage(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
            Navigation.RemovePage(this);
        }

        //Узнать, есть ли преподаватель в избранных
        static bool TeacherInDBIncluding(string teacherID)
        {
            bool result = false;
            List<Teacher> teachers = GetNoticedTeachers();

            foreach (Teacher someTeacher in teachers)
            {
                if (someTeacher.person_id == teacherID) { result = true; return result; }
            }
            return result;
        }

        //Получить массив избранных преподавателей через запрос
        static List<Teacher> GetNoticedTeachers()
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
            foreach(TeacherForDB someTeacher in searchedTeachers)
            {
                result.Add(new Teacher { name = someTeacher.name, person_id = Convert.ToString(someTeacher.person_id) });
            }

            return result;
        }
    }
}