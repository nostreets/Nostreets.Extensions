using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace NostreetsExtensions.Extend.Command
{
    public static class Command
    {

        /// <summary>
        /// Runs the power shell command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        public static void RunPowerShellCommand(this string command, params string[] parameters)
        {
            string script = "Set-ExecutionPolicy -Scope Process -ExecutionPolicy Unrestricted; Get-ExecutionPolicy"; // the second command to know the ExecutionPolicy level

            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddScript(script);
                var someResult = powershell.Invoke();

                powershell.AddCommand(command);
                powershell.AddParameters(parameters);
                var results = powershell.Invoke();
            }
        }



    }
}