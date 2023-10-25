#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 1. Get model lines
            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            collector.OfClass(typeof(CurveElement));

            // 2. Create reference array and point list
            ReferenceArray referenceArray = new ReferenceArray();
            List<XYZ> pointList = new List<XYZ>();

            // 3. Loop through lines
            foreach (ModelLine curLine in collector)
            {
                // 3a. Get midpoint of line
                Curve curve = curLine.GeometryCurve;
                XYZ midPoint = curve.Evaluate(0.75, true);

                // 7. Check if line is vertical
                if (IsLineVertical(curve) == false)
                    continue;

                // 3b. Add lines to ref array
                referenceArray.Append(new Reference(curLine));

                // 3c. Add midpoint to list
                pointList.Add(midPoint);

            }

            // 4. Order list left to right
            List<XYZ> sortedList = pointList.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            XYZ point1 = sortedList.First();
            XYZ point2 = sortedList.Last();

            // 5. Create line for dimension
            //Line dimLine = Line.CreateBound(point1, point2);
            Line dimLine = Line.CreateBound(point1, new XYZ(point2.X, point1.Y, 0));

            // 6. Create dimension
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
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
