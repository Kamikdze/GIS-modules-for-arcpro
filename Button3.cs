using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
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
    internal class Button3 : Button
    {
        protected async override void OnClick()
        {
            await QueuedTask.Run(async () =>
            {
                try
                {
                    // Получаем активный слой с объектами
                    var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();

                    if (featureLayer == null)
                    {
                        MessageBox.Show("Нет активного слоя с объектами.");
                        return;
                    }

                    // Получаем выбранные объекты
                    var selection = featureLayer.GetSelection();
                    if (selection.GetCount() == 0)
                    {
                        MessageBox.Show("Нет выбранных объектов.");
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
                                    double scaleFactor = 1 / Math.Sqrt(3); // Масштабирование для уменьшения площади в 3 раза
                                    var scaledPolygon = GeometryEngine.Instance.Scale(originalPolygon, originalPolygon.Extent.Center, scaleFactor, scaleFactor);

                                    // 2. Смещение центра по оси Y
                                    double offsetY = 200000; // Задайте величину смещения
                                    var shiftedPolygon = GeometryEngine.Instance.Move(scaledPolygon, 0, offsetY);

                                    // 3. Запуск транзакции редактирования для добавления нового объекта
                                    var editOperation = new EditOperation();
                                    editOperation.Name = "Создание уменьшенного и смещенного полигона";
                                    editOperation.Create(featureLayer, shiftedPolygon);
                                    editOperation.Execute();
                                }
                            }
                        }
                    }

                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            });


        }
        private Polyline ScaleAndMovePolyline(Polyline polyline, double scaleFactor, double shiftY)
        {
            // Получаем коллекцию вершин полилинии
            var points = polyline.Points;

            if (points.Count < 2)
            {
                MessageBox.Show("Полилиния содержит недостаточно вершин.");
                return polyline; // Возвращаем оригинальную геометрию, если недостаточно вершин
            }

            // Находим центральную точку полилинии (среднее по всем координатам)
            double centerX = 0;
            double centerY = 0;
            foreach (var point in points)
            {
                centerX += point.X;
                centerY += point.Y;
            }
            centerX /= points.Count;
            centerY /= points.Count;

            var centerPoint = MapPointBuilder.CreateMapPoint(centerX, centerY, polyline.SpatialReference);

            // Масштабируем каждую точку относительно центральной точки
            var newPoints = new List<MapPoint>();
            foreach (var point in points)
            {
                // Вычисляем вектор от центральной точки к текущей
                var vectorX = point.X - centerX;
                var vectorY = point.Y - centerY;

                // Масштабируем этот вектор
                var newPointX = centerX + vectorX * scaleFactor;
                var newPointY = centerY + vectorY * scaleFactor + shiftY; // Добавляем сдвиг вниз по Y

                // Добавляем новую точку
                var newPoint = MapPointBuilder.CreateMapPoint(newPointX, newPointY, polyline.SpatialReference);
                newPoints.Add(newPoint);
            }

            // Создаем новую полилинию с увеличенной длиной и смещением вниз
            return PolylineBuilder.CreatePolyline(newPoints);
        }

    }
}
 

