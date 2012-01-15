using System.Linq;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using NUnit.Framework;
using UniAspect.Visitor;

namespace UniAspect.Tests.AspectElementTest {
	public class AdviceTest {
		private AstVisitor _visitor;

		[SetUp]
		public void SetUp() {
			//アスペクトファイルのパスを取得
			var input = new ANTLRFileStream(FixtureUtil.GetAspectPath("simple_advice_sample.apt"));

			//アスペクトファイルをパースして抽象構文木を生成する
			var lex = new UniAspectLexer(input);
			var tokens = new CommonTokenStream(lex);
			var parser = new UniAspectParser(tokens);

			var result = parser.aspect();
			var ast = (CommonTree)result.Tree;

			//抽象構文木を走査して、ポイントカット・アドバイス情報を格納する
			_visitor = new AstVisitor();
			_visitor.Visit(ast, 0, null);
		}

		[Test]
		public void アドバイスの種類を取得できる() {
			var adviceType = _visitor.Advices.ElementAt(0).GetAdviceType();
			Assert.That(adviceType, Is.EqualTo("before"));
		}

		[Test]
		public void アドバイスの対象ポイントカット名を取得できる() {
			var target = _visitor.Advices.ElementAt(0).GetTarget();
			Assert.That(target, Is.EqualTo("move"));
		}

		[Test, Ignore]
		public void アドバイスのパラメータを取得できる() {
			//TODO アドバイスにパラメータを指定できるようにジョインポイントモデルを拡張する
		}

		[Test]
		public void アドバイスのコード断片を取得できる() {
			var advice = _visitor.Advices.ElementAt(1);
			var javaBlock = advice.GetFragments().ElementAt(0).GetLanguageType();
			Assert.That(javaBlock, Is.EqualTo("Java"));
			var jsBlock = advice.GetFragments().ElementAt(1).GetLanguageType();
			Assert.That(jsBlock, Is.EqualTo("JavaScript"));
		}
	}
}