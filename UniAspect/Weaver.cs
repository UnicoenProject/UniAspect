﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using UniAspect.AspectElement;
using UniAspect.Processor;
using UniAspect.Processor.Pointcut;
using UniAspect.Visitor;
using Unicoen;
using Unicoen.Model;

namespace UniAspect {
	public static class Weaver {

		// アスペクトファイルを解析してその結果を保持する
		private static AstVisitor _visitor;

		// アスペクトファイルを解析してその結果をフィールドに格納します
		public static void AnalizeAspect(string aspectPath) {
			//アスペクト情報を持つオブジェクトを生成する
			var aspect = new ANTLRFileStream(aspectPath);
			var lexer = new UniAspectLexer(aspect);
			var tokens = new CommonTokenStream(lexer);
			var parser = new UniAspectParser(tokens);

			//アスペクトファイルを解析してASTを生成する
			var result = parser.aspect();
			var ast = (CommonTree)result.Tree;

			//ASTを走査してパース結果をアスペクトオブジェクトとしてvisitor内に格納する
			_visitor = new AstVisitor();
			_visitor.Visit(ast, 0, null);
		}
		
		// アスペクトの合成処理を実行します
		// このメソッドはすべてのファイルに対してWeave()メソッドを呼び出す処理を行い、
		// 実際の合成処理はWeave()メソッドが行います
		public static void Run(string directoryPath) {
			
			//指定されたパス以下にあるディレクトリをすべてoutput以下にコピーします
			var workPath = CleanOutputAndGetOutputPath();
			var directories = Directory.EnumerateDirectories(
					directoryPath, "*", SearchOption.AllDirectories);
			foreach (var dir in directories) {
				var newDir = dir.Replace(directoryPath, workPath);
				//WeavedSourceArea.Text += newDir;
				Directory.CreateDirectory(newDir);
			}

			//指定されたパス以下にあるソースコードのパスをすべて取得します
			var targetFiles = Collect(directoryPath);

			foreach (var file in targetFiles) {
				var newPath = file.Replace(directoryPath, workPath);

				// 対象ファイルの統合コードオブジェクトを生成する
				var gen = UnifiedGenerators.GetProgramGeneratorByExtension(Path.GetExtension(file));
				if(gen == null) {
					File.Copy(file, newPath);
					continue;
				}
				var model = gen.GenerateFromFile(file);

				//アスペクトの合成を行う
				Weave(ExtenstionToLanguageName(Path.GetExtension(file)), model);
				// 結果のファイル出力
				File.WriteAllText(newPath, UnifiedGenerators.GetCodeGeneratorByExtension(Path.GetExtension(file)).Generate(model));
			}
		}

		// ポイントカットをポイントカット名で参照できるようにします
		private static readonly Dictionary<string, Pointcut> Pointcuts =
				new Dictionary<string, Pointcut>();

		// 統合コードオブジェクトに対してアスペクトの合成処理を行います
		public static void Weave(string language, UnifiedProgram model) {
			//以前のアスペクトファイルの情報を消去するために辞書の内容を初期化する
			Pointcuts.Clear();

			//与えられたモデルに対してインタータイプを合成する
			foreach (var intertype in _visitor.Intertypes) {
				if (intertype.GetLanguageType() != language)
					continue;
				var members = UcoGenerator.CreateIntertype(
						intertype.GetLanguageType(), intertype.GetContents());
				InterType.AddIntertypeDeclaration(model, intertype.GetTarget(), members);
			}

			//ポイントカットを登録する
			foreach (var pointcut in _visitor.Pointcuts) {
				var name = pointcut.GetName();
				//同じ名前のポイントカットがある場合にはエラーとする
				if (Pointcuts.ContainsKey(name))
					throw new InvalidOperationException(
							"同名のポイントカットがすでに宣言されています: " + name);
				//ポイントカットを自身の名前で登録
				Pointcuts.Add(name, pointcut);
			}

			//アドバイスの適用
			foreach (var advice in _visitor.Advices) {
				//アドバイスのターゲットがポイントカット宣言されていない場合はエラーとする
				if (!Pointcuts.ContainsKey(advice.GetTarget()))
					throw new InvalidOperationException(
							"指定されたポイントカットは宣言されていません");

				//アドバイスのターゲットが登録されていれば、それに対応するポイントカットを取得する
				Pointcut target;
				Pointcuts.TryGetValue(advice.GetTarget(), out target);

				//指定された言語のアドバイスがあればそれをモデルに変換する
				UnifiedBlock code = null;
				foreach (var languageDependBlock in advice.GetFragments()) {
					//
					if (languageDependBlock.GetLanguageType().Equals(language)) {
						code = UcoGenerator.CreateAdvice(
								language, languageDependBlock.GetContents());
						break;
					}
				}
				//現在の対象ファイルの言語向けアドバイスが定義されていない場合は次のアドバイス処理に移る
				if (code == null)
					continue;

				//ポイントカットの指定に応じて適切なアドバイスの合成処理を行う
				//TODO ワイルドカードなどへの対応
				//TODO 複数のターゲットを持つポイントカットへの対応(これはそもそもパーサを改良する必要あり)

				var methodName = target.GetTargetName().ElementAt(1);

				// アドバイスの合成
				// リフレクション(MEF)を用いて、対応するメソッドが呼び出されます
				switch (advice.GetAdviceType()) {
				case "before":
					CodeProcessorProvider.WeavingBefore(target.GetPointcutType(), model, target.DeepCopy(), code);
					break;
				case "after":
					CodeProcessorProvider.WeavingAfter(target.GetPointcutType(), model, target.DeepCopy(), code);
					break;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		# region Utilities

		public static string CleanOutputAndGetOutputPath() {
			var path = Path.Combine(".", "output");
			if (Directory.Exists(path)) {
				var dirPaths = Directory.EnumerateDirectories(
						path, "*", SearchOption.TopDirectoryOnly);
				foreach (var dirPath in dirPaths) {
					Directory.Delete(dirPath, true);
				}
				var filePaths = Directory.EnumerateFiles(
						path, "*", SearchOption.TopDirectoryOnly);
				foreach (var filePath in filePaths) {
					File.Delete(filePath);
				}
			}
			Directory.CreateDirectory(path);
			return path.GetFullPathAddingSubNames();
		}

		// 指定されたフォルダ以下にあるファイルのパスのリストを返します
		public static IEnumerable<string> Collect(string folderRootPath) {
			//指定された文字列がフォルダじゃなかった場合
			if (!Directory.Exists(folderRootPath)) {
				var list = new List<string>();
				list.Add(folderRootPath);
				return list;
			}

			//指定された文字列に該当するフォルダがある場合
			return Directory.EnumerateFiles(
					folderRootPath, "*", SearchOption.AllDirectories);
		}

		// 拡張子名から言語名を返します
		private static string ExtenstionToLanguageName(string ext) {
			switch (ext) {
				case ".java":
					return "Java";
				case ".js":
					return "JavaScript";
				case ".c":
					return "C";
				case ".rb":
					return "Ruby";
				case ".py":
					return "Python";
				case ".cs":
					return "CSharp";
				default:
					throw new NotImplementedException();
			}
		}

		# endregion
	}
}