using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace NUnitTestCategoryAssertor.Scanner {

	public sealed class TestAssembly {
		private readonly IList<string> m_violations;

		public TestAssembly( 
			Assembly assembly, 
			IList<string> assemblyCategories, 
			IList<TestFixture> fixtures 
		) {
			Assembly = assembly;
			AssemblyCategories = new ReadOnlyCollection<string>( assemblyCategories );
			Fixtures = new ReadOnlyCollection<TestFixture>( fixtures );
			m_violations = new List<string>();
		}

		public Assembly Assembly { get; }

		public IReadOnlyList<string> AssemblyCategories { get; }

		public string Name => Assembly.GetName().Name;

		public IReadOnlyList<TestFixture> Fixtures { get; }

		public IReadOnlyList<string> Violations => new ReadOnlyCollection<string>( m_violations );

		public void AddViolation( string violation ) {
			m_violations.Add( violation );
		}

		public bool HasViolation => m_violations.Count > 0;

	}
}
