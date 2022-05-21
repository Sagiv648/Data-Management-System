using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SQL_CSharp_final_project.Models
{
    public class Order
    {
        public int Id;
        public string Date;
        public string companyName;
        public Department Department;
        public Brand Brand;
        public Product Product;
        public int Quantity;
    }
}
