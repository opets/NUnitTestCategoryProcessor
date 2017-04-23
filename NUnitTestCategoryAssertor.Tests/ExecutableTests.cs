using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace NUnitTestCategoryAssertor.Tests {

	[TestFixture]
	public sealed class ExecutableTests {

		private static readonly string AssemblyLevelCategorizationPath = Path.Combine(
				TestAssembliesLocator.Path, "assemblylevelcategorization"
			);

		[Test]
		public void NUnitTestCategoryAssertor_WhenAllTestsHaveCategories_ShouldReturn0() {

			int result = ExecuteNUnitTestCategoryAssertor(
					"--assembliesPath",
					Path.Combine( TestAssembliesLocator.Path, "nunit3" ),
					"--category",
					"Integration,Unit"
				);

			Assert.AreEqual( 0, result );
		}

		[Test]
		public void NUnitTestCategoryAssertor_WhenOldNUnit_ShouldNotReturn0() {

			int result = ExecuteNUnitTestCategoryAssertor(
					"--assembliesPath",
					Path.Combine( TestAssembliesLocator.Path, "nunit2" ),
					"--category",
					"Integration,Unit"
				);

			Assert.AreEqual( -102, result );
		}

		[Test]
		public void NUnitTestCategoryAssertor_WhenMissingCategories_ShouldNotReturn0() {

			int result = ExecuteNUnitTestCategoryAssertor(
					"--assembliesPath",
					Path.Combine( TestAssembliesLocator.Path, "nunit3" ),
					"--category",
					"Crazy"
				);

			Assert.AreEqual( 2, result );
		}

		[Test]
		public void NUnitTestCategoryAssertor_WhenAssemblyDoesNotContainProhibitedCategory_ShouldReturn0() {

			int result = ExecuteNUnitTestCategoryAssertor(
					"--assembliesPath",
					AssemblyLevelCategorizationPath,
					"--prohibitedAssemblyCategories",
					"OtherCategory"
				);

			Assert.AreEqual( 0, result );
		}

		[Test]
		public void NUnitTestCategoryAssertor_WhenAssemblyContainsProhibitedCategory_ShouldReturnViolations() {

			int result = ExecuteNUnitTestCategoryAssertor(
					"--assembliesPath",
					AssemblyLevelCategorizationPath,
					"--prohibitedAssemblyCategories",
					"Assembly"
				);

			Assert.AreEqual( 1, result );
		}

		private static int ExecuteNUnitTestCategoryAssertor( params string[] arguments ) {

			Assembly assembly = typeof( NUnitTestCategoryAssertor.Program ).Assembly;

			AppDomain appDomain = AppDomain.CreateDomain(
					friendlyName: TestContext.CurrentContext.Test.Name,
					securityInfo: AppDomain.CurrentDomain.Evidence,
					info: new AppDomainSetup {
						ApplicationBase = Path.GetDirectoryName( assembly.Location )
					}
				);

			try {
				int exitCode = appDomain.ExecuteAssembly( assembly.CodeBase, arguments );
				return exitCode;

			} finally {
				AppDomain.Unload( appDomain );
			}
		}

	}
}
