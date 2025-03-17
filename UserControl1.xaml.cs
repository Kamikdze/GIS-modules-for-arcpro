using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProAppModule6
{
    /// <summary>
    /// Логика взаимодействия для SearchForm.xaml
    /// </summary>
    public partial class SearchForm
    {
        // Коллекция для хранения и отображения данных
        private ObservableCollection<SearchResult> dataTable;

        public SearchForm()
        {
            InitializeComponent();
            // Инициализация коллекции и привязка к DataGrid
            dataTable = new ObservableCollection<SearchResult>();
            dataGridResult.ItemsSource = dataTable;
        }

        internal void ShowDialog()
        {
            throw new NotImplementedException();
        }

        // Обработчик кнопки "Найти"
        private async void OnFindClick(object sender, RoutedEventArgs e)
        {
            string streetName = textStreet.Text; // Поле для имени улицы
            string streetID = IDStreet.Text;     // Поле для ID

            // Проверка на пустые значения
            if (string.IsNullOrEmpty(streetID) && string.IsNullOrEmpty(streetName))
            {
                MessageBox.Show("Введите ID или название улицы.");
                return;
            }

            // Очистка предыдущих данных
            dataTable.Clear();

            // Асинхронный запрос к базе данных
            await Task.Run(() =>
            {
                try
                {
                    string geodatabasePath = @"C:\Users\mifta\Documents\ArcGIS\Projects\MyProject2\MyProject2.gdb";

                    using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(geodatabasePath))))
                    {
                        Table table = geodatabase.OpenDataset<Table>("lab");

                        // Формирование условия WHERE
                        QueryFilter queryFilter;
                        if (!string.IsNullOrEmpty(streetID)) // Если указан ID
                        {
                            queryFilter = new QueryFilter
                            {
                                WhereClause = $"OBJECTID = {streetID}"
                            };
                        }
                        else // Если указано имя улицы
                        {
                            queryFilter = new QueryFilter
                            {
                                WhereClause = $"UPPER(Name) LIKE UPPER('{streetName}%')"
                            };
                        }

                        // Выполнение поиска
                        using (RowCursor cursor = table.Search(queryFilter, false))
                        {
                            while (cursor.MoveNext())
                            {
                                using (Row row = cursor.Current)
                                {
                                    // Получение значений из строки таблицы
                                    int objectId = Convert.ToInt32(row["OBJECTID"]);
                                    string name = row["Name"]?.ToString();
                                    string description = row["description"]?.ToString();

                                    // Обновление коллекции на главном потоке
                                    Dispatcher.Invoke(() =>
                                    {
                                        dataTable.Add(new SearchResult
                                        {
                                            OBJECTID = objectId,
                                            Name = name,
                                            Description = description
                                        });
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => MessageBox.Show($"Ошибка: {ex.Message}"));
                }
            });
        }
    }

    // Класс данных для привязки к DataGrid
    public class SearchResult
    {
        public int OBJECTID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
