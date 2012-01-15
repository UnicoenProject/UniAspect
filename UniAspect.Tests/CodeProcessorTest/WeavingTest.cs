using NUnit.Framework;

namespace UniAspect.Tests.CodeProcessorTest {
	/// <summary>
	///   アスペクトが正しく織り込まれているかテストする。
	/// </summary>
	[TestFixture]
	public class WeavingTest {
		private readonly string _fibonacciPath =
				FixtureUtil.GetInputPath("Java", "Default", "Fibonacci.java");

		private readonly string _studentPath =
				FixtureUtil.GetInputPath("Java", "Default", "Student.java");


		//TODO インタータイプ宣言の合成に関するテストを追加する

		//TODO 多項式中や、プロパティとしての関数呼び出し、関数の引数として現れるUnifiedCallに対しては、処理が行われないことを確認するテストを書く
	}
}