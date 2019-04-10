using System;
using System.Collections.Generic;

namespace Visual_Music
{
	internal class UndoItems
	{
		public class Item
		{
			public string Desc;
			public Project Project;
			public Item(string desc, Project project)
			{
				Desc = desc;
				Project = project;
			}
		}

		LinkedList<Item> items = new LinkedList<Item>();
		LinkedListNode<Item> currentItem;

		public Item Current => currentItem == null ? null : currentItem.Value;
		public Item Previous => currentItem.Previous == null ? null : currentItem.Previous.Value;
		public Item Next => currentItem.Next == null ? null : currentItem.Next.Value;
		public string UndoDesc => Current == null ? "" : Current.Desc;
		public string RedoDesc => Next == null ? "" : Next.Desc;

		public void clear()
		{
			foreach (var item in items)
				item.Project.Dispose();
			items.Clear();
			currentItem = null;
		}
		public void add(string desc, Project project)
		{
			var newItem = new Item(desc, project.clone());

			if (currentItem == null)
			{
				items.AddLast(newItem);
				currentItem = items.Last;
			}
			else
			{
				items.AddAfter(currentItem, newItem);
				currentItem = currentItem.Next;
				while (currentItem.Next != null)
				{
					currentItem.Next.Value.Project.Dispose();
					items.Remove(currentItem.Next);
				}
			}
		}

		void replaceLast(string desc, Project project)
		{
			
		}

		public static UndoItems operator++(UndoItems obj)
		{
			obj.currentItem = obj.currentItem.Next;
			return obj;
		}
		public static UndoItems operator--(UndoItems obj)
		{
			obj.currentItem = obj.currentItem.Previous;
			return obj;
		}
	}
}