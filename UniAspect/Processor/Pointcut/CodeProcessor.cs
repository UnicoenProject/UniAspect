using Unicoen.Model;

namespace UniAspect.Processor.Pointcut {
	public abstract class CodeProcessor {
		public abstract string PointcutName { get; }

		// TODO Pointcutの名前が競合しないようにする
		public abstract void Before(IUnifiedElement model, AspectElement.Pointcut target, UnifiedBlock advice);
		public abstract void After(IUnifiedElement model, AspectElement.Pointcut target, UnifiedBlock advice);
		public abstract void Around(IUnifiedElement model);
	}
}
