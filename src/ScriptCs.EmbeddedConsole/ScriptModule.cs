namespace ScriptCs.Embedded
{
    // AutoFac configuration
    using Autofac;
    using Common.Logging;
    using Common.Logging.Simple;
    using NuGet;
    using ScriptCs;
    using ScriptCs.Contracts;
    using ScriptCs.Engine.Roslyn;
    using ScriptCs.Hosting.Package;
    using System.Collections.Generic;
    using System.Linq;
    public class ScriptModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<FileSystem>()
                .As<ScriptCs.Contracts.IFileSystem>()
                .SingleInstance();

            builder
                .RegisterType<ConsoleOutLogger>()
                .As<Common.Logging.ILog>()
                .SingleInstance()
                .WithParameter("logName", @"Custom ScriptCs from C#")
                .WithParameter("logLevel", Common.Logging.LogLevel.All)
                .WithParameter("showLevel", true)
                .WithParameter("showDateTime", true)
                .WithParameter("showLogName", true)
                .WithParameter("dateTimeFormat", @"yyyy-mm-dd hh:mm:ss");

            builder
                .RegisterType<ScriptCs.FilePreProcessor>()
                .As<IFilePreProcessor>()
                .SingleInstance()
                .UsingConstructor(new System.Type[] { typeof(ScriptCs.Contracts.IFileSystem), typeof(Common.Logging.ILog), typeof(IEnumerable<ILineProcessor>) });

            builder
                .RegisterType<ScriptCs.FilePreProcessor>()
                .As<IFilePreProcessor>()
                .SingleInstance()
                .UsingConstructor(new System.Type[] { typeof(ScriptCs.Contracts.IFileSystem), typeof(Common.Logging.ILog), typeof(IEnumerable<ILineProcessor>) });

            builder
                .RegisterType<ScriptCs.ScriptHostFactory>()
                .As<IScriptHostFactory>()
                .SingleInstance();

            builder
                .RegisterType<CSharpReplEngine>()
                .As<IScriptEngine>();

            builder
                .RegisterType<ScriptExecutor>()
                .As<IScriptExecutor>()
                .UsingConstructor(new System.Type[] { typeof(ScriptCs.Contracts.IFileSystem), typeof(IFilePreProcessor), typeof(IScriptEngine), typeof(Common.Logging.ILog) });


            builder
                .RegisterType<NugetInstallationProvider>()
                .As<IInstallationProvider>()
                .SingleInstance()
                .UsingConstructor(new System.Type[] { typeof(ScriptCs.Contracts.IFileSystem), typeof(Common.Logging.ILog) });

            builder
                .RegisterType<PackageAssemblyResolver>()
                .As<IPackageAssemblyResolver>()
                .SingleInstance()
                .UsingConstructor(new System.Type[] { typeof(ScriptCs.Contracts.IFileSystem), typeof(IPackageContainer), typeof(Common.Logging.ILog), typeof(IAssemblyUtility) });

            builder
                .RegisterType<DefaultLogProvider>()
                .As<ILogProvider>()
                .SingleInstance();

            builder
                .RegisterType<AssemblyUtility>()
                .As<IAssemblyUtility>()
                .SingleInstance();

            builder
                .RegisterType<PackageContainer>()
                .As<IPackageContainer>()
                .SingleInstance()
                .UsingConstructor(new System.Type[] { typeof(ScriptCs.Contracts.IFileSystem), typeof(Common.Logging.ILog) });

            builder
                .RegisterType<PackageInstaller>()
                .As<IPackageInstaller>()
                .SingleInstance()
                .UsingConstructor(new System.Type[] { typeof(IInstallationProvider), typeof(Common.Logging.ILog) });

            builder
                .RegisterType<PackageManager>()
                .As<IPackageManager>()
                .SingleInstance();

            builder
                .RegisterType<ScriptPackResolver>()
                .As<IScriptPackResolver>()
                .SingleInstance();

            builder
                .RegisterType<ExecuteScriptCs>();

            RegisterLineProcessors(builder);
        }


        private System.Type[] GetFilePreProcessors()
        {
            return    new System.Type[]
            {
                    typeof( UsingLineProcessor),
                    typeof( ReferenceLineProcessor ),
                    typeof( LoadLineProcessor ),
                    typeof( ShebangLineProcessor )
            }; 
        }
        protected void RegisterLineProcessors(ContainerBuilder builder)
        {
            var processorList = (GetFilePreProcessors() as IEnumerable<System.Type> ?? Enumerable.Empty<System.Type>()).ToArray();

            var loadProcessorType = processorList
                .FirstOrDefault(x => typeof(ILoadLineProcessor).IsAssignableFrom(x))
                ?? typeof(LoadLineProcessor);

            var usingProcessorType = processorList
                .FirstOrDefault(x => typeof(IUsingLineProcessor).IsAssignableFrom(x))
                ?? typeof(UsingLineProcessor);

            var referenceProcessorType = processorList
                .FirstOrDefault(x => typeof(IReferenceLineProcessor).IsAssignableFrom(x))
                ?? typeof(ReferenceLineProcessor);

            var shebangProcessorType = processorList
                .FirstOrDefault(x => typeof(IShebangLineProcessor).IsAssignableFrom(x))
                ?? typeof(ShebangLineProcessor);

            var processorArray = new[] { loadProcessorType, usingProcessorType, referenceProcessorType, shebangProcessorType }
                .Union(processorList).ToArray();

            builder.RegisterTypes(processorArray).As<ILineProcessor>();
        }
    }
}