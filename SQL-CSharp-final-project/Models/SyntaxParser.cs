using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_CSharp_final_project.Models
{
    public static class SyntaxParser
    {
        public static string[] operators = { "=", "!=", ">", ">=", "<", "<=" };
        

        public static bool operatorExists(string op)
        {
            foreach (string item in operators)
            {
                if (item == op) return true;
            }
            return false;
        }
       


    }
}
