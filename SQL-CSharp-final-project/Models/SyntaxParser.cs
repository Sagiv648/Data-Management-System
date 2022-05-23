using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_CSharp_final_project.Models
{
    public class Token
    {

    }


    public static class SyntaxParser
    {
        public static string[] operators = { "=", "!=", ">", ">=", "<", "<=" };
        public static string[] logicalOperators = { "or", "and" };

        public static bool operatorExists(string op)
        {
            foreach (string item in operators)
            {
                if (item == op) return true;
            }
            return false;
        }
        public static bool logicalOperatorExists(string logicalOp)
        {
            foreach (string item in logicalOperators)
            {
                if (item == logicalOp) return true;
            }
            return false;
        }

        // Grade<=55
        //Token column name
        //Token operator
        //Token value

    }
}
