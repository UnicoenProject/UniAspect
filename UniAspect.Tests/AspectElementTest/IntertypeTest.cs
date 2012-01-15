using System.Linq;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using NUnit.Framework;
using UniAspect.Processor;
using UniAspect.Visitor;
using Unicoen.Processor;

namespace UniAspect.Tests.AspectElementTest {
	[TestFixture]
	internal class IntertypeTest {
		private AstVisitor _visitor;

		[SetUp]
		public void SetUp() {
			//アスペクトファイルのパスを取得
			var input =
					new ANTLRFileStream(FixtureUtil.GetAspectPath("simple_intertype_sample.apt"));
			
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
		public void インタータイプ宣言の対象言語を取得する() {
			Assert.That(
					_visitor.Intertypes.ElementAt(0).GetLanguageType(),
					Is.EqualTo("Java"));
		}

		[Test]
		public void インタータイプ宣言の対象クラス名を取得する() {
			Assert.That(
					_visitor.Intertypes.ElementAt(0).GetTarget(),
					Is.EqualTo("Foo"));
		}

		[Test]
		public void インタータイプ宣言のブロック内のコードを取得する() {
			var contents = _visitor.Intertypes.ElementAt(0).GetContents();
			var code = "";
			foreach (var e in contents) {
				code += e;
			}
			const string actual = "private int x = 10 ; public int getX ( ) { return x ; } ";
			
			//文字列による比較
			Assert.That(code, Is.EqualTo(actual));
			
			//文字列をモデル変換した結果を比較
			Assert.That(
					UcoGenerator.CreateIntertype("Java", code),
					Is.EqualTo(UcoGenerator.CreateIntertype("Java", actual)).Using(StructuralEqualityComparer.Instance));

		}
	}
}