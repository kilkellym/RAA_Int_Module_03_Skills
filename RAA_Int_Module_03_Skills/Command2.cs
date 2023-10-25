#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#endregion

namespace RAA_Int_Module_03_Skills
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 1. Select room
            Reference curRef = uiapp.ActiveUIDocument.Selection.PickObject(ObjectType.Element, "Select a room");
            Room curRoom = doc.GetElement(curRef) as Room;

            // 2. Create reference array and point list
            ReferenceArray referenceArray = new ReferenceArray();
            List<XYZ> pointList = new List<XYZ>();

            // 3. Set options and get room boundaries
            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            options.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;

            List<BoundarySegment> boundSegList = curRoom.GetBoundarySegments(options).First().ToList();

            // 4. Loop through room boundaries
            foreach (BoundarySegment curSeg in boundSegList)
            {
                // 4a. Get boundary geometry
                Curve boundCurve = curSeg.GetCurve();
                XYZ midPoint = boundCurve.Evaluate(0.25, true);

                // 4b. Check if line is vertical
                if (IsLineVertical(boundCurve) == false)
                {
                    // 5. Get boundary wall
                    Element curWall = doc.GetElement(curSeg.ElementId);

                    // 6. Add to ref and point array
                    referenceArray.Append(new Reference(curWall));
                    pointList.Add(midPoint);
                }
            }

            // 7. Create line for dimension
            XYZ point1 = pointList.First();
            XYZ point2 = pointList.Last();
            //Line dimLine = Line.CreateBound(point1, new XYZ(point2.X, point1.Y, 0));
            Line dimLine = Line.CreateBound(point1, new XYZ(point1.X, point2.Y, 0));

            // 8. Create dimension
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create dimension");
                Dimension newDim = doc.Create.NewDimension(doc.ActiveView, dimLine, referenceArray);
                t.Commit();
            }

            return Result.Succeeded;
        }

        private bool IsLineVertical(Curve curLine)
        {
            XYZ p1 = curLine.GetEndPoint(0);
            XYZ p2 = curLine.GetEndPoint(1);

            if (Math.Abs(p1.X - p2.X) < Math.Abs(p1.Y - p2.Y))
                return true;

            return false;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
    }
}
