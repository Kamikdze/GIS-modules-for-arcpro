using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppModule6
{
    internal class Button2 : Button
    {
        protected async override void OnClick()
        {
            MessageBox.Show("fuck");
            await QueuedTask.Run(async () =>
            {
                try
                {
                    // Получаем активный слой с объектами
                    var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();

                    if (featureLayer == null)
                    {

                        return;
                    }

                    // Получаем выбранные объекты
                    var selection = featureLayer.GetSelection();
                    if (selection.GetCount() == 0)
                    {

                        return;
                    }

                    // Получаем первый выбранный объект
                    var objectID = selection.GetObjectIDs().FirstOrDefault();

                    using (var rowCursor = featureLayer.Search(new QueryFilter { ObjectIDs = new[] { objectID } }))
                    {
                        if (rowCursor.MoveNext())
                        {
                            using (var feature = rowCursor.Current as Feature)
                            {
                                if (feature != null)
                                {
                                    // Получаем геометрию объекта
                                    var originalPolygon = feature.GetShape() as Polygon;

                                    // 1. Уменьшение площади в 3 раза
                                    double scaleFactor = 2; // Масштабирование для уменьшения площади в 3 раза
                                    var scaledPolygon = GeometryEngine.Instance.Scale(originalPolygon, originalPolygon.Extent.Center, scaleFactor, scaleFactor);

                                    // 3. Запуск транзакции редактирования для добавления нового объекта
                                    var editOperation = new EditOperation();
                                    editOperation.Name = "Создание уменьшенного и смещенного полигона";
                                    editOperation.Create(featureLayer, scaledPolygon);
                                    editOperation.Execute();
                                }
                            }
                        }
                    }
                }
                catch
                {

                }

            });
        }
    }
}
