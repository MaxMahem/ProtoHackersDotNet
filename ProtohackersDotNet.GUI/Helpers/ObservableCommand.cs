using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Diagnostics;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.Helpers;

public class ObservableCommand<T>(Func<T> execute, IObservable<bool> canExecute, bool initialState) : IObservableCommand<T>
{
    readonly Subject<T> resultSubject = new();
    readonly BehaviorSubject<bool> canExecuteSubject = new(initialState);

    public IObservable<bool> CanExecute => Observable.CombineLatest(canExecute, this.canExecuteSubject.AsObservable(), 
        (external, intern) => external && intern).DistinctUntilChanged();

    public void Execute()
    {
        if (!this.canExecuteSubject.Value) { ThrowInvalidOperationException(); }
        try {
            this.canExecuteSubject.OnNext(false);

            T result = execute.Invoke();
            this.resultSubject.OnNext(result);

            this.canExecuteSubject.OnNext(true);
        }
        catch(Exception exception) {
            this.resultSubject.OnError(exception);
            this.canExecuteSubject.OnNext(false);
        }
    }

    public IObservable<T> Results => this.resultSubject.AsObservable();
}

// public class ObservableValueMarkupExtension<T>(BindingBase observableValue)
// {
//     public Binding ProvideValue(IServiceProvider serviceProvider)
//     {
//         var observer = Observer.Create<object>(value => observableValue.Value = value);
//         var InstancedBinding.TwoWay(observableValue.Changes.Select(value => (object) value), observer);
//     }
// 
//     public Binding ProvideValue<T>()
//     {
//         var bin = new MultiBinding()
//         {
//             Bindings = new[] { _first, _second },
//             Converter = new FuncMultiValueConverter<double, double>(doubles => doubles.Aggregate(1d, (x, y) => x * y))
//         };
// 
//         return mb;
//     }
// }