using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data;
using System.IO;


namespace SalesOrdEntry
{


    public class E10Lookup
    {
        string connectionString = "Persist Security Info=False; Integrated Security=true; Database=Epicor10Production; server=ITSQL";
        public E10Lookup()
        {
            // ctor
        }
        public string GetPartDescr(string PartNum)
        {
            DataContext context = new DataContext(this.connectionString);
            Table<Part> parts = context.GetTable<Part>();
            IQueryable<string> query =
                from n in parts
                where n.PartNum.Equals(PartNum)
                select n.PartDescription.ToString();

            string partDescription = query.First();
            return partDescription;
        }
        public bool IsPartInActive(string PartNum)
        {
            DataContext context = new DataContext(this.connectionString);
            Table<Part> parts = context.GetTable<Part>();
            IQueryable<string> query =
                from n in parts
                where n.PartNum.Equals(PartNum)
                select n.InActive.ToString();

            string result = query.First();
            bool q = false;
            if (result.Equals("True"))
            {
                q = true;
            }
            return q;
        }
        public int GetCustomerNum(string CustId)
        {
            DataContext context = new DataContext(this.connectionString);
            Table<Customer> customers = context.GetTable <Customer>();
            IQueryable<string> query =
                from n in customers
                where n.CustID.Equals(CustId)
                select n.CustNum.ToString();
            
            int custNum = System.Convert.ToInt32( query.First());
            return custNum;
        }
    }
}