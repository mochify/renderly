using System;
using System.Collections.Generic;
using System.Reflection;

using Mochify.Simile.Core.Imaging;
using ManyConsole;

using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.NLog;

using Autofac;

namespace Renderly
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();
        private static ContainerBuilder RegisterAssemblyTypes()
        {
            // TODO do the IoC wiring here, which probably also means
            // using abstract type factories for deep object creation
            var programAssembly = Assembly.GetExecutingAssembly();
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(programAssembly).AsImplementedInterfaces();
            builder.RegisterType<ExhaustiveTemplateComparer>().As<IImageComparer>();

            return builder;
        }

        private static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            // UnhandledExceptionEventArgs.ExceptionObject can be a type of non-Exception when
            // thrown from a non C#/VB.net assembly, but by default, .NET 2.0 and newer
            // wrap the object in a RuntimWrappedException
            // The assembly property to set to disable/enable would be:
            // [assembly:RuntimeCompatibilityAttribute(WrapNonExceptionThrows = false)];
            _log.Info("Something unexpected happened! Please check for updates or file an issue on github", e.ExceptionObject as Exception);
            Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            NameValueCollection logProperties = new NameValueCollection();
            logProperties["configType"] = "FILE";
            logProperties["configFile"] = "~/NLog.config";

            LogManager.Adapter = new NLogLoggerFactoryAdapter(logProperties);

            //var containerBuilder = RegisterAssemblyTypes();
            var commands = GetCommands();
            ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
       }
    }
}
