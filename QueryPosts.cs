using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProAppModule6
{
    public class QueryPosts
    {
        public async Task QueryGeodatabaseAsync(string geodatabasePath, string tableName)
        {
            await QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(geodatabasePath))))
                {
                    // Открываем таблицу
                    using (Table postsTable = geodatabase.OpenDataset<Table>(tableName))
                    {
                        // Создаем SQL-запрос для получения названий постов, в именах которых есть буква "с"
                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = "Name LIKE '%А%'"
                        };

                        // Выполняем запрос и обрабатываем результаты
                        using (RowCursor rowCursor = postsTable.Search(queryFilter, false))
                        {
                            List<string> postNames = new List<string>();

                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    // Получаем значения Name
                                    string postName = row["Name"].ToString();

                                    // Добавляем в список если условие выполнено
                                    postNames.Add(postName);
                                }
                            }

                            string res = "";
                            // Выводим результат
                            if (postNames.Any())
                            {
                                MessageBox.Show("Посты в именах которые начинаются на А:");
                                foreach (string postName in postNames)
                                {
                                    res += postName + "\n";
                                }
                                MessageBox.Show(res);
                            }
                            else
                            {
                                MessageBox.Show("Не найдено");
                            }
                        }
                    }
                }
            });
        }
    }
}

