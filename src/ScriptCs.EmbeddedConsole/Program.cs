using Autofac;
using System;
using System.Text;

namespace ScriptCs.Embedded
{
    public static class Program
    {
        public static void Main()
        {
            const string scriptPath = @".\EmbededScriptCs.csx";

            ExecutionManager.InitRuntime();
            try
            {
                var result = ExecutionManager.ExecuteFile(scriptPath);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ExecutionManager.GetExceptionDetails(ex));
            }
            finally
            {
                ExecutionManager.DestroyRuntime();
            }
        }
    }
}