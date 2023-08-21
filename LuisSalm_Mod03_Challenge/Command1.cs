#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Data;

#endregion

namespace LuisSalm_Mod03_Challenge
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

            // Paths to CSV files
            string furnSetsPath = "C:\\Users\\luis.salmeron\\source\\repos\\LuisSalm_Mod03_Challenge\\RAB_Module 03_Furniture Sets.csv";
            string furnTypesPath = "C:\\Users\\luis.salmeron\\source\\repos\\LuisSalm_Mod03_Challenge\\RAB_Module 03_Furniture Types.csv";

            // Lists of string arrays and classes for CSV data
            List<string[]> furnSetsData = CSVRead(furnSetsPath);
            List<FurnitureSets> fsList = new List<FurnitureSets>();
            List<string[]> furnTypesData = CSVRead(furnTypesPath);
            List<FurnitureSetTypes> ftList = new List<FurnitureSetTypes>();

            // Add CSV data to lists of class objects
            foreach (string[] furnTypes in furnTypesData)
            {
                FurnitureSetTypes ftListAdd = new FurnitureSetTypes(furnTypes);
                ftList.Add(ftListAdd);
            }
            foreach (string[] furnSets in furnSetsData)
            {
                FurnitureSets fsListAdd = new FurnitureSets(furnSets);

                string[] Temp = new string[furnSets.Length - 2];
                for (int i = 2; i < furnSets.Length; i++)
                {
                    if (furnSets[i].StartsWith("\""))
                    {
                        furnSets[i] = furnSets[i].Remove(0, 1);
                    }
                    if (furnSets[i].EndsWith("\""))
                    {
                        furnSets[i] = furnSets[i].Remove(furnSets[i].Length - 1);
                    }
                    Temp[i - 2] = furnSets[i];
                }
                fsListAdd.Furn = Temp;
                fsList.Add(fsListAdd);
            }

            // Find Rooms
	        FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);

            Transaction t = new Transaction(doc);
            t.Start("Add Furniture to Rooms");

            foreach (SpatialElement room in collector)
            {
                // Room location
                LocationPoint loc = room.Location as LocationPoint;
                XYZ roomPoint = loc.Point as XYZ;
                
                // Cross-reference the Furniture Set of the room with the CSV data
                string param = Utils.GetParameterValueAsString(room, "Furniture Set");
                foreach (FurnitureSets fs in fsList)
                {
                    if (fs.FurnitureSet == param)
                    {
                        int fCount = 0;
                        foreach (string f in fs.Furn)
                        {
                            foreach (FurnitureSetTypes type in ftList)
                            {
                                if (type.FName == f)
                                {
                                    // Set the furniture in the room
                                    FamilySymbol curFS = GetFamilySymbolByName(doc, type.RFamName, type.RFamType);
                                    curFS.Activate();
                                    FamilyInstance curFI = doc.Create.NewFamilyInstance(roomPoint, curFS, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                                    fCount++;
                                }
                            }
                        }
                        Utils.SetParameterValue(room, "Furniture Count", fCount.ToString());
                    }
                }
            }
            t.Commit();
            t.Dispose();
            return Result.Succeeded;
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
        internal List<string[]> CSVRead(string path)
        {
            List<string[]> CsvData = new List<string[]>();

            // Read CSV files and save the lines as different list items
            string[] CsvArray = System.IO.File.ReadAllLines(path);

            // Loop through file data and put into list
            foreach (string CsvString in CsvArray)
            {
                string[] rowArray = CsvString.Split(',');
                CsvData.Add(rowArray);
            }

            // Remove header row
            CsvData.RemoveAt(0);

            return CsvData;
        }
        internal FamilySymbol GetFamilySymbolByName(Document doc, string famName, string fsName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(FamilySymbol));

            foreach (FamilySymbol fs in collector)
            {
                if (fs.Name == fsName && fs.FamilyName == famName)
                    return fs;
            }

            return null;
        }
    }
    public class FurnitureSetTypes
    {
        public string FName { get; set; }
        public string RFamName { get; set; }
        public string RFamType { get; set; }
        public FurnitureSetTypes(string[] fTypes)
        {
            FName = fTypes[0];
            RFamName = fTypes[1];
            RFamType = fTypes[2];
        }
    }
    public class FurnitureSets
    {
        public string FurnitureSet { get; set; }
        public string RoomType { get; set; }
        public string[] Furn { get; set;  }
        public FurnitureSets(string[] fSets)
        {
            FurnitureSet = fSets[0];
            RoomType = fSets[1];
            //List<string> Furn = new List<string>();
            //for (int i = 2; i < fSets.Length;i++)
            //{
            //    //Remove quotation marks from Included Furniture
            //    if (fSets[i].StartsWith("\""))
            //    {
            //        fSets[i] = fSets[i].Remove(0, 1);
            //    }
            //    if (fSets[i].EndsWith("\""))
            //    {
            //        fSets[i] = fSets[i].Remove(fSets[i].Length - 1);
            //    }
            //    //Add furniture to list of Included furniture
            //    Furn.Add(fSets[i]);
            //}
        }
    }
}