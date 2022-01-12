using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Page2 : ContentPage
    {
        static DateTime defaultDate = new DateTime(2022, 01, 10);

        //Кнопка "Назад"
        static Button backButton = new Button { Text = "Назад" };

        static Label searchedName = new Label { Text = $"{DataStorage.teacherName}", FontSize = 20, TextColor = Color.Black };

        //Фильтры
        static Label filtersLabel = new Label { Text = "Фильтры", FontSize = 25, TextColor = Color.Black };
        static Entry lessonNomberEntry = new Entry { Placeholder = "Введите номер пары" };
        static DatePicker lessonDate = new DatePicker { Date = defaultDate };
        static Button applyFilters = new Button { Text = "Применить" };
        
        static StackLayout filtersStack = new StackLayout { Orientation = StackOrientation.Vertical, Children = { filtersLabel, lessonNomberEntry, lessonDate, applyFilters }, Margin = new Thickness(20) };
        //

        public Page2(List<Timetable> searchedTimetable)
        {
            InitializeComponent();

            backButton.Clicked += OnPrevPage;
            applyFilters.Clicked += OutputApplyedFiltersPage;

            //Отрисовка расписания
            //Группа кабинет тип предемета предмет дата время
            ListView listView = new ListView
            {
                HasUnevenRows = true,
                // Определяем источник данных
                ItemsSource = searchedTimetable,

                // Определяем формат отображения данных
                ItemTemplate = new DataTemplate(() =>
                {
                    //Группа
                    Label grouopLabel = new Label { FontSize = 18 };
                    grouopLabel.Text = "Группа:";
                    Label grouopLabel2 = new Label { FontSize = 18 };
                    grouopLabel2.SetBinding(Label.TextProperty, "education_group_name");

                    StackLayout stackLayoutGroup = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { grouopLabel, grouopLabel2 } };

                    //Кабинет
                    Label plaseLabel = new Label { FontSize = 18 };
                    plaseLabel.Text = "Кабинет:";
                    Label plaseLabel2 = new Label { FontSize = 18 };
                    plaseLabel2.SetBinding(Label.TextProperty, "place");

                    StackLayout stackLayoutPlace = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { plaseLabel, plaseLabel2 } };

                    //Тип предмета
                    Label typrLabel = new Label { FontSize = 18 };
                    typrLabel.Text = "Тип:";
                    Label typrLabel2 = new Label { FontSize = 18 };
                    typrLabel2.SetBinding(Label.TextProperty, "type");

                    StackLayout stackLayoutType = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { typrLabel, typrLabel2 } };

                    //Предмет 
                    Label subjctLabel = new Label { FontSize = 18 };
                    subjctLabel.Text = "Предмет:";
                    Label subjectLabel2 = new Label { FontSize = 18 };
                    subjectLabel2.SetBinding(Label.TextProperty, "subject");

                    StackLayout stackLayoutSubject = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { subjctLabel, subjectLabel2 } };

                    //Дата
                    Label dateLabel = new Label { FontSize = 18 };
                    dateLabel.Text = "Дата:";
                    Label dateLabel2 = new Label { FontSize = 18 };
                    dateLabel2.SetBinding(Label.TextProperty, "date_lesson");

                    StackLayout stackLayoutDate = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { dateLabel, dateLabel2 } };

                    //Номер пары
                    Label lessonLabel = new Label { FontSize = 18 };
                    lessonLabel.Text = "Номер пары:";
                    Label lessonLabel2 = new Label { FontSize = 18 };
                    lessonLabel2.SetBinding(Label.TextProperty, "lesson_number");

                    StackLayout stackLayoutLesson = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { lessonLabel, lessonLabel2 } };

                    // создаем объект ViewCell.
                    return new ViewCell
                    {
                        View = new StackLayout
                        {
                            Padding = new Thickness(0, 5),
                            Orientation = StackOrientation.Vertical,
                            //Группа кабинет тип предемета предмет дата время
                            Children = { stackLayoutGroup, stackLayoutPlace, stackLayoutType, stackLayoutSubject, stackLayoutDate, stackLayoutLesson },
                        }
                    };
                })
            };
            this.Content = new StackLayout {Margin = new Thickness(0, 0, 0, 10), Children = {searchedName, filtersStack, listView, backButton } };
        }
        private async void OutputApplyedFiltersPage(object sender, EventArgs e)
        {
            string lessonNomber = lessonNomberEntry.Text;
            string date = ConvertTime(lessonDate.Date);

            if (lessonNomber == " ")
            {
                string url2 = $"https://portal.kuzstu.ru/extra/api/teacher_schedule?teacher_id={DataStorage.teacherId}";

                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(url2);
                request2.ServerCertificateValidationCallback = delegate { return true; };

                HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse();

                string textData2;
                List<Timetable> result = new List<Timetable>();

                using (StreamReader streamReader = new StreamReader(response2.GetResponseStream()))
                {
                    textData2 = streamReader.ReadToEnd();
                    //textData2 = Regex.Replace(textData2, @"\\u([0-9A-Fa-f]{4})", m => "" + (char)Convert.ToInt32(m.Groups[1].Value, 16));
                    result = JsonConvert.DeserializeObject<List<Timetable>>(textData2);
                }

                result = result.FindAll(item => item.date_lesson == date);
                await Navigation.PushAsync(new Page2(result));
            }
            else
            {
                string url2 = $"https://portal.kuzstu.ru/extra/api/teacher_schedule?teacher_id={DataStorage.teacherId}";

                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(url2);
                request2.ServerCertificateValidationCallback = delegate { return true; };

                HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse();

                string textData2;
                List<Timetable> result = new List<Timetable>();

                using (StreamReader streamReader = new StreamReader(response2.GetResponseStream()))
                {
                    textData2 = streamReader.ReadToEnd();
                    result = JsonConvert.DeserializeObject<List<Timetable>>(textData2);
                }

                result = result.FindAll(item => item.date_lesson == date);
                result = result.FindAll(item => item.lesson_number == Convert.ToString(lessonNomber));
                await Navigation.PushAsync(new Page2(result));
            }
        }

        private async void OnPrevPage(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
            Navigation.RemovePage(this);
        }

        static string GetTime(string lessonNomber)
        {
            return "";
        }

        static string ConvertTime(DateTime selectedDate)
        {
            string returnedDate = $"{selectedDate.Year}-0{selectedDate.Month}-{selectedDate.Day}";
            return returnedDate;
        }
    }
}