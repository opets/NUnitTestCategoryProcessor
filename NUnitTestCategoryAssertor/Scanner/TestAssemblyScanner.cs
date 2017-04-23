using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnitTestCategoryAssertor.Reflection;

namespace NUnitTestCategoryAssertor.Scanner {

	public sealed class TestAssemblyScanner {

		private static readonly CSharpTypeNameFormatter m_typeNameFormatter = new CSharpTypeNameFormatter();

		private readonly ISet<string> m_requiredCategories;
		private readonly ISet<string> m_prohibitedAssemblyCategories;

		public TestAssemblyScanner(
				ISet<string> requiredCategories,
				ISet<string> prohibitedAssemblyCategories
			) {
			m_requiredCategories = requiredCategories;
			m_prohibitedAssemblyCategories = prohibitedAssemblyCategories;
		}

		public TestAssembly Scan( Assembly assembly ) {

			IEnumerable<string> assemblyCategories = assembly
				.GetCustomAttributes<CategoryAttribute>()
				.Select( attr => attr.Name )
				.ToArray();

			IEnumerable<string> assemblyViolations = assemblyCategories
				.Where( m_prohibitedAssemblyCategories.Contains )
				.Select( c => $"Invalid assembly-level category \"{c}\"" );

			TestAssembly testAssembly = new TestAssembly(
					name: assembly.GetName().Name,
					violations: assemblyViolations.ToList()
				);

			Type[] types = assembly.GetTypes();
			foreach( Type type in types ) {

				TestFixture fixture = LoadTestFixtureOrNull( type, assemblyCategories );
				if( fixture != null ) {

					testAssembly.Fixtures.Add( fixture );
				}
			}

			return testAssembly;
		}

		private TestFixture LoadTestFixtureOrNull(
				Type type,
				IEnumerable<string> assemblyCategories
			) {

			TestFixtureAttribute[] testFixtureAttrs = type
				.GetCustomAttributes<TestFixtureAttribute>( true )
				.ToArray();

			if( testFixtureAttrs.Length == 0 ) {
				return null;
			}

			// -----------------------------------------------------

			IEnumerable<string> testFixtureCategoryNames = testFixtureAttrs
				.Where( attr => attr.Category != null )
				.SelectMany( attr => attr.Category.Split( ',' ) );

			IEnumerable<string> categoryNames = type
				.GetCustomAttributes<CategoryAttribute>( true )
				.Select( attr => attr.Name );

			IEnumerable<string> testFixtureCategories = testFixtureCategoryNames
				.Concat( categoryNames )
				.ToArray();

			// -----------------------------------------------------

			string fixtureName = m_typeNameFormatter.FormatFullName( type );
			TestFixture fixture = new TestFixture( fixtureName );

			BindingFlags bindingFlags = (
					BindingFlags.Public
					| BindingFlags.NonPublic
					| BindingFlags.Static
					| BindingFlags.Instance
				);

			MethodInfo[] methods = type.GetMethods( bindingFlags );
			foreach( MethodInfo method in methods ) {

				bool isTest = IsTestMethod( method );
				if( isTest ) {

					TestViolation violation = AssertTestOrNull(
							type,
							method,
							assemblyCategories,
							testFixtureCategories
						);

					if( violation != null ) {
						fixture.Violations.Add( violation );
					}
				}
			}

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

		private TestViolation AssertTestOrNull(
				Type type,
				MethodInfo method,
				IEnumerable<string> assemblyCategories,
				IEnumerable<string> testFixtureCategories
			) {

			ISet<string> testCategories = method
				.GetCustomAttributes<CategoryAttribute>( true )
				.Select( attr => attr.Name )
				.ToHashSet( StringComparer.OrdinalIgnoreCase );

			testCategories.UnionWith( assemblyCategories );
			testCategories.UnionWith( testFixtureCategories );

			testCategories.IntersectWith( m_requiredCategories );
			if( testCategories.Count == 0 ) {

				return new TestViolation( method.Name, "No test category defined" );

				/*
			} else if( testCategories.Count > 1 ) {

				string msg = String.Format(
						"Multiple test categories defined: {0}",
						String.Join( ", ", testCategories )
					);

				return new TestViolation( method.Name, msg );*/

			} else {
				return null;
			}
		}

	}
}
