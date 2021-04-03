using ConnectionManster.UI.PC.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionManster.UI.PC.ViewModels
{
    public abstract class ScannerViewModel<T>:ObservableObject
    {
        protected ScannerViewModel()
        {
            Items = new ObservableCollection<T>();
            ScanCommand = new Command(Scan, () => !Scanning);
            CancelCommand = new Command(Cancel, () => Scanning);
            ThreadCount = 10;
        }

        public ObservableCollection<T> Items { get; }

        public int Timeout { get; set; }

        private T _from;
        public T From
        {
            get { return _from; }
            set { SetValue(ref _from, value, nameof(From)); }
        }

        private T _to;
        public T To
        {
            get { return _to; }
            set { SetValue(ref _to, value, nameof(T)); }
        }

        private bool _scanning;
        public bool Scanning
        {
            get { return _scanning; }
            private set { SetValue(ref _scanning, value, nameof(Scanning)); }
        }

        private int _total;
        public int Total
        {
            get { return _total; }
            private set { SetValue(ref _total, value, nameof(Total)); }
        }

        private int _finished;
        public int Finished
        {
            get { return _finished; }
            private set { SetValue(ref _finished, value, nameof(Finished)); }
        }

        public int ThreadCount { get; set; }

        public Command ScanCommand { get; }

        public Command CancelCommand { get; }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if(propertyName == nameof(Scanning))
            {
                ScanCommand.OnCanExecuteChanged();
                CancelCommand.OnCanExecuteChanged();
            }
        }

        private async void Scan()
        {
            string error;
            if(!Validate(out error))
            {
                Notify.ShowError(error, "校验");
                return;
            }
            Items.Clear();
            var queue = new ConcurrentQueue<T>();
            foreach(var item in Generate())
            {
                queue.Enqueue(item);
            }
            Total = queue.Count;
            Finished = 0;
            Scanning = true;
            if(source != null)
            {
                source.Dispose();
            }
            source = new CancellationTokenSource();
            var tasks = Enumerable.Range(0, ThreadCount)
                .Select(async i =>
                {
                    T item;
                    while (queue.TryDequeue(out item))
                    {
                        source.Token.ThrowIfCancellationRequested();
                        if (await TestAsync(item,source.Token))
                        {
                            App.Current.Dispatcher.Invoke(() => Items.Add(item));
                        }
                        Finished++;
                    }
                });
            try
            {
                await Task.WhenAll(tasks);
            }
            catch(OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Notify.ShowError(ex.Message, "扫描失败");
            }
            finally
            {
                Scanning = false;
            }
        }

        protected abstract IEnumerable<T> Generate();

        protected abstract Task<bool> TestAsync(T item, CancellationToken token = default);

        protected abstract bool Validate(out string message);

        private CancellationTokenSource source;
        private void Cancel()
        {
            source.Cancel();
        }
    }
}
