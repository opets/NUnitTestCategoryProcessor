using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using NUnitTestCategoryAssertor.Reflection;

namespace NUnitTestCategoryAssertor.Scanner {

	public sealed class TestFixture {

		private static readonly CSharpTypeNameFormatter m_typeNameFormatter = new CSharpTypeNameFormatter();

		private readonly Type m_type;
		private readonly IList<TestViolation> m_violations;

		public TestFixture( 
			Type type,
			IList<string> testFixtureCategories,
			IList<MethodInfo> testMethods
		) {
			m_type = type;
			TestFixtureCategories = new ReadOnlyCollection<string>( testFixtureCategories );
			TestMethods = new ReadOnlyCollection<MethodInfo>( testMethods );
			m_violations = new List<TestViolation>();
		}

		public string Name => m_typeNameFormatter.FormatFullName( m_type );

		public IReadOnlyList<string> TestFixtureCategories { get;  }

		public IReadOnlyList<MethodInfo> TestMethods { get; }

		public IReadOnlyList<TestViolation> Violations => new ReadOnlyCollection<TestViolation>( m_violations );

		public void AddViolation( TestViolation testViolation ) {
			m_violations.Add( testViolation );
		}

		public bool HasViolation => m_violations.Count > 0;

	}
}
