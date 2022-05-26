using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

using SQL_CSharp_final_project.Shell;

namespace SQL_CSharp_final_project
{
    class Program
    {
        static void Main(string[] args)
        {


            //TODO: Final task -> Organize and finalize the project and prepare it for submission.

            Shell.Shell.shellInit();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Shell.Shell.buffer = Shell.Shell.shellInput(Shell.Shell.location);

                if (Shell.Shell.buffer == "quit") break;

                Shell.Shell.parseCommand(Shell.Shell.buffer);
            }
            
        }
    }
}
