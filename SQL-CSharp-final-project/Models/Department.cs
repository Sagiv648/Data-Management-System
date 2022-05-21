using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SQL_CSharp_final_project.Models
{
    public class Department
    {
        public int Id;
        public string Name;
        public List<Product> Products;
        
        
    }
}
