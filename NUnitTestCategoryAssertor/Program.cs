using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NDesk.Options;
using NUnitTestCategoryAssertor.Scanner;

namespace NUnitTestCategoryAssertor {

	internal static class Program {

		private static readonly char[] ArgumentsSeperator = new char[] { ',' };

		private sealed class Arguments {

			public DirectoryInfo AssembliesPath = new DirectoryInfo( Environment.CurrentDirectory );
			public ISet<string> ExcludedAssemblies = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

			public ISet<string> RequiredCategories = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

			public ISet<string> ProhibitedAssemblyCategories = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

		}

		private static readonly OptionSet<Arguments> m_argumentParser = new OptionSet<Arguments>()
			.Add(
				"assembliesPath=",
				"The input path of assemblies",
				( args, v ) => args.AssembliesPath = new DirectoryInfo( v )
			)
			.Add(
				"excludedAssembly=",
				"The filename of an assembly to exclude",
				( args, v ) => {
					string[] assemblies = v.Split( ArgumentsSeperator, StringSplitOptions.RemoveEmptyEntries );
					args.ExcludedAssemblies.UnionWith( assemblies );
				}
			)
			.Add(
				"category=",
				"One of the categories to assert",
				( args, v ) => {
					string[] categories = v.Split( ArgumentsSeperator, StringSplitOptions.RemoveEmptyEntries );
					args.RequiredCategories.UnionWith( categories );
				}
			)
			.Add(
				"prohibitedAssemblyCategories=",
				"Categories which are not allowed at the assembly level",
				( args, v ) => {
					string[] categories = v.Split( ArgumentsSeperator, StringSplitOptions.RemoveEmptyEntries );
					args.ProhibitedAssemblyCategories.UnionWith( categories );
				}
			);

		internal static int Main( string[] arguments ) {

			if( arguments.Length == 0 ) {

				m_argumentParser.WriteOptionDescriptions( Console.Out );
				return -1;

			} else if( arguments.Length == 1 ) {

				switch( arguments[ 0 ] ) {

					case "help":
					case "/help":
					case "-help":
					case "--help":

					case "?":
					case "/?":

						m_argumentParser.WriteOptionDescriptions( Console.Out );
						return 0;
				}
			}

			Arguments args;
			try {
				List<string> extras = new List<string>();
				args = m_argumentParser.Parse( arguments, out extras );

				if( extras.Count > 0 ) {

					Console.Error.Write( "Invalid arguments: " );
					Console.Error.WriteLine( String.Join( " ", extras ) );

					Console.WriteLine( "{0}Usage:", Environment.NewLine );
					m_argumentParser.WriteOptionDescriptions( Console.Out );
					return -3;
				}

			} catch( Exception e ) {

				Console.Error.Write( "Invalid arguments: " );
				Console.Error.WriteLine( e.Message );

				Console.WriteLine( "{0}Usage:", Environment.NewLine );
				m_argumentParser.WriteOptionDescriptions( Console.Out );
				return -2;
			}

			try {
				int violations = Run( args );
				return violations;

			} catch( UnexpectedNUnitVersionException err ) {
				Console.Error.WriteLine( err.Message );
				return -102;

			} catch( ReflectionTypeLoadException err ) {

				Console.Error.WriteLine( err.Message );
				Console.Error.WriteLine();

				foreach( Exception loaderErr in err.LoaderExceptions ) {

					Console.Error.WriteLine( loaderErr );
					Console.Error.WriteLine();
				}

				return -101;

			} catch( Exception err ) {
				Console.Error.WriteLine( err );
				return -100;
			}
		}

		private static int Run( Arguments args ) {

			string path = args.AssembliesPath.FullName;
			SetupAssemblyResolver( path );

			Console.WriteLine( "Loading assemblies from '{0}'", path );

			IEnumerable<Assembly> assemblies = args.AssembliesPath
				.EnumerateFiles( "*.dll", SearchOption.TopDirectoryOnly )
				.Where( file => !args.ExcludedAssemblies.Contains( file.Name ) )
				.Select( GetAssemblyOrNull )
				.Where( ass => ass != null )
				.Where( NUnitFrameworkReferenceChecker.ReferencesNUnitFramework );

			int violations = 0;

			TestAssemblyScanner scanner = new TestAssemblyScanner(
					args.RequiredCategories,
					args.ProhibitedAssemblyCategories
				);

			using( IndentedTextWriter writer = new IndentedTextWriter( Console.Error, "\t" ) ) {

				foreach( Assembly assembly in assemblies ) {

					TestAssembly testAssembly = scanner.Scan( assembly );
					violations += Report( testAssembly, writer );
				}
			}

			return violations;
		}

		private static int Report(
				TestAssembly assembly,
				IndentedTextWriter writer
			) {

			int violations = 0;

			if( assembly.Violations.Count > 0 ) {
				writer.WriteLine( "Assembly: {0}", assembly.Name );
				writer.Indent++;

				foreach( string violation in assembly.Violations ) {
					writer.WriteLine( violation );
				}

				writer.Indent--;

				violations += assembly.Violations.Count;
			}

			foreach( TestFixture fixture in assembly.Fixtures ) {

				if( fixture.Violations.Count > 0 ) {

					if( violations == 0 ) {

						writer.WriteLine( "Assembly: {0}", assembly.Name );
						writer.WriteLine();
						writer.Indent++;
					}

					writer.WriteLine( "Fixture: {0}", fixture.Name );
					writer.WriteLine();
					writer.Indent++;

					foreach( TestViolation violation in fixture.Violations ) {

						writer.WriteLine( "Test: {0}", violation.Name );
						writer.Indent++;
						writer.WriteLine( violation.Message );
						writer.Indent--;
						writer.WriteLine();
					}

					writer.Indent--;

					violations += fixture.Violations.Count;
				}
			}

			if( violations > 0 ) {
				writer.Indent--;
				writer.WriteLine();
			}

			return violations;
		}

		private static void SetupAssemblyResolver( string path ) {

			AssemblyResolver resolver = new AssemblyResolver( path );

			AppDomain.CurrentDomain.AssemblyResolve +=
				delegate ( object senderJunk, ResolveEventArgs args ) {

					Assembly assemblyObj = resolver.Resolve( args.Name );
					return assemblyObj;
				};
		}

		private static Assembly GetAssemblyOrNull( FileInfo file ) {

			string fileName = file.FullName;
			try {
				Assembly assembly = Assembly.LoadFile( fileName );
				return assembly;

			} catch( BadImageFormatException ) {
				Console.Error.WriteLine( "Failed to load: {0}", fileName );

			} catch( FileLoadException ) {
				Console.Error.WriteLine( "Failed to load: {0}", fileName );

			} catch( ReflectionTypeLoadException ) {
				Console.Error.WriteLine( "Failed to load: {0}", fileName );

			} catch( TypeLoadException ) {
				Console.Error.WriteLine( "Failed to load: {0}", fileName );
			}

			return null;
		}

	}
}
