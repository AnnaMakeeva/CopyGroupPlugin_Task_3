using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyGroupPlugin_Task_3
{
    [Transaction(TransactionMode.Manual)]
    public class CopyGroupPlugin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
             try
             {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                GroupPickFilter groupPickFilter = new GroupPickFilter();

                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element,  "Выбeрите группу oбъектов");
                Element element = doc.GetElement(reference);
                Group group = element as Group;
                XYZ groupCenter = GetElementCenter(group);
                Room room = GetRoomByPoint(doc, groupCenter);
                XYZ roomCenter = GetElementCenter(room);
                XYZ offset = groupCenter - roomCenter;

                XYZ point = uiDoc.Selection.PickPoint("Выберите точку");
                Room room2 = GetRoomByPoint(doc, point);
                XYZ room2Center = GetElementCenter(room2);
                XYZ offset2 =  offset + room2Center ;


                Transaction transaction = new Transaction(doc);
                transaction.Start("Копирование группы объектов");
                doc.Create.PlaceGroup(offset2, group.GroupType);
                transaction.Commit();
             }
              catch (Autodesk.Revit.Exceptions.OperationCanceledException)
             {
                return Result.Cancelled;
             }
              catch(Exception ex)
             {
                message=ex.Message;
                return Result.Failed;

             }

            return Result.Succeeded;
        }

        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ bounding = element.get_BoundingBox(null);
            return (bounding.Max+bounding.Min)/2;
        }

        public Room GetRoomByPoint(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            foreach(Element e in collector)
            {
                Room room = e as Room;

                if(room!=null)
                {
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }

            }
            return null;
        }
    }

    public class GroupPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
                return true;
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
        
    
}
