using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VitalConnection.AAL.Builder
{
	public class DelegateCommand : ICommand
	{
		private readonly Action _action;

		public DelegateCommand(Action action)
		{
			_action = action;
		}

		public void Execute(object parameter)
		{
			_action();
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}


		public event EventHandler CanExecuteChanged;

	}

	public class DelegateCommand<T> : ICommand
	{
		private readonly Action<T> _action;

		public DelegateCommand(Action<T> action)
		{
			_action = action;
		}

		public void Execute(object parameter)
		{
			_action((T)parameter);
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}


		public event EventHandler CanExecuteChanged;

	}


}
