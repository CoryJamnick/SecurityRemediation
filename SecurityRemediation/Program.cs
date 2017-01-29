using System;
using System.Security.Cryptography;
using System.IO;
using System.Management;
using System.DirectoryServices.AccountManagement;

namespace SecurityRemediation
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo key;
            string wNumber, username, password = "";

            Console.WriteLine("Please enter a W number for testing: ");
            wNumber = Console.ReadLine();
            Console.WriteLine("Please enter your username: ");
            username = Console.ReadLine();
            Console.WriteLine("Please enter your password: ");
            do                                                                      // Have the user enter their password, masking it from the console as they type
            {
                key = Console.ReadKey(true);
                if(key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    Console.Write("\b");
                    if (password.Length != 0)                                       // Can't remove the last char of an empty string...
                    {
                        password = password.Remove(password.Length - 1);            // Does this work??
                    }
                }
            } while (key.Key != ConsoleKey.Enter);

            if (validateUser(username, password) == true)
            {
                Console.WriteLine("\nCredentials validated successfully\n");
                flowchartLogic(wNumber);
            }
            else
            {
                Console.WriteLine("\nCredentials could not be validated, exiting program\n");
            }


            Console.ReadKey();
        }

        public static void flowchartLogic(string wNumber)
        {
            if (agentIsInstalled(wNumber) == false)                                 // Is agent installed?
            {
                installAgent(wNumber);                                              // NO - Install it
            }

            deleteStuckTasks(wNumber);                                              // YES - Delete stuck tasks. This will run whether the agent was already installed, or if it was just completed via 'installAgent()'


        }

        public static bool validateUser(string username, string password)
        {
            password = password.Remove(password.Length - 1);                        // Remove the Enter char from the end of the string before passing it on

            PrincipalContext context = new PrincipalContext(ContextType.Domain, "medcampus", "Users", ContextOptions.Negotiate, username, password);
            Console.WriteLine("\nAttempting to validate your credentials...\n");
            bool valid = context.ValidateCredentials(username, password);

            return valid;
        }

        public static bool agentIsInstalled(string wNumber)
        {
            // If this .exe exists, the LanDesk agent is installed. Basically any file in that directory would be suitable for this File.Exists() check
            bool isRunning = File.Exists(@"\\" + wNumber + @"\c$\Program Files (x86)\LANDesk\LDClient\LANDESKAgentBootStrap.exe");   

            return isRunning;
        }

        // Waiting on Bryan Haakenson on info for interfacing with LanDesk progromatically
        public static void installAgent(string wNumber) 
        {

        }
        
        // This can probably be done by interfacing with LanDesk, for now doing it another way
        public static void deleteStuckTasks(string wNumber)
        {
            ManagementScope mScope = new ManagementScope(@"\\" + wNumber + @"\root\cimv2");

            // WMI query
            var query = new SelectQuery("SELECT * FROM Win32_process WHERE Name= 'calculator.exe'");

            // Use query to search through the ManagementScope, terminating specified processes if it finds them
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(mScope, query);
            ManagementObjectCollection collection = searcher.Get();
            foreach(ManagementObject process in collection)
            {
                process.InvokeMethod("Terminate", null);
            }
        }
    }
}