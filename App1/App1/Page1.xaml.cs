using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Page1 : ContentPage
    {
        public Page1(List<Teacher> searchedTeachers)
        {
            InitializeComponent();

            Button backButton = new Button
            {
                Text = "Назад"
            };

            backButton.Clicked += OnPrevPage;

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
                    // привязка к свойству Name
                    Label nameLabel = new Label { FontSize = 18 };
                    nameLabel.SetBinding(Label.TextProperty, "name");

                    // привязка к свойству Company
                    Label idLabel = new Label();
                    idLabel.SetBinding(Label.TextProperty, "person_id");

                    Label likeLabel = new Label { Text = "Избранное", FontSize = 18 };
                    CheckBox likeCheckBox = new CheckBox();
                    StackLayout forLikeStack = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { likeLabel, likeCheckBox } };

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

            this.Content = new StackLayout { Children = { header, listView, backButton } };
        }

        private async void OnPrevPage(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
            Navigation.RemovePage(this);
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

                //string url2 = $"{RandomUrl.randomUrl}/api/THS/GiveTimetable/{teacherID}"; //Запрос через мой гениальный api
                string url2 = $"https://portal.kuzstu.ru/extra/api/teacher_schedule?teacher_id={teacherID}"; //Запрос через КузГТУ api для тестирования фронта

                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(url2);
                request2.ServerCertificateValidationCallback = delegate { return true; };
                //request2.Headers.Add("Bypass-Tunnel-Reminder", "true");

                HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse();

                string textData2;
                List<Timetable> result2 = new List<Timetable>();

                using (StreamReader streamReader = new StreamReader(response2.GetResponseStream()))
                {
                    textData2 = streamReader.ReadToEnd();
                    //textData2 = Regex.Replace(textData2, @"\\u([0-9A-Fa-f]{4})", m => "" + (char)Convert.ToInt32(m.Groups[1].Value, 16));
                    result2 = JsonConvert.DeserializeObject<List<Timetable>>(textData2);
                }

                await Navigation.PushAsync(new Page2(result2));
            }
        }
    }
}