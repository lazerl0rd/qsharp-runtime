﻿using System;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Quantum.Runtime;

namespace Microsoft.Quantum.CsharpGeneration.EntryPointDriver
{
    /// <summary>
    /// Provides entry point submission to Azure Quantum.
    /// </summary>
    internal static class Azure
    {
        /// <summary>
        /// Submits the entry point to Azure Quantum.
        /// </summary>
        /// <param name="entryPoint">The entry point.</param>
        /// <param name="parseResult">The command-line parsing result.</param>
        /// <param name="settings">The submission settings.</param>
        /// <typeparam name="TIn">The entry point's argument type.</typeparam>
        /// <typeparam name="TOut">The entry point's return type.</typeparam>
        internal static async Task<int> Submit<TIn, TOut>(
            IEntryPoint<TIn, TOut> entryPoint, ParseResult parseResult, AzureSettings settings)
        {
            var machine = CreateMachine(settings);
            if (machine is null)
            {
                DisplayUnknownTargetError(settings.Target);
                return 1;
            }
            var output = await machine.ExecuteAsync(entryPoint.Info, entryPoint.CreateArgument(parseResult));
            // TODO: Provide output options and show the most frequent output by default. 
            foreach (var (result, frequency) in output.Histogram)
            {
                Console.WriteLine($"{result} (frequency = {frequency})");
            }
            return 0;
        }

        /// <summary>
        /// Creates a quantum machine based on the Azure Quantum submission settings.
        /// </summary>
        /// <param name="settings">The Azure Quantum submission settings.</param>
        /// <returns>A quantum machine.</returns>
        private static IQuantumMachine? CreateMachine(AzureSettings settings)
        {
            if (settings.Target == "quantum.simulator")
            {
                return new SimulatorMachine();
            }
            else if (!(settings.Target is null) && settings.Target.StartsWith("ionq."))
            {
                var ionQType = Type.GetType(
                    "Microsoft.Quantum.Providers.IonQ.Targets.IonQQuantumMachine, Microsoft.Quantum.Providers.IonQ",
                    throwOnError: true);
                return (IQuantumMachine)Activator.CreateInstance(
                    ionQType, settings.Target, settings.Storage, settings.ToWorkspace());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Displays an error message for attempting to use an unknown target machine.
        /// </summary>
        /// <param name="target">The target machine.</param>
        private static void DisplayUnknownTargetError(string? target)
        {
            var originalForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"The target '{target}' was not recognized.");
            Console.ForegroundColor = originalForeground;
        }
    }
    
    /// <summary>
    /// Settings for a submission to Azure Quantum.
    /// </summary>
    internal sealed class AzureSettings
    {
        /// <summary>
        /// The target device ID.
        /// </summary>
        public string? Target { get; set; }
        
        /// <summary>
        /// The Azure subscription ID.
        /// </summary>
        public string? Subscription { get; set; }
        
        /// <summary>
        /// The Azure resource group name.
        /// </summary>
        public string? ResourceGroup { get; set; }
        
        /// <summary>
        /// The Azure workspace name.
        /// </summary>
        public string? Workspace { get; set; }
        
        /// <summary>
        /// The Azure storage account connection string.
        /// </summary>
        public string? Storage { get; set; }

        /// <summary>
        /// Converts these settings into a Microsoft.Azure.Quantum.Workspace object.
        /// </summary>
        /// <returns>The workspace object corresponding to these settings.</returns>
        internal object ToWorkspace()
        {
            var workspaceType = Type.GetType(
                "Microsoft.Azure.Quantum.Workspace, Microsoft.Azure.Quantum.Client", throwOnError: true);
            var tokenCredentialType = Type.GetType("Azure.Core.TokenCredential, Azure.Core", throwOnError: true);
            var constructor = workspaceType.GetConstructor(new[]
                { typeof(string), typeof(string), typeof(string), tokenCredentialType, typeof(Uri) });
            return constructor.Invoke(new object?[] { Subscription, ResourceGroup, Workspace, null, null });
        }
    }
}