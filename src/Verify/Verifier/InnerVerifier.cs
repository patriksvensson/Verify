﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace VerifyTests
{
    /// <summary>
    /// Not for public use.
    /// </summary>
    public partial class InnerVerifier :
        IDisposable
    {
        string directory;
        string testName;
        Assembly assembly;
        VerifySettings settings;
        internal static Func<string, Exception> exceptionBuilder = null!;

        public static void Init(Func<string, Exception> exceptionBuilder)
        {
            InnerVerifier.exceptionBuilder = exceptionBuilder;
        }

        public InnerVerifier(string testName, string sourceFile, Assembly assembly, VerifySettings settings)
        {
            directory = VerifierSettings.DeriveDirectory(sourceFile, assembly);
            this.testName = testName;
            this.assembly = assembly;
            this.settings = settings;
            CounterContext.Start();
        }

        FilePair GetFileNames(string extension, Namer namer)
        {
            return FileNameBuilder.GetFileNames(extension, namer, directory, testName, assembly);
        }

        FilePair GetFileNames(string extension, Namer namer, string suffix)
        {
            return FileNameBuilder.GetFileNames(extension, suffix, namer, directory, testName, assembly);
        }

        public void Dispose()
        {
            CounterContext.Stop();
        }

        async Task HandleResults(List<ResultBuilder> results, VerifyEngine engine)
        {
            async Task HandleBuilder(ResultBuilder item, FilePair file)
            {
                var result = await item.GetResult(file);
                engine.HandleCompareResult(result, file);
            }

            if (results.Count == 1)
            {
                var item = results[0];
                var file = GetFileNames(item.Extension, settings.Namer);
                await HandleBuilder(item, file);
                return;
            }

            for (var index = 0; index < results.Count; index++)
            {
                var item = results[index];
                var file = GetFileNames(item.Extension, settings.Namer, $"{index:D2}");
                await HandleBuilder(item, file);
            }
        }
    }
}