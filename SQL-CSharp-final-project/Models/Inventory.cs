using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SQL_CSharp_final_project.Models
{
    public class Inventory
    {
        public int Id;
        public Product Product;
        public Order Order;
        public Department Department;
        public Brand Brand;
        public char Priorty;
        public int Quantity;
        
        
        
    }
}
