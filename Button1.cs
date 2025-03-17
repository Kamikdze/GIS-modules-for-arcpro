using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Linq;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Dialogs;

namespace YourNamespace
{
        internal class IncreasePolygonSizeButton : Button
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
