using System.Collections.Generic;
using System.Linq;
using NUnitTestCategoryAssertor.Scanner;

namespace NUnitTestCategoryAssertor.Validation {

	internal sealed class ProhibitedAssemblyCategoryValidator: IAssemblyValidator {

		private readonly ISet<string> m_prohibitedAssemblyCategories;

		public ProhibitedAssemblyCategoryValidator( ISet<string> prohibitedAssemblyCategories ) {
			m_prohibitedAssemblyCategories = prohibitedAssemblyCategories;
		}

		public void Validate( TestAssembly testAssembly ) {

			testAssembly.AssemblyCategories
				.Where( m_prohibitedAssemblyCategories.Contains )
				.Select( category => $"Invalid assembly-level category \"{category}\"" )
				.ToList()
				.ForEach( testAssembly.AddViolation );
		}

	}
}
