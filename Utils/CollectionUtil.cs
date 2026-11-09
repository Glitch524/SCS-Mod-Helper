using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SCS_Mod_Helper.Utils; 
static class CollectionUtil {
	public static void AddItem<T>(DataGrid table, ObservableCollection<T> list, T newItem) {
		int selectedIndex = table.SelectedIndex;
		if (selectedIndex == -1) {
			selectedIndex = list.Count;
			list.Add(newItem);
		} else
			list.Insert(selectedIndex, newItem);
		table.SelectedIndex = selectedIndex;
		table.Focus();
	}

	public static void RemoveItem<T>(DataGrid table, ObservableCollection<T> list) {
		if (table.SelectedIndex == -1)
			return;
		while (table.SelectedItem != null) {
			list.Remove((T)table.SelectedItem);
		}
	}

	public static void MoveDataGridItems<T>(bool up, DataGrid table, ObservableCollection<T> list) {
		if (table.SelectedIndex == -1)
			return;
		int target = table.SelectedIndex;
		List<T> l = [];
		while (table.SelectedIndex != -1) {
			if (table.SelectedIndex < target) {
				target--;
			}
			l.Add(list[table.SelectedIndex]);
			list.RemoveAt(table.SelectedIndex);
		}
		if (up) {
			if (target > 0)
				target--;
		} else if (target < list.Count)
			target++;

		if (table.SelectionMode == DataGridSelectionMode.Extended) {
			var IList = table.SelectedItems;
			foreach (T pair in l) {
				list.Insert(target, pair);
				IList.Add(pair);
				target++;
			}
		} else {
			list.Insert(target, l.First());
			table.SelectedIndex = target;
		}
		table.Focus();
	}

	public static void MoveListBoxItems<T>(bool up, ListBox listBox, ObservableCollection<T> list) {
		if (listBox.SelectedIndex == -1)
			return;
		int target = listBox.SelectedIndex;
		List<T> l = [];
		while (listBox.SelectedIndex != -1) {
			if (listBox.SelectedIndex < target) {
				target--;
			}
			l.Add(list[listBox.SelectedIndex]);
			list.RemoveAt(listBox.SelectedIndex);
		}
		if (up) {
			if (target > 0)
				target--;
		} else if (target < list.Count)
			target++;

		if (listBox.SelectionMode == SelectionMode.Extended) {
			var IList = listBox.SelectedItems;
			foreach (T pair in l) {
				list.Insert(target, pair);
				IList.Add(pair);
				target++;
			}
		} else {
			list.Insert(target, l.First());
			listBox.SelectedIndex = target;
		}
		listBox.Focus();
	}
}
