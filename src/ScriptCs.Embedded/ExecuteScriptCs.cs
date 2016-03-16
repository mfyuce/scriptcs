/////Original Gist from https://gist.github.com/akimboyko/5754346
namespace ScriptCs.Embedded
{
    using Common.Logging;
    using ScriptCs.Contracts;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    /// <summary>
    /// 
    /// </summary>
    public class ExecuteScriptCs
    {
        // dependencies
        private readonly Common.Logging.ILog logger;
        private readonly IFileSystem fileSystem;
        private readonly IPackageAssemblyResolver packageAssemblyResolver;
        private readonly IPackageInstaller packageInstaller;
        private readonly IScriptPackResolver scriptPackResolver;
        private readonly IScriptExecutor scriptExecutor;
        private IFilePreProcessor filePreprocessor;

        public ExecuteScriptCs(Common.Logging.ILog logger, IFileSystem fileSystem,
                                IPackageAssemblyResolver packageAssemblyResolver,
                                IPackageInstaller packageInstaller, IScriptPackResolver scriptPackResolver,
                                IScriptExecutor scriptExecutor,
                                IFilePreProcessor filePreprocessor)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.packageAssemblyResolver = packageAssemblyResolver;
            this.packageInstaller = packageInstaller;
            this.scriptPackResolver = scriptPackResolver;
            this.scriptExecutor = scriptExecutor;
            this.filePreprocessor = filePreprocessor;
        }

        // run script from file
        public ScriptResult RunText(string script)
        {
            // preserve current directory
            var previousCurrentDirectory = Environment.CurrentDirectory;

            try
            {
                // set directory to where script is
                // required to find NuGet dependencies
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // prepare NuGet dependencies, download them if required
                var nuGetReferences = PreparePackages(
                                                Environment.CurrentDirectory,
                                                fileSystem, packageAssemblyResolver,
                                                packageInstaller, logger.Info);

                // get script packs: not fully tested yet        
                var scriptPacks = scriptPackResolver.GetPacks();

                // execute script from file
                scriptExecutor.Initialize(nuGetReferences, scriptPacks);
                var res = filePreprocessor.ProcessScript(script);
               return  scriptExecutor.ExecuteScript(res.Code);
            }
            finally
            {
                // restore current directory
                Environment.CurrentDirectory = previousCurrentDirectory;
            }
        }

        // run script from file
        public ScriptResult RunFile(string scriptPath)
        {
            // preserve current directory
            var previousCurrentDirectory = Environment.CurrentDirectory;

            try
            {
                // set directory to where script is
                // required to find NuGet dependencies
                Environment.CurrentDirectory = Path.GetDirectoryName(scriptPath);

                // prepare NuGet dependencies, download them if required
                var nuGetReferences = PreparePackages(
                                                scriptPath,
                                                fileSystem, packageAssemblyResolver,
                                                packageInstaller, logger.Info);

                // get script packs: not fully tested yet        
                var scriptPacks = scriptPackResolver.GetPacks();

                // execute script from file
                scriptExecutor.Initialize(nuGetReferences, scriptPacks);
                var res = filePreprocessor.ProcessFile(scriptPath);
               return  scriptExecutor.ExecuteScript(res.Code);
            }
            finally
            {
                // restore current directory
                Environment.CurrentDirectory = previousCurrentDirectory;
            }
        }
        // prepare NuGet dependencies, download them if required
        private static IEnumerable<string> PreparePackages(
                                string scriptPath,
                                IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver,
                                IPackageInstaller packageInstaller, Action<string> outputCallback = null)
        {
            var workingDirectory = Path.GetDirectoryName(scriptPath);
            var binDirectory = Path.Combine(workingDirectory, fileSystem.BinFolder);

            var packages = packageAssemblyResolver.GetPackages(workingDirectory);

            packageInstaller.InstallPackages(
                                packages,
                                allowPreRelease: true);

            // current implementeation of RoslynCTP required dependencies to be in 'bin' folder
            if (!fileSystem.DirectoryExists(binDirectory))
            {
                fileSystem.CreateDirectory(binDirectory);
            }

            // copy dependencies one by one from 'packages' to 'bin'
            foreach (var assemblyName
                        in packageAssemblyResolver.GetAssemblyNames(workingDirectory))
            {
                var assemblyFileName = Path.GetFileName(assemblyName);
                var destFile = Path.Combine(binDirectory, assemblyFileName);

                var sourceFileLastWriteTime = fileSystem.GetLastWriteTime(assemblyName);
                var destFileLastWriteTime = fileSystem.GetLastWriteTime(destFile);

                if (sourceFileLastWriteTime == destFileLastWriteTime)
                {
                    outputCallback(string.Format("Skipped: '{0}' because it is already exists", assemblyName));
                }
                else
                {
                    fileSystem.Copy(assemblyName, destFile, overwrite: true);

                    if (outputCallback != null)
                    {
                        outputCallback(string.Format("Copy: '{0}' to '{1}'", assemblyName, destFile));
                    }
                }

                yield return destFile;
            }
        }
    }
}