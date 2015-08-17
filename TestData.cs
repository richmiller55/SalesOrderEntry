using System;
using System.IO;
// using System.Collections.ArrayList;
using System.Collections;
using System.Collections.Generic;
using FastLoad;
namespace SalesOrdEntry
{

    public class TestData
    {
        SalesOrder so;
        public TestData()
        {
            // ctor 
        }
        public SalesOrder get_so()
        {
            return this.so;
        }
        public void MoveFile(string fullName, string message)
        {
            string fileName = Path.GetFileName(fullName);
            string prefix = Path.GetFileNameWithoutExtension(fullName);

            DateTime now = DateTime.Now;
            string date = now.Year.ToString("0000") +
                          now.Month.ToString("00") +
                          now.Day.ToString("00");
            string time = now.Hour.ToString("00") +
                          now.Minute.ToString("00") +
                          now.Second.ToString("00");
            string newFileName = prefix + "_" + message + "_" + date + "_" + time + ".xml";
            string dumpPath = @"Z:\e10\EDI_Data\p20150817";
            if (!System.IO.File.Exists(dumpPath))
            {
                System.IO.Directory.CreateDirectory(dumpPath);
            }
            
            System.Threading.Thread.Sleep(1000);  // one second
            try
            {
                File.Move(fullName, Path.Combine(dumpPath, newFileName));
            }
            catch (Exception e)
            {
                message = e.Message;
            }
        }

    }
}