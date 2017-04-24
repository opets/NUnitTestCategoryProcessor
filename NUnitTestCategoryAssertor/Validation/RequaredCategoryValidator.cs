using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnitTestCategoryAssertor.Helpers;
using NUnitTestCategoryAssertor.Scanner;

namespace NUnitTestCategoryAssertor.Validation {

	internal sealed class RequaredCategoryValidator : IAssemblyValidator {

		private readonly ISet<string> m_requaredCategories;

		public RequaredCategoryValidator( ISet<string> requaredCategories ) {
			m_requaredCategories = requaredCategories;
		}

		public void Validate( TestAssembly testAssembly ) {

			foreach( var fixture in testAssembly.Fixtures ) {

				foreach( var method in fixture.TestMethods ) {

					TestViolation violation = AssertTestOrNull(
						method,
						testAssembly.AssemblyCategories,
						fixture.TestFixtureCategories
					);

					if( violation != null ) {
						fixture.AddViolation( violation );
					}

				}
			}
		}

		private TestViolation AssertTestOrNull(
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

			testCategories.IntersectWith( m_requaredCategories );
			if( testCategories.Count == 0 ) {

				return new TestViolation( method.Name, "No test category defined" );

				/*
			} else if( testCategories.Count > 1 ) {

				string msg = String.Format(
						"Multiple test categories defined: {0}",
						String.Join( ", ", testCategories )
					);

				return new TestViolation( method.Name, msg );*/

			}

			return null;
		}

	}
}
