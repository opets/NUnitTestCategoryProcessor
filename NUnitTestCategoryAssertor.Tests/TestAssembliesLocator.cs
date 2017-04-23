using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace NUnitTestCategoryAssertor.Tests {

	internal static class TestAssembliesLocator {

		private const string TestAssemblies = "test-assemblies";

		private static readonly Lazy<string> m_testAssembliesPath = new Lazy<string>(
				SearchForTestAssembliesPath,
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		public static string Path {
			get { return m_testAssembliesPath.Value; }
		}

		private static string SearchForTestAssembliesPath() {

			Assembly assembly = typeof( TestAssembliesLocator ).Assembly;
			FileInfo assemblyFile = new FileInfo( assembly.Location );

			DirectoryInfo dir = CheckParentForTestAssemblies( assemblyFile.Directory );
			return dir.FullName;
		}

		private static DirectoryInfo CheckParentForTestAssemblies( DirectoryInfo dir ) {

			DirectoryInfo parent = dir.Parent;
			if( parent == null ) {
				throw new DirectoryNotFoundException( $"{ TestAssemblies } directory not found" );
			}

			string path = System.IO.Path.Combine( parent.FullName, TestAssemblies );

			DirectoryInfo testAssembliesDir = new DirectoryInfo( path );
			if( testAssembliesDir.Exists ) {
				return testAssembliesDir;
			}

			return CheckParentForTestAssemblies( parent );
		}
	}
}
