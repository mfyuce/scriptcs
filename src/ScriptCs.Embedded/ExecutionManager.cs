using Autofac;
using System;
using System.Text;

namespace ScriptCs.Embedded
{
    public static class ExecutionManager
    {
        private static ILifetimeScope scope = null;
        private static ContainerBuilder builder = null;
        private static IContainer container = null;

        public static void DestroyRuntime()
        {
            scope.Dispose();
            container.Dispose();

            builder = null;
            scope = null;
            container = null;
        }
        public static void InitRuntime()
        {
            //const string scriptPath = @".\EmbededScriptCs.csx";

            // AutoFac container
            builder = new ContainerBuilder();

            // register ScripotCs specific module
            builder.RegisterModule(new ScriptModule());

            container = builder.Build();
            scope = container.BeginLifetimeScope();
        }
        public static object ExecuteFile(string file)
        {
            //const string scriptPath = @".\EmbededScriptCs.csx";


            var logger = scope.Resolve<Common.Logging.ILog>();
            var executeScriptCs = scope.Resolve<ExecuteScriptCs>();

            try
            {
                var result = executeScriptCs.RunFile(file);
                if (result.ExecuteExceptionInfo != null)
                {
                    logger.Error((result.ExecuteExceptionInfo.SourceException));
                    throw result.ExecuteExceptionInfo.SourceException;
                }
                if (result.CompileExceptionInfo != null)
                {
                    logger.Error((result.CompileExceptionInfo.SourceException));
                    throw result.CompileExceptionInfo.SourceException;
                }
                return result.ReturnValue;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }
        public static object ExecuteText(string text)
        {
            //const string scriptPath = @".\EmbededScriptCs.csx";


            var logger = scope.Resolve<Common.Logging.ILog>();
            var executeScriptCs = scope.Resolve<ExecuteScriptCs>();

            try
            {
                var result = executeScriptCs.RunText(text);
                if (result.ExecuteExceptionInfo != null)
                {
                    logger.Error((result.ExecuteExceptionInfo.SourceException));
                    throw result.ExecuteExceptionInfo.SourceException;
                }
                if (result.CompileExceptionInfo != null)
                {
                    logger.Error((result.CompileExceptionInfo.SourceException));
                    throw result.CompileExceptionInfo.SourceException;
                }
                return result.ReturnValue;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }
        public static string GetExceptionDetails(Exception exception)
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