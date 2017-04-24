using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnitTestCategoryAssertor.Helpers;
using NUnitTestCategoryAssertor.Reflection;
using NUnitTestCategoryAssertor.Validation;

namespace NUnitTestCategoryAssertor.Scanner {

	public sealed class TestAssemblyScanner {

		private readonly IEnumerable<IAssemblyValidator> m_validators;

		public TestAssemblyScanner( IEnumerable<IAssemblyValidator> validators ) {
			m_validators = validators;
		}

		public TestAssembly Scan( Assembly assembly ) {

			var sw = new DebugStopwatch( "2.GetAssemblyCategories" );
			List<string> assemblyCategories = assembly
				.GetCustomAttributes<CategoryAttribute>()
				.Select( attr => attr.Name )
				.ToList();
			sw.Dispose();

			sw = new DebugStopwatch( "3.LoadTestFixturs" );
			List<TestFixture> fixtures = assembly.GetTypes()
				.Select( LoadTestFixtureOrNull )
				.Where( f => f != null )
				.ToList();
			sw.Dispose();

			TestAssembly testAssembly = new TestAssembly(
				assembly,
				assemblyCategories,
				fixtures
				);

			foreach( var validator in m_validators ) {
				using( new DebugStopwatch( $"4.{validator.GetType().Name}" ) ) {
					validator.Validate( testAssembly );
				}
			}

			return testAssembly;
		}

		private TestFixture LoadTestFixtureOrNull( Type type ) {

			TestFixtureAttribute[] testFixtureAttrs = type
				.GetCustomAttributes<TestFixtureAttribute>( true )
				.ToArray();

			if( !testFixtureAttrs.Any() ) {
				return null;
			}

			IEnumerable<string> testFixtureCategoryNames = testFixtureAttrs
				.Where( attr => attr.Category != null )
				.SelectMany( attr => attr.Category.Split( ',' ) );

			IEnumerable<string> categoryNames = type
				.GetCustomAttributes<CategoryAttribute>( true )
				.Select( attr => attr.Name );

			IList<string> testFixtureCategories = testFixtureCategoryNames
				.Concat( categoryNames )
				.ToList();

			BindingFlags bindingFlags = (
				BindingFlags.Public
				| BindingFlags.NonPublic
				| BindingFlags.Static
				| BindingFlags.Instance
			);

			IList<MethodInfo> methods = type.GetMethods( bindingFlags ).Where( IsTestMethod ).ToList();
			TestFixture fixture = new TestFixture( type, testFixtureCategories, methods );

			return fixture;
		}

		private static bool IsTestMethod( MethodInfo method ) {

			bool isTest = method.IsDefined( typeof( TestAttribute ), true );
			if( isTest ) {
				return true;
			}

			bool isTestCase = method.IsDefined( typeof( TestCaseAttribute ), true );
			if( isTestCase ) {
				return true;
			}

			return false;
		}

	}
}
