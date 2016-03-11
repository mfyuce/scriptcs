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

            // AutoFac container
            var builder = new ContainerBuilder();

            // register ScripotCs specific module
            builder.RegisterModule(new ScriptModule());

            using (var container = builder.Build())
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var logger = scope.Resolve<Common.Logging.ILog>();
                    var executeScriptCs = scope.Resolve<ExecuteScriptCs>();

                    try
                    {
                        var result = executeScriptCs.Run(scriptPath);
                        if (result.ExecuteExceptionInfo != null)
                        {
                            logger.Error((result.ExecuteExceptionInfo.SourceException));
                            throw result.ExecuteExceptionInfo.SourceException;
                        }
                        if (result.CompileExceptionInfo!= null)
                        {
                            logger.Error((result.CompileExceptionInfo.SourceException));
                            throw result.CompileExceptionInfo.SourceException;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        throw;
                    }
                }
            }
        }

        private static string GetExceptionDetails(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            GetExceptionDetails(exception, sb);
            return sb.ToString();
        }
        private static void GetExceptionDetails(Exception exception, StringBuilder sb)
        {
            if (exception != null)
            {
                sb.Append(exception.Message);
                sb.Append("\r\n");
                sb.Append("\r\n");
                sb.Append(exception.StackTrace);
                sb.Append("\r\n");
                sb.Append("-----------------------------------------");
                sb.Append("\r\n");
                GetExceptionDetails(exception.InnerException, sb);
            }
        }
    }
}