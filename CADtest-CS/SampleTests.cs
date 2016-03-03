﻿// CADtest.NET by CAD bloke. http://CADbloke.com - See License.txt for license details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
#if (ACAD2006 || ACAD2007 || ACAD2008 || ACAD2009|| ACAD2010|| ACAD2011 || ACAD2012)
    using Autodesk.AutoCAD.ApplicationServices;
    using Application = Autodesk.AutoCAD.ApplicationServices.Application;
    #else
using Autodesk.AutoCAD.ApplicationServices.Core;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
using Autodesk.AutoCAD.Geometry;

using CADtest.Helpers;
using NUnit.Framework;
using CADTestRunner;
using System.IO;
using System.Reflection;

namespace NUnitAutoCADTestRunner

{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class TestClass1 : BaseTests
    {
        [Test]
        public void Test_method_should_pass()
        {
            Assert.Pass("Test that should pass did not pass");
        }


        [Test]
        public void Test_method_should_fail()
        {
            Assert.Fail("This test was supposed to fail.");
        }

        [Test]
        public void Test_method_existing_drawing()
        {
            //Use existing drawing

            string drawingPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Drawings", "DrawingTest.dwg");
            long lineId = 526;

            Action<Database, Transaction> action1 = (db, trans) =>
            {
                ObjectId objectId;
                if (!db.TryGetObjectId(new Handle(lineId), out objectId))
                {
                    Assert.Fail("ObjectID doesn't exist");
                }
                
                Line line = trans.GetObject(objectId, OpenMode.ForWrite) as Line;

                Assert.IsNotNull(line, "Line didn't found");

                line.Erase();
            };

            Action<Database, Transaction> action2 = (db, trans) =>
            {
                //Check in another transaction if the line was erased

                ObjectId objectId;
                if (db.TryGetObjectId(new Handle(lineId), out objectId))
                {
                    Assert.IsTrue(objectId.IsErased, "Line didn't erased");
                }
            };

            ExecuteActionDWG(drawingPath, action1, action2);

        }


        [Test]
        public void Test_method_new_drawing()
        {
            //Use a new drawing

            long lineId = 0;

            Action<Database, Transaction> action1 = (db, trans) =>
            {
                Line line = new Line(new Point3d(0, 0, 0), new Point3d(100, 100, 100));

                var blockTable = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                var modelSpace = (BlockTableRecord)trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var objectId = modelSpace.AppendEntity(line);
                trans.AddNewlyCreatedDBObject(line, true);

                lineId = objectId.Handle.Value;
            };

            Action<Database, Transaction> action2 = (db, trans) =>
            {
                //Check in another transaction if the line was created

                ObjectId objectId;
                if (!db.TryGetObjectId(new Handle(lineId), out objectId))
                {
                    Assert.Fail("Line didn't created");
                }
            };

            ExecuteActionDWG(null, action1, action2);
        }

        //[Test]
        //    public void Test_method_name()
        //    {
        //// Arrange
        //      Database db = HostApplicationServices.WorkingDatabase;
        //      Document doc = Application.DocumentManager.GetDocument(db);
        //      DBText dbText = new DBText {TextString = "cat"};
        //      string testMe;
        //// Act
        //      using (doc.LockDocument())
        //      {
        //        using (db.TransactionManager.StartTransaction())
        //        {
        //          ObjectId dbTextObjectId = DbEntity.AddToModelSpace(dbText, db);
        //          dbText.TextString = "dog";

        //          DBText testText = dbTextObjectId.Open(OpenMode.ForRead, false) as DBText;
        //          testMe = testText != null ? testText.TextString : string.Empty;
        //        }
        //      }
        //// Assert
        //      StringAssert.AreEqualIgnoringCase("dog", testMe, "DBText string was not changed to \"dog\".");
        //      StringAssert.AreNotEqualIgnoringCase("cat", testMe, "DBText string was not changed.");
        //    }
    }
}